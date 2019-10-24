using cakeslice;
using Leap.Unity.Interaction;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("Element Properties")]
    public float objectHeightFromPin = 5.0f;
    public Color lineRendererColor = new Color(1, 0, 0, 0.5f);
    public float lineRendererWidth = 0.005f;

    private GameObject rootObj;
    private GameObject elementContainer;
    private GameObject element;
    private GameObject pin;

    private InteractionBehaviour interactionBehaviour;
    private Rigidbody rigidbody;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        Initialize();
    }
    
    private void CreatePrefabStructure()
    {
        element = this.gameObject;
        Vector3 initialElementPosition = element.transform.localPosition;

        rootObj = new GameObject("rootObject");
        elementContainer = new GameObject("elementContainer");

        //Get element size to set the value on container so, the object can stay with scale 1
        Vector3 originalElementScale = element.transform.localScale;
        
        //put element inside of elementContainer
        gameObject.transform.SetParent(elementContainer.transform);

        //Set element scale to 1 and element container to original element scale
        elementContainer.transform.localScale = originalElementScale;
        element.transform.localScale = Vector3.one;

        //Move element to up, the pin is the anchor of the prefab
        element.transform.localPosition = new Vector3(0, 0.3f, 0);

        //put elementContainer inside of the rootObj
        elementContainer.transform.SetParent(rootObj.transform);

        pin = new GameObject("worldPosition");
        //put pin insde of rootObj
        pin.transform.SetParent(rootObj.transform);

        //Set rootObj to initial position of element
        rootObj.transform.localPosition = initialElementPosition;
    }

    private void InitializeObject()
    {
        interactionBehaviour = element.GetComponent<InteractionBehaviour>();
        if (interactionBehaviour == null)
        {
            interactionBehaviour = element.AddComponent<InteractionBehaviour>();
        }

        rigidbody = element.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = element.AddComponent<Rigidbody>();
        }

        element.AddComponent<BoxCollider>();
        element.AddComponent<InteractionOutline>();

        interactionBehaviour.ignoreGrasping = false;
        interactionBehaviour.moveObjectWhenGrasped = true;
        interactionBehaviour.contactForceMode = ContactForceMode.Object;

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }
    
    public void Initialize()
    {
        CreatePrefabStructure();
        InitializeObject();
        
        lineRenderer = Helper.CreateLineRendererOnObject(this.gameObject, lineRendererWidth*0.5f, lineRendererWidth, lineRendererColor, lineRendererColor);
        lineRenderer.SetPosition(0, pin.transform.position);
        lineRenderer.SetPosition(1, element.transform.position);
        
        interactionBehaviour.OnGraspStay += RefreshLineRenderer;
    }
    
    public void RefreshLineRenderer()
    {
        lineRenderer.SetPosition(0, pin.transform.position);
        lineRenderer.SetPosition(1, element.transform.position);
    }

    public void SetPosition(Vector3 position, bool useWorldPosition = true)
    {
        if (useWorldPosition)
        {
            rootObj.transform.position = position;
        }
        else
        {
            rootObj.transform.localPosition = position;
        }
    }

    public void SetModelScale(float scale)
    {
        element.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetModelScale(Vector3 scale)
    {
        element.transform.localScale = scale;
    }
    
    public void SetParent(Transform parent)
    {
        rootObj.transform.SetParent(parent);
    }
}
