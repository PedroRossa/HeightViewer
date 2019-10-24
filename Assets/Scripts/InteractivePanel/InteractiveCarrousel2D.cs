using UnityEngine.UI;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class InteractiveCarrousel2D : MonoBehaviour
{
    public Transform contentTransform;
    public IconButton2D btnToLeft;
    public IconButton2D btnToRight;
    public IconButton2D btnAction;

    private List<GameObject> elements = new List<GameObject>();
    private int currentElement = 0;
    private int numberOfElements = 0;

    //Disabled element to storage all unused elements of the list
    private Transform disabledContentTransform;

    void Start()
    {
        //Add behaviours of left and right buttons
        btnToLeft.GetComponent<InteractionButton>().OnContactEnd += MoveToLeft;
        btnToRight.GetComponent<InteractionButton>().OnContactEnd += MoveToRight;

        CreateDisabledElementsContentObject();
        UpdateCarrouselButton();
    }

    private void CreateDisabledElementsContentObject()
    {
        GameObject disabledElementsObject = new GameObject("Disabled Elements");
        disabledElementsObject.transform.SetParent(contentTransform);

        disabledElementsObject.transform.localPosition = Vector3.zero;
        disabledElementsObject.transform.localRotation = Quaternion.identity;
        disabledElementsObject.transform.localScale = Vector3.one;

        disabledElementsObject.SetActive(false);

        disabledContentTransform = disabledElementsObject.transform;
    }
    
    private void UpdateCarrouselButton()
    {
        if(numberOfElements <=0)
        {
            btnAction.DisableButton();
        }
        else
        {
            btnAction.EnableButton();
        }

        //Manage buttons based on current element selected
        if(numberOfElements <= 1)
        {
            btnToLeft.DisableButton();
            btnToRight.DisableButton();
            return;
        }

        if (currentElement <= 1)
        {
            btnToLeft.DisableButton();
            btnToRight.EnableButton();
        }
        else if (currentElement < numberOfElements)
        {
            btnToLeft.EnableButton();
            btnToRight.EnableButton();
        }
        else
        {
            btnToLeft.EnableButton();
            btnToRight.DisableButton();
        }
    }

    private GameObject CreateCarrouselElement(string name, Sprite sprite = null)
    {
        //Instantiate a new InteractivePanel Element
        GameObject go = Instantiate(Resources.Load("InteractivePanel/InteractivePanelElement", typeof(GameObject))) as GameObject;

        go.GetComponentInChildren<Text>().text = name;

        if (sprite != null)
        {
            go.GetComponentInChildren<Image>().sprite = sprite;
        }

        //Put the element on disabled objects
        go.transform.SetParent(disabledContentTransform);

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        elements.Add(go);

        return go;
    }


    public void ClearCarrousel()
    {
        //Clear List
        elements.Clear();

        //Delete gameObjects from content
        for (int i = 0; i < contentTransform.childCount; i++)
        {
            Destroy(contentTransform.GetChild(i).gameObject);
        }

        //Recreate disabledElementsTransform
        CreateDisabledElementsContentObject();
        UpdateCarrouselButton();
    }

    public void MoveToLeft()
    {
        if (currentElement <= 0)
        {
            return;
        }

        elements[currentElement - 1].transform.SetParent(disabledContentTransform);
        //Decrease count of current element
        currentElement--;

        elements[currentElement - 1].transform.SetParent(contentTransform);
        UpdateCarrouselButton();
    }

    public void MoveToRight()
    {
        if (currentElement >= numberOfElements)
        {
            return;
        }

        elements[currentElement - 1].transform.SetParent(disabledContentTransform);
        //Decrease count of current element
        currentElement++;

        elements[currentElement - 1].transform.SetParent(contentTransform);
        UpdateCarrouselButton();
    }

    public void AddElement(GameObject go)
    {
        //TODO: Preciso achar uma forma de salvar o modelo que vem do GDC em algum lugar (pelo menos a referencia)

        CreateCarrouselElement(go.name);
        numberOfElements = elements.Count;

        SetFirstElement();
    }

    public void AddElement(string name, Sprite spt = null)
    {
        CreateCarrouselElement(name, spt);
        numberOfElements = elements.Count;

        SetFirstElement();
    }

    public void RemoveElement(int id)
    {
        try
        {
            elements.RemoveAt(id);
            numberOfElements = elements.Count;
        }
        catch (System.Exception error)
        {
            Debug.Log("Error: " + error.Message);
        }
    }

    public GameObject GetCurrentElement()
    {
        return elements[currentElement - 1];
    }
    
    public int GetCurrentElementID()
    {
        return currentElement - 1;
    }

    [Button]
    public void MockDataToCarrousel()
    {
        ClearCarrousel();

        int rand = Random.Range(1, 10);

        for (int i = 0; i < rand; i++)
        {
           CreateCarrouselElement("Element_" + i.ToString());
        }

        numberOfElements = elements.Count;
        SetFirstElement();
    }

    private void SetFirstElement()
    {
        elements[0].transform.SetParent(contentTransform);
        currentElement = 1;
        UpdateCarrouselButton();
    }
}
