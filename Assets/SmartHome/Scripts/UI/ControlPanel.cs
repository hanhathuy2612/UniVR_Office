using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    [Tooltip("Control Panel open by default")]
    private bool openByDefault = true;

    [SerializeField]
    [Tooltip("Control Panel Width")]
    [Range(300f, 500f)]
    private float width = 300f;

    [SerializeField]
    [Tooltip("expand/collapse speed of Control Panel")]
    [Range(1f, 10f)]
    private float expandSpeed = 5f;

    [SerializeField]
    private GameObject deviceUI = null;

    private float transformTime = 0f;

    private bool expand = true;
    private bool onTransform = false;

    void Start()
    {

        if (expandSpeed <= 0 || expandSpeed > 10)
        {
            expandSpeed = 5f;
        }

        GameObject.Find("Main Camera").gameObject.GetComponent<AudioListener>().enabled = true;
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (openByDefault)
        {
            GetComponent<RectTransform>().anchoredPosition = width * Vector2.left;
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        expand = openByDefault;
        setExpand(expand);

        selectionInit();
        desellectDevice();
    }

    // Update is called once per frame
    void Update()
    {
        if (onTransform)
        {
            if(transformTime < 1)
            {
                transformTime += expandSpeed * Time.deltaTime;
            }

            if(transformTime >= 1)
            {
                transformTime = 1f;
                onTransform = false;
            }

            if (expand)
            {
                GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-width, 0), transformTime);
            }
            else
            {
                GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(new Vector2(-width, 0), Vector2.zero, transformTime);
            }

        }
    }

    private void selectionInit()
    {
        Dropdown layerSelect = GameObject.Find("Layer_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown roomSelect = GameObject.Find("Room_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown viewSelect = GameObject.Find("View_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown deviceSelect = GameObject.Find("Device_Select").transform.GetChild(1).GetComponent<Dropdown>();

        layerSelect.onValueChanged.AddListener(delegate
        {
            onLayerSelect(layerSelect);
        });

        roomSelect.onValueChanged.AddListener(delegate
        {
            onRoomSelect(roomSelect);
        });

        viewSelect.onValueChanged.AddListener(delegate
        {
            onViewSelect(viewSelect);
        });

        deviceSelect.onValueChanged.AddListener(delegate
        {
            onDeviceSelect(deviceSelect);
        });


        GameObject root = GameObject.Find("Building_Manager").gameObject;
        List<string> layerList = new List<string>();
        List<string> roomList = new List<string>();
        List<string> viewList = new List<string>();
        List<string> deviceList = new List<string>();

        // Clear all selection
        layerSelect.ClearOptions();
        roomSelect.ClearOptions();
        viewSelect.ClearOptions();
        deviceSelect.ClearOptions();

        // Generate Layer List
        foreach(Transform child in root.transform)
        {
            layerList.Add(child.GetComponent<SpecificName>().getName());
        }

        layerSelect.AddOptions(layerList);

        // Get first Layer
        if (root.transform.childCount > 0)
        {
            root = root.transform.GetChild(0).gameObject;
        }
        else return;

        // Generate Room List
        foreach (Transform child in root.transform)
        {
            roomList.Add(child.GetComponent<SpecificName>().getName());
        }

        roomSelect.AddOptions(roomList);

        // Get first Room View
        if (root.transform.childCount > 0)
        {
            root = root.transform.GetChild(0).transform.GetChild(0).gameObject;
        }
        else return;

        // Generate View List
        foreach (Transform child in root.transform)
        {
            viewList.Add(child.GetComponent<SpecificName>().getName());
        }

        viewSelect.AddOptions(viewList);

    }

    private void onLayerSelect(Dropdown select)
    {
        GameObject root = GameObject.Find("Building_Manager").transform.GetChild(select.value).gameObject;

        Dropdown roomSelect = GameObject.Find("Room_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown viewSelect = GameObject.Find("View_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown deviceSelect = GameObject.Find("Device_Select").transform.GetChild(1).GetComponent<Dropdown>();


        List<string> roomList = new List<string>();
        List<string> viewList = new List<string>();
        List<string> deviceList = new List<string>();

        // Clear all selection

        roomSelect.ClearOptions();
        viewSelect.ClearOptions();
        deviceSelect.ClearOptions();

        // Generate Room List
        foreach (Transform child in root.transform)
        {
            roomList.Add(child.GetComponent<SpecificName>().getName());
        }

        roomSelect.AddOptions(roomList);

        // Get first Room Views
        if (root.transform.childCount > 0)
        {
            root = root.transform.GetChild(0).transform.GetChild(0).gameObject;
        }
        else return;

        // Generate View List
        foreach (Transform child in root.transform)
        {
            viewList.Add(child.GetComponent<SpecificName>().getName());
        }

        viewSelect.AddOptions(viewList);

        // Get first View
        if (root.transform.childCount > 0)
        {
            root = root.transform.GetChild(0).gameObject;
        }
        else return;

        // Move Camera
        GameObject.Find("Main Camera").GetComponent<CameraManager>().setCamera(root.GetComponent<Camera>());

    }

    private void onRoomSelect(Dropdown select)
    {
        Dropdown layerSelect = GameObject.Find("Layer_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown viewSelect = GameObject.Find("View_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown deviceSelect = GameObject.Find("Device_Select").transform.GetChild(1).GetComponent<Dropdown>();

        GameObject root = GameObject.Find("Building_Manager").transform.GetChild(layerSelect.value).transform.GetChild(select.value).transform.GetChild(0).gameObject;

        List<string> viewList = new List<string>();
        List<string> deviceList = new List<string>();

        // Clear all selection
        viewSelect.ClearOptions();
        deviceSelect.ClearOptions();


        // Generate View List
        foreach (Transform child in root.transform)
        {
            viewList.Add(child.GetComponent<SpecificName>().getName());
        }

        viewSelect.AddOptions(viewList);

        // Get first View
        if (root.transform.childCount > 0)
        {
            root = root.transform.GetChild(0).gameObject;
        }
        else return;

        // Move Camera
        GameObject.Find("Main Camera").GetComponent<CameraManager>().setCamera(root.GetComponent<Camera>());
    }

    private void onViewSelect(Dropdown select)
    {
        Dropdown layerSelect = GameObject.Find("Layer_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown roomSelect = GameObject.Find("Room_Select").transform.GetChild(1).GetComponent<Dropdown>();
        Dropdown deviceSelect = GameObject.Find("Device_Select").transform.GetChild(1).GetComponent<Dropdown>();

        GameObject root = GameObject.Find("Building_Manager").transform.GetChild(layerSelect.value).transform.GetChild(roomSelect.value).transform.GetChild(0).transform.GetChild(select.value).gameObject;

        List<string> deviceList = new List<string>();

        // Clear all selection
        deviceSelect.ClearOptions();

        // Move Camera
        GameObject.Find("Main Camera").GetComponent<CameraManager>().setCamera(root.GetComponent<Camera>());
    }

    private void onDeviceSelect(Dropdown select)
    {

    }

    public void setExpand(bool _expand)
    {
        expand = _expand;

        if (!expand)
        {
            Vector3 scale = GameObject.Find("expandButton").GetComponent<RectTransform>().localScale;
            GameObject.Find("expandButton").GetComponent<RectTransform>().localScale = new Vector3(-0.5f, scale.y, scale.z);
            transformTime = (GetComponent<RectTransform>().anchoredPosition.x + width) / width;
            onTransform = true;
        }
        else
        {
            Vector3 scale = GameObject.Find("expandButton").GetComponent<RectTransform>().localScale;
            GameObject.Find("expandButton").GetComponent<RectTransform>().localScale = new Vector3(0.5f, scale.y, scale.z);
            transformTime = -GetComponent<RectTransform>().anchoredPosition.x / width;
            onTransform = true;
        }
    }

    public bool getExpand()
    {
        return expand;
    }

    public void desellectDevice()
    {
        foreach(Transform child in deviceUI.transform)
        {
            child.gameObject.SetActive(false);
        }

        deviceUI.SetActive(false);
    }


    public void selectDevice(GameObject device)
    {
        desellectDevice();
        deviceUI.transform.GetChild((int)device.GetComponent<TouchSwitchDevice>().deviceType).GetComponent<TouchSwitch>().setTarget(device);
        deviceUI.SetActive(true);
    }
}
