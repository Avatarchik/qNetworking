using UnityEngine.Networking;

public class MasterServer : NetworkManager {

	void Awake() {
        // Initializing the Transport Layer with no arguments (default settings)
        NetworkTransport.Init();

        // Configuration Channels
        ConnectionConfig config = new ConnectionConfig();
        int reliableChanId = config.AddChannel(QosType.Reliable);
        int unreliableChanId = config.AddChannel(QosType.Unreliable);


    }
}
