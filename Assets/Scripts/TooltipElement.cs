using Leap.Unity.Interaction;
using TMPro;
using UnityEngine;

public class TooltipElement : MonoBehaviour
{
    public string message;

    private TextMeshPro txtMessage;
    private InteractionButton interactionButton;

    void Awake()
    {
        txtMessage = GetComponentInChildren<TextMeshPro>();
        txtMessage.SetText(message);

        interactionButton = GetComponentInParent<InteractionButton>();

        //Set button events that this tooltip is child
        if(interactionButton != null)
        {
            interactionButton.OnPrimaryHoverBegin += ShowTooltip;

            interactionButton.OnContactBegin += HideTooltip;
            interactionButton.OnPrimaryHoverEnd += HideTooltip;
        }

        gameObject.SetActive(false);
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
