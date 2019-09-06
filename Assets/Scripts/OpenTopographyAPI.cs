using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class OpenTopographyAPI
{
    /// <summary>
    /// Define the dataset to get raster images
    /// SRTMGL3 = global raster dataset - SRTM GL3 (90m)
    /// SRTMGL1 = SRTM GL1 (30m)
    /// SRTMGL1_E = SRTM GL1 (Ellipsoidal)
    /// AW3D30 = ALOS World 3D 30m
    /// AW3D30_E = ALOS World 3D (Ellipsoidal)
    /// </summary>
    public enum RasterDataset
    {
        SRTMGL3,
        SRTMGL1,
        SRTMGL1_E,
        AW3D30,
        AW3D30_E
    }

    /// <summary>
    /// Define the image output format
    ///  GTiff = GeoTiff
    ///  AAIGrid = Arc ASCII Grid
    ///  HFA = Erdas Imagine (.IMG)
    /// </summary>
    public enum OutputFormat
    {
        GTiff,
        AAIGrid,
        HFA
    }
    
    public struct OpenTopographyElement
    {
        public Helper.WGS84 coordinates;
        public RasterDataset dataset;
        public OutputFormat outputFormat;

        public OpenTopographyElement(Helper.WGS84 coordinates, RasterDataset dataset = RasterDataset.SRTMGL1, OutputFormat outputFormat = OutputFormat.GTiff)
        {
            this.coordinates = coordinates;
            this.dataset = dataset;
            this.outputFormat = outputFormat;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="responseCallback">Return the image path and the status code.</param>
    /// <returns></returns>
    public static IEnumerator DownloadGeoTiff(OpenTopographyElement element, string filePath, string fileName, Action<string, int> responseCallback)
    {
        string proxyUri = "https://test-proxy-31415.appspot.com/?";

        string parameters =
            "url=http://opentopo.sdsc.edu/otr/getdem?" +
            "demtype=" + element.dataset.ToString() +
            "&west=" + element.coordinates.west +
            "&south=" + element.coordinates.south +
            "&east=" + element.coordinates.east +
            "&north=" + element.coordinates.north +
            "&outputFormat=" + element.coordinates.ToString();

        using (UnityWebRequest www = UnityWebRequest.Get(proxyUri + parameters))
        {
            www.chunkedTransfer = true;
            www.timeout = 25;
            yield return www.SendWebRequest();

            string finalFilePath;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                finalFilePath = string.Empty;
                //TODO: Deal to show an error message here
            }
            else
            {
                DownloadHandler dh = www.downloadHandler;

                finalFilePath = filePath + fileName + ".tif";

                //save file on system
                System.IO.File.WriteAllBytes(finalFilePath, dh.data);
            }

            responseCallback?.Invoke(finalFilePath, (int)www.responseCode);
        }
    }
}
