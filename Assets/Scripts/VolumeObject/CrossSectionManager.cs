using UnityEngine;
using System.Collections.Generic;

namespace UnityVolumeRendering
{
    public enum CrossSectionType
    {
        Plane = 1,
        BoxInclusive = 2,
        BoxExclusive = 3,
        SphereInclusive = 4,
        SphereExclusive = 5
    }

    public struct CrossSectionData
    {
        public CrossSectionType type;
        public Matrix4x4 matrix;
    }

    /// <summary>
    /// Manager for all cross section objects (planes and boxes).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionManager : MonoBehaviour
    {
        private const int MAX_CROSS_SECTIONS = 8;

        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        private VolumeRenderedObject targetObject;
        private List<CrossSectionObject> crossSectionObjects = new List<CrossSectionObject>();
        private List<CrossSectionObject> permanentCuts = new List<CrossSectionObject>();
        private Matrix4x4[] crossSectionMatrices = new Matrix4x4[MAX_CROSS_SECTIONS];
        private float[] crossSectionTypes = new float[MAX_CROSS_SECTIONS];
        private CrossSectionData[] crossSectionData = new CrossSectionData[MAX_CROSS_SECTIONS];

        public CrossSectionData[] GetCrossSectionData()
        {
            return crossSectionData;
        }
        
        public void AddCrossSectionObject(CrossSectionObject crossSectionObject)
        {
            crossSectionObjects.Add(crossSectionObject);
        }

        public void RemoveCrossSectionObject(CrossSectionObject crossSectionObject)
        {
            crossSectionObjects.Remove(crossSectionObject);
        }

        public void AddPermanentCut(CrossSectionObject cut)
        {
            permanentCuts.Add(cut);
            UpdateCrossSectionData();
        }

        public void RemoveLastPermanentCut()
        {
            if (permanentCuts.Count > 0)
            {
                permanentCuts.RemoveAt(permanentCuts.Count - 1);
                UpdateCrossSectionData();
            }
        }

        private void Awake()
        {
            targetObject = GetComponent<VolumeRenderedObject>();
        }

        private void Update()
        {
            if (targetObject == null)
                return;

            Material mat = targetObject.meshRenderer.sharedMaterial;

            if (crossSectionObjects.Count > 0)
            {
                int numCrossSections = System.Math.Min(crossSectionObjects.Count, MAX_CROSS_SECTIONS);

                for (int i = 0; i < numCrossSections; i++)
                {
                    CrossSectionObject crossSectionObject = crossSectionObjects[i];
                    crossSectionMatrices[i] = crossSectionObject.GetMatrix();
                    crossSectionTypes[i] = (int)crossSectionObject.GetCrossSectionType();
                    crossSectionData[i] = new CrossSectionData() { type = crossSectionObjects[i].GetCrossSectionType(), matrix = crossSectionMatrices[i] };
                }

                mat.EnableKeyword("CROSS_SECTION_ON");
                mat.SetMatrixArray("_CrossSectionMatrices", crossSectionMatrices);
                mat.SetFloatArray("_CrossSectionTypes", crossSectionTypes);
                mat.SetInt("_NumCrossSections", numCrossSections);
            }
            else
            {
                mat.DisableKeyword("CROSS_SECTION_ON");
            }
        }

        private void UpdateCrossSectionData()
        {
            // Combine permanent cuts with active cross sections
            int totalCuts = Mathf.Min(permanentCuts.Count + crossSectionObjects.Count, MAX_CROSS_SECTIONS);
            
            // First add permanent cuts
            for (int i = 0; i < permanentCuts.Count && i < totalCuts; i++)
            {
                crossSectionData[i].type = permanentCuts[i].GetCrossSectionType();
                crossSectionData[i].matrix = permanentCuts[i].GetMatrix();
            }

            // Then add active cross sections
            for (int i = permanentCuts.Count; i < totalCuts; i++)
            {
                int activeIndex = i - permanentCuts.Count;
                if (activeIndex < crossSectionObjects.Count)
                {
                    crossSectionData[i].type = crossSectionObjects[activeIndex].GetCrossSectionType();
                    crossSectionData[i].matrix = crossSectionObjects[activeIndex].GetMatrix();
                }
            }
        }
    }
}
