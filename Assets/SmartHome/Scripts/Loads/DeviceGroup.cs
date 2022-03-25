using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceGroup : BMSEngine.Device {

    private List<BMSEngine.Device> _devices = new List<BMSEngine.Device>();
    
    public void Start(){

        gameObject.layer = 0;

        foreach(Transform child in transform) {
            _devices.Add(child.gameObject.GetComponent<BMSEngine.Device>());
        }
    }

    public override void message(string msg){
        foreach(BMSEngine.Device device in _devices) {
            device.message(msg);
        }
    }
    public override void setBool(string key, bool value){
        foreach(BMSEngine.Device device in _devices) {
            device.setBool(key, value);
        }
    }
    public override void setFloat(string key, float value){
        foreach(BMSEngine.Device device in _devices) {
            device.setFloat(key, value);
        }
    }
    public override void setInt(string key, int value){
        foreach(BMSEngine.Device device in _devices) {
            device.setInt(key, value);
        }
    }

    public override void setVector3(string key, Vector3 value){
        foreach(BMSEngine.Device device in _devices) {
            device.setVector3(key, value);
        }
    }

    public override void setString(string key, string value){
        foreach(BMSEngine.Device device in _devices) {
            device.setString(key, value);
        }
    }

}
