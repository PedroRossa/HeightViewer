using OSGeo.GDAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

using Atlas.IO;
using SkiaSharp;
using System.IO;

public class Helper : MonoBehaviour
{
    public static void Resize(ref Texture2D source, int newWidth, int newHeight, FilterMode filterMode = FilterMode.Point)
    {
        float ratio = Mathf.Min((float)newWidth / source.width, (float)newHeight / source.height);

        int nW = (int)(source.width * ratio);
        int nH = (int)(source.height * ratio);

        source.filterMode = filterMode;
        RenderTexture rt = RenderTexture.GetTemporary(nW, nH);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        source = new Texture2D(nW, nH);
        source.ReadPixels(new Rect(0, 0, nW, nH), 0, 0);
        source.Apply();
        RenderTexture.active = null;
    }

    public static void Resize(ref Texture2D source, int maxWidth, FilterMode filterMode = FilterMode.Point)
    {
        float factor = source.width > source.height ? ((float)maxWidth / source.width) : ((float)maxWidth / source.height);
        int newWidth = (int)(source.width * factor);
        int newHeight = (int)(source.height * factor);

        source.filterMode = filterMode;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        source = new Texture2D(newWidth, newHeight);
        source.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
        source.Apply();
        RenderTexture.active = null;
    }

    #region Coroutines
    
