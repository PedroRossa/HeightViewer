using Leap.Unity.Interaction;
using System;
using UnityEngine;

public class Pin3DElement : MonoBehaviour
{
    public InteractionBehaviour interactionBehaviour;
    public Color color = Color.white;
    public Transform anchorPosition;
    
    public void Initialize()
    {
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        GetComponent<MeshRenderer>().material.color = color;
    }
    
    public void SetColor(Color color)
    {
        this.color = color;
        GetComponent<MeshRenderer>().material.color = color;
    }

    public void SetOnContactBegin(Action onContactBegin)
    {
        interactionBehaviour.OnContactBegin = null;
        interactionBehaviour.OnContactBegin += onContactBegin;
    }
}
