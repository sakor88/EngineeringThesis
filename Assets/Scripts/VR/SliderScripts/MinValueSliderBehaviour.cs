using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace UnityVolumeRendering
{
    public class MinValueSliderBehaviour : NetworkBehaviour
    {
        public void UpdateMinValue(float percent)
        {
            if (percent < 0 || percent > 100)
            {
                return;
            }

            UpdateMinValueRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateMinValueRpc(float percent)
        {
            

            if (FindObjectsOfType<VolumeRenderedObject>().Length > 0)
            {
                VolumeRenderedObject volumeRenderedObject = FindObjectsOfType<VolumeRenderedObject>()[0];
                float minValue = percent / 100;
                volumeRenderedObject.SetVisibilityWindow(minValue, volumeRenderedObject.GetVisibilityWindow()[1]);
            }

            
        }
    }
}
