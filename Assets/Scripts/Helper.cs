using OSGeo.GDAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class Helper
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
}




