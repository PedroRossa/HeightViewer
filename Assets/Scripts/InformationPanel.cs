using MaterialUI;
using UnityEngine;
using UnityEngine.UI;

public class InformationPanel : MonoBehaviour
{
    public Text txtTitle;
    public Text txtContent;
    public VectorImage imgTitleIcon;

    public void SetPanelTitle(string title)
    {
        txtTitle.text = title;
    }
    
    public void SetTitleIcon(VectorImageData imgIcon)
    {
        imgTitleIcon.vectorImageData = imgIcon;
    }

    public void Positionate(Vector3 position, bool useLocalReference = true, float degreesOfInclination = 0)
    {
        if (useLocalReference)
        {
            transform.localPosition = position;
            transform.localEulerAngles = new Vector3(degreesOfInclination, 0, 0);
        }
        else
        {
            transform.position = position;
            transform.eulerAngles = new Vector3(degreesOfInclination, 0, 0);
        }
    }
}
