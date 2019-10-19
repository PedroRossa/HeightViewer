using System.IO;
using UnityEngine;

namespace Vizlab
{
    class GDCElementPanoramic : GDCElement
    {
        #region Attributes

        protected string panoramicPath;
        protected GameObject goPanoramic;

        public string PanoramicPath { get => panoramicPath; set => panoramicPath = value; }
        public GameObject GoPanoramic { get => goPanoramic; set => goPanoramic = value; }

        #endregion

        #region Constructors

        public GDCElementPanoramic()
        {
            name = string.Empty;
            description = string.Empty;
            latitude = -1;
            longitude = -1;
            panoramicPath = string.Empty;
            type = ElementType.Panoramic;
            relativePath = string.Empty;
            goPanoramic = null;
        }

        public GDCElementPanoramic(SO_PackageData.gdc_element element, string rootPath)
        {
            name = element.name;
            description = element.description;
            latitude = element.latitude;
            longitude = element.longitude;
            type = ElementType.Panoramic;
            relativePath = element.relativePath;
            goPanoramic = null;

            LoadData(rootPath);
        }

        #endregion

        public override void LoadData(string rootPath)
        {
            panoramicPath = rootPath + relativePath;
            string extension = Path.GetExtension(panoramicPath);

            switch (extension)
            {
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".tif":
                case ".tiff":
                    goPanoramic = Helper.LoadPanoramicImage(name, panoramicPath);
                    goPanoramic.SetActive(false);
                    break;
                default:
                    Debug.Log("Unknown extension detected on panoramic element object. Extension: " + extension);
                    break;
            }
        }
    }
}
