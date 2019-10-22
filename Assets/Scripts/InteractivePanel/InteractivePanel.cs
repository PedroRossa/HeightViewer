using Leap.Unity.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vizlab;

public class InteractivePanel : MonoBehaviour
{
    public GDCRoot gdcRoot;

    public InteractiveCarrousel2D modelsCorrousel;
    public InteractiveCarrousel2D panoramicsCorrousel;
    public InteractiveCarrousel2D filesCorrousel;

    private List<GDCElement> models = new List<GDCElement>();
    private List<GDCElement> panoramics = new List<GDCElement>();
    private List<GDCElement> files = new List<GDCElement>();

    private void LoadElementsFromGDC()
    {
        Clear();

        foreach (var item in gdcRoot.gdc.Elements)
        {
            GameObject go = null;
            switch (item.Type)
            {
                case ElementType.Sample:
                    go = ((GDCElementSample)item).GoModel;
                    if (go != null)
                    {
                        Texture2D tex = Helper.LoadImageAsTexture(((GDCElementSample)item).TexturePath);
                        Sprite spt = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                        modelsCorrousel.AddElement(go.name, spt);
                        models.Add(item);
                    }
                    break;
                case ElementType.Panoramic:
                    go = ((GDCElementPanoramic)item).GoPanoramic;
                    if (go != null)
                    {
                        Texture2D tex = Helper.LoadImageAsTexture(((GDCElementPanoramic)item).PanoramicPath);
                        Sprite spt = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

                        panoramicsCorrousel.AddElement(go.name, spt);
                        panoramics.Add(item);
                    }
                    break;
                case ElementType.File:
                    go = ((GDCElementFile)item).GoFile;
                    if (go != null)
                    {
                        filesCorrousel.AddElement(go);
                        files.Add(item);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void Initialize(GDCRoot gdcRoot)
    {
        this.gdcRoot = gdcRoot;
        LoadElementsFromGDC();
    }

   public void Clear()
    {
        models = new List<GDCElement>();
        panoramics = new List<GDCElement>();
        files = new List<GDCElement>();

        modelsCorrousel.ClearCarrousel();
        panoramicsCorrousel.ClearCarrousel();
        filesCorrousel.ClearCarrousel();
    }
  
}
