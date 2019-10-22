using UnityEngine;
using MaterialUI;
using Leap.Unity.Interaction;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(InteractionButton))]
public class IconButton2D : MonoBehaviour
{
    public VectorImage imgBackground;
    public VectorImage imgIcon;

    public Color defaultColor = new Color(0,0,0,0);
    public Color hoverColor = Color.gray;
    public Color contactColor = Color.green;

    private InteractionButton interactionButton;
    private Rigidbody rigidbody;

    private Color imgIconColor;

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

        imgIconColor = imgIcon.color;
    }

    private void PrimaryHoverBegin()
    {
        imgBackground.color = hoverColor;
    }

    private void PrimaryHoverEnd()
    {
        imgBackground.color = defaultColor;
    }

    private void ContactBegin()
    {
        imgBackground.color = contactColor;
    }

    private void ContactEnd()
    {
        imgBackground.color = hoverColor;
    }

    public void DisableButton()
    {
        interactionButton.enabled = false;
        imgIcon.color = Color.gray;
    }

    public void EnableButton()
    {
        interactionButton.enabled = true;
        imgIcon.color = imgIconColor;
    }
}
