using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityVolumeRendering
{
    public static class VolumeInteractionSettings
    {
        public static void SetupInteractable(XRGrabInteractable grabInteractable)
        {
            // Configure for instantaneous movement
            grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.throwOnDetach = false;
            grabInteractable.retainTransformParent = false;
            
            // Disable smoothing for more stable movement
            grabInteractable.smoothPosition = false;
            grabInteractable.smoothRotation = false;
            
            // Quick attach settings
            grabInteractable.attachEaseInTime = 0f;
            grabInteractable.matchAttachPosition = true;
            grabInteractable.matchAttachRotation = true;
        }
    }
} 