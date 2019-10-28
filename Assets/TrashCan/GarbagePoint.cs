using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class GarbagePoint : GarbageMarkerBase
    {

        //Public variables
        public string title;
        public Color titleVisibleColor;
        public Color titleHighlightedColor;

        public Material pointVisibleMaterial;
        public Material pointHighlightedMaterial;

        //Private data
        private bool gotReleventComponents = false;
        private MeshRenderer markerMesh;
        private MeshRenderer moveLocationIcon;
        private MeshRenderer pointIcon;
        private Transform lookAtJointTransform;
        private Animator animator;
        private Text titleText;
        private Vector3 lookAtPosition = Vector3.zero;
        private int tintColorID = 0;
        private Color tintColor = Color.clear;
        private Color titleColor = Color.clear;
        private float fullTitleAlpha = 0.0f;

        //Constants
        private const string moveLocationAnimation = "move_location_idle";

        //-------------------------------------------------
        public override bool showReticle
        {
            get
            {
                return false;
            }
        }

        //-------------------------------------------------
        void Awake()
        {
            GetRelevantComponents();

            animator = GetComponent<Animator>();

            tintColorID = Shader.PropertyToID("_TintColor");

            moveLocationIcon.gameObject.SetActive(true);

            UpdateVisuals();
        }


        //-------------------------------------------------
        void Start()
        {
            //animator.SetTrigger("Move");
        }

        //-------------------------------------------------
        void Update()
        {
            lookAtJointTransform.LookAt(Camera.main.transform);
        }

        //-------------------------------------------------
        public override bool ShouldActivate(Vector3 playerPosition)
        {
            return (Vector3.Distance(transform.position, playerPosition) > 1.0f);
        }

        //-------------------------------------------------
        public override void Highlight(bool highlight)
        {
            if (highlight)
            {
                SetMeshMaterials(pointHighlightedMaterial, titleHighlightedColor);
                pointIcon.gameObject.SetActive(true);
                animator.SetTrigger("Move");
            }
            else
            {
                SetMeshMaterials(pointVisibleMaterial, titleVisibleColor);
                pointIcon.gameObject.SetActive(false);
                //animator.SetTrigger("Stop");
            }
        }

        //-------------------------------------------------
        public override void UpdateVisuals()
        {
            if (!gotReleventComponents)
            {
                return;
            }
            pointIcon = moveLocationIcon;

            SetMeshMaterials(pointVisibleMaterial, titleVisibleColor);
            titleText.text = title;
        }

        //-------------------------------------------------
        public override void SetAlpha(float tintAlpha, float alphaPercent)
        {
            tintColor = markerMesh.material.GetColor(tintColorID);
            tintColor.a = tintAlpha;

            markerMesh.material.SetColor(tintColorID, tintColor);
            moveLocationIcon.material.SetColor(tintColorID, tintColor);

            titleColor.a = fullTitleAlpha * alphaPercent;
            titleText.color = titleColor;
        }


        //-------------------------------------------------
        public void SetMeshMaterials(Material material, Color textColor)
        {
            markerMesh.material = material;
            moveLocationIcon.material = material;

            titleColor = textColor;
            fullTitleAlpha = textColor.a;
            titleText.color = titleColor;
        }


        public void GetRelevantComponents()
        {
            markerMesh = transform.Find("garbage_marker_mesh").GetComponent<MeshRenderer>();
            moveLocationIcon = transform.Find("teleport_marker_lookat_joint/teleport_marker_icons/move_location_icon").GetComponent<MeshRenderer>();
            lookAtJointTransform = transform.Find("teleport_marker_lookat_joint");

            titleText = transform.Find("teleport_marker_lookat_joint/garbage_marker_canvas/garbage_marker_canvas_text").GetComponent<Text>();

            gotReleventComponents = true;
        }


        //-------------------------------------------------
        public void ReleaseRelevantComponents()
        {
            markerMesh = null;
            moveLocationIcon = null;
            lookAtJointTransform = null;
            titleText = null;
        }


        //-------------------------------------------------
        public void UpdateVisualsInEditor()
        {
            if (Application.isPlaying)
            {
                return;
            }

            GetRelevantComponents();

            markerMesh.sharedMaterial = pointVisibleMaterial;
            moveLocationIcon.sharedMaterial = pointVisibleMaterial;

            titleText.color = titleVisibleColor;

            moveLocationIcon.gameObject.SetActive(true);

            titleText.text = title;

            ReleaseRelevantComponents();
        }
    }


#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [CustomEditor(typeof(GarbagePoint))]
    public class GarbagePointEditor : Editor
    {
        //-------------------------------------------------
        void OnEnable()
        {
            if (Selection.activeTransform)
            {
                GarbagePoint garbagePoint = Selection.activeTransform.GetComponent<GarbagePoint>();
                if (garbagePoint != null)
                    garbagePoint.UpdateVisualsInEditor();
            }
        }


        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Selection.activeTransform)
            {
                GarbagePoint garbagePoint = Selection.activeTransform.GetComponent<GarbagePoint>();
                if (GUI.changed)
                {
                    garbagePoint.UpdateVisualsInEditor();
                }
            }
        }
    }
#endif
}
