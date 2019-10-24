using UnityEngine;
using MaterialUI;
using TMPro;
using Vizlab;
using NaughtyAttributes;
using Leap.Unity.Interaction;
using System;
using System.Collections;

[RequireComponent(typeof(AnchorableBehaviour), typeof(InteractionBehaviour))]
public class GDCRoot : MonoBehaviour
{
    private Canvas canvas;
    public GDC gdc;

    private AnchorableBehaviour anchorableBehaviour;
    private InteractionBehaviour interactionBehaviour;
    private Rigidbody rigidbody;

    [Header("Sample Properties")]
    public GameObject sampleNotificationBallon;
    private TextMeshProUGUI txtSampleNotificationCount;
    private VectorImage imgSampleBallon;
    private int sampleCount;

    [Header("Panoramic Properties")]
    public GameObject panoramicNotificationBallon;
    private TextMeshProUGUI txtPanoramicNotificationCount;
    private VectorImage imgPanoramicBallon;
    private int panoramicCount;

    [Header("File Properties")]
    public GameObject fileNotificationBallon;
    private TextMeshProUGUI txtFileNotificationCount;
    private VectorImage imgFileBallon;
    private int fileCount;

    [Header("General Properties")]
    public bool isGrasped;

    private void Awake()
    {
        anchorableBehaviour = GetComponent<AnchorableBehaviour>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        anchorableBehaviour.interactionBehaviour = interactionBehaviour;
        anchorableBehaviour.anchorRotation = true;

        anchorableBehaviour.OnAttachedToAnchor += AttachedToAnchor;
        anchorableBehaviour.OnDetachedFromAnchor += DettachedFromAnchor;
    }

    private void AttachedToAnchor()
    {
        anchorableBehaviour.anchor.GetComponentInParent<InteractivePanel>().Initialize(this);
    }

    private void DettachedFromAnchor()
    {
        //Wait some time after dettach to avoid inifinte attachment
        StartCoroutine(DettachCoroutine());
    }

    IEnumerator DettachCoroutine()
    {
        anchorableBehaviour.enabled = false;
        anchorableBehaviour.anchor.GetComponentInParent<InteractivePanel>().Clear();
        yield return new WaitForSeconds(3);

        anchorableBehaviour.enabled = true;
    }

    void Update()
    {
        LookAtCamera();
    }

    private void LookAtCamera()
    {
        canvas.transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    private void SetBallonData(TextMeshProUGUI txtField, VectorImage imgBallon, int value)
    {
        if (value <= 0)
        {
            imgBallon.color = Color.gray;
            txtField.faceColor = Color.white;
            txtField.SetText("0");
        }
        else
        {
            imgBallon.color = Color.white;
            txtField.faceColor = Color.red;
            txtField.SetText(value.ToString());
        }
    }

    private void UpdateNotificationBallon(ElementType type, int value)
    {
        switch (type)
        {
            case ElementType.Sample:
                SetBallonData(txtSampleNotificationCount, imgSampleBallon, value);
                break;
            case ElementType.Panoramic:
                SetBallonData(txtPanoramicNotificationCount, imgPanoramicBallon, value);
                break;
            case ElementType.File:
                SetBallonData(txtFileNotificationCount, imgFileBallon, value);
                break;
            default:
                break;
        }
    }
    
    private void InitalizeVariables()
    {
        canvas = GetComponentInChildren<Canvas>();

        txtSampleNotificationCount = sampleNotificationBallon.GetComponentInChildren<TextMeshProUGUI>();
        imgSampleBallon = sampleNotificationBallon.GetComponent<VectorImage>();

        txtPanoramicNotificationCount = panoramicNotificationBallon.GetComponentInChildren<TextMeshProUGUI>();
        imgPanoramicBallon = panoramicNotificationBallon.GetComponent<VectorImage>();

        txtFileNotificationCount = fileNotificationBallon.GetComponentInChildren<TextMeshProUGUI>();
        imgFileBallon = fileNotificationBallon.GetComponent<VectorImage>();

        sampleCount = 0;
        panoramicCount = 0;
        fileCount = 0;

    }

    public void InitializeData(GDC gdc)
    {
        this.gdc = gdc;
        InitalizeVariables();
        
        foreach (GDCElement element in gdc.Elements)
        {
            switch (element.Type)
            {
                case ElementType.Sample:
                    //Update linerenderer of element
                    interactionBehaviour.OnGraspStay += ((GDCElementSample)element).MovingElement;
                    sampleCount++;
                    break;
                case ElementType.Panoramic:
                    //Update linerenderer of element
                    interactionBehaviour.OnGraspStay += ((GDCElementPanoramic)element).MovingElement;
                    panoramicCount++;
                    break;
                case ElementType.File:
                    fileCount++;
                    break;
                default:
                    break;
            }
        }

        UpdateNotificationBallon(ElementType.Sample, sampleCount);
        UpdateNotificationBallon(ElementType.Panoramic, panoramicCount);
        UpdateNotificationBallon(ElementType.File, fileCount);
    }
    
    [Button]
    public void SimulateGDC()
    {
        UpdateNotificationBallon(ElementType.Sample, 0);
        UpdateNotificationBallon(ElementType.Panoramic, 8);
        UpdateNotificationBallon(ElementType.File, 3);
    }
}
