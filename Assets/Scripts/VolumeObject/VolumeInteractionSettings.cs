using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityVolumeRendering
{
    public static class VolumeInteractionSettings
    {
        public static void SetupInteractable(XRGrabInteractable grabInteractable)
        {
            grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.throwOnDetach = false;
            grabInteractable.retainTransformParent = false;
            grabInteractable.smoothPosition = false;
            grabInteractable.smoothRotation = false;
            grabInteractable.attachEaseInTime = 0f;
            grabInteractable.matchAttachPosition = false;
            grabInteractable.matchAttachRotation = false;
            grabInteractable.useDynamicAttach = true;
            grabInteractable.reinitializeDynamicAttachEverySingleGrab = true;
            grabInteractable.snapToColliderVolume = false;
            grabInteractable.attachTransform = null;
        }
    }
} 