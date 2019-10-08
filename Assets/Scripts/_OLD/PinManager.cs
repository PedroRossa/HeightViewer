using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinManager : MonoBehaviour
{
    public Color color = Color.white;

    private SpriteRenderer sptPin;
    public SpriteRenderer sptOutline;
    public GameObject canvas;

    void Start()
    {
        sptPin = GetComponent<SpriteRenderer>();
        sptPin.color = color;

        sptOutline.enabled = false;
    }

    private void OnMouseEnter()
    {
        sptOutline.enabled = true;
        Debug.Log("Mouse is enter GameObject.");
    }

    private void OnMouseDown()
    {
        canvas.SetActive(!canvas.activeSelf);
        Debug.Log("Mouse is down GameObject.");
    }

    private void OnMouseExit()
    {
        sptOutline.enabled = false;
        Debug.Log("Mouse is no longer on GameObject.");
    }
}
