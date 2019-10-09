using cakeslice;
using Leap.Unity.Interaction;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class InteractionOutline : MonoBehaviour
{
    private Outline outline;
    private InteractionBehaviour interactionBehaviour;

    void Start()
    {
        outline = GetComponent<Outline>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        
        interactionBehaviour.OnHoverBegin += OnHoverBegin;
        interactionBehaviour.OnHoverEnd += OnHoverEnd;

        interactionBehaviour.OnContactBegin += OnContactBegin;
        interactionBehaviour.OnContactEnd += OnContactEnd;

        interactionBehaviour.OnGraspBegin += OnGraspBegin;
        interactionBehaviour.OnGraspEnd += OnGraspEnd;

    }
    private void OnHoverBegin()
    {
        outline.eraseRenderer = false;
        outline.color = 0;
    }

    private void OnHoverEnd()
    {
        outline.eraseRenderer = true;
    }
    
    public void OnContactBegin()
    {
        outline.color = 1;
    }

    public void OnContactEnd()
    {
        if (!interactionBehaviour.isGrasped)
        {
            outline.color = 0;
        }
    }

    public void OnGraspBegin()
    {
        outline.color = 2;
    }

    public void OnGraspEnd()
    {
        outline.color = 1;
    }

}
