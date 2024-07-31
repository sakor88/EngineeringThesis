using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace UnityVolumeRendering
{
    public class IsosurfaceRenderClick : NetworkBehaviour
    {
        public void OnButtonPress()
        {
            SetRenderModeRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void SetRenderModeRpc()
        {
            VolumeRenderedObject volumeRenderedObject = FindObjectsOfType<VolumeRenderedObject>()[0];
            RenderMode mode = RenderMode.IsosurfaceRendering;
            volumeRenderedObject.SetRenderMode(mode);
        }
    }
}
