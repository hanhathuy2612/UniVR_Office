using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchLedDriverButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPress()
    {
        // Debug.Log("OnPressing..");
        // if (BMSEngine.DeviceManager.Ready)
        // {
        //     Debug.Log("OnPress");
        //     int index = transform.GetSiblingIndex();
        //     LedDriver ledDriver = transform.parent.GetComponent<LedDriver>();
        //     PowerState ps = PowerState.Off;
        //     if (ledDriver.powerState == PowerState.Off)
        //     {
        //         ps = PowerState.On;
        //     }

        //     if (ledDriver.isVirtual)
        //     {

        //     }
        //     else
        //     {
        //         ledDriver.setLedDriver((uint)index + 1, ledDriver._brightnessLights[index + 1], ps);
        //     }
        // }
    }
}
