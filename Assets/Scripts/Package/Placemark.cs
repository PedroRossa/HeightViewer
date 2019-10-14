using System.Collections.Generic;
using UnityEngine;

public class Placemark
{
    public enum PlacemarkType
    {
        SAMPLE,
        PANORAMIC,
        ROUTE,
        OTHER
    }

    int id;
    string name;
    double latitude;
    double longitude;
    PlacemarkType type;
    List<Vector2> routeValues;

    public int Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public double Latitude { get => latitude; set => latitude = value; }
    public double Longitude { get => longitude; set => longitude = value; }
    public PlacemarkType Type { get => type; set => type = value; }
    public List<Vector2> RouteValues { get => routeValues; set => routeValues = value; }

    public Placemark()
    {
        Id = -1;
        Name = "";
        Latitude = -1;
        Longitude = -1;
        Type = PlacemarkType.OTHER;
        RouteValues =  new List<Vector2>();
    }

    public Placemark(int id, string name, double latitude = 0, double longitude = 0, PlacemarkType type = PlacemarkType.OTHER, List<Vector2> routeValues = null)
    {
        this.Id = id;
        this.Name = name;
        this.Latitude = latitude;
        this.Longitude = longitude;
        this.Type = type;

        this.RouteValues = routeValues != null ? routeValues : new List<Vector2>();
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
