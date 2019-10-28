
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public abstract class GarbageMarkerBase : MonoBehaviour
    {
        public bool markerActive = true;

        //-------------------------------------------------
        public virtual bool showReticle
        {
            get
            {
                return true;
            }
        }
        //-------------------------------------------------
        public virtual void GarbagePlayer(Vector3 pointedAtPosition)
        {

        }
        //-------------------------------------------------
        public abstract void UpdateVisuals();
        //-------------------------------------------------
        public abstract void Highlight(bool highlight);

        //-------------------------------------------------
        public abstract void SetAlpha(float tintAlpha, float alphaPercent);

        //-------------------------------------------------
        public abstract bool ShouldActivate(Vector3 playerPosition);

    }
}
