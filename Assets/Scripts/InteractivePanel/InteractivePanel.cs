using Leap.Unity.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vizlab;

public class InteractivePanel : MonoBehaviour
{
    public GDCRoot gdcRoot;

    public InteractiveCarrousel2D modelsCarrousel;
    public InteractiveCarrousel2D panoramicsCarrousel;
    public InteractiveCarrousel2D filesCarrousel;

    public Transform loadTableTransform;

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
                        //Texture2D tex = Helper.LoadImageAsTexture(((GDCElementSample)item).TexturePath);
                        //Sprite spt = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

                        modelsCarrousel.AddElement(go.name);
                        models.Add(item);
                    }
                    break;
                case ElementType.Panoramic:
                    go = ((GDCElementPanoramic)item).GoPanoramic;
                    if (go != null)
                    {
                        //Texture2D tex = Helper.LoadImageAsTexture(((GDCElementPanoramic)item).PanoramicPath);
                        //Sprite spt = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

                        panoramicsCarrousel.AddElement(go.name);
                        panoramics.Add(item);
                    }
                    break;
                case ElementType.File:
                    go = ((GDCElementFile)item).GoFile;
                    if (go != null)
                    {
                        filesCarrousel.AddElement(go);
                        files.Add(item);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void MoveObjectToLoadTable(GameObject go)
    {
        if(go == null)
        {
            return;
        }

        //go.transform.SetParent(loadTableTransform);
        go.transform.position = loadTableTransform.position;
        go.transform.localRotation = loadTableTransform.localRotation;
        //go.transform.localScale = Vector3.one;
        go.SetActive(true);
    }

    private void LoadModelToScene()
    {
        GameObject go = ((GDCElementSample)models[modelsCarrousel.GetCurrentElementID()]).GoModel;
        MoveObjectToLoadTable(go);
    }

    private void LoadPanoramicToScene()
    {
        GameObject go = ((GDCElementPanoramic)panoramics[panoramicsCarrousel.GetCurrentElementID()]).GoPanoramic;
        MoveObjectToLoadTable(go);
    }

    private void LoadFileToScene()
    {
        GameObject go = ((GDCElementFile)files[filesCarrousel.GetCurrentElementID()]).GoFile;
        MoveObjectToLoadTable(go);
    }

    public void Initialize(GDCRoot gdcRoot)
    {
        this.gdcRoot = gdcRoot;
        LoadElementsFromGDC();

        modelsCarrousel.btnAction.GetComponent<InteractionButton>().OnContactEnd += LoadModelToScene;
        panoramicsCarrousel.btnAction.GetComponent<InteractionButton>().OnContactEnd += LoadPanoramicToScene;
        filesCarrousel.btnAction.GetComponent<InteractionButton>().OnContactEnd += LoadFileToScene;
    }

    public void Clear()
    {
        models = new List<GDCElement>();
        panoramics = new List<GDCElement>();
        files = new List<GDCElement>();

        modelsCarrousel.ClearCarrousel();
        panoramicsCarrousel.ClearCarrousel();
        filesCarrousel.ClearCarrousel();
    }
}
