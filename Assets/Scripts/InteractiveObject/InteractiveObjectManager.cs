using Leap.Unity.Interaction;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InteractiveObjectManager : MonoBehaviour
{
    public Transform pinPoint;
    public Transform windowPoint;

    public bool initialized = false;

    private LineRenderer lineRenderer;

    private HolographicWindow holograficWindow;
    private Pin3DElement pinElement;

    private Coroutine toggleWindow;
    
    void Update()
    {
        if (initialized)
        {
            if (holograficWindow.interactionBehaviour.isGrasped)
            {
                lineRenderer.SetPosition(0, windowPoint.position);
                lineRenderer.SetPosition(1, pinPoint.position);

                holograficWindow.transform.eulerAngles = new Vector3(0, holograficWindow.transform.eulerAngles.y, 0);
            }
        }
    }

    private void InitLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.startWidth = 0.005f;

        lineRenderer.endWidth = 0.005f;

        lineRenderer.startColor = new Color(1, 0, 0, 0.5f);
        lineRenderer.endColor = new Color(1, 0, 0, 0.5f);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, windowPoint.position);
        lineRenderer.SetPosition(1, pinPoint.position);

    }

    private void ToggleWindow()
    {
        if (toggleWindow == null)
        {
            //Wait 0.65f seconds to avoid trepidation on contact
            toggleWindow = StartCoroutine(ToggleWindowWithDelay(0.65f));
        }
    }

    IEnumerator ToggleWindowWithDelay(float waitTime)
    {
        //Toggle State
        holograficWindow.windowOpened = !holograficWindow.windowOpened;

        //toogle visibility of lineRenderer and holografic window
        lineRenderer.enabled = holograficWindow.windowOpened;
        holograficWindow.gameObject.SetActive(holograficWindow.windowOpened);
        yield return new WaitForSeconds(waitTime);

        toggleWindow = null;
    }

    public void Initialize()
    {
        holograficWindow = GetComponentInChildren<HolographicWindow>();
        pinElement = GetComponentInChildren<Pin3DElement>();

        holograficWindow.Initialize(false);
        pinElement.Initialize();

        pinElement.SetOnContactBegin(ToggleWindow);

        InitLineRenderer();

        initialized = true;
    }
}
