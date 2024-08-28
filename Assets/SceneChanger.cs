using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneChanger : NetworkBehaviour
{
    public void SwitchScene()
    {
        if (IsHost)
        {
            SwitchSceneRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SwitchSceneRpc()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "TestScene")
        {
            NetworkManager.SceneManager.LoadScene("newGUI", LoadSceneMode.Single);
        }
        else if (currentSceneName == "newGUI")
        {
            NetworkManager.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
        }
    }
}
