using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Vizlab;

public class PackageManager : MonoBehaviour
{
    public TerrainManager terrainManager;
    public Transform loadedElementsTransform;

    public bool loadingFromOpenTopography;
    public bool loadingFinished;

    public Helper.WGS84 limits;
    public List<Package> packages = new List<Package>();

    private string rootPath;

    //SCENE DATA
    private List<GDC> loadedGDCs = new List<GDC>();
    private GameObject route;


    private void CreateLineRendererRoute()
    {
        if (string.IsNullOrEmpty(SO_PackageData.instance.kmlPath))
        {
            return;
        }

        List<Vector3> routePositions = Helper.RouteDataFromKML(rootPath + SO_PackageData.instance.kmlPath, terrainManager, limits);

        if (routePositions.Count <= 0)
        {
            return;
        }

        route = new GameObject("Route");
        route.transform.SetParent(terrainManager.transform);
        route.transform.localPosition = Vector3.zero;
        route.transform.localScale = Vector3.one;

        LineRenderer lr = route.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.015f;
        lr.startColor = new Color(0, 0, 1, 0.5f);
        lr.endColor = new Color(0, 0, 1, 0.5f);
        lr.useWorldSpace = false;
        lr.sortingOrder = 5;

        lr.positionCount = routePositions.Count;
        lr.SetPositions(routePositions.ToArray());

        lr.Simplify(5);
    }
    
    private void AddInteractiveObjectToGDC(ref GDC gdc)
    {
        Vector3 pos = terrainManager.GetPositionOnMapByLatLon(limits, gdc.Latitude, gdc.Longitude);

        InteractiveObject io = gdc.GoModel.AddComponent<InteractiveObject>();

        //Put element inside of map to positionate correctly
        io.SetParent(terrainManager.transform);
        io.SetPosition(pos, false);

        //then put element out to correct scale
        io.SetParent(loadedElementsTransform);
        io.SetModelScale(0.035f);

        io.RefreshLineRenderer();
    }

    private void CreateGDCs()
    {
        foreach (SO_PackageData.gdc_data item in SO_PackageData.instance.gdcs)
        {
            GDC gdc = new GDC(item.name, item.description, item.latitude, item.longitude);

            foreach (SO_PackageData.gdc_element currElement in item.elements)
            {
                GDCElement newElement;
                //Convert string to enum
                Enum.TryParse(currElement.type, out ElementType type);

                switch (type)
                {
                    case ElementType.Sample:
                        //Helper.UnzipFile(rootPath + currElement.relativePath);
                        newElement = new GDCElementSample(currElement, rootPath, gdc.ElementsContentTransform);
                        break;
                    case ElementType.Panoramic:
                        newElement = new GDCElementPanoramic(currElement, rootPath, gdc.ElementsContentTransform);
                         break;
                    case ElementType.File:
                        newElement = new GDCElementFile(currElement, rootPath, gdc.ElementsContentTransform);
                        //((GDCElementFile)newElement).GoFile.transform.SetParent(gdc.ElementsContentTransform);
                        break;
                    default:
                        Debug.Log("Unknown element type detected. Type: " + type);
                        continue;
                }
                gdc.Elements.Add(newElement);
            }
            gdc.ConfigureGDCRootObject();

            AddInteractiveObjectToGDC(ref gdc);
            loadedGDCs.Add(gdc);
        }
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


    //Function to read folder with packages and create content inside of project
    [Button]
    public void LoadPackages()
    {
        //Load world heightmap texture 
        Texture2D tex = Resources.Load("worldHeightMap", typeof(Texture2D)) as Texture2D;
        terrainManager.LoadTerrain(tex);

        string root = "";
#if UNITY_EDITOR
        root = "C:\\MOSIS_LAB\\Packages";
#else
        root = Application.dataPath + "\\" + Packages;
#endif

        //Check if packages directory exists, if not, create!
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }

        string[] packagesPaths = Directory.GetDirectories(root);

        foreach (var item in packagesPaths)
        {
            //If the folder don't contains a package.json, so it isn't a valid package
            if (!File.Exists(item + "\\gdcPackage.json"))
            {
                continue;
            }

            //TODO: VER AQUI PRA ADAPTAR O SO_PACKAGEDATA PARA UMA LISTA DE PACKAGES

            //Get Json properties using SO
            rootPath = SO_PackageData.SelectPackage(item + "\\gdcPackage.json");
            SO_PackageData.Init();
            
            GameObject go = Instantiate(Resources.Load("PackagePin", typeof(GameObject)) as GameObject);

            Package package = go.AddComponent<Package>();
            
            package.Initialize(SO_PackageData.instance);
            package.SetFullPath(item + "\\gdcPackage.json");

            //Get PackagePin script to Load data to Prefab
            PackagePin packagePin = go.GetComponent<PackagePin>();
            packagePin.LoadPanel(this, package);


            go.name = "PIN_" + package.name;
            
            Helper.WGS84 worldLimits = new Helper.WGS84(-180, -90, 180, 90);
            float lat = package.latitude;
            float lon = package.longitude;
            Vector3 pos = terrainManager.GetPositionOnMapByLatLon(worldLimits, lat, lon);

            //Put element inside of map to positionate correctly
            go.transform.SetParent(terrainManager.transform);
            go.transform.localPosition = pos;

            //then put element out to correct scale
            go.transform.SetParent(loadedElementsTransform);

            packages.Add(package);
        }
    }


    public void LoadPackage(string path)
    {
        ClearScene();
        rootPath = SO_PackageData.SelectPackage(path);
        SO_PackageData.Init();

        //Texture2D tex = Helper.LoadImageAsTexture(rootPath + SO_PackageData.instance.heightMapPath);

        //Read Geotiff image downloaded, set the coordinates and the heightTexture
        Texture2D tex;
        Helper.LoadGeotiffData(rootPath + SO_PackageData.instance.geoTiffPath, out limits, out tex);
        terrainManager.LoadTerrain(tex);

        //limits are got on geotif reading

        CreateGDCs();
        CreateLineRendererRoute();
    }

    [Button]
    public void LoadPackage()
    {
        ClearScene();
        rootPath = SO_PackageData.SelectPackageFromFileBrowser();
        SO_PackageData.Init();

        //Texture2D tex = Helper.LoadImageAsTexture(rootPath + SO_PackageData.instance.heightMapPath);

        //Read Geotiff image downloaded, set the coordinates and the heightTexture
        Texture2D tex;
        Helper.LoadGeotiffData(rootPath + SO_PackageData.instance.geoTiffPath, out limits, out tex);
        terrainManager.LoadTerrain(tex);

        //limits are got on geotif reading

        CreateGDCs();
        CreateLineRendererRoute();
    }

    [Button]
    public void LoadMapFromOpenTopography()
    {
        SO_PackageData.Init();

        //TODO: AQUI EH A CHAMADA PRA QUANDO QUISER FAZER LOAD DE NOVA AREA A PARTIR DA INTERACAO COM O MAPA
        Helper.WGS84 coordinates = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);
        LoadFromOpenTopography(coordinates);
    }

    [Button]
    public void ClearScene()
    {
        terrainManager.ResetTerrain();

        //Clear lists
        loadedGDCs = new List<GDC>();
        Destroy(route);

        //Delete Pins
        for (int i = 0; i < loadedElementsTransform.childCount; i++)
        {
            Destroy(loadedElementsTransform.GetChild(i).gameObject);
        }
    }
}
