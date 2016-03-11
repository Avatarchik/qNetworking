using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

//http://docs.unity3d.com/Manual/UNetUsingTransport.html

public class MasterServer : MonoBehaviour {
    #region Essentials
    bool debugging = true;
    public void debug(string msg) {
        if(debugging)
            print(msg);
    }

    public byte[] StringToByteArray(string str, Encoding encoding) {
        return encoding.GetBytes(str);
    }

    public string ByteArrayToString(byte[] bytes, Encoding encoding) {
        return encoding.GetString(bytes);
    }
    #endregion

    #region Network
    #region Network Variables
    // Configuration Channels
    byte reliableChanId;

    // Sockect Configurations
    int socketId;
    int socketPort = 1337;

    // Communication
    int connectionId;
    #endregion

    void Start() {
        // Initializing the Transport Layer with no arguments (default settings)
        NetworkTransport.Init();

        // Configuration Channels
        ConnectionConfig config = new ConnectionConfig();
        reliableChanId = config.AddChannel(QosType.Reliable);

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
                // Find a way to tell if it was serialized or not.
                ///Stream stream = new MemoryStream(recBuffer);
                ///BinaryFormatter formatter = new BinaryFormatter();
                ///string message = formatter.Deserialize(stream) as string;
                string message = ByteArrayToString(recBuffer, Encoding.UTF8);
                print("Message received: " + message);

                byte code = Convert.ToByte(message.Substring(0, message.IndexOf(' ') + 1));
                PerformAction(code, message.Substring(message.IndexOf(' ') + 1));
                break;

            case NetworkEventType.ConnectEvent:
                if (connectionId == recConnectionId) {
                    print("Self-connection approved.");
                }
                else {
                    print("Remote connection incoming.");
                }
                break;

            case NetworkEventType.DisconnectEvent:
                if (connectionId == recConnectionId) {
                    print("Self-connection failed: " + (NetworkError)error);
                }
                else {
                    print("Remote connection closed.");
                }
                break;

            case NetworkEventType.Nothing:
                //print("Nothing received.");
                break;

            case NetworkEventType.BroadcastEvent:
                print("Broadcast discovery event received.");
                break;
        }
    }
    #endregion

    #region Action Variables
    [Flags]
    public enum Actions : byte {
        Auth = 0x00,
        Debug = 0x01 // test
    }

    void PerformAction(byte code, string msg) {
        // Unsure if this cast will work.
        switch ((Actions)code) {
            case Actions.Debug:
                debug("Debugging socket: " + msg);
                break;

        }
    }
    #endregion

}