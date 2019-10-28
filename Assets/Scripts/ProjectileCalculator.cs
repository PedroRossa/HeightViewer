using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCalculator : MonoBehaviour
{

    //https://en.wikipedia.org/wiki/Projectile_motion

    //to render projectile path 
    private LineRenderer lineRendererPath;
    //line thickness
    [Tooltip("Line Thickness ")]
    public float thickness = 0.01f;
    private Transform arcObjectsTransfrom;
    [Tooltip("Line Material")]
    public Material lineMaterial;

    private Vector3 projectileVelocity;
    //array of vector until object 
    private Vector3[] pathToObject;
    private float initialSpeed;
    private float timeDuration;
    [Tooltip("Preference Angle")]
    public float arcAngle = 45.0f;
    private float radianAngle;
    public float gravityForce = 9.8f;

    private int resolution = 1;
    [Tooltip("The amount of distance between each resolution of the projectile.")]
    public float smoothPath = 0.05f;

    private bool showLine = false;
    // Start is called before the first frame update
    void Start()
    {
        initialSpeed = timeDuration = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateLineRendererObjects()
    {
        //Destroy any existing line renderer objects
        if (arcObjectsTransfrom != null)
        {
            Destroy(arcObjectsTransfrom.gameObject);
        }

        GameObject arcObjectsParent = new GameObject("ArcObjects");
        arcObjectsTransfrom = arcObjectsParent.transform;
        arcObjectsTransfrom.SetParent(this.transform);

        GameObject newObjectLr = new GameObject("LineRendererPath");
        newObjectLr.transform.SetParent(arcObjectsTransfrom);
        lineRendererPath = new LineRenderer();
        lineRendererPath = newObjectLr.AddComponent<LineRenderer>();

        lineRendererPath.receiveShadows = false;
        lineRendererPath.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        lineRendererPath.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lineRendererPath.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRendererPath.material = lineMaterial;
        lineRendererPath.startWidth = thickness;
        lineRendererPath.endWidth = thickness;
        lineRendererPath.enabled = false;

        ///linerender
        Color c1 = Color.blue;
        Color c2 = Color.blue;
        //LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRendererPath.material = new Material(Shader.Find("Sprites/Default"));
        //lineRendererPath.widthMultiplier = 0.04f;
        lineRendererPath.positionCount = resolution + 1;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRendererPath.colorGradient = gradient;
    }
    /// <summary>
    /// Get pre calculated path
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetPath()
    {
        return pathToObject;
    }
    /// <summary>
    /// Hide Debug Line 
    /// </summary>
    public void HideLine()
    {
        if (showLine)
        {
            if (lineRendererPath != null)
                lineRendererPath.enabled = false;
        }
        showLine = false;
    }
    /// <summary>
    /// Show Debug Line
    /// </summary>
    public void Show()
    {
        showLine = true;
        if (lineRendererPath == null)
        {
            CreateLineRendererObjects();
        }
    }
    /// <summary>
    /// Instance Line and set color 
    /// </summary>
    /// <param name="color"></param>
    public void SetLineColor(Color color)
    {
        Color c1 = color;
        Color c2 = color;
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRendererPath.colorGradient = gradient;
    }
    /// <summary>
    /// Draw Debug Line 
    /// </summary>
    public void DrawLine()
    {
        if (pathToObject == null || pathToObject.Length == 0)
        {
            return;
        }
        lineRendererPath.enabled = true;

        lineRendererPath.positionCount = resolution + 1;

        lineRendererPath.SetPositions(pathToObject);
    }
    /// <summary>
    /// Calculate a arc points 
    /// </summary>
    /// <param name="fractionResolution"> resolution</param>
    /// <param name="maxDistance"> total Distance </param>
    /// <param name="speed"> initial speed </param>
    /// <returns></returns>
    private Vector3 CalculateArcPoint(float fractionResolution, float maxDistance, float speed)
    {
        float x = fractionResolution * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((gravityForce * x * x) / (2 * speed * speed * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));

        return new Vector3(x, y);
    }
    /// <summary>
    /// calculate the angle path to the final position
    /// </summary>
    /// <param name="initialPos"> Initial Position </param>
    /// <param name="finalPos"> Final position</param>
    /// <returns> Vector3 of segments of path </returns>
    public Vector3[] CalculatePath(Vector3 initialPos, Vector3 finalPos)
    {
        //todo:: verificar altura maxima q irá atingir e aumentar o angulo conforme necessario..

        radianAngle = Mathf.Deg2Rad * arcAngle;

        //float difH = finalPos.y > initialPos.y ? (finalPos.y - initialPos.y) : 0f ;
        float difH = finalPos.y > initialPos.y ? (finalPos - initialPos).y : 0f; 

        float offsetDist = difH/ Mathf.Tan(radianAngle);

        //distancia wanted
        float distance = Vector3.Distance(finalPos, initialPos) + offsetDist;
        resolution = Mathf.RoundToInt(distance / smoothPath);

        Vector3[] arcArray = new Vector3[resolution + 1];

        Vector3 initialPosR = new Vector3(initialPos.x, initialPos.y - finalPos.y, initialPos.z);
        float velIni = CalculateInitialSpeed(distance, arcAngle, initialPosR);
//        float velIni = CalculateInitialSpeed(distance, arcAngle, initialPos);

        initialSpeed = velIni;

        float maxDistance = distance;

        //(2* Vi * sen (@)) / g
        timeDuration = (2 * velIni * Mathf.Sin(radianAngle)) / gravityForce;

        Vector3 pointerDir = (finalPos - initialPos).normalized;

        projectileVelocity = pointerDir * velIni;

        float distanceLastResolution = -1;
        int maxIt = 0;
        for (int i = 0; i < resolution; i++)
        {
            float fractionResolution = (float)i / (float)resolution;
            Vector3 nVect = CalculateArcPoint(fractionResolution, maxDistance, velIni);

            arcArray[i] = new Vector3(initialPos.x + (pointerDir * nVect.x).x, nVect.y + initialPos.y, initialPos.z + (pointerDir * nVect.x).z);

            //if last distance < next -- 
            if (distanceLastResolution >= 0 && distanceLastResolution < Vector3.Distance(arcArray[i], finalPos))
            {
                break;
            }

            maxIt++;
            distanceLastResolution = Vector3.Distance(arcArray[i], finalPos);
            //Debug.Log("Dist:" + distanceLastResolution.ToString() + "   ---- it = " + i.ToString());
        }
        resolution = maxIt;
        System.Array.Resize(ref arcArray, resolution + 1);
        arcArray[resolution] = finalPos;

        pathToObject = arcArray;

        return arcArray;
    }
    /// <summary>
    /// Calculate initial speed
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="angle"></param>
    /// <param name="initialPos"></param>
    /// <returns></returns>
    public float CalculateInitialSpeed(float distance, float angle, Vector3 initialPos)
    {
        float radAngle = Mathf.Deg2Rad * angle;
        //t  = dist/ cos(@)
        float td = distance / Mathf.Cos(radAngle);
        //calc initial speed
        float velIni = Mathf.Sqrt(((gravityForce / 2) * (td * td)) / (initialPos.y + (Mathf.Sin(radAngle) * td)));

        initialSpeed = velIni;

        return velIni;
    }
    /// <summary>
    /// Get pre calculate initial speed 
    /// </summary>
    /// <returns></returns>
    public float GetInitialSpeed()
    {
        return initialSpeed;
    }
    /// <summary>
    /// Calculate time of travel 
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="angle"></param>
    /// <param name="initialPos"></param>
    /// <returns></returns>
    public float CalculateDuration(float distance, float angle, Vector3 initialPos)
    {
        float radAngle = Mathf.Deg2Rad * angle;

        float InitialSpeed = CalculateInitialSpeed(distance, angle, initialPos);
        //(2* Vi * sen (@)) / g
        float time = (2 * initialSpeed * Mathf.Sin(radAngle)) / gravityForce;

        return time;
    }
    /// <summary>
    /// get pre calculate time of travel 
    /// </summary>
    /// <returns></returns>
    public float GetTimeDuration()
    {
        return timeDuration;
    }

}
