using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    [Tooltip("Base shared material to billboard")]
    public Material biilboardMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // SetShelfs();
    }
    // Update is called once per frame
    void Update()
    {
    }
    /// <summary>
    /// Initialize shelves childrens and set billboard 
    /// </summary>
    /// <param name="billboards">List of billboards texture path </param>
    public void SetShelfs(List<string> billboards)
    {

        int left = billboards.Count;
        int itBill = 0;

        if (left <= 0)
            return;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.tag == "Shelf")
            {

                ContentBillboard billboard = child.gameObject.GetComponent<ContentBillboard>();
                if (billboard && billboard.enabled)
                {

                    left = billboard.SetBillbords(left, biilboardMaterial, billboards, itBill);
                    itBill = billboards.Count - left;

                    if (left == 0)
                        break;
                }
            }
        }

        if (left > 0)
        {
            Debug.LogWarning("Required more Shelfs:: left " + left + " billboards");
        }
    }
}
