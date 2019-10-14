using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using Vizlab;

public class PackageManager : MonoBehaviour
{
    public TerrainManager terrainManager;
    public Transform loadedElementsTransform;

    public bool loadingFromOpenTopography;
    public bool loadingFinished;

    //SCENE DATA
    private List<GDC> loadedGDCs = new List<GDC>();
    
    private LineRenderer loadedRoute = new LineRenderer();
    private List<Placemark> loadedPlacemarks = new List<Placemark>();
   

    private void LoadFromOpenTopography(Helper.WGS84 coordinates)
    {
        loadingFromOpenTopography = true;
        loadingFinished = false;
        OpenTopographyAPI.OpenTopographyElement element = new OpenTopographyAPI.OpenTopographyElement(coordinates);

        StartCoroutine(OpenTopographyAPI.DownloadGeoTiff(element, "C:/", "myFile", DownloadedGeoTiffCallback));
    }

    //REVISAR
    private LineRenderer CreateRoute(Helper.WGS84 limits)
    {
        double horizontalSize = limits.east - limits.west;
        double verticalSize = limits.north - limits.south;

        foreach (Placemark item in loadedPlacemarks)
        {
            if (item.Type == Placemark.PlacemarkType.ROUTE)
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
                int width = terrainManager.width;
                int height = terrainManager.height;

                foreach (Vector2 value in item.RouteValues)
                {
                    double auxLong = value.x - limits.west;
                    double auxLat = value.y - limits.south;

                    double calculatedX = width * auxLong / horizontalSize;
                    double calculatedY = height * auxLat / verticalSize;

                    float auxHeight = terrainManager.terrain.GetTexture().GetPixel((int)calculatedX, (int)calculatedY).r;
                    //Add 10% of maxHeight to unblock from model
                    float finalHeight = auxHeight * terrainManager.maxHeight * 1.1f;

                    lr.SetPosition(count, new Vector3((float)calculatedX, finalHeight, (float)calculatedY));

                    count++;
                }
                lr.Simplify(5);
                return lr;
            }
        }
        return null;
    }
    
    private void CreateGDCs(Helper.WGS84 limits)
    {
        double horizontalSize = limits.east - limits.west;
        double verticalSize = limits.north - limits.south;

        foreach (SandBoxData.Sample item in SandBoxData.instance.samples)
        {
            string texPath = SandBoxData.rootPath + item.texturePath;
            string modelPath = SandBoxData.rootPath + item.modelPath;

            GDC gdc = new GDC(item.id, item.name, item.latitude, item.longitude);
            Model3D model = new Model3D(item.name, texPath, modelPath);
            gdc.Model3D = model;
            InteractiveObject io = gdc.Model3D.GoModel.AddComponent<InteractiveObject>();

            double auxLong = item.longitude - limits.west;
            double auxLat = item.latitude - limits.south;

            double calculatedX = terrainManager.width * auxLong / horizontalSize;
            double calculatedY = terrainManager.height * auxLat / verticalSize;
            float auxHeight = terrainManager.terrain.GetTexture().GetPixel((int)calculatedX, (int)calculatedY).r;

            Vector3 pos = new Vector3((float)calculatedX, auxHeight * terrainManager.maxHeight, (float)calculatedY);

            //Put element inside of map to positionate correctly
            io.SetParent(terrainManager.transform);
            io.SetPosition(pos, false);

            //then put element out to correct scale
            io.SetParent(loadedElementsTransform);
            io.SetModelScale(0.035f);

            //GDCSamples.Add(gdcSample);
        }
        //return GDCSamples;
    }

    private void CreateGDCSamples(Helper.WGS84 limits)
    {
        double horizontalSize = limits.east - limits.west;
        double verticalSize = limits.north - limits.south;

        foreach (SandBoxData.Sample item in SandBoxData.instance.samples)
        {
            string texPath = SandBoxData.rootPath + item.texturePath;
            string modelPath = SandBoxData.rootPath + item.modelPath;

            //GDCSample gdcSample = new GDCSample(-1, item.name, item.latitude, item.longitude, texPath, modelPath);
            //InteractiveObject io = gdcSample.GoModel.AddComponent<InteractiveObject>();

            double auxLong = item.longitude - limits.west;
            double auxLat = item.latitude - limits.south;

            double calculatedX = terrainManager.width * auxLong / horizontalSize;
            double calculatedY = terrainManager.height * auxLat / verticalSize;
            float auxHeight = terrainManager.terrain.GetTexture().GetPixel((int)calculatedX, (int)calculatedY).r;

            Vector3 pos = new Vector3((float)calculatedX, auxHeight * terrainManager.maxHeight, (float)calculatedY);

            //Put element inside of map to positionate correctly
            //io.SetParent(terrainManager.transform);
            //io.SetPosition(pos, false);

            //then put element out to correct scale
            //io.SetParent(loadedElementsTransform);
            //io.SetModelScale(0.035f);

            //GDCSamples.Add(gdcSample);
        }
    }

    private void CreateGDCPanoramics(Helper.WGS84 limits)
    {
        double horizontalSize = limits.east - limits.west;
        double verticalSize = limits.north - limits.south;

        foreach (SandBoxData.PanoramicImage item in SandBoxData.instance.panoramicImages)
        {
            string panoramicPath = SandBoxData.rootPath + item.path;
            //GDCPanoramic gdcPanoramic = new GDCPanoramic(item.id, item.name, item.latitude, item.longitude, panoramicPath);

            //InteractiveObject io = gdcPanoramic.GoPanoramic.AddComponent<InteractiveObject>();

            double auxLong = item.longitude - limits.west;
            double auxLat = item.latitude - limits.south;

            double calculatedX = terrainManager.width * auxLong / horizontalSize;
            double calculatedY = terrainManager.height * auxLat / verticalSize;
            float auxHeight = terrainManager.terrain.GetTexture().GetPixel((int)calculatedX, (int)calculatedY).r;

            Vector3 pos = new Vector3((float)calculatedX, auxHeight * terrainManager.maxHeight, (float)calculatedY);

            //Put element inside of map to positionate correctly
            //io.SetParent(terrainManager.transform);
            //io.SetPosition(pos, false);
            //then put element out to correct scale
            //io.SetParent(loadedElementsTransform);
            //io.SetModelScale(0.035f);

            //GDCPanoramics.Add(gdcPanoramic);
        }
    }


    private void LoadPackagePins()
    {

    }

    public void DownloadedGeoTiffCallback(string path, int responseCode)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Geotiff downloaded from OpenTopography. Path: " + path);

            Helper.WGS84 coordinates;

            //Read Geotiff image downloaded, set the coordinates and the heightTexture
            Texture2D tex;
            Helper.LoadGeotiffData(path, out coordinates, out tex);
            terrainManager.LoadTerrain(tex);
        }
        else
        {
            Debug.Log("Error on download geotiff image from OpenTopography. Response error: " + responseCode);
        }

        loadingFromOpenTopography = false;
        loadingFinished = true;
    }

    public void LoadPackage(string path)
    {
        string minPath = SandBoxData.SelectPackage(path);
        SandBoxData.Init();

        minPath = SO_PackageData.SelectPackage(path);
        SO_PackageData.Init();

        //Load heightmap texture by package
        Texture2D tex = Helper.LoadImageAsTexture(minPath + SandBoxData.instance.heightMapPath);

        terrainManager.LoadTerrain(tex);

        //Load kml from googleEarth, with all the informations
        loadedPlacemarks = Helper.ReadKMLFile(minPath + SandBoxData.instance.kmlPath);

        //TODO: AQUI PRECISO VER COMO PEGAR A COORDENADA DO HEIGHTMAP (ACHO QUE ADD NO PACKAGE ISSO É O MAIS FACIL)
        Helper.WGS84 coordinates = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);

        //loadedSamples = CreateGDCSamples(coordinates);
        //loadedPanoramics = CreateGDCPanoramics(coordinates);
        loadedRoute = CreateRoute(coordinates);

        //CreateSamplePins(sampleObjects);
        //GameObject[] panoramicObjects = SandBoxData.LoadPanoramicImages(minPath);
        //CreatePanoramicPins(panoramicObjects);
    }

    [Button]
    public void LoadPackage()
    {
        string path = SandBoxData.SelectPackageFromFileBrowser();
        SandBoxData.Init();

        path = SO_PackageData.SelectPackageFromFileBrowser();
        SO_PackageData.Init();

        //Load heightmap texture by package
        Texture2D tex = Helper.LoadImageAsTexture(path + SandBoxData.instance.heightMapPath);

        terrainManager.LoadTerrain(tex);

        //Load kml from googleEarth, with all the informations
        loadedPlacemarks = Helper.ReadKMLFile(path + SandBoxData.instance.kmlPath);

        //TODO: AQUI PRECISO VER COMO PEGAR A COORDENADA DO HEIGHTMAP (ACHO QUE ADD NO PACKAGE ISSO É O MAIS FACIL)
        Helper.WGS84 coordinates = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);

        //loadedSamples = CreateGDCSamples(coordinates);
        //loadedPanoramics = CreateGDCPanoramics(coordinates);
        loadedRoute = CreateRoute(coordinates);
    }

    [Button]
    public void LoadMapFromOpenTopography()
    {
        SandBoxData.Init();

        //TODO: AQUI EH A CHAMADA PRA QUANDO QUISER FAZER LOAD DE NOVA AREA A PARTIR DA INTERACAO COM O MAPA
        Helper.WGS84 coordinates = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);
        LoadFromOpenTopography(coordinates);
    }

    [Button]
    public void ClearScene()
    {
        terrainManager.ResetTerrain();

        //Clear lists
       // loadedSamples = new List<GDCSample>();
       // loadedPanoramics = new List<GDCPanoramic>();
        loadedPlacemarks = new List<Placemark>();

        //Delete Pins
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        //Delete LineRenderer Route
        if (gameObject.GetComponent<LineRenderer>() != null)
        {
            Destroy(gameObject.GetComponent<LineRenderer>());
        }
    }

    [Button]
    public void LoadWorldMapTerrain()
    {
        //Load world heightmap texture 
        Texture2D tex = Resources.Load("worldHeightMap", typeof(Texture2D)) as Texture2D;
        terrainManager.LoadTerrain(tex);
    }
}
