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

    private bool isPlaneActive = true;
    private bool wasPressed = false;

    private void Start()
    {
        if (togglePlaneAction != null)
        {
            togglePlaneAction.action.started += OnTogglePlane;
            togglePlaneAction.action.Enable();
        }
    }

    private void OnDestroy()
    {
        if (togglePlaneAction != null)
        {
            togglePlaneAction.action.started -= OnTogglePlane;
            togglePlaneAction.action.Disable();
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

    private void Update()
    {
        // Reset the press state when button is released
        if (togglePlaneAction != null && !togglePlaneAction.action.IsPressed())
        {
            wasPressed = false;
        }

        // Rest of the update code remains the same
        if (crossSectionPlane == null || currentVolume == null)
        {
            VolumeRenderedObject foundVolume = FindObjectOfType<VolumeRenderedObject>();
            
            if (foundVolume != null && foundVolume != currentVolume)
            {
                currentVolume = foundVolume;
                CreateCrossSectionPlane();
            }
        }
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
            crossSectionPlane.transform.SetParent(null);
            crossSectionPlane.transform.SetParent(transform, false);
            
            // Changed rotation to cut in the correct direction
            crossSectionPlane.transform.localRotation = Quaternion.Euler(-135, 0, 0);
            
            crossSectionPlane.transform.localPosition = new Vector3(0, 0, 0.2f);
            
            // Keep the small scale
            Vector3 scale = currentVolume.transform.localScale * 0.005f;
            scale.y = 0.001f; // Keep it very thin
            crossSectionPlane.transform.localScale = scale;
            
            crossSectionPlane.gameObject.SetActive(isPlaneActive);
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
}