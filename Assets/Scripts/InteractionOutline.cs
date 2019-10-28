using cakeslice;
using Leap.Unity.Interaction;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class InteractionOutline : MonoBehaviour
{
    public bool imitateOnBillboar = false;
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

        copyToBillboard(true, 0);
    }

    private void OnHoverEnd()
    {
        outline.eraseRenderer = true;
        copyToBillboard(false, 0);
    }

    public void OnContactBegin()
    {
        outline.color = 1;
        copyToBillboard(true, 1);
    }

    public void OnContactEnd()
    {
        if (!interactionBehaviour.isGrasped)
        {
            outline.color = 0;
        }
        copyToBillboard(false, 0);
    }

    public void OnGraspBegin()
    {
        outline.color = 2;
        copyToBillboard(true, 2);
    }

    public void OnGraspEnd()
    {
        outline.color = 1;
        copyToBillboard(false, 1);
    }

    public void SetOutlineColor(bool erase, int color)
    {
        outline.eraseRenderer = erase;
        outline.color = color;

        copyToBillboard(!erase, color);
    }

    private void copyToBillboard(bool renderLine, int color)
    {
        if (!imitateOnBillboar)
            return;

        BaseBillboard billboard = gameObject.GetComponent<BaseBillboard>();
        if (billboard)
        {
            if (billboard.billboard)
            {
                var outlines = billboard.billboard.GetComponentsInChildren<Outline>();
                foreach (var oLine in outlines)
                {
                    oLine.eraseRenderer = !renderLine;
                    oLine.color = color;
                }

            }
        }
    }

}
