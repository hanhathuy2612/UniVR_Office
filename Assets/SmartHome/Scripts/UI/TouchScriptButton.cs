using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchScriptButton : MonoBehaviour
{
    // Start is called before the first frame update

    private Color deviceColorOn = new Color32(0xFF, 0xC8, 0x64, 0xFF);
    private Color deviceColorOff = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    private Color transformEndColor;
    private Color transformStartColor;

    //private bool deviceOn = false;
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
                if (transformEndColor != deviceColorOff)
                {
                    transformEndColor = deviceColorOff;
                    transformStartColor = deviceColorOn;
                    transformTime = 0f;
                    onTransform = true;
                }
                else
                {
                    transformTime = 1f;
                    onTransform = false;
                }
            }

            transform.GetChild(0).GetComponent<Image>().color = Color.Lerp(transformStartColor, transformEndColor, transformTime);
        }
    }

    private void onToggle()
    {
        gameObject.transform.parent.transform.parent.GetComponent<TouchSwitch>().toggleSwitch(gameObject.transform.GetSiblingIndex());
        transformStartColor = transform.GetChild(0).GetComponent<Image>().color;
        transformEndColor = deviceColorOn;
        transformTime = 0f;
        onTransform = true;
    }
}
