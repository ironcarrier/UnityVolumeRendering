// New file: Assets/Scripts/TransferFunction/HounsfieldUnit.cs
using UnityEngine;

namespace UnityVolumeRendering
{
    public static class HounsfieldUnit
    {
        public const float MIN_HU = -1024.0f;
        public const float MAX_HU = 3071.0f;
        public const float HU_RANGE = MAX_HU - MIN_HU;

        public static float ToNormalized(float hounsfield)
        {
            return Mathf.Clamp01((hounsfield - MIN_HU) / HU_RANGE);
        }

        public static float FromNormalized(float normalized)
        {
            return Mathf.Lerp(MIN_HU, MAX_HU, normalized);
        }
    }
}