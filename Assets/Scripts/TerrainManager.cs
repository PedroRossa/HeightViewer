using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using OSGeo.GDAL;
using UnityEngine.Networking;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainManager : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    [Header("Terrain properties")]
    public int width = 512;
    public int height = 256;
    public int maxHeight = 20;

    private int diffWidth;
    private int diffHeight;

    public Texture2D texHeight;
    public Gradient gradient;
    public AnimationCurve animCurve;
    public float animationSpeed;

    //west, south, east, north
    private double westCoord;
    private double southCoord;
    private double eastCoord;
    private double northCoord;

    private List<Placemark> placemarks = new List<Placemark>();

    public enum PlacemarkType
    {
        SAMPLE,
        PANORAMIC,
        ROUTE,
        OTHER
    }

    public struct Placemark
    {
        public int id;
        public string name;
        public double latitude;
        public double longitude;
        public PlacemarkType type;
        public List<Vector2> routeValues;

        public Placemark(int id, string name, double latitude = 0, double longitude = 0, PlacemarkType type = PlacemarkType.OTHER, List<Vector2> routeValues = null)
        {
            this.id = id;
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
            this.type = type;

            this.routeValues = routeValues != null ? routeValues : new List<Vector2>();
        }

        public static PlacemarkType TypeByName(string name)
        {
            switch (name.Split('_')[0].ToLower())
            {
                case "sample":
                    return PlacemarkType.SAMPLE;
                case "panoramic":
                    return PlacemarkType.PANORAMIC;
                case "route":
                    return PlacemarkType.ROUTE;
                default:
                    return PlacemarkType.OTHER;
            }

        }
    }

    void Start()
    {
        string uri = "http://opentopo.sdsc.edu";
        string paramss = "/otr/getdem?demtype=SRTMGL1&west=-120.168457&south=36.738884&east=-118.465576&north=38.091337&outputFormat=GTiff";

        //StartCoroutine(Upload(uri, paramss));

        LoadGeoTiffOnHeightTexture("C:/yosemite.tif");
        ReadKMLFile();

        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        meshRenderer.material = new Material(Shader.Find("Custom/VertexColorShader"));
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        CreateMesh();
    }

    IEnumerator Upload(string uri, string paramss)
    {
        string value = "http://opentopo.sdsc.edu/otr/getdem?" +
            "demtype=SRTMGL1&" +
            "west=-119.65227127075197&" +
            "south=37.69903420794415&" +
            "east=-119.52283859252931&" +
            "north=37.77804178967591&" +
            "outputFormat=GTiff";

        using (UnityWebRequest www = UnityWebRequest.Post(uri, paramss))
        {
            www.chunkedTransfer = true;
            www.timeout = 25;
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                DownloadHandler dh = www.downloadHandler;
                Gdal.FileFromMemBuffer("C:/testzin.tif", dh.data);

                // LoadGeoTiffOnHeightTexture("C:/testzin.tif");
                Debug.Log(dh.text);
            }

        }
    }

    private void SetPlacemarkData(XmlNode xmlElement, out Placemark placemark)
    {
        placemark = new Placemark();

        foreach (XmlNode item in xmlElement.ChildNodes)
        {
            if(item.Name.Equals("name"))
            {
                placemark.name = item.InnerText;
                placemark.type = Placemark.TypeByName(placemark.name);
            }
            //pins
            if(item.Name.Equals("Point"))
            {
                foreach (XmlNode child in item)
                {
                    if(child.Name.Equals("coordinates"))
                    {
                        string val = child.InnerText;
                        placemark.longitude = Convert.ToDouble(val.Split(',')[0]);
                        placemark.latitude = Convert.ToDouble(val.Split(',')[1]);
                    }
                }
            }
            //routes
            if(item.Name.Equals("LineString"))
            {
                placemark.routeValues = new List<Vector2>();
                string val = item.FirstChild.InnerText;
                //Remove last ',' and spaces
                val = val.Remove(item.FirstChild.InnerText.LastIndexOf(',')).Trim();
                //Remove altitude values from array (,0 )
                val = val.Replace("0 ", "");
                //Split values by ','
                string[] routeVals = val.Split(',');

                for (int i = 0; i < routeVals.Length; i+=2)
                {
                    placemark.routeValues.Add(
                        new Vector2(
                            Convert.ToSingle(routeVals[i]), 
                            Convert.ToSingle(routeVals[i + 1])
                    ));
                }
            }
        }
    }

    private void ReadKMLFile()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load("C:/test.kml");
        XmlNodeList placesmarks = xmlDoc.GetElementsByTagName("Placemark");

        foreach (XmlNode item in placesmarks)
        {            
            Placemark pl = new Placemark();
            SetPlacemarkData(item, out pl);

            placemarks.Add(pl);

            //Debug.Log(pl.type + " | " + pl.name + " lat:" + pl.latitude + " long:" + pl.longitude);
        }
        CreatePins();
    }

    private void CreatePins()
    {
        double horizontalSize = eastCoord - westCoord;
        double verticalSize = northCoord - southCoord;

        foreach (Placemark item in placemarks)
        {
            if (item.type == PlacemarkType.SAMPLE || item.type == PlacemarkType.PANORAMIC)
            {
                double auxLong = item.longitude - westCoord;
                double auxLat = item.latitude - southCoord;

                double calculatedX = width * auxLong / horizontalSize;
                double calculatedY = height * auxLat / verticalSize;

                GameObject go = CreatePin(PinType.SAMPLE);
                float auxHeight = texHeight.GetPixel((int)calculatedX, (int)calculatedY).r;
                go.transform.localPosition = new Vector3((float)calculatedX, auxHeight * maxHeight, (float)calculatedY);
            }
            else if (item.type == PlacemarkType.ROUTE)
            {
                LineRenderer lr = gameObject.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.widthMultiplier = 0.05f;
                lr.positionCount = item.routeValues.Count;
                lr.startColor = new Color(0, 0, 1, 0.5f);
                lr.endColor = new Color(0, 0, 1, 0.5f);
                lr.useWorldSpace = false;

                int count = 0;
                foreach (Vector2 value in item.routeValues)
                {
                    double auxLong = value.x - westCoord;
                    double auxLat = value.y - southCoord;

                    double calculatedX = width * auxLong / horizontalSize;
                    double calculatedY = height * auxLat / verticalSize;

                    float auxHeight = texHeight.GetPixel((int)calculatedX, (int)calculatedY).r;
                    //Add 10% of maxHeight to unblock from model
                    float finalHeight = auxHeight * maxHeight + maxHeight * 0.1f;

                    lr.SetPosition(count, new Vector3((float)calculatedX, finalHeight, (float)calculatedY));

                    count++;
                }                
            }
        }
    }

    private void LoadGeoTiffOnHeightTexture(string path)
    {
        Gdal.AllRegister();

        Dataset rasterDataset = Gdal.Open(path, Access.GA_ReadOnly);
        if (rasterDataset == null)
        {
            Debug.Log("Unable to read input raster..");
        }
        //raster bands  
        int bandCount = rasterDataset.RasterCount;
        if (bandCount > 1)
        {
            Debug.Log("Input error, please provide single band raster image only..");
        }

        //raster size  
        int rasterCols = rasterDataset.RasterXSize;
        int rasterRows = rasterDataset.RasterYSize;

        //Extract geotransform  
        double[] geotransform = new double[6];
        rasterDataset.GetGeoTransform(geotransform);

        //Get raster bounding box  
        double originX = geotransform[0];
        double originY = geotransform[3];
        double pixelWidth = geotransform[1];
        double pixelHeight = geotransform[5];

        //Calculate box
        double finalX = originX + (rasterCols * pixelWidth);
        double finalY = originY + (rasterRows * pixelHeight);

        westCoord = originX;
        southCoord = finalY;
        eastCoord = finalX;
        northCoord = originY;
        
        //Read 1st band from raster  
        Band band = rasterDataset.GetRasterBand(1);
        int rastWidth = rasterCols;
        int rastHeight = rasterRows;

        texHeight = GrayScaleTexture2DFromGDALBand(band);
    }

    private Texture2D GrayScaleTexture2DFromGDALBand(Band band)
    {
        //Get band min-max
        double[] bandMinMax = new double[2];
        band.ComputeRasterMinMax(bandMinMax, 0);
        double bandMin = bandMinMax[0];
        double bandMax = bandMinMax[1];

        int width = band.XSize;
        int height = band.YSize;

        Texture2D tex = new Texture2D(width, height);

        //int16
        short[] int16Pixels = new short[width * height];
        band.ReadRaster(0, 0, width, height, int16Pixels, width, height, 0, 0);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int currVal = (int16Pixels[i + j * width]);

                float f = (currVal - (float)bandMin) / ((float)bandMax - (float)bandMin);
                Color col = new Color(f, f, f, 1.0f);
                tex.SetPixel(i, j, col);
            }
        }

        tex.Apply();
        return tex;
    }

    private void CreateVertices(ref Vector3[] vertices, ref Color[] colors)
    {
        vertices = new Vector3[width * height];
        colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                vertices[j + i * width] = new Vector3(j, 0, i);
                colors[j + i * width] = Color.gray;
            }
        }
    }

    private void CreateTriangles(ref int[] triangles)
    {
        //Number of triangles -> (w-1)*2*(h-1)
        int nt = (width - 1) * 2 * (height - 1);

        //number of triangle points -> nt * 3
        triangles = new int[nt * 3];
        //Triangle index -> 1º = (index, index+1, index + w) // 2º = (index+1, index+1+w, index+w)
        int index = 0;
        int count = 0;

        for (int i = 0; i < height - 1; i++)
        {
            for (int j = 0; j < width - 1; j++)
            {
                index = j + (i * width);
                //First
                triangles[0 + (count) * 6] = index;
                triangles[1 + (count) * 6] = index + width;
                triangles[2 + (count) * 6] = index + 1;
                //Second       
                triangles[3 + (count) * 6] = index + 1;
                triangles[4 + (count) * 6] = index + width;
                triangles[5 + (count) * 6] = index + 1 + width;

                count++;
            }
        }
    }

    private Color[] ColorizeVertices()
    {
        Helper.Resize(ref texHeight, width, height);
        CalculateDiffFromTerrainToTexture();

        Color[] colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;

                if ((j - diffWidth) < texHeight.width && j > diffWidth &&
                    (i - diffHeight) < texHeight.height && i > diffHeight)
                {
                    float currHeight = texHeight.GetPixel(j - diffWidth, i - diffHeight).r;
                    colors[currIndex] = gradient.Evaluate(currHeight);
                }
                else
                {
                    colors[currIndex] = new Color(0, 0, 0, 0);
                }
            }
        }
        return colors;
    }

    private void SetTerrainHeight()
    {
        Helper.Resize(ref texHeight, width, height);
        CalculateDiffFromTerrainToTexture();

        Vector3[] vertices = new Vector3[width * height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;
                if ((j - diffWidth) < texHeight.width && j > diffWidth &&
                    (i - diffHeight) < texHeight.height && i > diffHeight)
                {
                    float currHeight = texHeight.GetPixel(j - diffWidth, i - diffHeight).r;
                    vertices[currIndex] = new Vector3(j, currHeight * maxHeight, i);
                }
                else
                {
                    vertices[currIndex] = new Vector3(j, 0, i);
                }
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    private void CalculateDiffFromTerrainToTexture()
    {
        diffWidth = (width - texHeight.width) / 2;
        diffHeight = (height - texHeight.height) / 2;
    }

    void CreateMesh()
    {
        Vector3[] vertices = new Vector3[0];
        Color[] colors = new Color[0];
        int[] triangles = new int[0];

        CreateVertices(ref vertices, ref colors);
        CreateTriangles(ref triangles);

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();

        //Render as points
        //int[] indices = new int[w * h];
        //for (int i = 0; i < w * h; i++)
        //{
        //    indices[i] = i;
        //}
        //mesh.SetIndices(indices, MeshTopology.Points, 0);
        //mesh.RecalculateBounds();

        mesh.UploadMeshData(false);
    }

    IEnumerator AnimateMeshCoroutine(float initScale = 0.001f, float finalScale = 1, float duration = 1)
    {
        float timer = 0;
        float percent = 0;

        meshRenderer.transform.localScale = new Vector3(1, initScale, 1);
        yield return new WaitForSeconds(1);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            percent = animCurve.Evaluate(timer / duration);
            meshRenderer.transform.localScale = new Vector3
                (
                    1,
                    Mathf.Lerp(0, finalScale * percent, percent),
                    1
                );
            yield return null;
        }

        meshRenderer.transform.localScale = Vector3.one;
    }

    private void AnimateVertices(float initScale = 0.001f, float finalScale = 1)
    {
        StartCoroutine(AnimateMeshCoroutine(initScale, finalScale, animationSpeed));
    }

    [Button]
    public void SetHeight()
    {
        SetTerrainHeight();
        AnimateVertices();
    }

    [Button]
    public void ColorizeWithGradient()
    {
        mesh.colors = ColorizeVertices();
        mesh.UploadMeshData(false);
    }

    [Button]
    public void LoadPackage()
    {
        SandBoxData.Init();

        string packagePath = SandBoxData.SelectPackage();

        //Load heightmap texture by package
        texHeight = SandBoxData.LoadImageAsTexture(packagePath + SandBoxData.instance.heightMapPath);
        SetHeight();

        GameObject[] sampleObjects = SandBoxData.LoadSamples(packagePath);
        CreateSamplePins(sampleObjects);
        GameObject[] panoramicObjects = SandBoxData.LoadPanoramicImages(packagePath);
        CreatePanoramicPins(panoramicObjects);
    }

    public enum PinType
    {
        SAMPLE,
        PANORAMIC,
        GENERIC
    }

    private void CreateSamplePins(GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject go = CreatePin(PinType.SAMPLE);

            go.transform.SetParent(transform);
            float x = SandBoxData.instance.samples[i].latitude;
            float z = SandBoxData.instance.samples[i].longitude;

            Color heightColor = texHeight.GetPixel((int)x, (int)z);

            Vector3 pos = Vector3.zero;
            pos.x = x + diffWidth;
            pos.y = heightColor.r * maxHeight;
            pos.z = z + diffHeight;

            go.transform.localPosition = pos;
        }
    }

    private void CreatePanoramicPins(GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject go = CreatePin(PinType.PANORAMIC);

            go.transform.SetParent(transform);
            float x = SandBoxData.instance.panoramicImages[i].latitude;
            float z = SandBoxData.instance.panoramicImages[i].longitude;

            Color heightColor = texHeight.GetPixel((int)x, (int)z);

            Vector3 pos = Vector3.zero;
            pos.x = x + diffWidth;
            pos.y = heightColor.r * maxHeight;
            pos.z = z + diffHeight;

            go.transform.localPosition = pos;
        }
    }

    private GameObject CreatePin(PinType pinType)
    {
        GameObject pin = Instantiate(Resources.Load("Pin", typeof(GameObject)) as GameObject, transform);

        switch (pinType)
        {
            case PinType.SAMPLE:
                pin.GetComponentInChildren<PinManager>().color = Color.red;
                break;
            case PinType.PANORAMIC:
                pin.GetComponentInChildren<PinManager>().color = Color.blue;
                break;
            case PinType.GENERIC:
                pin.GetComponentInChildren<PinManager>().color = Color.green;
                break;
        }

        return pin;
    }
}