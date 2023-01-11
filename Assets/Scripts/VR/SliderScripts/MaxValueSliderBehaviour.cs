using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityVolumeRendering;

public class MaxValueSliderBehaviour : MonoBehaviour
{
    public void UpdateMaxValue(float percent)
    {
        if (percent <= 0 || percent >= 100)
        {
            return;
        }
        VolumeRenderedObject volumeRenderedObject = FindObjectsOfType<VolumeRenderedObject>()[0];
        float maxValue = percent / 100;
        volumeRenderedObject.SetVisibilityWindow(volumeRenderedObject.GetVisibilityWindow()[0], maxValue);
    }
}
