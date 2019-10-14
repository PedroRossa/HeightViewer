using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;

public class AreaSelectionTool : MonoBehaviour
{
    public InteractionHand leftHand;
    public InteractionHand rightHand;

    private LineRenderer lineRenderer;
    
    private Leap.Finger leftIndicator;
    private Leap.Finger rightIndicator;
    
    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        leftIndicator = leftHand.leapHand.Fingers[1];
        rightIndicator = rightHand.leapHand.Fingers[1];

        InitializeLineRenderer();
    }

    private void Update()
    {
            if (this.gameObject.GetComponentInChildren<InteractionBehaviour>().isGrasped)
            {
                ScaleAreaSelector();
                //UpdateSelectionArea();
            }
    }

    private void InitializeLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        lineRenderer.loop = true;
        lineRenderer.sortingOrder = 1;

        lineRenderer.positionCount = 0;
    }

    public void UpdateSelectionArea()
    {
        Vector3 lFingerPos = new Vector3(leftIndicator.TipPosition.x, leftIndicator.TipPosition.y, leftIndicator.TipPosition.z);
        Vector3 rFingerPos = new Vector3(rightIndicator.TipPosition.x, rightIndicator.TipPosition.y, rightIndicator.TipPosition.z);

        lineRenderer.positionCount = 4;

        Vector3 ul = new Vector3(lFingerPos.x, lFingerPos.y, rFingerPos.z);
        Vector3 dl = lFingerPos;
        Vector3 ur = rFingerPos;
        Vector3 dr = new Vector3(rFingerPos.x, rFingerPos.y, lFingerPos.z);

        lineRenderer.SetPosition(0, dl);
        lineRenderer.SetPosition(1, ul);
        lineRenderer.SetPosition(2, ur);
        lineRenderer.SetPosition(3, dr);
    }

    private void ScaleAreaSelector()
    {
        Vector3 lFingerPos = new Vector3(leftIndicator.TipPosition.x, leftIndicator.TipPosition.y, leftIndicator.TipPosition.z);
        Vector3 rFingerPos = new Vector3(rightIndicator.TipPosition.x, rightIndicator.TipPosition.y, rightIndicator.TipPosition.z);

        float distance = leftIndicator.TipPosition.DistanceTo(rightIndicator.TipPosition);

        this.transform.localScale = Vector3.one * distance;
        //this.transform.localScale = new Vector3(distance * 1.25f, 1, distance * 1.25f);
    }
}
