using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AirConditioner : BMSEngine.Device
{
    [SerializeField] private VisualEffect[] _effects;
    private int _intensity = 0;

    void Start()
    {

    }

    public override void message(string msg)
    {
        if(msg == "Toggle"){
            if(_intensity == 0){
                _intensity = 10;
            }else{
                _intensity = 0;
            }

            foreach (VisualEffect _effect in _effects)
            {
                _effect?.SetFloat("Intensity", (float)_intensity);
            }
        }
    }

    public override void setInt(string key, int value)
    {
        if(key == "PowerState"){
            _intensity = value;
            foreach (VisualEffect _effect in _effects)
            {
                _effect.SetFloat("Intensity", (float)_intensity);
            }
        }
    }

    public override void setVector3(string key, Vector3 value)
    {
        if(key == "Color"){
            
            foreach (VisualEffect _effect in _effects)
            {
                _effect.SetVector3("Color", value);
            }
        }
    }

}
