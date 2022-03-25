using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomCamera : MonoBehaviour
{
    [SerializeField] private LayerMask _interactionLayer;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            doRayCast(Input.mousePosition);
        }
    }

    private void doRayCast(Vector3 dir)
    {
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(dir);

        if (Physics.Raycast(ray, out hit, 10, _interactionLayer))
        {
            TouchSwitchButton button = hit.transform.GetComponent<TouchSwitchButton>();
            button?.onPress();
        }
    }

}
