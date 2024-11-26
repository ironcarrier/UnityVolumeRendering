using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityVolumeRendering
{
    public static class VolumeInteractionSettings
    {
        public static void SetupInteractable(XRGrabInteractable grabInteractable)
        {
            // Configure for kinematic interaction
            grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.throwOnDetach = false;
            grabInteractable.retainTransformParent = true;
            
            // Smooth movement settings
            grabInteractable.smoothPosition = true;
            grabInteractable.smoothPositionAmount = 5f;
            grabInteractable.tightenPosition = 0.5f;
            grabInteractable.smoothRotation = true;
            grabInteractable.smoothRotationAmount = 5f;
            grabInteractable.tightenRotation = 0.5f;
            
            // Attach transform settings
            grabInteractable.attachEaseInTime = 0.15f;
            grabInteractable.matchAttachPosition = true;
            grabInteractable.matchAttachRotation = true;
        }
    }
} 