using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Can debatbly be made into a script object.
// If need arises I will do so.
// Will inform Behaviours about actions to be taken

// Does PCController and NPCCOntroller need to be of the same heritage?
// Conceptually it makes sense.
// But one makes major use of input data, and could use some nav agent functionality if needed.
// The other can make no use of input data.


public abstract class Controller : MonoBehaviour
{
    public bool arrested;

    protected void Start() {
        arrested = false;
    } 

    protected void Update() {

    }

    protected abstract void ReadInput();

    public void LookAt(Vector3 target) {
        Vector3 target_ = new Vector3(target.x, this.transform.position.y, target.z);
        this.transform.LookAt(target_, Vector3.up);
    }


    //protected abstract void SendInputToBehaviour();
    
}
