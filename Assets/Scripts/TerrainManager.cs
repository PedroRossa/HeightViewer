using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using OSGeo.GDAL;
using UnityEngine.Networking;
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

    public bool loadingFromOpenTopography;
    public bool loadingFinished;

    private List<Placemark> placemarks = new List<Placemark>();

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        meshRenderer.material = new Material(Shader.Find("Custom/VertexColorShader"));
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        CreateMesh();
    }

    private void LoadFromOpenTopography(Helper.WGS84 coordinates)
    {
        loadingFromOpenTopography = true;
        loadingFinished = false;
        OpenTopographyAPI.OpenTopographyElement element = new OpenTopographyAPI.OpenTopographyElement(coordinates);

        StartCoroutine(OpenTopographyAPI.DownloadGeoTiff(element, "C:/", "myFile", DownloadedGeoTiffCallback));
    }

    public void DownloadedGeoTiffCallback(string path, int responseCode)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Geotiff downloaded from OpenTopography. Path: " + path);

            Helper.WGS84 coordinates;

            //Read Geotiff image downloaded, set the coordinates and the heightTexture
            Helper.LoadGeotiffData(path, out coordinates, out texHeight);
            
            //Get texture returned and set on map
            SetHeight();
            ColorizeWithGradient();
        }
        else
        {
            Debug.Log("Error on download geotiff image from OpenTopography. Response error: " + responseCode);
        }

        loadingFromOpenTopography = false;
        loadingFinished = true;
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

    private void CreateMesh()
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

    private void AnimateVertices(float initScale = 0.001f, float finalScale = 1)
    {
        StartCoroutine(Helper.AnimateMeshCoroutine(meshRenderer, animCurve, initScale, finalScale, animationSpeed));
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
        ColorizeWithGradient();

        //Load kml from googleEarth, with all the informations
        placemarks = Helper.ReadKMLFile(packagePath + SandBoxData.instance.kmlPath);

        //GameObject[] sampleObjects = SandBoxData.LoadSamples(packagePath);
        //CreateSamplePins(sampleObjects);
        //GameObject[] panoramicObjects = SandBoxData.LoadPanoramicImages(packagePath);
        //CreatePanoramicPins(panoramicObjects);
    }

    [Button]
    public void LoadMapFromOpenTopography()
    {
        SandBoxData.Init();
        
        //TODO: AQUI EH A CHAMADA PRA QUANDO QUISER FAZER LOAD DE NOVA AREA A PARTIR DA INTERACAO COM O MAPA
        Helper.WGS84 coordinates = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);
        LoadFromOpenTopography(coordinates);
    }

    //REVISAR
    private void CreatePins(Helper.WGS84 coordinates)
    {
        double horizontalSize = coordinates.east - coordinates.west;
        double verticalSize = coordinates.north - coordinates.south;

        foreach (Placemark item in placemarks)
        {
            if (item.Type == Placemark.PlacemarkType.SAMPLE || item.Type == Placemark.PlacemarkType.PANORAMIC)
            {
                double auxLong = item.Longitude - coordinates.west;
                double auxLat = item.Latitude - coordinates.south;

                double calculatedX = width * auxLong / horizontalSize;
                double calculatedY = height * auxLat / verticalSize;

                //GameObject go = CreatePin(PinType.SAMPLE);
                GameObject go = CreateInteractiveObject();
                float auxHeight = texHeight.GetPixel((int)calculatedX, (int)calculatedY).r;
                go.transform.localPosition = new Vector3((float)calculatedX, auxHeight * maxHeight, (float)calculatedY);
            }
            else if (item.Type == Placemark.PlacemarkType.ROUTE)
            {
                LineRenderer lr = gameObject.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.widthMultiplier = 0.015f;
                lr.positionCount = item.RouteValues.Count;
                lr.startColor = new Color(0, 0, 1, 0.5f);
                lr.endColor = new Color(0, 0, 1, 0.5f);
                lr.useWorldSpace = false;
                lr.sortingOrder = 5;

                int count = 0;
                foreach (Vector2 value in item.RouteValues)
                {
                    double auxLong = value.x - coordinates.west;
                    double auxLat = value.y - coordinates.south;

                    double calculatedX = width * auxLong / horizontalSize;
                    double calculatedY = height * auxLat / verticalSize;

                    float auxHeight = texHeight.GetPixel((int)calculatedX, (int)calculatedY).r;
                    //Add 10% of maxHeight to unblock from model
                    float finalHeight = auxHeight * maxHeight + maxHeight * 0.1f;

                    lr.SetPosition(count, new Vector3((float)calculatedX, finalHeight, (float)calculatedY));

                    count++;
                }

                lr.Simplify(5);
            }
        }
    }


    //AQUI PRECISO FAZER CERTINHO OS LOADS

    private void CreateSamplePins(GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            //GameObject go = CreatePin();

            //go.transform.SetParent(transform);
            //float x = SandBoxData.instance.samples[i].latitude;
            //float z = SandBoxData.instance.samples[i].longitude;

            //Color heightColor = texHeight.GetPixel((int)x, (int)z);

            //Vector3 pos = Vector3.zero;
            //pos.x = x + diffWidth;
            //pos.y = heightColor.r * maxHeight;
            //pos.z = z + diffHeight;

            //go.transform.localPosition = pos;
        }
    }

    private void CreatePanoramicPins(GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            //GameObject go = CreatePin(PinType.PANORAMIC);

            //go.transform.SetParent(transform);
            //float x = SandBoxData.instance.panoramicImages[i].latitude;
            //float z = SandBoxData.instance.panoramicImages[i].longitude;

            //Color heightColor = texHeight.GetPixel((int)x, (int)z);

            //Vector3 pos = Vector3.zero;
            //pos.x = x + diffWidth;
            //pos.y = heightColor.r * maxHeight;
            //pos.z = z + diffHeight;

            //go.transform.localPosition = pos;
        }
    }

    private GameObject CreateInteractiveObject()
    {
        GameObject pin = Instantiate(Resources.Load("InteractiveObject", typeof(GameObject)) as GameObject, transform);
        pin.transform.localScale = Vector3.one * 100;

        pin.GetComponent<InteractiveObjectManager>().Initialize();

        return pin;
    }
}