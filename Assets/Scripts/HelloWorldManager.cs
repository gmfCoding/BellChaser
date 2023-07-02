using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        string address = "127.0.0.1:7777";
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client"))
            {
                try
                {
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(address.Split(':')[0], ushort.Parse(address.Split(':')[1]));
                }
                catch (System.Exception)
                {
                    return;
                }
                NetworkManager.Singleton.StartClient();
            }
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            address = GUILayout.TextField(address);
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }
    }
}