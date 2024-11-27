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
    private InputActionReference saveCutAction;  // A button
    
    [SerializeField]
    private InputActionReference undoCutAction;  // Left hand A button
    
    private bool isPlaneActive = true;
    private bool wasPressed = false;
    
    private CutHistory cutHistory;
    
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

        // Move cutHistory initialization to when we actually have a volume
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

    private void OnSaveCut(InputAction.CallbackContext context)
    {
        if (crossSectionPlane != null && crossSectionPlane.gameObject.activeSelf && currentVolume != null)
        {
            // Ensure cutHistory exists
            if (cutHistory == null)
            {
                cutHistory = currentVolume.GetComponent<CutHistory>();
                if (cutHistory == null)
                {
                    cutHistory = currentVolume.gameObject.AddComponent<CutHistory>();
                }
            }

            // Check if we've reached the maximum number of cuts
            if (cutHistory.GetActiveCuts().Length >= 8)
            {
                Debug.LogWarning("Maximum number of cuts reached (8). Remove some cuts to add more.");
                return;
            }

            // Create a permanent copy at the exact same position
            GameObject permanentPlane = Instantiate(crossSectionPlane.gameObject, 
                crossSectionPlane.transform.position, 
                crossSectionPlane.transform.rotation);
            
            // Set up the permanent plane
            CrossSectionPlane permanentCut = permanentPlane.GetComponent<CrossSectionPlane>();
            permanentCut.SetTargetObject(currentVolume);
            permanentPlane.transform.parent = currentVolume.transform;
            
            // Save the cut state
            CutState newCut = new CutState(
                permanentPlane.transform.localToWorldMatrix,
                permanentPlane.transform.position,
                permanentPlane.transform.rotation
            );
            cutHistory.AddCut(newCut);
            
            // Hide the visual plane and disable collider
            MeshRenderer renderer = permanentPlane.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;
            
            Collider collider = permanentPlane.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            
            // Add to permanent cuts
            currentVolume.GetCrossSectionManager().AddPermanentCut(permanentCut);
        }
    }
    
    private void OnUndoCut(InputAction.CallbackContext context)
    {
        if (cutHistory != null && currentVolume != null)
        {
            if (cutHistory.UndoLastCut())
            {
                // Get all permanent cut planes
                CrossSectionPlane[] planes = currentVolume.GetComponentsInChildren<CrossSectionPlane>();
                
                // Remove the last permanent cut plane if there are any
                if (planes.Length > 0)  // > 0 because we want to keep the active cutting plane
                {
                    CrossSectionPlane lastPlane = planes[planes.Length - 1];
                    if (lastPlane != crossSectionPlane)  // Make sure we don't destroy the active cutting plane
                    {
                        currentVolume.GetCrossSectionManager().RemoveLastPermanentCut();
                        Destroy(lastPlane.gameObject);
                    }
                }
            }
        }
    }
}