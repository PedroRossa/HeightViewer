using System;
using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.UI;

public class PackagePin : MonoBehaviour
{
    [Header("Panel Properties")]
    public GameObject panelObject;
    public Text txtName;
    public Text txtDescription;
    public Text txtLatitude;
    public Text txtLongitude;
    public Toggle chkHeightmap;
    public Toggle chkKMLFile;
    public Text txtGDCsCount;
    public TextButton2D btnLoadPackage;

    private PackageManager packageManager;
    private string packageFullPath;

    private void Awake()
    {
        //Set event to load this package using Package Manager

        //Added on prefab because when set on script enter in a infinite loop
        //btnLoadPackage.GetComponent<InteractionButton>().OnContactEnd += LoadPackage;
    }

    public void LoadPanel(PackageManager packageManager, Package package)
    {
        //Activate panel to set values of elements
        bool currPanelState = panelObject.activeSelf;
        if (!currPanelState)
        {
            panelObject.SetActive(true);
        }

        this.packageManager = packageManager;
        packageFullPath = package.fullPath;

        txtName.text = package.name;
        txtDescription.text = package.description;
        txtLatitude.text = package.latitude.ToString();
        txtLongitude.text = package.longitude.ToString();
        txtGDCsCount.text = package.gdcs.Length.ToString();

        chkHeightmap.isOn = !string.IsNullOrEmpty(package.geoTiffPath);
        chkKMLFile.isOn = !string.IsNullOrEmpty(package.kmlPath);


        //If panel start's deactivated, return it's to original state
        if (!currPanelState)
        {
            panelObject.SetActive(false);
        }
    }

    //TODO: This function is called in a infinite loop. For now, it's removed the event of OnContactEnd of the button
    //that call's this function
    public void LoadPackage()
    { 
        packageManager.LoadPackage(packageFullPath);
    }

    public void TogglePanel()
    {
        panelObject.SetActive(!panelObject.activeSelf);
    }
}
