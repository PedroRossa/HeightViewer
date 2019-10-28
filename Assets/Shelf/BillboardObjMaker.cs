using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using cakeslice;
using Leap.Unity.Interaction;


public class BillboardObjMaker : MonoBehaviour
{
    //Create a Wire cube around billboard 
    private bool makeWireCube = true;
    //Show wire cube mesh ///can hide mesh and outline still works 
    private bool renderWireCube = true;
    //False to set look at camera on quad instead of inside shader 
    private bool shaderBillboard = true;
    //Create quad extra with look at camera to set outline on texture ....mesh renderer is off 
    private bool outilineOnQuad = false;

    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// Create Billboard object 
    /// </summary>
    /// <param name="texturePath">file texture with path </param>
    /// <param name="material">Base shader material without texture to use on billboard</param>
    /// <param name="position">position to set billboard</param>
    /// <param name="scale">scale of billboard</param>
    /// <param name="parent"> parent object to attach billboard</param>
    public void CreateBillboardObj(string texturePath, Material material, Vector3 position, float scale, Transform parent)
    {
        GameObject obj;
        //make a quad to set material 
        obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.transform.localScale = new Vector3(scale, scale, scale);
        //set position
        obj.transform.position = new Vector3(position.x + scale, position.y + scale, position.z);

        //get texture from path 
        Texture2D t = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
        Material objMat = new Material(material);
        objMat.mainTexture = t;

        float scaleText = 1.0f;
        objMat.SetFloat("_Scale", scaleText);
        //objMat.SetFloat("_GrayEffectAmount", 0.85f);

        //try set look at camera on quad and disable from texture
        if (!shaderBillboard)
        {
            objMat.SetFloat("_DisableLookAtCam", 1f);

            Outline outl = obj.AddComponent(typeof(Outline)) as Outline;
            outl.eraseRenderer = true;
            outl.color = 2;

            LookAtCamera look = obj.AddComponent(typeof(LookAtCamera)) as LookAtCamera;
            look.invertFoward = true;
            look.forceUp = true;
        }

        // assign the material to the renderer
        obj.GetComponent<Renderer>().sharedMaterial = objMat;

        //Get name from file path 
        string namePath = texturePath.Remove(texturePath.Length - 4);
        int posLastSlash = namePath.LastIndexOf('/');
        string fileName = posLastSlash >= 0 ? namePath.Substring(posLastSlash + 1) : namePath;
        //set name 
        obj.name = fileName;

        //set outline on secundary quad 
        if (outilineOnQuad)
        {
            GameObject outlineQuad = CreateQuadToOutline();

            Material quadMat = new Material(material);
            quadMat.mainTexture = t;

            quadMat.SetFloat("_Scale", scaleText);
            quadMat.SetFloat("_DisableLookAtCam", 1f);

            // assign the material to the renderer
            outlineQuad.GetComponent<Renderer>().sharedMaterial = quadMat;

            outlineQuad.name = "outline of " + fileName;

            outlineQuad.transform.localScale = new Vector3(scale, scale, scale);
            outlineQuad.transform.position = new Vector3(position.x + scale, position.y + scale, position.z - (0.07f));

            outlineQuad.transform.SetParent(obj.transform);
            obj.transform.SetParent(parent, true);
        }

        //make a wire cube around billboard 
        if (makeWireCube)
        {
            float offsetZ = (scale / 2f);
            Vector3 wirePosition = new Vector3(position.x + scale, position.y + scale, position.z - offsetZ);
            CreateWireCube(fileName, scale, wirePosition, obj.transform);
        }

        //set billboard component to facilitate origin obj link
        Billboard billboard = obj.gameObject.AddComponent(typeof(Billboard)) as Billboard;
        billboard.billboardMaterial = objMat;

        obj.transform.SetParent(parent, true);
    }
    /// <summary>
    /// Create WireCube from prefab 
    /// </summary>
    /// <param name="name">Name of billboard</param>
    /// <param name="scale">Scale</param>
    /// <param name="position">Position relative from billboard objetc</param>
    /// <param name="parent"> billboard objetc</param>
    private void CreateWireCube(string name, float scale, Vector3 position, Transform parent)
    {
        string path = "Assets/Shelf/WireCubeMesh.prefab";
        Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        prefab.name = "WireCube";
        GameObject wireCube = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);

        if (wireCube != null && wireCube.transform.childCount > 0)
        {
            Transform child = wireCube.transform.GetChild(0);

            Renderer rend = child.gameObject.GetComponent<Renderer>();
            if (rend)
            {
                Outline outl = child.gameObject.AddComponent(typeof(Outline)) as Outline;
                outl.eraseRenderer = true;
                outl.color = 2;

                rend.enabled = renderWireCube;
            }
        }
        wireCube.name = "WireCube of " + name;
        wireCube.transform.localScale = new Vector3(scale, scale, scale);
        wireCube.transform.position = position;

        wireCube.transform.SetParent(parent);
    }
    /// <summary>
    ///Create quad extra with look at camera to set outline on texture ....mesh renderer is off  
    /// </summary>
    /// <returns></returns>
    private GameObject CreateQuadToOutline()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

        if (quad)
        {
            Renderer rend = quad.GetComponent<Renderer>();
            if (rend)
            {
                Outline outl = quad.AddComponent(typeof(Outline)) as Outline;
                outl.eraseRenderer = true;
                outl.color = 0;

                rend.enabled = false;
            }

            LookAtCamera look = quad.AddComponent(typeof(LookAtCamera)) as LookAtCamera;
            look.invertFoward = true;
            look.forceUp = true;
        }
        quad.name = "QuadSecundary";
        return quad;
    }

}

