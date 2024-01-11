using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkConnect : MonoBehaviour
{

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(150, 0, 100, 100));


        if (GUILayout.Button("Create host"))
        {
            NetworkManager.Singleton.StartHost();
        }

        if (GUILayout.Button("Join as a client"))
        {
            NetworkManager.Singleton.StartClient();
        }

        GUILayout.EndArea();

    }

}
