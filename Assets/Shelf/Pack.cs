using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pack : MonoBehaviour
{
    [Tooltip("Base Prefab object to get components and set on pack objets childrens")]
    public GameObject BasePrefab;
    [Tooltip("Shelf object on scene with shelfquad child")]
    public GameObject shelf;
    [Tooltip("InteractionManager from leap rig to instance objects with interaction beahviour")]
    public InteractionManager manager;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetBillboards());
    }

    void Awake()
    {
        SetChilds();
    }
    /// <summary>
    /// Set components from base prefab to chield objs 
    /// </summary>
    private void SetChilds()
    {
        if (!BasePrefab)
            return;

        Component[] components;
        components = BasePrefab.GetComponents<Component>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            MeshFilter mesh = child.gameObject.GetComponent<MeshFilter>();
            if (mesh)
            {
                //start with one to skip transform component 
                //for (int j = 1; i < components.Length; ++i)
                for (int j = 1; j < components.Length - 2; ++j)
                {
                    if (components[j] != null && components[j] != this)
                    {
                        child.tag = BasePrefab.tag;
                        UnityEditorInternal.ComponentUtility.CopyComponent(components[j]);
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(child.gameObject);

                    }
                }
            }
        }

        var childIntMan = transform.GetComponentsInChildren<InteractionBehaviour>();
        foreach (var interactionB in childIntMan)
        {
            interactionB.manager = manager;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Creates relationship between mesh objects and billboards
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetBillboards()
    {
        yield return new WaitForSeconds(1);
        var contents = shelf.GetComponentsInChildren<Billboard>();
        foreach (var billbord in contents)
        {
            GameObject baseBill = GameObject.Find("/" + name + "/" + billbord.gameObject.name);
            if (baseBill)
            {
                BaseBillboard baseBillboard = baseBill.GetComponent<BaseBillboard>();
                if (baseBillboard)
                {
                    baseBillboard.billboard = billbord.gameObject;
                }
            }
            else
            {
                billbord.ActiveGrayEffetc();
            }
        }
        yield return null;
    }
}
