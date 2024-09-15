using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace UnityVolumeRendering
{
    public class MaxValueSliderBehaviour : NetworkBehaviour
    {
        public void UpdateMaxValue(float percent)
        {
            if (percent < 0 || percent > 100)
            {
                return;
            }

            UpdateMaxValueRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateMaxValueRpc(float percent)
        {

            if(FindObjectsOfType<VolumeRenderedObject>().Length > 0)
            {
                VolumeRenderedObject volumeRenderedObject = FindObjectsOfType<VolumeRenderedObject>()[0];
                float maxValue = percent / 100;
                volumeRenderedObject.SetVisibilityWindow(volumeRenderedObject.GetVisibilityWindow()[0], maxValue);
            }

        }
    }
}
