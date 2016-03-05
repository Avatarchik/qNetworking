﻿using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class Server : MonoBehaviour {
    // Configuration Channels
    byte reliableChanId;

    // Sockect Configurations
    int socketId = 0;
    int socketPort = 1234;

    // Communication
    int masterServerId;
    string masterServerIp = "72.91.241.243";
    int masterServerPort = 1337;

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
        print("Socket open on port " + socketPort + ": ID #" + socketId);

        // Master Server Connection
        masterServerId = Connect(masterServerIp, masterServerPort, socketId);
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

    void Update() {
        Listen();
        Send("Hello Master Server"); // Think about shortening messages in to numbered codes to minimize data.
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
            print("Failed to connect because:" + (NetworkError)error);
        }
        else {
            print("Connected to server (" + ip + ":" + port + "): Connection ID #" + newConnectionId);
        }

        return newConnectionId;
    }

    /// <summary>
    /// Sends a message to the Master Server. It should be modified to send a message to any connection provided in the future.
    /// </summary>
    public void Send(string msg) {
        byte error;

        // Serialization
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, msg);
        int bufferSize = 1024;

        NetworkTransport.Send(socketId, masterServerId, reliableChanId, buffer, bufferSize, out error);

        if((NetworkError)error != NetworkError.Ok) {
            print("Message failed to send because: " + (NetworkError)error);
        }
        else {
            print("Message successfully sent.");
        }
    }
}