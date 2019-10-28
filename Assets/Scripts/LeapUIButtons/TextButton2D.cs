using UnityEngine;
using UnityEngine.UI;
using MaterialUI;
using Leap.Unity.Interaction;

[RequireComponent(typeof(InteractionButton))]
public class TextButton2D : MonoBehaviour
{
    public Image imgBackground;
    public Text txtTextValue;

    public Color defaultColor = new Color(0, 0, 0, 0);
    public Color hoverColor = Color.gray;
    public Color contactColor = Color.green;

    private InteractionButton interactionButton;
    private Rigidbody rigidbody;

    private Color txtTextColor;

    private void Awake()
    {
        interactionButton = GetComponent<InteractionButton>();
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        interactionButton.ignoreGrasping = true;
        interactionButton.contactForceMode = ContactForceMode.UI;

        interactionButton.OnPrimaryHoverBegin += PrimaryHoverBegin;
        interactionButton.OnPrimaryHoverEnd += PrimaryHoverEnd;

        interactionButton.OnContactBegin += ContactBegin;
        interactionButton.OnContactEnd += ContactEnd;

        txtTextColor = txtTextValue.color;
    }

    private void PrimaryHoverBegin()
    {
        if (imgBackground != null)
        {
            imgBackground.color = hoverColor;
        }
    }

    private void PrimaryHoverEnd()
    {
        if (imgBackground != null)
        {
            imgBackground.color = defaultColor;
        }
    }

    private void ContactBegin()
    {
        if (imgBackground != null)
        {
            imgBackground.color = contactColor;
        }
    }

    private void ContactEnd()
    {
        if (imgBackground != null)
        {
            imgBackground.color = hoverColor;
        }
    }

    public void DisableButton()
    {
        interactionButton.enabled = false;
        txtTextValue.color = Color.gray;
    }

    public void EnableButton()
    {
        interactionButton.enabled = true;
        txtTextValue.color = txtTextColor;
    }
}
