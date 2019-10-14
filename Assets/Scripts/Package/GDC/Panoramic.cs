using UnityEngine;

namespace Vizlab
{
    public class Panoramic
    {
        #region Attributes

        protected string name;
        protected string panoramicPath;
        protected GameObject goPanoramic;

        public string Name { get => name; set => name = value; }
        public string PanoramicPath { get => panoramicPath; set => panoramicPath = value; }
        public GameObject GoPanoramic { get => goPanoramic; set => goPanoramic = value; }

        #endregion

        #region Constructors

        public Panoramic()
        {
            name = string.Empty;
            panoramicPath = string.Empty;
            goPanoramic = null;
        }

        public Panoramic(string name, string panoramicPath = "")
        {
            this.name = name;
            this.panoramicPath = panoramicPath;
            goPanoramic = null;

            if (!string.IsNullOrEmpty(panoramicPath))
            {
                LoadPanoramic();
            }
        }

        #endregion

        public void LoadPanoramic()
        {
            goPanoramic = Helper.LoadPanormicImage(name, panoramicPath);
        }
    }
}