    public static IEnumerator AnimateMeshCoroutine(MeshRenderer meshRenderer, AnimationCurve animationCurve = null, float initScale = 0.001f, float finalScale = 1, float duration = 1)
    {
        if (meshRenderer == null)
        {
            yield return null;
        }
        if (animationCurve == null)
        {
            animationCurve = new AnimationCurve();
        }

        float timer = 0;
        float percent = 0;

        meshRenderer.transform.localScale = new Vector3(1, initScale, 1);
        yield return new WaitForSeconds(1);
        while (timer < duration)
        {
            timer += Time.deltaTime;

            percent = animationCurve.Evaluate(timer / duration);
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

    #endregion

    #region GDAL_Helper

    public struct WGS84
    {
        public double west, south, east, north;

        public WGS84(double west, double south, double east, double north)
        {
            this.west = west;
            this.south = south;
            this.east = east;
            this.north = north;
        }
    }

    private static Texture2D GrayScaleTexture2DFromGDALBand(Band band)
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
    
    private static void LoadGeotiffImage(string path, out Band band, out WGS84 coordinates)
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

        coordinates.west = originX;
        coordinates.south = finalY;
        coordinates.east = finalX;
        coordinates.north = originY;

        //Read 1st band from raster  
        band = rasterDataset.GetRasterBand(1);
        int rastWidth = rasterCols;
        int rastHeight = rasterRows;
    }

    public static void LoadGeotiffData(string path, out WGS84 coordinates, out Texture2D heightTexture)
    {
        Band band;
        LoadGeotiffImage(path, out band, out coordinates);
        heightTexture = GrayScaleTexture2DFromGDALBand(band);
    }

    #endregion

    #region KML_Helper

    public static List<Placemark> ReadKMLFile(string kmlPath)
    {
        List<Placemark> placemarks = new List<Placemark>();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(kmlPath);
        XmlNodeList placesmarks = xmlDoc.GetElementsByTagName("Placemark");

        foreach (XmlNode item in placesmarks)
        {
            Placemark pl = new Placemark();
            SetPlacemarkData(item, out pl);

            placemarks.Add(pl);
            //Debug.Log(pl.type + " | " + pl.name + " lat:" + pl.latitude + " long:" + pl.longitude);
        }
        return placemarks;
    }

    private static void SetPlacemarkData(XmlNode xmlElement, out Placemark placemark)
    {
        placemark = new Placemark();

        foreach (XmlNode item in xmlElement.ChildNodes)
        {
            if (item.Name.Equals("name"))
            {
                placemark.Name = item.InnerText;
                placemark.Type = Placemark.TypeByName(placemark.Name);
            }
            //pins
            if (item.Name.Equals("Point"))
            {
                foreach (XmlNode child in item)
                {
                    if (child.Name.Equals("coordinates"))
                    {
                        string val = child.InnerText;
                        placemark.Longitude = Convert.ToDouble(val.Split(',')[0]);
                        placemark.Latitude = Convert.ToDouble(val.Split(',')[1]);
                    }
                }
            }
            //routes
            if (item.Name.Equals("LineString"))
            {
                placemark.RouteValues = new List<Vector2>();
                string val = item.FirstChild.InnerText;
                //Remove last ',' and spaces
                val = val.Remove(item.FirstChild.InnerText.LastIndexOf(',')).Trim();
                //Remove altitude values from array (,0 )
                val = val.Replace("0 ", "");
                //Split values by ','
                string[] routeVals = val.Split(',');

                for (int i = 0; i < routeVals.Length; i += 2)
                {
                    placemark.RouteValues.Add(
                        new Vector2(
                            Convert.ToSingle(routeVals[i]),
                            Convert.ToSingle(routeVals[i + 1])
                    ));
                }
            }
        }
    }

    #endregion

    #region Loaders

    private static void LoadDXM(string path, out string error, out DXMModel model)
    {
        error = string.Empty;
        float progress = 0;
        string[] textPaths;
        model = new DXMModel();

        DXMImporter.Load(path, ref model, ref error, ref progress, out textPaths);
    }

    private static void ConvertSKBitmapToTexture2D(SKBitmap bitmap, out Texture2D texture)
    {
        texture = new Texture2D(bitmap.Width, bitmap.Height);
        byte[] bytes = bitmap.Bytes;
        int length = bytes.Length;
        Color32[] pixels = new Color32[length / 4];
        int count = 0;

        //TODO: See how to save properly the texture, actually the texture it's upside down
        for (int i = 0; i < length; i += 4)
        {
            pixels[count] = new Color32(bytes[i + 2], bytes[i + 1], bytes[i], bytes[i + 3]);
            count++;
        }

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private static void PopulateMeshWithDXMModel(DXMModel model, out Mesh mesh)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = model.vertexUnity;
        mesh.normals = model.normalUnity;
        mesh.uv = model.uvUnity;
        mesh.subMeshCount = model.groups.Length;

        for (int i = 0; i < model.groups.Length; i++)
        {
            //Flips the indices of the model to account for the flipping of the z axis.
            for (int j = 0; j < model.groups[i].index32.Length / 3; ++j)
            {
                int temp = model.groups[i].index32[j * 3 + 0];
                model.groups[i].index32[j * 3 + 0] = model.groups[i].index32[j * 3 + 1];
                model.groups[i].index32[j * 3 + 1] = temp;
            }
            mesh.SetIndices(model.groups[i].index32, MeshTopology.Triangles, i);
        }

        mesh.UploadMeshData(true);
    }


    public static Texture2D LoadImageAsTexture(string path)
    {
        SKBitmap bitmap;
        try
        {
            FileStream imgStream = File.OpenRead(path);
            SKManagedStream skManagedStream = new SKManagedStream(imgStream);
            bitmap = SKBitmap.Decode(skManagedStream);

            //Guarantee BGRA8888 to the loaded image
            if (bitmap.ColorType != SKColorType.Bgra8888)
            {
                SKBitmap tempBitmap = new SKBitmap(bitmap.Width, bitmap.Height, false);
                bitmap.CopyTo(tempBitmap, SKColorType.Bgra8888);
                bitmap.Dispose();
                bitmap = tempBitmap;
            }
            skManagedStream.Dispose();
            imgStream.Close();
            imgStream.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Could not load file: {0} with error: {1}", Path.GetFileNameWithoutExtension(path), ex.Message);
            return null;
        }

        if (bitmap == null) //It appears that Skia lib sometimes don't read the image but don't throw a exception.
        {
            Debug.LogErrorFormat("Could not load file: {0} the image cannot be opened.", Path.GetFileNameWithoutExtension(path));
            return null;
        }

        Texture2D texture;
        ConvertSKBitmapToTexture2D(bitmap, out texture);
        texture.name = Path.GetFileNameWithoutExtension(path);

        return texture;
    }

    public static GameObject Load3DModel(string name, string modelPath, string texturePath)
    {
        GameObject go = new GameObject(name);
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        Mesh mesh = meshFilter.mesh;
        meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));

        DXMModel model;
        string error = string.Empty;
        LoadDXM(modelPath, out error, out model);

        if (string.IsNullOrEmpty(error))
        {
            PopulateMeshWithDXMModel(model, out mesh);
            meshFilter.mesh = mesh;

            //Load and apply texture
            Texture2D texture = LoadImageAsTexture(texturePath);
            meshRenderer.material.mainTexture = texture;

            return go;
        }
        else
        {
            Debug.Log("Error on load the model - Error: " + error);
            return null;
        }
    }

    public static GameObject LoadPanormicImage(string name, string path)
    {
        GameObject go = Instantiate(Resources.Load("InvertedSphere") as GameObject);
        go.name = name;

        Material mat = new Material(Shader.Find("Unlit/Texture"));

        mat.mainTexture = LoadImageAsTexture(path);
        mat.SetTextureScale("_MainTex", new Vector2(-1, 1));

        go.GetComponent<MeshRenderer>().material = mat;

        return go;
    }

    #endregion
}




