using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityVolumeRendering
{
    public class VolumeObjectFactory
    {
        public static VolumeRenderedObject CreateObject(VolumeDataset dataset)
        {
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();

            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            volObj.volumeContainerObject = meshContainer;
            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();

            CreateObjectInternal(dataset, meshContainer, meshRenderer, volObj, outerObject);

            meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());

            return volObj;
        }
        public static async Task<VolumeRenderedObject> CreateObjectAsync(VolumeDataset dataset, IProgressHandler progressHandler = null)
        {
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();

            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            volObj.volumeContainerObject = meshContainer;
            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();

            CreateObjectInternal(dataset,meshContainer, meshRenderer,volObj,outerObject) ;

            meshRenderer.sharedMaterial.SetTexture("_DataTex", await dataset.GetDataTextureAsync(progressHandler));

            return volObj;
        }

        private static void CreateObjectInternal(VolumeDataset dataset, GameObject meshContainer, MeshRenderer meshRenderer, VolumeRenderedObject volObj, GameObject outerObject, IProgressHandler progressHandler = null)
        {            
            meshContainer.transform.parent = outerObject.transform;
            meshContainer.transform.localScale = Vector3.one;
            meshContainer.transform.localPosition = Vector3.zero;
            meshContainer.transform.parent = outerObject.transform;
            outerObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            outerObject.transform.position = new Vector3(outerObject.transform.position.x, 1f, outerObject.transform.position.z);

            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
            volObj.meshRenderer = meshRenderer;
            volObj.dataset = dataset;

            const int noiseDimX = 512;
            const int noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            volObj.transferFunction = tf;

            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            volObj.transferFunction2D = tf2D;

            meshRenderer.sharedMaterial.SetTexture("_GradientTex", null);
            meshRenderer.sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
            meshRenderer.sharedMaterial.SetTexture("_TFTex", tfTexture);

            meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");

            meshContainer.transform.localScale = dataset.scale;
            meshContainer.transform.localRotation = dataset.rotation;

            if (PlayerPrefs.GetInt("NormaliseScaleOnImport") > 0)
                volObj.NormaliseScale();

            SetupXRInteraction(outerObject, meshContainer, volObj);
        }

        private static void SetupXRInteraction(GameObject outerObject, GameObject meshContainer, VolumeRenderedObject volObj)
        {
            Rigidbody rb = outerObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            BoxCollider collider = outerObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            
            Vector3 colliderCenter = Vector3.zero;
            MeshFilter meshFilter = meshContainer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Bounds localBounds = meshFilter.sharedMesh.bounds;
                Vector3 worldCenter = meshContainer.transform.TransformPoint(localBounds.center);
                colliderCenter = outerObject.transform.InverseTransformPoint(worldCenter);
                collider.center = colliderCenter;

                Vector3 localSize = localBounds.size;
                Vector3 worldSize = Vector3.Scale(localSize, meshContainer.transform.lossyScale);
                Vector3 parentScale = outerObject.transform.lossyScale;
                collider.size = new Vector3(
                    parentScale.x != 0f ? worldSize.x / parentScale.x : worldSize.x,
                    parentScale.y != 0f ? worldSize.y / parentScale.y : worldSize.y,
                    parentScale.z != 0f ? worldSize.z / parentScale.z : worldSize.z);
            }
            else
            {
                Renderer renderer = meshContainer.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Vector3 worldCenter = renderer.bounds.center;
                    colliderCenter = outerObject.transform.InverseTransformPoint(worldCenter);
                    collider.center = colliderCenter;
                    collider.size = renderer.bounds.size;
                }
            }

            GameObject attachPoint = new GameObject("AttachPoint");
            attachPoint.transform.SetParent(outerObject.transform, false);
            attachPoint.transform.localPosition = colliderCenter;
            attachPoint.transform.localRotation = meshContainer.transform.localRotation;
            attachPoint.transform.localScale = Vector3.one;
            volObj.attachPoint = attachPoint.transform;

            XRGrabInteractable grabInteractable = outerObject.AddComponent<XRGrabInteractable>();
            VolumeInteractionSettings.SetupInteractable(grabInteractable);

            VolumeGrabPivotController pivotController = outerObject.AddComponent<VolumeGrabPivotController>();
            pivotController.Initialise(grabInteractable, volObj);
        }

        public static void SpawnCrossSectionPlane(VolumeRenderedObject volobj)
        {
            GameObject quad = GameObject.Instantiate((GameObject)Resources.Load("CrossSectionPlane"));
            quad.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CrossSectionPlane csplane = quad.gameObject.GetComponent<CrossSectionPlane>();
            csplane.SetTargetObject(volobj);
            quad.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { quad };
#endif
        }

        public static void SpawnCutoutBox(VolumeRenderedObject volobj)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutBox"));
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutBox cbox = obj.gameObject.GetComponent<CutoutBox>();
            cbox.SetTargetObject(volobj);
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
        }
        public static void SpawnCutoutSphere(VolumeRenderedObject volobj)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutSphere"));
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutSphere cSphere = obj.gameObject.GetComponent<CutoutSphere>();
            cSphere.SetTargetObject(volobj);
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
        }
    }
}
