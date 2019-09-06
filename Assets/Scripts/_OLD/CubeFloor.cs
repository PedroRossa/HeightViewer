using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFloor : MonoBehaviour
{
    public int width;
    public int height;

    public float cubeSize;
    public float cubeMaxHeight;
    public Texture2D texture;
    public Material material;

    private Transform cubesParent;

    private List<GameObject> cubeList;

    void Start()
    {
        Init();
    }

    void Update()
    {

    }

    private void Init()
    {
        if (texture != null)
        {
            //    width = width <= 0 ? texture.width : width;
            //    height = height <= 0 ? texture.height : height;

            width = texture.width;
            height = texture.height;

            cubeSize = cubeSize <= 0 ? 0.1f : cubeSize;
            cubeMaxHeight = cubeMaxHeight <= 0 ? 2f : cubeMaxHeight;

            cubeList = new List<GameObject>(width * height);

            cubesParent = new GameObject("CubesParent").transform;
            cubesParent.position = Vector3.zero;
        }
        else
        {
            Debug.Log("A texture is obligatory");
        }

    }

    public void CreateFloor()
    {
        int aux = 0;
        float cubeHeightAux = 0;
        GameObject auxObj;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                auxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                aux = j + i * width;
                cubeHeightAux = texture.GetPixel(j, i).b * cubeMaxHeight;

                auxObj.GetComponent<Renderer>().material = material;
                auxObj.transform.SetParent(cubesParent);
                auxObj.transform.localScale = new Vector3(cubeSize, cubeHeightAux, cubeSize);
                auxObj.transform.position = new Vector3(j * cubeSize, cubeHeightAux/2, i * cubeSize);
                auxObj.hideFlags = HideFlags.HideInHierarchy;
                Destroy(auxObj.GetComponent<BoxCollider>());


                cubeList.Add(auxObj);
            }
        }
    }
}
