using Leap.Unity;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This simple script changes the color of an InteractionBehaviour as
/// a function of its distance to the palm of the closest hand that is
/// hovering nearby.
/// </summary>
[AddComponentMenu("")]
[RequireComponent(typeof(InteractionBehaviour))]
public class SimpleUIInteractionGlow : MonoBehaviour
{
    public bool useHover = true;

    [Header("InteractionBehaviour Colors")]
    public Color defaultColor = Color.Lerp(Color.black, Color.white, 0.1F);
    public Color suspendedColor = Color.red;
    public Color hoverColor = Color.Lerp(Color.black, Color.white, 0.7F);
    public Color primaryHoverColor = Color.Lerp(Color.black, Color.white, 0.8F);


    [Header("InteractionBehaviour Transform")]
    public float resizeFactor = 0.75f;
    public Vector3 contactPosition = new Vector3(0, 0, 8);
    public float speed = 4.0f;


    [Header("InteractionButton Colors")]
    public Color pressedColor = Color.white;

    private Image _image;
    private InteractionBehaviour _intObj;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;

        _intObj = GetComponent<InteractionBehaviour>();
        _intObj.OnContactBegin += OnContactBegin;
        _intObj.OnContactEnd += OnContactEnd;

        _image = GetComponent<Image>();
        if (_image == null)
        {
            _image = GetComponentInChildren<Image>();
        }
    }

    void Update()
    {
        if (_image != null)
        {
            Color targetColor = defaultColor;

            if (_intObj.isHovered && useHover)
            {
                float glow = _intObj.closestHoveringControllerDistance.Map(0F, 0.2F, 1F, 0.0F);
                targetColor = Color.Lerp(defaultColor, hoverColor, glow);
            }

            if (_intObj.isSuspended)
            {
                // If the object is held by only one hand and that holding hand stops tracking, the
                // object is "suspended." InteractionBehaviour provides suspension callbacks if you'd
                // like the object to, for example, disappear, when the object is suspended.
                // Alternatively you can check "isSuspended" at any time.
                targetColor = suspendedColor;
            }

            // We can also check the depressed-or-not-depressed state of InteractionButton objects
            // and assign them a unique color in that case.
            if (_intObj is InteractionButton && (_intObj as InteractionButton).isPressed)
            {
                targetColor = pressedColor;
            }

            // Lerp actual material color to the target color.
            _image.color = Color.Lerp(_image.color, targetColor, 30F * Time.deltaTime);
        }
    }

    private void OnContactBegin()
    {
        Debug.Log("Contact Begin");
        StopAllCoroutines();
        StartCoroutine(MoveObject(transform.localPosition, contactPosition, speed));
        StartCoroutine(ScaleObject(transform.localScale, Vector3.one * resizeFactor, speed));
    }

    private void OnContactEnd()
    {
        Debug.Log("Contact End");
        StopAllCoroutines();
        StartCoroutine(MoveObject(transform.localPosition, Vector3.zero, speed));
        StartCoroutine(ScaleObject(transform.localScale, Vector3.one, speed));

    }

    IEnumerator MoveObject(Vector3 from, Vector3 to, float speed)
    {
        float step = (speed / (from - to).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            t += step;
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return new WaitForFixedUpdate();
        }
        transform.localPosition = to;
    }

    IEnumerator ScaleObject(Vector3 from, Vector3 to, float speed)
    {
        float step = (speed / (from - to).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            t += step;
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return new WaitForFixedUpdate();
        }
        transform.localScale = to;
    }
}
