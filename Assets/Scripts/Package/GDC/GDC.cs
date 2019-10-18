﻿using System.Collections.Generic;

namespace Vizlab
{
    //GDC Geo-referenced Data Container
    public class GDC
    {
        #region Attributes
        
        protected string name;
        protected string description;
        protected float latitude;
        protected float longitude;

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public float Longitude { get => longitude; set => longitude = value; }

        #endregion

        #region Constructors

        public GDC()
        {
            name = string.Empty;
            description = string.Empty;
            latitude = -1;
            longitude = -1;
        }

        public GDC(string name, string description, float latitude, float longitude)
        {
            this.name = name;
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        #endregion
    }
}