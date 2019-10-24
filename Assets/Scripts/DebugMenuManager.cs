using UnityEngine;

public class DebugMenuManager : MonoBehaviour
{
    public PackageManager packageManager;
    public GameObject content;
    public GameObject packagesListObject;
    
    public void ToggleDebugMenu()
    {
        content.SetActive(!content.activeSelf);
    }

    public void ClearMap()
    {
        packageManager.ClearScene();
    }

    public void LoadWorldMap()
    {
        packageManager.LoadPackages();
    }

    public void LoadPackage(int id)
    {
        switch (id)
        {
            case 0:
                packageManager.LoadPackage("C:\\Users\\prossa\\Desktop\\Arapua\\package.json");
                break;
            case 1:
                break;
            default:
                break;
        }

        //close menu
        content.SetActive(false);
        packagesListObject.SetActive(false);
    }
}
