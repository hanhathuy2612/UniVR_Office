using UnityEngine;
using UnityEngine.SceneManagement;
 
public class ConsoleToGUI : MonoBehaviour
{

    [SerializeField] private Canvas canvas = null;
    string myLog = "*begin log";
    bool _doShow = true;
    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }
    void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            _doShow = !_doShow;

            if(_doShow){
                canvas.enabled = true;
            }else{
                canvas.enabled = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F5)) {
            BMSEngine.DeviceManager.connectBMSServer();

        }
    }
    public void Log(string logString, string stackTrace, LogType type)
    {
    // for onscreen...
        myLog = logString + "\n" + myLog;
    }

    void OnGUI()
    {
        if (!_doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
        new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 20, 540, 400), myLog);
    }
}