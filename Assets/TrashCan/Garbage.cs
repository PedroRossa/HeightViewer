using Leap.Unity.Interaction;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class Garbage : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource enterTrashCanAudioSource;

    [Header("Sounds")]
    public AudioClip soundEnterTrash;

    [Header("Controllers")]
    public GameObject player;

    public GameObject controllerViveLeft;
    public GameObject controllerViveRight;

    public GameObject controllerOculusLeft;
    public GameObject controllerOculusRight;

    public float throwingSpeed= 0.0f;

    private LineRenderer pointerLineRenderer;
    private GameObject garbagePointerObject;

    private InteractionController pointerHand = null;

    private InteractionController pointerHandLeft = null;
    private InteractionController pointerHandRight = null;

    private ProjectileCalculator projectileCalculator = null;

    private bool visible = false;

    private GarbageMarkerBase[] garbageMarkers;
    private GarbageMarkerBase closerGarbageMaker;


    private GameObject objectTothrowLeftHand;
    private GameObject objectTothrowRightHand;

    private InteractionXRController interactHandLeft;
    private InteractionXRController interactHandRight;

    public bool showLineDebug = true;

    enum HandType { Left, Right };

    //-------------------------------------------------
    private static Garbage _instance;

    public static Garbage instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Garbage>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;

        pointerLineRenderer = GetComponentInChildren<LineRenderer>();
        garbagePointerObject = pointerLineRenderer.gameObject;

        int tintColorID = Shader.PropertyToID("_TintColor");

        projectileCalculator = GetComponent<ProjectileCalculator>();

    }

    // Start is called before the first frame update
    void Start()
    {
        garbageMarkers = GameObject.FindObjectsOfType<GarbageMarkerBase>();

        HidePointers();
    }

    void Update()
    {
        SetUpdateControlls();
    }

    private InteractionXRController GetTrackedController(HandType hand)
    {

        if (hand == HandType.Left)
        {
            //verify vive first 
            if (controllerViveLeft != null)
            {
                InteractionXRController interactionHandLeft = controllerViveLeft.GetComponent<InteractionXRController>();
                if (interactionHandLeft != null && interactionHandLeft.isTracked)
                    return interactionHandLeft;
            }
            else if (controllerOculusLeft != null)
            {
                InteractionXRController interactionHandLeft = controllerOculusLeft.GetComponent<InteractionXRController>();
                if (interactionHandLeft != null && interactionHandLeft.isTracked)
                    return interactionHandLeft;
            }
        }
        else
        {
            //verify vive first 
            if (controllerViveRight != null)
            {
                InteractionXRController interactionHandRight = controllerViveRight.GetComponent<InteractionXRController>();
                if (interactionHandRight != null && interactionHandRight.isTracked)
                    return interactionHandRight;
            }
            else if (controllerOculusLeft != null)
            {
                InteractionXRController interactionHandRight = controllerOculusRight.GetComponent<InteractionXRController>();
                if (interactionHandRight != null && interactionHandRight.isTracked)
                    return interactionHandRight;
            }
        }


        return null;
    }
    private void SetUpdateControlls()
    {
        if (interactHandLeft == null || !interactHandLeft.isTracked)
            interactHandLeft = GetTrackedController(HandType.Left);

        if (interactHandRight == null || !interactHandRight.isTracked)
            interactHandRight = GetTrackedController(HandType.Right);

        if (!HasSightTrashCan())
        {
            if (interactHandLeft != null)
            {
                ClearControlls(interactHandLeft, HandType.Left);
            }

            if (interactHandRight != null)
            {
                ClearControlls(interactHandRight, HandType.Right);
            }

            HidePointers();
            projectileCalculator.HideLine();

            return;
        }


        if (interactHandLeft != null)
        {
            UpdateControlls(interactHandLeft, HandType.Left);
        }

        if (interactHandRight != null)
        {
            UpdateControlls(interactHandRight, HandType.Right);
        }

    }
    void ClearControlls(InteractionXRController controller, HandType handType)
    {
        GameObject objToThrow = handType == HandType.Left ? objectTothrowLeftHand : objectTothrowRightHand;

        if (objToThrow != null)
        {

            if (HandType.Left == handType)
                objectTothrowLeftHand = null;
            else
                objectTothrowRightHand = null;
        }
    }

    void UpdateControlls(InteractionXRController controller, HandType handType)
    {
        GameObject objToThrow = handType == HandType.Left ? objectTothrowLeftHand : objectTothrowRightHand;

        if (controller.isGraspingObject)
        {
            if (objToThrow != controller.graspedObject.gameObject)
            {
                if (objToThrow != null)
                {
                    MoveAlongPath moveAlong = objToThrow.GetComponent<MoveAlongPath>();
                    if (moveAlong != null)
                    {
                        moveAlong.SetSight(true);
                    }
                }
                objToThrow = controller.graspedObject.gameObject;
                visible = true;
            }
        }
        else if (objToThrow != null && visible)
        {
            //ajustar a verificacao para mao q ativou a lixeira 
            MoveAlongPath moveAlong = objToThrow.GetComponent<MoveAlongPath>();
            if (moveAlong != null)
            {
                moveAlong.SetSight(true);

                Rigidbody rigB = objToThrow.GetComponent<Rigidbody>();
                if (rigB != null)
                {
                    rigB.mass = 1f;
                    rigB.drag = 0;
                    rigB.angularDrag = 0.1f;
                    rigB.useGravity = true;
                    rigB.angularVelocity = Vector3.zero;
                }

                UpdateGarabagePointer(objToThrow.transform.position);


                float speed = throwingSpeed > 0 ? throwingSpeed : (projectileCalculator.GetInitialSpeed() * 0.6f);

                moveAlong.SetMoveParams(projectileCalculator.GetPath(), speed, MoveAlongPath.MoveType.Speed, soundEnterTrash, enterTrashCanAudioSource);
                //}
            }
            visible = false;
            objToThrow = null;
            //hide after burn in trash
            HidePointers();
        }

        if (visible && objToThrow != null && controller.isGraspingObject)
        {
            UpdateGarabagePointer(objToThrow.transform.position);
        }

        ///save obj 
        if (HandType.Left == handType)
            objectTothrowLeftHand = objToThrow;
        else
            objectTothrowRightHand = objToThrow;

    }

    private bool HasSightTrashCan()
    {

        if (closerGarbageMaker != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(closerGarbageMaker.transform.position);

            if (screenPoint.z <= 0.25f || screenPoint.x <= 0.25f || screenPoint.x >= 0.75f || screenPoint.y <= 0.25f || screenPoint.y >= 0.75f)
            {
                closerGarbageMaker = null;
            }
        }

        foreach (var trashcan in garbageMarkers)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(trashcan.transform.position);

            trashcan.Highlight(false);

            if (screenPoint.z > 0.25f && screenPoint.x > 0.25f && screenPoint.x < 0.75f && screenPoint.y > 0.25f && screenPoint.y < 0.75f)
            {

                if (closerGarbageMaker != null && trashcan != closerGarbageMaker)
                {
                    if (Vector3.Distance(trashcan.gameObject.transform.position, player.transform.position) < Vector3.Distance(closerGarbageMaker.gameObject.transform.position, player.transform.position))
                    {
                        trashcan.gameObject.SetActive(true);
                        closerGarbageMaker = trashcan;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (closerGarbageMaker == null)
                {
                    trashcan.gameObject.SetActive(true);
                    closerGarbageMaker = trashcan;
                }
            }
        }

        return closerGarbageMaker != null;

    }
    private void HidePointers()
    {
        visible = false;

        garbagePointerObject.SetActive(false);

        projectileCalculator.HideLine();
        foreach (GarbageMarkerBase garabageMarker in garbageMarkers)
        {
            if (garabageMarker != null && garabageMarker.markerActive && garabageMarker.gameObject != null)
            {
                garabageMarker.gameObject.SetActive(false);
            }
        }

        pointerHand = null;
    }
    private void UpdateGarabagePointer(Vector3 pointerStart)
    {
        if (closerGarbageMaker == null)
            return;

        Vector3 pointerEnd = closerGarbageMaker.gameObject.transform.position;
        Vector3 pointerDir = closerGarbageMaker.gameObject.transform.position - pointerStart;

        projectileCalculator.CalculatePath(pointerStart, pointerEnd);
        if (showLineDebug)
        {
            projectileCalculator.Show();
            projectileCalculator.DrawLine();
        }

        closerGarbageMaker.Highlight(true);
    }

    //-------------------------------------------------
    void FixedUpdate()
    {
        if (!visible)
        {
            return;
        }
    }
}
