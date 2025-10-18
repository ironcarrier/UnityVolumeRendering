# Stabilize Grab and Preserve Buttons

1. Update `VolumeObjectFactory.SetupXRInteraction` to give the outer object a precise collider (using meshFilter.sharedMesh bounds transformed into parent space) and create a child `AttachPoint` aligned to the mesh container’s center/orientation.
2. Pass that attach point to `SetupInteractable` and re-enable dynamic attach (matchAttachPosition/Rotation true, snapToColliderVolume false, retainTransformParent true) so the grab uses hand-aligned attach, eliminating the jump while keeping controller-driven buttons operational.
3. Fix `CrossSectionPlane.GetMatrix` to multiply by `targetObject.transform.localToWorldMatrix`, ensuring cuts stay aligned after moving the volume.
