using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchButton : MonoBehaviour
{
    // Start is called before the first frame update


    private Color deviceColorOn = new Color32(255, 50, 0, 255);
    private Color deviceColorOff = new Color32(255, 255, 255, 255);
    private Color transformStartColor;

    private bool deviceOn = false;
    private bool onTransform = false;

    private float transformSpeed = 3f;
    private float transformTime = 0f;

    void Start()
    {
        // gameObject.GetComponent<Button>().onClick.AddListener(onToggle);
        transform.GetChild(0).GetComponent<Image>().color = deviceColorOff;
    }

    // Update is called once per frame
    void Update()
    {
        if (onTransform)
        {
            if (transformTime < 1)
            {
                transformTime += transformSpeed * Time.deltaTime;
            }

            if (transformTime >= 1)
            {
                transformTime = 1.0f;
                onTransform = false;
            }

            if (deviceOn)
            {
                transform.GetChild(0).GetComponent<Image>().color = Color.Lerp(transformStartColor, deviceColorOn, transformTime);
            }
            else
            {
                transform.GetChild(0).GetComponent<Image>().color = Color.Lerp(transformStartColor, deviceColorOff, transformTime);
            }
        }
    }

    private void onToggle()
    {
        gameObject.transform.parent.transform.parent.GetComponent<TouchSwitch>().toggleSwitch(gameObject.transform.GetSiblingIndex());
        setDevice(!deviceOn);
    }

    private void setDevice(bool _deviceOn)
    {
        deviceOn = _deviceOn;
        transformStartColor = transform.GetChild(0).GetComponent<Image>().color;
        transformTime = 0f;
        onTransform = true;
    }
}
