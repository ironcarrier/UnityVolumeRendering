using UnityEngine;
using System.Collections.Generic;
using UnityVolumeRendering;

public class CutState
{
    public Matrix4x4 planeMatrix;
    public Vector3 planePosition;
    public Quaternion planeRotation;
    public bool isActive;

    public CutState(Matrix4x4 matrix, Vector3 position, Quaternion rotation)
    {
        planeMatrix = matrix;
        planePosition = position;
        planeRotation = rotation;
        isActive = true;
    }
}

public class CutHistory : MonoBehaviour
{
    private Stack<CutState> cutHistory = new Stack<CutState>();
    private VolumeRenderedObject volumeObject;

    public void AddCut(CutState state)
    {
        cutHistory.Push(state);
    }

    public bool UndoLastCut()
    {
        if (cutHistory.Count > 0)
        {
            cutHistory.Pop();
            return true;
        }
        return false;
    }

    public CutState[] GetActiveCuts()
    {
        return cutHistory.ToArray();
    }
}