using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityVolumeRendering;

public class ControllerSlicingPlane : MonoBehaviour
{
    private CrossSectionPlane crossSectionPlane;
    private VolumeRenderedObject currentVolume;

    [SerializeField]
    private InputActionReference togglePlaneAction;

    [SerializeField]
    private InputActionReference saveCutAction;

    [SerializeField]
    private InputActionReference undoCutAction;

    [SerializeField, Tooltip("Distance in metres to place the cutting plane along the controller forward axis.")]
    private float planeForwardOffset = 0f;

    [SerializeField, Tooltip("Flip the cutting direction so volume is removed toward the controller instead of away from it.")]
    private bool invertCutDirection = false;

    [SerializeField, Tooltip("Visual thickness of the controller plane (local Y scale).")]
    private float planeVisualThickness = 0.00000000000001f;

    private bool isPlaneActive = true;
    private bool wasPressed = false;

    private CutHistory cutHistory;

    private static readonly Quaternion ForwardPlaneRotation = Quaternion.AngleAxis(180f, Vector3.up);
    private static readonly Quaternion BackwardPlaneRotation = Quaternion.identity;

    private void Start()
    {
        if (togglePlaneAction != null)
        {
            togglePlaneAction.action.started += OnTogglePlane;
            togglePlaneAction.action.Enable();
        }

        if (saveCutAction != null)
        {
            saveCutAction.action.started += OnSaveCut;
            saveCutAction.action.Enable();
        }

        if (undoCutAction != null)
        {
            undoCutAction.action.started += OnUndoCut;
            undoCutAction.action.Enable();
        }

        VolumeRenderedObject foundVolume = FindObjectOfType<VolumeRenderedObject>();
        if (foundVolume != null)
        {
            currentVolume = foundVolume;
            cutHistory = currentVolume.gameObject.AddComponent<CutHistory>();
            CreateCrossSectionPlane();
        }
    }

    private void OnDestroy()
    {
        if (togglePlaneAction != null)
        {
            togglePlaneAction.action.started -= OnTogglePlane;
            togglePlaneAction.action.Disable();
        }

        if (saveCutAction != null)
        {
            saveCutAction.action.started -= OnSaveCut;
            saveCutAction.action.Disable();
        }

        if (undoCutAction != null)
        {
            undoCutAction.action.started -= OnUndoCut;
            undoCutAction.action.Disable();
        }
    }

    private void Update()
    {
        if (togglePlaneAction != null && !togglePlaneAction.action.IsPressed())
        {
            wasPressed = false;
        }

        if (crossSectionPlane == null || currentVolume == null)
        {
            VolumeRenderedObject foundVolume = FindObjectOfType<VolumeRenderedObject>();

            if (foundVolume != null && foundVolume != currentVolume)
            {
                currentVolume = foundVolume;
                CreateCrossSectionPlane();
            }
        }
        else
        {
            ApplyPlaneTransform();
        }
    }

    private void ApplyPlaneTransform()
    {
        if (crossSectionPlane == null)
            return;

        crossSectionPlane.transform.localPosition = new Vector3(0f, 0f, planeForwardOffset);
        crossSectionPlane.transform.localRotation = invertCutDirection ? BackwardPlaneRotation : ForwardPlaneRotation;
    }

    private void CreateCrossSectionPlane()
    {
        if (currentVolume == null) return;

        if (crossSectionPlane != null)
        {
            Destroy(crossSectionPlane.gameObject);
        }

        VolumeObjectFactory.SpawnCrossSectionPlane(currentVolume);
        crossSectionPlane = FindObjectOfType<CrossSectionPlane>();

        if (crossSectionPlane != null)
        {
            crossSectionPlane.transform.SetParent(transform, true);

            ApplyPlaneTransform();

            Vector3 scale = currentVolume.transform.localScale * 0.005f;
            scale.y = planeVisualThickness;
            crossSectionPlane.transform.localScale = scale;

            crossSectionPlane.gameObject.SetActive(isPlaneActive);
        }
    }

    private void OnTogglePlane(InputAction.CallbackContext context)
    {
        if (!wasPressed)
        {
            wasPressed = true;
            isPlaneActive = !isPlaneActive;
            if (crossSectionPlane != null)
            {
                crossSectionPlane.gameObject.SetActive(isPlaneActive);
            }
        }
    }

    public void SetTargetVolume(VolumeRenderedObject volume)
    {
        if (volume != currentVolume)
        {
            currentVolume = volume;
            CreateCrossSectionPlane();
        }
    }

    private void OnSaveCut(InputAction.CallbackContext context)
    {
        if (crossSectionPlane != null && crossSectionPlane.gameObject.activeSelf && currentVolume != null)
        {
            if (cutHistory == null)
            {
                cutHistory = currentVolume.GetComponent<CutHistory>();
                if (cutHistory == null)
                {
                    cutHistory = currentVolume.gameObject.AddComponent<CutHistory>();
                }
            }

            if (cutHistory.GetActiveCuts().Length >= 8)
            {
                Debug.LogWarning("Maximum number of cuts reached (8). Remove some cuts to add more.");
                return;
            }

            GameObject permanentPlane = Instantiate(crossSectionPlane.gameObject,
                crossSectionPlane.transform.position,
                crossSectionPlane.transform.rotation);

            CrossSectionPlane permanentCut = permanentPlane.GetComponent<CrossSectionPlane>();
            permanentCut.SetTargetObject(currentVolume);
            permanentPlane.transform.parent = currentVolume.transform;

            CutState newCut = new CutState(
                permanentPlane.transform.localToWorldMatrix,
                permanentPlane.transform.position,
                permanentPlane.transform.rotation
            );
            cutHistory.AddCut(newCut);

            MeshRenderer renderer = permanentPlane.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;

            Collider collider = permanentPlane.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            currentVolume.GetCrossSectionManager().AddPermanentCut(permanentCut);
        }
    }

    private void OnUndoCut(InputAction.CallbackContext context)
    {
        if (cutHistory != null && currentVolume != null)
        {
            if (cutHistory.UndoLastCut())
            {
                CrossSectionPlane[] planes = currentVolume.GetComponentsInChildren<CrossSectionPlane>();

                if (planes.Length > 0)
                {
                    CrossSectionPlane lastPlane = planes[planes.Length - 1];
                    if (lastPlane != crossSectionPlane)
                    {
                        currentVolume.GetCrossSectionManager().RemoveLastPermanentCut();
                        Destroy(lastPlane.gameObject);
                    }
                }
            }
        }
    }
}