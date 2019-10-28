using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


namespace Valve.VR.InteractionSystem.Sample
{
    public class ForcePull : MonoBehaviour
    {
        private int currentId;
        private GameObject currentObj;
        private bool currObjUseGravity;

        public GameObject holder;

        public AnimationCurve curve;

        private ProjectileCalculator projectileCalc = null;

        private Vector3 originalPosition;
        private Vector3 halfWayPosition;
        private Quaternion originalRotation;

        private bool backOriginalPos = false;

        private float speedSidesMult = 0.0f;

        private bool startCleanCount = false;
        private float timerToClean = 0.0f;

        private bool hasObject = false;
        private bool gettingObject = false;

        public float flyDistVal = 0.025f;

        private float time = 0.0f;

        public bool debugLine = false;
        public float thickness = 0.01f;
        LineRenderer lineRendererDebug;

        [Header("Controllers")]
        public GameObject controller;

        private InteractionXRController interact;

        void Start()
        {
            currentId = 0;
            currentObj = null;
            originalPosition = Vector3.zero;
            halfWayPosition = Vector3.zero;
            originalRotation = new Quaternion();

            holder = new GameObject();
            holder.transform.parent = this.transform;
            //holder.transform.localPosition = Vector3.zero;
            holder.transform.localPosition = new Vector3(0.0f, 0f, -0.1f);
            holder.transform.localRotation = Quaternion.Euler(45.0f, 45.0f, 0f);

            //holder.transform.localPosition = (holder.transform.forward.normalized * 0.05f);

            projectileCalc = GetComponent<ProjectileCalculator>();

            if (controller)
                interact = controller.GetComponent<InteractionXRController>();

            if (debugLine)
            {
                Color c1 = Color.blue;
                Color c2 = Color.blue;
                LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startWidth = thickness;
                lineRenderer.endWidth = thickness;
                lineRenderer.positionCount = 2;

                // A simple 2 color gradient with a fixed alpha of 1.0f.
                float alpha = 1.0f;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                );
                lineRenderer.colorGradient = gradient;
            }

        }

        // Update is called once per frame
        void Update()
        {
            hasObject = interact.isGraspingObject;

            if (!hasObject)
            {
                if (Input.GetAxis("PrimaryTrigger") > 0.4f) //SteamVR_Input.GetState("forcePull", "grip", SteamVR_Input_Sources.Any) || 
                {

                    if (!gettingObject)
                        ForceControlls();

                    if (currentId != 0 && !hasObject)
                    {
                        StartCoroutine(ShakeObj());
                        PullAction();
                    }
                }
                else if (currentId != 0)
                {
                    ResetCurrObj();
                }
            }
            else
            {
                ResetCurrObj();
            }

            if (debugLine)
            {
                float dist = 100.0f;

                LineRenderer lineRenderer = GetComponent<LineRenderer>();
                lineRenderer.enabled = !hasObject;
                lineRenderer.SetPosition(0, holder.transform.position);
                //Vector3 dir = (holder.transform.forward * dist - transform.position).normalized;
                //lineRenderer.SetPosition(0, transform.position + (dir * 0.07f));

                lineRenderer.SetPosition(1, holder.transform.forward * dist);
            }
        }
        void ForceControlls()
        {
            float dist = 100.0f;

            Ray raycast = new Ray(holder.transform.position, holder.transform.forward * dist);

            RaycastHit hit;
            bool bHit = Physics.Raycast(raycast, out hit);

            if (bHit)
            {
                int id = hit.collider.GetInstanceID();

                if (hit.collider.gameObject.tag.Contains("grabblable") && currentId != id)
                {

                    if (currentId != 0)
                        ResetCurrObj();

                    MoveAlongPath moveAlong = hit.collider.gameObject.GetComponent<MoveAlongPath>();
                    if (moveAlong)
                    {
                        if (moveAlong.GetIsMoving())
                            return;
                    }

                    currentId = id;
                    currentObj = hit.collider.gameObject;
                    originalPosition = currentObj.transform.position;
                    halfWayPosition = currentObj.transform.position;
                    originalRotation = currentObj.transform.rotation;

                    SetObjUseGravity(false, true);
                    float flyDist = currObjUseGravity ? flyDistVal : 0f;

                    SetHighlight(true, currentObj);

                    currentObj.transform.position = new Vector3(originalPosition.x, originalPosition.y + flyDist, originalPosition.z);

                }
                else if (!hit.collider.gameObject.tag.Contains("grabblable") && currentId != 0)
                {
                    SetCounter();
                }
                else if (currentId == id)
                {
                    startCleanCount = false;
                }
            }
            else if (currentId != 0)
            {
                SetCounter();
            }
            if (startCleanCount)
            {
                CounterToClean();
            }

        }

        private void SetHighlight(bool highLight, GameObject obj)
        {
            if (obj == null)
                return;

            InteractionOutline interactionOutline = obj.GetComponent<InteractionOutline>();
            if (interactionOutline != null)
            {
                interactionOutline.SetOutlineColor(!highLight, 1);
            }
        }

