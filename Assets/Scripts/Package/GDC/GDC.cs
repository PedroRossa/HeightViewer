using System.Collections.Generic;
using UnityEngine;

namespace Vizlab
{
    public enum RepresentativeModel
    {
        SAMPLE,
        PANORAMIC,
        FILE
    }
    //GDC Geo-referenced Data Container
    public class GDC
    {
        #region Attributes
        
        protected string name;
        protected string description;
        protected float latitude;
        protected float longitude;
        protected List<GDCElement> elements;
        protected GameObject goModel;

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public float Longitude { get => longitude; set => longitude = value; }
        public List<GDCElement> Elements { get => elements; set => elements = value; }
        public GameObject GoModel { get => goModel; set => goModel = value; }

        #endregion

        #region Constructors

        public GDC()
        {
            name = string.Empty;
            description = string.Empty;
            latitude = -1;
            longitude = -1;
            elements = new List<GDCElement>();
            goModel = null;
        }

        public GDC(string name, string description, float latitude, float longitude)
        {
            this.name = name;
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            elements = new List<GDCElement>();
            goModel = null;
        }

        #endregion

        public void SetInteractiveModel(RepresentativeModel modelType)
        {
            switch (modelType)
            {
                case RepresentativeModel.SAMPLE:
                    goModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case RepresentativeModel.PANORAMIC:
                    goModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case RepresentativeModel.FILE:
                    goModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                default:
                    break;
            }
        }
    }
}