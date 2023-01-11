using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.SnapZone;
using UnityEngine;
using UnityVolumeRendering;

public class SnapZoneRotationClick : MonoBehaviour
{
    public void PitchPlus()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(10, 0, 0);

    }

    public void PitchMinus()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(-10, 0, 0);
    }

    public void YawMinus()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, 0, -10);
    }

    public void YawPlus()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, 0, 10);
    }

    public void RollPlus()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, -10, 0);
    }

    public void RollMinus()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        snapZone.transform.Rotate(0, 10, 0);
    }

    /*    public void RotateSideLeft()
        {
            GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
            snapZone.transform.Rotate(0, 5, 0);
        }

        public void RotateSideRight()
        {
            GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
            snapZone.transform.Rotate(0, -5, 0);
        }*/

    public void ScaleXUp()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if(snapZone.transform.localScale.x < 1.5)
        {
            snapZone.transform.localScale += new Vector3(0.1f, 0.0f, 0.0f);
        }
        
    }

    public void ScaleXDown()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.x > 0.2)
        {
            snapZone.transform.localScale -= new Vector3(0.1f, 0.0f, 0.0f);
        }

    }

    public void ScaleYUp()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.y < 1.5)
        {
            snapZone.transform.localScale += new Vector3(0.0f, 0.1f, 0.0f);
        }

    }

    public void ScaleYDown()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.y > 0.2)
        {
            snapZone.transform.localScale -= new Vector3(0.0f, 0.1f, 0.0f);
        }

    }

    public void ScaleZUp()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.z < 1.5)
        {
            snapZone.transform.localScale += new Vector3(0.0f, 0.0f, 0.1f);
        }

    }

    public void ScaleZDown()
    {
        GameObject snapZone = FindObjectsOfType<SnapZoneFacade>()[0].gameObject;
        if (snapZone.transform.localScale.z > 0.2)
        {
            snapZone.transform.localScale -= new Vector3(0.0f, 0.0f, 0.1f);
        }

    }

}
