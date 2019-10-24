using System.Collections.Generic;
using UnityEngine;

namespace Vizlab
{
    //GDC Geo-referenced Data Container
    public class GDC : MonoBehaviour
    {
        #region Attributes

        protected string name;
        protected string description;
        protected float latitude;
        protected float longitude;
        protected List<GDCElement> elements;
        protected GameObject goModel;
        protected Transform elementsContentTransform;

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public float Longitude { get => longitude; set => longitude = value; }
        public List<GDCElement> Elements { get => elements; set => elements = value; }
        public GameObject GoModel { get => goModel; set => goModel = value; }
        public Transform ElementsContentTransform { get => elementsContentTransform; set => elementsContentTransform = value; }

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
            goModel = Instantiate(Resources.Load("GDCRoot", typeof(GameObject)) as GameObject);

            //Get second element of prefab to save future loaded elements
            elementsContentTransform = goModel.transform.GetChild(1);
        }

        #endregion
        
        public void ConfigureGDCRootObject()
        {
            GDCRoot gdcRoot = goModel.GetComponent<GDCRoot>();
            gdcRoot.InitializeData(this);
        }
    }
}