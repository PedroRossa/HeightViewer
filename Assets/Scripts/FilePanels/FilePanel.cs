using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InteractionBehaviour))]
public abstract class FilePanel : MonoBehaviour
{
    public Text txtTitle;

    protected InteractionBehaviour interactionBehaviour;
    protected Rigidbody rigidbody;
    
    public virtual void Initialize()
    {
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        if (interactionBehaviour == null)
        {
            interactionBehaviour = gameObject.AddComponent<InteractionBehaviour>();
        }

        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        interactionBehaviour.ignoreGrasping = false;
        interactionBehaviour.moveObjectWhenGrasped = true;
        interactionBehaviour.contactForceMode = ContactForceMode.Object;

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    public void SetTextTitle(string value)
    {
        txtTitle.text = value;
    }
}
