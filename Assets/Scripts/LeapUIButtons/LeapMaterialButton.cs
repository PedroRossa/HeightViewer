using Leap.Unity.Interaction;
using MaterialUI;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(InteractionBehaviour))]
public class LeapMaterialButton : MonoBehaviour
{
    [Header("Properties")]
    public bool fillHover;

    [Header("Events")]
    public UnityEvent contactBeginEvent;
    public UnityEvent contactEndEvent;

    private InteractionBehaviour interactionBehaviour;
    private Rigidbody rigidbody;
    private Image imgHover;
    private MaterialButton btnMaterial;

    private bool beginCoroutineIsRunning;
    private bool endCoroutineIsRunning;

    private void Awake()
    {
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        rigidbody = GetComponent<Rigidbody>();
        imgHover = GetComponent<Image>();

        btnMaterial = GetComponentInChildren<MaterialButton>();
        
        imgHover.fillCenter = fillHover;

        //Add highlight behavior on contact begin-end
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
