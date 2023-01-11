using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityVolumeRendering;

public class MinValueSliderBehaviour : MonoBehaviour
{
    public void UpdateMinValue(float percent)
    {
        if (percent <= 0 || percent >= 100)
        {
            return;
        }
        VolumeRenderedObject volumeRenderedObject = FindObjectsOfType<VolumeRenderedObject>()[0];
        float minValue = percent / 100;
        volumeRenderedObject.SetVisibilityWindow(minValue, volumeRenderedObject.GetVisibilityWindow()[1]);
    }
}
