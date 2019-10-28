using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentBillboard : MonoBehaviour
{
    [Tooltip("Spacing between blocks")]
    public float spacingBlock = 0.001f;
    [Tooltip("Minimum block size ... used to calculate quantities of billboard per shelf")]
    public float minBlockSize = 0.2f;
    [Tooltip("Centralize horizontal position on shelf")]
    public bool CentralizeX = true;
    [Tooltip("keeps horizontal centered on next layers")]
    public bool keepCentralizationX = true;
    [Tooltip("Centralize vertical position on shelf")]
    public bool CentralizeY = true;
    [Tooltip("Stack up billboard if shelf is high enough")]
    public bool stackUp = true;
    
    private List<BillboardObjMaker> billboardMaker;

    // Start is called before the first frame update
    void Start()
    {
        //disable renderer of quad shelf 
        MeshRenderer renderer = gameObject.transform.GetComponent<MeshRenderer>();
        if (renderer)
        {
            renderer.enabled = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
    /// <summary>
    /// Calculate and set billboards on shelf 
    /// </summary>
    /// <param name="numBilboards">Number of billboards to position</param>
    /// <param name="biilboardMaterial">Billboard base material</param>
    /// <param name="billboardTextures">List of billboards textures path</param>
    /// <param name="iteratorBillboard">iIterator position of list of billboards</param>
    /// <returns>Return how many billboards are left to position</returns>
    public int SetBillbords(int numBilboards, Material biilboardMaterial,List<string> billboardTextures, int iteratorBillboard)
    {
        int numBboards = numBilboards;
        //get width and height from mesh 
        float width = gameObject.transform.GetComponent<MeshFilter>().mesh.bounds.size.x * gameObject.transform.localScale.x;
        float height = gameObject.transform.GetComponent<MeshFilter>().mesh.bounds.size.y * gameObject.transform.localScale.y;
        
        //size of block with spacing block 
        float minBlockS = minBlockSize + spacingBlock;
        //maximum number of horizontal blocks
        int numblocksX = Mathf.FloorToInt(width / minBlockS);

        if (width < minBlockSize)
        {
            numblocksX = 1;
            minBlockS = width;
        }
        if (minBlockS > height)
        {
            minBlockS = height;
        }
        //maximum number of vertical blocks
        int numblocksY = stackUp ? (Mathf.FloorToInt(height / minBlockS)) : 1;

        billboardMaker = new List<BillboardObjMaker>();

        float offsetY = CentralizeY ? (height - (numblocksY * minBlockS) + (spacingBlock / 2f)) : 0f;
        if (offsetY != 0f)
        {
            offsetY = (offsetY / 2f);
        }

        Quaternion originalRotation = gameObject.transform.localRotation;
        Vector3 originalPosition = gameObject.transform.position;

        gameObject.transform.SetPositionAndRotation(new Vector3(0f,0f,0f), new Quaternion(0f, 0f, 0f, 0f));

        float numB = numBboards > numblocksX ? numblocksX : numBboards;

        float offsetX = CentralizeX ? (width - (numB * minBlockS) + (spacingBlock / 2f)) : 0f;
        if (offsetX != 0f)
        {
            offsetX = (offsetX / 2f);
        }

        for (int j = 0; j < numblocksY; j++)
        {
            if (j > 0 && keepCentralizationX && CentralizeX && numBboards < numblocksX)
            {
                numB = numBboards > numblocksX ? numblocksX : numBboards;

                offsetX = (width - (numB * minBlockS) + (spacingBlock / 2f));
                if (offsetX != 0f)
                {
                    offsetX = (offsetX / 2f);
                }
            }

            for (int i = 0; i < numblocksX; i++)
            {
                float positionXi = (gameObject.transform.localPosition.x + (spacingBlock / 2f) - (width / 2)) - (minBlockS / 2) + (i * (minBlockS)) + offsetX;
                float positionYi = (gameObject.transform.localPosition.y + (spacingBlock / 2f) - (height / 2)) - (minBlockS / 2) + (j * (minBlockS)) + offsetY;
                float positionZi = gameObject.transform.localPosition.z;

                Vector3 posIni = new Vector3(positionXi, positionYi, positionZi);

                BillboardObjMaker billboard = new BillboardObjMaker();

                billboard.CreateBillboardObj(billboardTextures[iteratorBillboard], biilboardMaterial, posIni, minBlockS - spacingBlock, gameObject.transform);
                iteratorBillboard++;

                billboardMaker.Add(billboard);

                numBboards--;

                if (numBboards == 0)
                    break;
            }

            if (numBboards == 0)
                break;
        }

        gameObject.transform.SetPositionAndRotation(originalPosition, originalRotation);

        return numBboards;
    }

}