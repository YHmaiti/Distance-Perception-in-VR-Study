using UnityEngine;
using NativeWebSocket;



public class Connection : MonoBehaviour {
    WebSocket websocket;
    CameraCapture capture;
    public string server = "192.168.0.100:5000";
    int id = 0;
    bool processing = false;

    // Start is called before the first frame update
    async void Start() {
        // websocket = new WebSocket("ws://echo.websocket.org");
        capture = new CameraCapture();
        websocket = new WebSocket("ws://"+server);

        websocket.OnOpen += () => {
            Debug.Log("Connection open!");
            // websocket.SendText("Hi");
        };

        websocket.OnError += (e) => { Debug.Log("Error! " + e); };

        websocket.OnClose += (e) => { Debug.Log("Connection closed!"); };

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendWebSocketMessage", 0.0f, 2f);

        await websocket.Connect();
    }

    void Update() {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    public void sendJson<T>(T msg) {
        websocket.SendText(JsonUtility.ToJson(msg, true));
    }


    private async void OnApplicationQuit() {
        await websocket.Close();
    }
}