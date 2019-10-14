using System.Collections.Generic;

namespace Vizlab
{
    //GDC Geo-referenced Data Container
    public class GDC
    {
        #region Attributes

        protected int id;
        protected string name;
        protected float latitude;
        protected float longitude;
        protected Model3D model3D;
        protected Panoramic panoramic;
        protected List<File> files;

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public float Longitude { get => longitude; set => longitude = value; }
        public Model3D Model3D { get => model3D; set => model3D = value; }
        public Panoramic Panoramic { get => panoramic; set => panoramic = value; }
        public List<File> Files { get => files; set => files = value; }

        #endregion

        #region Constructors

        public GDC()
        {
            id = -1;
            name = string.Empty;
            latitude = -1;
            longitude = -1;
            model3D = null;
            panoramic = null;
            files = new List<File>();
        }

        public GDC(int id, string name, float latitude, float longitude, Model3D model3D = null, Panoramic panoramic = null, List<File> files = null)
        {
            this.id = id;
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
            this.model3D = model3D;
            this.panoramic = panoramic;
            this.files = files;
        }

        #endregion
    }
}