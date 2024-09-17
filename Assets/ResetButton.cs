using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityVolumeRendering;

public class ResetButton : NetworkBehaviour
{
    //private void Start()
    //{
    //    NetworkManager.Singleton.StartHost();
    //}

    public void ResetClick()
    {
        ResetPositionRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ResetPositionRpc()
    {
        GameObject volumeObj = GameObject.Find("Interactions.Interactable(Clone)_VolumeContainer(Clone)");
        VolumeRenderedObject doseObj = null; //FindObjectsOfType<VolumeRenderedObject>()[0];

        if (SceneManager.GetActiveScene().name == "TestScene")
        {
            if (doseObj != null)
            {
                doseObj.transform.position = new Vector3(2.245f, 2.63f, -1.95f);
            }
            if (volumeObj != null)
            {
                volumeObj.transform.position = new Vector3(0f, 2.5f, 0f);
            }
        }
        else
        {
            if (doseObj != null)
            {
                doseObj.transform.position = new Vector3(1f, 2.4f, -0.9f);
            }
            if (volumeObj != null)
            {
                volumeObj.transform.position = new Vector3(0f, 2.375f, 0.258f);
            }
        }
    }
}
