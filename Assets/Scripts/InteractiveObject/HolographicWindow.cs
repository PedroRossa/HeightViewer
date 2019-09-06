using Leap.Unity.Interaction;
using UnityEngine;

public class HolographicWindow : MonoBehaviour
{
    public InteractionBehaviour interactionBehaviour;
    public bool windowOpened;
    
    public void Initialize(bool windowOpened)
    {
        interactionBehaviour = GetComponent<InteractionBehaviour>();

        this.windowOpened = windowOpened;
        gameObject.SetActive(windowOpened);

    }
}