        private void SetCounter()
        {
            if (startCleanCount)
                return;

            startCleanCount = true;
            timerToClean = 2.5f;
        }
        private void CounterToClean()
        {
            timerToClean -= (Time.deltaTime) % 60;
            if (timerToClean <= 0)
            {
                ResetCurrObj();
            }
        }
        private void SetObjUseGravity(bool useGravity, bool firstCall = false)
        {
            Rigidbody rigb = currentObj.GetComponent<Rigidbody>();
            if (rigb != null)
            {
                if (firstCall)
                    currObjUseGravity = rigb.useGravity;

                rigb.useGravity = useGravity;
                if (!rigb.useGravity)
                    rigb.angularVelocity = Vector3.zero;
            }
        }
        private void ResetCurrObj()
        {
            if (currentObj != null)
            {
                SetHighlight(false, currentObj);

                SetObjUseGravity(currObjUseGravity);

                if (backOriginalPos)
                {
                    currentObj.transform.position = originalPosition;
                    currentObj.transform.Rotate(originalRotation.x, originalRotation.y, originalRotation.z);
                }
            }
            speedSidesMult = 1.0f;
            currentId = 0;
            currentObj = null;
            timerToClean = 0.0f;
            startCleanCount = false;
            backOriginalPos = false;
            hasObject = false;
            gettingObject = false;
        }
        private void PullAction()
        {
            MoveAlongPath objMovePath = null;
            if (currentObj != null)
            {
                objMovePath = currentObj.GetComponent<MoveAlongPath>();

                if (objMovePath != null && objMovePath.GetIsMoving())
                    halfWayPosition = currentObj.transform.position;

                //teste  angle 
                //Vector3[] path = projectileCalc.CalculatePath(transform.position, currentObj.transform.position);
                //projectileCalc.Show();
                //projectileCalc.SetLineColor(Color.yellow);
                //projectileCalc.DrawLine();
            }

            if (Vector3.Distance(transform.position, currentObj.transform.position) <= 0.2f)
            {
                hasObject = true;
                backOriginalPos = false;
                currentObj.transform.position = transform.position;
                if (objMovePath != null && objMovePath.GetIsMoving())
                    objMovePath.Stop();

            }

            if (gettingObject)
            {
                time += Time.deltaTime;
                float speed = curve.Evaluate(time);

                //Vector3[] path = projectileCalc.CalculatePath(transform.position, currentObj.transform.position);
                //projectileCalc.Show();
                //projectileCalc.SetLineColor(Color.yellow);
                //projectileCalc.DrawLine();

                if (objMovePath != null && objMovePath.GetIsMoving())
                {
                    objMovePath.SetSight(true);
                    //System.Array.Reverse(path);
                    Vector3[] Simplepath = { transform.position, currentObj.transform.position };
                    // objMovePath.SetMoveParams(path, curve.Evaluate(0.0f), MoveAlongPath.MoveType.Speed);
                    objMovePath.SetMoveParams(Simplepath, speed, MoveAlongPath.MoveType.Speed, null, null);
                }
            }

            if (Input.GetAxis("SecondaryTrigger") > 0.4f)//SteamVR_Input.GetState("forcePull", "pull", SteamVR_Input_Sources.LeftHand)
            {
                if (!gettingObject && !hasObject)
                {
                    gettingObject = true;
                    time = 0.0f;
                    //StartCoroutine(MoveObject());

                    if (currentObj != null && Vector3.Distance(transform.position, currentObj.transform.position) > 0.2f)
                    {
                        //Vector3[] path = projectileCalc.CalculatePath(transform.position, currentObj.transform.position);
                        //projectileCalc.Show();
                        //projectileCalc.SetLineColor(Color.yellow);
                        //projectileCalc.DrawLine();

                        //  objMovePath = currentObj.GetComponent<MoveAlongPath>();

                        if (objMovePath != null && !objMovePath.GetIsMoving())
                        {
                            objMovePath.SetSight(true);
                            //System.Array.Reverse(path);
                            Vector3[] Simplepath = { transform.position, currentObj.transform.position };
                            // objMovePath.SetMoveParams(path, curve.Evaluate(0.0f), MoveAlongPath.MoveType.Speed);
                            objMovePath.SetMoveParams(Simplepath, curve.Evaluate(0.0f), MoveAlongPath.MoveType.Speed, null, null);
                        }
                    }
                }
            }
            else if ((Input.GetAxis("SecondaryTrigger") < 0.4f && gettingObject))//SteamVR_Input.GetStateUp("forcePull", "pull", SteamVR_Input_Sources.LeftHand) ||
            {
                projectileCalc.HideLine();
                gettingObject = false;
                time = 0.0f;
                if (currentObj != null)
                {
                    if (objMovePath != null)
                        objMovePath.Stop();
                }
            }
        }
        private IEnumerator ShakeObj()
        {
            float speed = 1.2f;
            float amount = 0.015f;
            float amountSides = 0.005f;
            float amountRotation = 0.001f;

            speedSidesMult = 5.0f;
            while (true)
            {
                if (!gettingObject && currentObj != null)
                {
                    Vector3 position = originalPosition != halfWayPosition ? halfWayPosition : originalPosition;
                    float flyDist = currObjUseGravity ? flyDistVal : 0f;
                    currentObj.transform.position = new Vector3((position.x - amountSides) + (Mathf.Sin(Time.time * speed * speedSidesMult) * amountSides), (position.y + flyDist + (Mathf.Sin(Time.time * speed) * (amount / 2.0f))), (position.z - amountSides) + (Mathf.Sin(Time.time * speed * speedSidesMult) * amountSides));
                    //currentObj.transform.Rotate((originalRotation.x ) + (Mathf.Sin(Time.time * speed) * amountSides), (originalRotation.y) + (Mathf.Sin(Time.time * speed) * amountSides), (originalRotation.z) + (Mathf.Sin(Time.time * speed) * amountSides));
                    //todo:: get what face object turn to player and rotate same face in "x" only 
                    currentObj.transform.Rotate((Mathf.Sin(Time.time * speed / 2) * amountRotation), 0, 0);// (Mathf.Sin(Time.time * speed) * amountRotation));
                }
                else
                    yield break;

                yield return null;
            }
        }
    }
}
