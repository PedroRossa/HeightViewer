using UnityEngine;
using static SO_PackageData;

public class Package : MonoBehaviour
{
    public string name;
    public string description;
    public float latitude;
    public float longitude;
    public string geoTiffPath;
    public string kmlPath;
    public string boundries;
    public gdc_data[] gdcs;

    public string fullPath;

    public void Initialize(SO_PackageData data)
    {
        name = data.name;
        description = data.description;
        latitude = data.latitude;
        longitude = data.longitude;
        geoTiffPath = data.geoTiffPath;
        kmlPath = data.kmlPath;
        boundries = data.boundries;
        gdcs = data.gdcs;
    }

    public void SetFullPath(string path)
    {
        fullPath = path;
    }
}
