using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//http://docs.unity3d.com/Manual/UNetUsingTransport.html
public class MasterServer : MonoBehaviour {
    // Sockect Configurations
    int socketId;
    int socketPort = 1337;

    // Communication
    int connectionId;

	void Start() {
        // Initializing the Transport Layer with no arguments (default settings)
        NetworkTransport.Init();

        // Configuration Channels
        ConnectionConfig config = new ConnectionConfig();

        // Max Connections
        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        // Socket Configurations
        socketId = NetworkTransport.AddHost(topology, socketPort);
        print("Socket open on port " + socketPort + ": ID #" + socketId);

        // Communication
        connectionId = Connect("127.0.0.1", socketPort, socketId);
    }

    void Update() {
        Listen();
    }

    /// <summary>
    /// Connect to... yeah.. It's self-explanatory, dammit.
    /// </summary>
    /// <param name="ip">The ip address to connect to.</param>
    /// <param name="port">The port that is being hosted on.</param>
    /// <param name="id">The host connection ID.</param>
    int Connect(string ip, int port, int id = 0) {
        byte error;

        int newConnectionId = NetworkTransport.Connect(id, ip, port, 0, out error);
        if ((NetworkError)error != NetworkError.Ok)
        {
            print("Failed to connect because:" + (NetworkError)error);
        }
        else {
            print("Connected to server (" + ip + ":" + port + "): Connection ID #" + newConnectionId);
        }

        return newConnectionId;
    }

    /// <summary>
    /// Receives data.
    /// </summary>
    void Listen() {
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(
            out recHostId,
            out recConnectionId,
            out recChannelId,
            recBuffer,
            bufferSize,
            out dataSize,
            out error
        );

        switch (recData) {
            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                print("Message received: " + message);
                break;

            case NetworkEventType.ConnectEvent:
                print("Remote connection received.");
                break;

            case NetworkEventType.DisconnectEvent:
                print("Remote connection closed.");
                break;

            case NetworkEventType.Nothing:
                //print("Nothing received.");
                break;

            case NetworkEventType.BroadcastEvent:
                print("Broadcast discovery event received.");
                break;
        }
    }
}