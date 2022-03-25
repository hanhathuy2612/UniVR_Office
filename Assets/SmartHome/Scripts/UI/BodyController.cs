using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] 
    private GameObject cameraLook;
    private Rigidbody body;
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector3.zero;

        if(Input.GetKey(KeyCode.W)){
            dir = cameraLook.transform.forward;
        }
        
        if(Input.GetKey(KeyCode.S)){
            dir = -cameraLook.transform.forward;
        }

        if(Input.GetKey(KeyCode.A)){
            dir = -cameraLook.transform.right;
        }

        if(Input.GetKey(KeyCode.D)){
            dir = cameraLook.transform.right;
        }

        dir[1] = 0;
        body.AddForce(dir, ForceMode.Impulse);
        body.velocity = Vector3.ClampMagnitude(body.velocity, 0.3f);
    }
}
