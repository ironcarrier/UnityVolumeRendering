using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityVolumeRendering
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VolumeGrabPivotController : MonoBehaviour
    {
        private XRGrabInteractable grabInteractable;
        private VolumeRenderedObject volumeObject;

        public void Initialise(XRGrabInteractable interactable, VolumeRenderedObject volObj)
        {
            grabInteractable = interactable;
            volumeObject = volObj;
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }

        private void OnDestroy()
        {
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
                grabInteractable.selectExited.RemoveListener(OnSelectExited);
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (volumeObject == null || volumeObject.attachPoint == null)
                return;

            Pose cachedObjectPose = new Pose(volumeObject.transform.position, volumeObject.transform.rotation);

            if (args.interactorObject is XRRayInteractor rayInteractor)
            {
                Transform interactorAttach = rayInteractor.GetAttachTransform(grabInteractable);
                if (interactorAttach != null)
                {
                    interactorAttach.SetPositionAndRotation(volumeObject.attachPoint.position, volumeObject.attachPoint.rotation);
                }

                grabInteractable.useDynamicAttach = false;
                grabInteractable.matchAttachPosition = false;
                grabInteractable.matchAttachRotation = false;
                grabInteractable.attachTransform = volumeObject.attachPoint;

                volumeObject.transform.SetPositionAndRotation(cachedObjectPose.position, cachedObjectPose.rotation);
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            grabInteractable.useDynamicAttach = true;
            grabInteractable.reinitializeDynamicAttachEverySingleGrab = true;
            grabInteractable.attachTransform = null;
        }
    }
}
