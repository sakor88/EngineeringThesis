using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class IsosurfaceRenderClick : MonoBehaviour
    {
        public void OnButtonPress()
        {
            VolumeRenderedObject volumeRenderedObject = FindObjectsOfType<VolumeRenderedObject>()[0];
            RenderMode mode = RenderMode.IsosurfaceRendering;
            volumeRenderedObject.SetRenderMode(mode);
        }

    }
}

