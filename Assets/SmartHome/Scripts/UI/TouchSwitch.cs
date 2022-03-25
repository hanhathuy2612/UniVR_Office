using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSwitch : MonoBehaviour
{
    // Start is called before the first frame update

    private GameObject targetDevice = null;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setTarget(GameObject target)
    {
        targetDevice = target;
        gameObject.SetActive(true);
    }

    public GameObject getTarget()
    {
        return targetDevice;
    }

    public void toggleSwitch(int id)
    {
        if(targetDevice != null)
        {
            targetDevice.SendMessage("toggleSwitch", id);
        }
    }
}
