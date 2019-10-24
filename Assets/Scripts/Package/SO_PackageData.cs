using System;
using System.IO;
using SFB;

using UnityEngine;

[CreateAssetMenu(fileName = "SO_PackageData", menuName = "SOs/PackageData/Create")]
public class SO_PackageData : ScriptableObject
{    
    [Serializable]
    public struct gdc_element
    {
        public string name;
        public string description;
        public float latitude;
        public float longitude;
        public string type;
        public string relativePath;
    }

    [Serializable]
    public struct gdc_data
    {
        public string name;
        public string description;
        public float latitude;
        public float longitude;
        public gdc_element[] elements;
    }


    public static SO_PackageData instance;

    #region Properties

    public string name;
    public string description;
    public float latitude;
    public float longitude;
    public string geoTiffPath;
    public string kmlPath;
    public string boundries;
    public gdc_data[] gdcs;
    
    #endregion

    public static string fullPackagePath;
    public static string rootPath;

    #region Save and Load by Json

    public void Save()
    {
        PlayerPrefs.SetString("SAVED_DATA", Encode());
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey("SAVED_DATA"))
        {
            PlayerPrefs.SetString("SAVED_DATA", Encode());
        }

        Decode(PlayerPrefs.GetString("SAVED_DATA"));
    }

    private string Encode()
    {
        return JsonUtility.ToJson(this);
    }

    private SO_PackageData Decode(string str)
    {
        JsonUtility.FromJsonOverwrite(str, this);
        return this;
    }

    #endregion

    public static void Init()
    {
        instance = Resources.Load("SO_PackageData", typeof(SO_PackageData)) as SO_PackageData;
        instance.Load();
    }

    public static string SelectPackageFromFileBrowser()
    {
        fullPackagePath = StandaloneFileBrowser.OpenFilePanel("Select a JSON", "", "json", false)[0];
        PlayerPrefs.SetString("SAVED_DATA", System.IO.File.ReadAllText(fullPackagePath));

        string fileName = Path.GetFileName(fullPackagePath);
        rootPath = fullPackagePath.Replace(fileName, "");

        return rootPath;
    }

    public static string SelectPackage(string path)
    {
        fullPackagePath = path;
        PlayerPrefs.SetString("SAVED_DATA", File.ReadAllText(fullPackagePath));

        string fileName = Path.GetFileName(fullPackagePath);
        rootPath = fullPackagePath.Replace(fileName, "");

        return rootPath;
    }
}
