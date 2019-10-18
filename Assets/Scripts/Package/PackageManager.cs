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

        if(routePositions.Count <= 0)
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
        double horizontalSize = limits.east - limits.west;
        double verticalSize = limits.north - limits.south;

        InteractiveObject io;

        if (gdc.Model3D != null)
        {
            io = gdc.Model3D.GoModel.AddComponent<InteractiveObject>();
        }
        else if (gdc.Panoramic != null)
        {
            io = gdc.Panoramic.GoPanoramic.AddComponent<InteractiveObject>();
        }
        else
        {
            //TODO: HERE CREATE GENERIC OBJECT TO MANIPULATE GDC WITHOUT MODEL AND PANORAMIC
            io = null;
        }

        double auxLong = gdc.Longitude - limits.west;
        double auxLat = gdc.Latitude - limits.south;

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
    }

    private void CreateGDCs()
    {
        foreach (SO_PackageData.gdc_data item in SO_PackageData.instance.gdcs)
        {
            GDC gdc = new GDC(item.name, item.description, item.latitude, item.longitude);

            foreach (SO_PackageData.gdc_element element in item.elements)
            {
                //Convert string to enum
                Enum.TryParse(element.type, out ElementType type);

                switch (type)
                {
                    case ElementType.Sample:
                        //Helper.UnzipFile(rootPath + element.relativePath);
                        break;
                    case ElementType.Panoramic:
                        //gdc.Panoramic = new Panoramic(item.panoramic.name, rootPath + item.panoramic.path);
                        break;
                    case ElementType.File:
                        break;
                    default:
                        break;
                }
            }

            //Check if exists 3d model on gdc item
            //if (!string.IsNullOrEmpty(item.model3D.texturePath) &&
            //    !string.IsNullOrEmpty(item.model3D.modelPath))
            //{
            //    gdc.Model3D = new Model3D(item.model3D.name, rootPath + item.model3D.texturePath, rootPath + item.model3D.modelPath);
            //}

            ////Check if exists panoramic on gdc item
            //if (!string.IsNullOrEmpty(item.panoramic.path))
            //{
            //    gdc.Panoramic = new Panoramic(item.panoramic.name, rootPath + item.panoramic.path);
            //}

            //AddInteractiveObjectToGDC(ref gdc);
            //loadedGDCs.Add(gdc);
        }
    }


    private void LoadFromOpenTopography(Helper.WGS84 coordinates)
    {
        loadingFromOpenTopography = true;
        loadingFinished = false;
        OpenTopographyAPI.OpenTopographyElement element = new OpenTopographyAPI.OpenTopographyElement(coordinates);

        StartCoroutine(OpenTopographyAPI.DownloadGeoTiff(element, "C:/", "myFile", DownloadedGeoTiffCallback));
    }

    private void LoadPackagePins()
    {
        //TODO: Create a PIN to every Package. Add pins on world Map
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
        rootPath = SO_PackageData.SelectPackage(path);
        SO_PackageData.Init();
        
        //Texture2D tex = Helper.LoadImageAsTexture(rootPath + SO_PackageData.instance.heightMapPath);

        //Load heightmap texture by package
        Helper.WGS84 coordinates;
        //Read Geotiff image downloaded, set the coordinates and the heightTexture
        Texture2D tex;
        Helper.LoadGeotiffData(SO_PackageData.instance.geoTiffPath, out coordinates, out tex);
        terrainManager.LoadTerrain(tex);

        //TODO: AQUI PRECISO VER COMO PEGAR A COORDENADA DO HEIGHTMAP (ACHO QUE ADD NO PACKAGE ISSO É O MAIS FACIL)
        limits = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);

        CreateGDCs();
        CreateLineRendererRoute();
    }

    [Button]
    public void LoadPackage()
    {
        rootPath = SO_PackageData.SelectPackageFromFileBrowser();
        SO_PackageData.Init();

        //Texture2D tex = Helper.LoadImageAsTexture(rootPath + SO_PackageData.instance.heightMapPath);

        //Load heightmap texture by package
        Helper.WGS84 coordinates;
        //Read Geotiff image downloaded, set the coordinates and the heightTexture
        Texture2D tex;
        Helper.LoadGeotiffData(rootPath + SO_PackageData.instance.geoTiffPath, out coordinates, out tex);
        terrainManager.LoadTerrain(tex);

        //TODO: AQUI PRECISO VER COMO PEGAR A COORDENADA DO HEIGHTMAP (ACHO QUE ADD NO PACKAGE ISSO É O MAIS FACIL)
        limits = new Helper.WGS84(-119.65227127075197, 37.69903420794415, -119.52283859252931, 37.77804178967591);

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
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
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
