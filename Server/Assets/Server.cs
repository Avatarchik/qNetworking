using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class Server : MonoBehaviour {
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
    int socketId = 0;
    int socketPort = 1234;

    // Communication
    int masterServerId;
    string masterServerIp = "72.91.241.243";
    int masterServerPort = 1337;
    #endregion

    /// <summary>
    /// Should change ports and IPs
    /// </summary>
    void LoadConfig() {

    }

	// Use this for initialization
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
        debug("Socket open on port " + socketPort + ": ID #" + socketId);

        // Master Server Connection
        masterServerId = Connect(masterServerIp, masterServerPort, socketId);
    }

    void Update() {
        Listen();
        Send((byte)Actions.Debug + " Hello world!", masterServerId); // Think about shortening messages in to numbered codes to minimize data.
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
        if ((NetworkError)error != NetworkError.Ok) {
            debug("Failed to connect because:" + (NetworkError)error);
        }
        else {
            debug("Connected to server (" + ip + ":" + port + "): Connection ID #" + newConnectionId);
        }

        return newConnectionId;
    }

    /// <summary>
    /// Sends a message to the Master Server. It should be modified to send a message to any connection provided in the future.
    /// </summary>
    public void Send(string msg, int connection, bool serialize = false) {
        byte error;

        // Serialization
        byte[] buffer = new byte[1024];
        int bufferSize = 1024;

        if(serialize) {
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, msg);
        }
        else {
            buffer = StringToByteArray(msg, Encoding.UTF8);
        }

        NetworkTransport.Send(socketId, connection, reliableChanId, buffer, bufferSize, out error);

        if((NetworkError)error != NetworkError.Ok) {
            debug("Message failed to send because: " + (NetworkError)error);
        }
        else {
            debug("Message successfully sent.");
        }
    }

    /// <summary>
    /// Receives data.
    /// </summary>
    void Listen()
    {
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
                debug("Message received: " + message);
                break;

            case NetworkEventType.ConnectEvent:
                if (masterServerId == recConnectionId) {
                    debug("Self-connection approved.");
                }
                else {
                    debug("Remote connection incoming.");
                }
                break;

            case NetworkEventType.DisconnectEvent:
                if (masterServerId == recConnectionId) {
                    debug("Self-connection failed: " + (NetworkError)error);
                }
                else {
                    debug("Remote connection closed.");
                }
                break;

            case NetworkEventType.Nothing:
                //debug("Nothing received.");
                break;

            case NetworkEventType.BroadcastEvent:
                debug("Broadcast discovery event received.");
                break;
        }
    }
    #endregion

    #region Actions
    #region Action Variables
    [Flags]
    public enum Actions : byte {
        Auth = 0x00,
        Debug = 0x01 // test - 
    }
    #endregion

    /// <summary>
    /// Send the requested auth password to the master server.
    /// </summary>
    /// <param name="key">The public key.</param>
    void Authenticate(string key) {
        Send(Actions.Auth + key, masterServerId);
    }
    #endregion
}
