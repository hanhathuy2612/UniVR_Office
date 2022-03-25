using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{

    [SerializeField]
    private GameObject instructionObject = null;


    // Start is called before the first frame update
    void Start()
    {
        if(instructionObject == null){
            instructionObject = GameObject.Find("Instruction");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void controlPanelButton()
    {
        ControlPanel controlPanel = GameObject.Find("ControlPanel").GetComponent<ControlPanel>();
        controlPanel.setExpand(!controlPanel.getExpand());
    }

    public void setInstructionUIEnabled(bool state){
        if(instructionObject != null){
            instructionObject.SetActive(state);
        }
    }
}
