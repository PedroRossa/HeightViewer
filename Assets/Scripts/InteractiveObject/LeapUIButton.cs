using Leap.Unity.Interaction;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(InteractionBehaviour), typeof(Image))]
public class LeapUIButton : MonoBehaviour
{
    public UnityEvent contactBeginEvent;
    public UnityEvent contactEndEvent;

    private InteractionBehaviour interactionBehaviour;
    private Rigidbody rigidbody;
    private Image imgHover;

    private bool beginCoroutineIsRunning;
    private bool endCoroutineIsRunning;

    private void Awake()
    {
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        rigidbody = GetComponent<Rigidbody>();
        imgHover = GetComponent<Image>();

        //Add highlight behaviour on contact begin-end
        interactionBehaviour.OnContactBegin += OnContactBegin;
        interactionBehaviour.OnContactEnd += OnContactEnd;

        interactionBehaviour.ignoreGrasping = true;
        interactionBehaviour.contactForceMode = ContactForceMode.UI;

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        //Set hover Image properties
        imgHover.color = new Color(1f, 1f, 0f, 0.5f);
        imgHover.transform.localPosition = new Vector3(0, 0, -5);
        imgHover.enabled = false;

        beginCoroutineIsRunning = false;
        endCoroutineIsRunning = false;
    }

    private void OnContactBegin()
    {
        if (!beginCoroutineIsRunning)
        {
            StartCoroutine(ContactBeginAction(0.75f));
        }
    }

    private void OnContactEnd()
    {
        if (!endCoroutineIsRunning)
        {
            StartCoroutine(ContactEndAction(0.75f));
        }
    }

    IEnumerator ContactBeginAction(float waitTime)
    {
        beginCoroutineIsRunning = true;
        imgHover.enabled = true;

        //Inovke inspector events
        contactBeginEvent.Invoke();

        yield return new WaitForSeconds(waitTime);

        beginCoroutineIsRunning = false;
    }

    IEnumerator ContactEndAction(float waitTime)
    {
        endCoroutineIsRunning = false;
        imgHover.enabled = false;

        //Inovke inspector events
        contactEndEvent.Invoke();

        yield return new WaitForSeconds(waitTime);

        endCoroutineIsRunning = false;
    }

    private void OnDisable()
    {
        if (imgHover != null)
        {
            imgHover.enabled = false;
        }

        beginCoroutineIsRunning = false;
        endCoroutineIsRunning = false;
    }
}
