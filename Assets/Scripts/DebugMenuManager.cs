using UnityEngine;

public class DebugMenuManager : MonoBehaviour
{
    public TerrainManager terrainManager;
    public GameObject content;
    public GameObject packagesListObject;
    
    public void ToggleDebugMenu()
    {
        content.SetActive(!content.activeSelf);
    }

    public void ClearMap()
    {
        terrainManager.ClearScene();
    }

    public void LoadWorldMap()
    {
        terrainManager.LoadWorldMapTerrain();
    }

    public void LoadPackage(int id)
    {
        switch (id)
        {
            case 0:
                terrainManager.LoadPackage("C:\\Users\\prossa\\Desktop\\Arapua\\package.json");
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
