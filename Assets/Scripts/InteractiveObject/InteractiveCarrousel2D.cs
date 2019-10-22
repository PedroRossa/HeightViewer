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
    public GameObject noDataPanel;

    private List<GameObject> elements = new List<GameObject>();
    private int currentElement = 0;
    private int numberOfElements = 0;

    //Disabled element to storage all unused elements of the list
    private Transform disabledContentTransform;

    void Start()
    {
        UpdateCarrouselButton();
        UpdateNoDataPanel();

        CreateDisabledElementsContentObject();

        //Add behaviours of left and right buttons
        btnToLeft.GetComponent<InteractionButton>().OnContactEnd += MoveToLeft;
        btnToRight.GetComponent<InteractionButton>().OnContactEnd += MoveToRight;
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

    private void UpdateNoDataPanel()
    {
        //Check number of elements on list
        if (numberOfElements <= 0)
        {
            noDataPanel.SetActive(true);
            return;
        }
        else
        {
            noDataPanel.SetActive(false);
        }
    }

    private void UpdateCarrouselButton()
    {
        //Manage buttons based on current element selected
        if(numberOfElements == 1)
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

    private void ClearCarrousel()
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

    public void AddElement(GameObject element)
    {
        elements.Add(element);
        numberOfElements = elements.Count;
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


    [Button]
    public void MockDataToCarrousel()
    {
        ClearCarrousel();

        int rand = Random.Range(1, 10);

        for (int i = 0; i < rand; i++)
        {
            //Instantiate a new InteractivePanel Element
            GameObject go = Instantiate(Resources.Load("InteractivePanel/InteractivePanelElement", typeof(GameObject))) as GameObject;
            //Add a random color to image
            go.GetComponentInChildren<Image>().color = new Color(Random.value, Random.value, Random.value, 1);
            //Set the name of element
            go.GetComponentInChildren<Text>().text = "Element_" + i.ToString();

            //Put the element on disabled objects
            go.transform.SetParent(disabledContentTransform);

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            elements.Add(go);
        }

        numberOfElements = elements.Count;
        elements[0].transform.SetParent(contentTransform);
        currentElement = 1;

        UpdateNoDataPanel();
        UpdateCarrouselButton();
    }
}
