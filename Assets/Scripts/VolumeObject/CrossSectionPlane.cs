using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Cross section plane.
    /// Used for cutting a model (cross section view).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionPlane : MonoBehaviour, CrossSectionObject
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        [SerializeField]
        private VolumeRenderedObject targetObject;

        private void OnEnable()
        {
            if (targetObject != null)
                targetObject.GetCrossSectionManager().AddCrossSectionObject(this);
        }

        private void OnDisable()
        {
            if (targetObject != null)
                targetObject.GetCrossSectionManager().RemoveCrossSectionObject(this);
        }

        public void SetTargetObject(VolumeRenderedObject target)
        {
            if (this.enabled && targetObject != null)
                targetObject.GetCrossSectionManager().RemoveCrossSectionObject(this);
            
            targetObject = target;

            if (this.enabled && targetObject != null)
                targetObject.GetCrossSectionManager().AddCrossSectionObject(this);
        }

        public CrossSectionType GetCrossSectionType()
        {
            return CrossSectionType.Plane;
        }

        public Matrix4x4 GetMatrix()
        {
            // Use the outer object's transform instead of volumeContainerObject
            // to properly account for parent transformations when volume is moved
            return transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix;
        }
    }
}
