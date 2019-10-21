using TMPro;
using UnityEngine;

public class TooltipElement : MonoBehaviour
{
    private TextMeshPro txtMessage;
    public string message;

    void Awake()
    {
        txtMessage = GetComponentInChildren<TextMeshPro>();
        txtMessage.SetText(message);
    }

    public void SetText(string text)
    {
        txtMessage.SetText(text);
    }

    public void ShowTooltip()
    {
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
