using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.SnapZone;
using UnityEngine;
using Unity.Netcode;

public class SnapZoneRotationClick : NetworkBehaviour
{
    public void PitchPlus()
    {
        PitchPlusRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void PitchPlusRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(10, 0, 0);
    }

    public void PitchMinus()
    {
        PitchMinusRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void PitchMinusRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(-10, 0, 0);
    }

    public void YawMinus()
    {
        YawMinusRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void YawMinusRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, 0, -10);
    }

    public void YawPlus()
    {
        YawPlusRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void YawPlusRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, 0, 10);
    }

    public void RollPlus()
    {
        RollPlusRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void RollPlusRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, -10, 0);
    }

    public void RollMinus()
    {
        RollMinusRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void RollMinusRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, 10, 0);
    }

    public void ScaleXUp()
    {
        ScaleXUpRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ScaleXUpRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.x < 1.5)
        {
            snapZone.transform.localScale += new Vector3(0.1f, 0.0f, 0.0f);
        }
    }

    public void ScaleXDown()
    {
        ScaleXDownRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ScaleXDownRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.x > 0.2)
        {
            snapZone.transform.localScale -= new Vector3(0.1f, 0.0f, 0.0f);
        }
    }

    public void ScaleYUp()
    {
        ScaleYUpRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ScaleYUpRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.y < 1.5)
        {
            snapZone.transform.localScale += new Vector3(0.0f, 0.1f, 0.0f);
        }
    }

    public void ScaleYDown()
    {
        ScaleYDownRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ScaleYDownRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.y > 0.2)
        {
            snapZone.transform.localScale -= new Vector3(0.0f, 0.1f, 0.0f);
        }
    }

    public void ScaleZUp()
    {
        ScaleZUpRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ScaleZUpRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.z < 1.5)
        {
            snapZone.transform.localScale += new Vector3(0.0f, 0.0f, 0.1f);
        }
    }

    public void ScaleZDown()
    {
        ScaleZDownRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ScaleZDownRpc()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.z > 0.2)
        {
            snapZone.transform.localScale -= new Vector3(0.0f, 0.0f, 0.1f);
        }
    }
}
