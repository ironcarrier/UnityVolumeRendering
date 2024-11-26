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

            SetupXRInteraction(outerObject, meshContainer);
        }

        private static void SetupXRInteraction(GameObject outerObject, GameObject meshContainer)
        {
            // Add rigidbody for physics interaction
            Rigidbody rb = outerObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Add box collider to the outer object
            BoxCollider collider = outerObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            
            // Size the collider to match the mesh container
            Renderer meshRenderer = meshContainer.GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                collider.center = meshRenderer.bounds.center - outerObject.transform.position;
                collider.size = meshRenderer.bounds.size;
            }

            // Add and configure XR Grab Interactable
            XRGrabInteractable grabInteractable = outerObject.AddComponent<XRGrabInteractable>();
            VolumeInteractionSettings.SetupInteractable(grabInteractable);
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
