using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.SnapZone;
using UnityEngine;
using Unity.Netcode;

public class NewGUISnapZoneControls : NetworkBehaviour
{
    public void RotateSnapZonePitch(float percent)
    {
        RotateSnapZonePitchRpc(percent);
    }

    [Rpc(SendTo.Everyone)]
    private void RotateSnapZonePitchRpc(float percent)
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        Debug.Log(percent);
        float pitchRotation = percent * 1.8f - 90f;  
        Vector3 currentRotation = snapZone.transform.localEulerAngles;
        snapZone.transform.localEulerAngles = new Vector3(pitchRotation, currentRotation.y, currentRotation.z);
    }
    public void RotateSnapZoneYaw(float percent)
    {
        RotateSnapZoneYawRpc(percent);
    }

    [Rpc(SendTo.Everyone)]
    private void RotateSnapZoneYawRpc(float percent)
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        Debug.Log(percent);
        float yawRotation = percent * 1.8f - 90f; 
        Vector3 currentRotation = snapZone.transform.localEulerAngles;
        snapZone.transform.localEulerAngles = new Vector3(currentRotation.x, yawRotation, currentRotation.z);
    }

    public void RotateSnapZoneRoll(float percent)
    {
        RotateSnapZoneRollRpc(percent);
    }

    [Rpc(SendTo.Everyone)]
    private void RotateSnapZoneRollRpc(float percent)
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        Debug.Log(percent);
        float rollRotation = percent * 1.8f - 90f;  
        Vector3 currentRotation = snapZone.transform.localEulerAngles;
        snapZone.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, rollRotation);
    }
}
