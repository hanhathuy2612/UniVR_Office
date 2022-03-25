using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using UnityEditor;
using UnityEngine.Rendering;
using BMSEngine;
public class FPSDisplay : MonoBehaviour
{
	private float deltaTime = 0.0f;
	[SerializeField]
	private DeviceGroup ac1;
	private void Awake(){
		// TextureXR.maxViews = 2;
	}
 
	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

		// added


		if(Input.GetKeyDown(KeyCode.Keypad1)){
			ac1.message("Toggle");
		}

		if(Input.GetKeyDown(KeyCode.Home)){
			BMSEngine.DeviceManager.connectBMSServer();
		}
	}
 
	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
 
		GUIStyle style = new GUIStyle();
 
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}

}