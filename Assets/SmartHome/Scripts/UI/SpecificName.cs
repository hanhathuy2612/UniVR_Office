using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificName : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    [Tooltip("The name that will be displayed on UI")]
    private string specificName = "Unknown";

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public string getName()
    {
        return specificName;
    }
}
