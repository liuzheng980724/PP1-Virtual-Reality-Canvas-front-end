using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0.Extensions
{
    public static class LayerMaskExtensions
    {
        public static bool IsInLayerMask(this LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) > 0);
        }

        public static bool IsInLayerMask(this LayerMask mask, GameObject obj)
        {
            return ((mask.value & (1 << obj.layer)) > 0);
        }
    }
    
}