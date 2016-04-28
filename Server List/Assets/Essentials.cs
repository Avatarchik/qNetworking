using UnityEngine;
using System.Text;

public class Essentials : MonoBehaviour {
    bool debugging = true;
    public void debug(string msg) {
        if (debugging)
            print(msg);
    }

    public byte[] StringToByteArray(string str, Encoding encoding) {
        return encoding.GetBytes(str);
    }

    public string ByteArrayToString(byte[] bytes, Encoding encoding) {
        return encoding.GetString(bytes);
    }
}

