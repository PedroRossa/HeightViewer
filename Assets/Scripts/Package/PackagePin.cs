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
    public InteractionButton btnLoadPackage;

    private PackageManager packageManager;
    private string packageFullPath;

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

        //Set event to load this package using Package Manager
        btnLoadPackage.OnContactEnd += LoadPackage;

        //If panel start's deactivated, return it's to original state
        if (!currPanelState)
        {
            panelObject.SetActive(false);
        }
    }

    private void LoadPackage()
    {
        packageManager.LoadPackage(packageFullPath);
    }

    public void TogglePanel()
    {
        panelObject.SetActive(!panelObject.activeSelf);
    }
}
