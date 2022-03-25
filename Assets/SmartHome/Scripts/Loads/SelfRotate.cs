using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRotate : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private bool _isRotate = false;
    [SerializeField] private float _speedX = 0f;
    [SerializeField] private float _speedY = 0f;
    [SerializeField] private float _speedZ = 0f;

    public bool isRotate {get => _isRotate; set => _isRotate = value;}
    public float speedX {get => _speedX; set => _speedX = value;}
    public float speedY {get => _speedY; set => _speedY = value;}
    public float speedZ {get => _speedZ; set => _speedZ = value;}

    // Update is called once per frame
    void Update()
    {
        if(_isRotate) transform.Rotate(_speedX * Time.deltaTime, _speedY * Time.deltaTime, _speedZ * Time.deltaTime);
    }
}
