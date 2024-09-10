using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCController : Controller
{
    [Header("Interaction 'Buttons'")]
    public KeyCode confirm;
    public KeyCode cancel;
    [Header("Selection 'D-Pad'")]
    public KeyCode selectUp;
    public KeyCode selectDown;
    public KeyCode selectLeft;
    public KeyCode selectRight;
    [Header("Targetting 'Bumpers'")]
    public KeyCode targetRight;
    public KeyCode targetLeft;

    GameObject mainCameraObject;
    
    Vector3 forwardDir;
    Vector3 rightDir;
    Vector3 _moveInput;
    public Vector3 moveInput {
        get {
            if(arrested) return Constants.NullVector;
            return _moveInput;
        }
    }

    /* --- INTERACTION --- */
    bool _confirmInput;
    public bool confirmInput {
        get {
            return Input.GetKey(confirm);
        }
    }
    bool _confirmPressed;
    public bool confirmPressed {
        get {
            return Input.GetKeyDown(confirm);
        }
    }

    bool _cancelInput;
    public bool cancelInput {
        get {
            return Input.GetKey(cancel);
        }
    }
    bool _cancelPressed;
    public bool cancelPressed {
        get {
            return Input.GetKeyDown(cancel);
        }
    }

    /* --- TARGETTING --- */
    bool _targetRightInput;
    public bool targetRightInput {
        get {
            return Input.GetKey(targetRight);
        }
    }
    bool _targetRightPressed;
    public bool targetRightPressed {
        get {
            return Input.GetKeyDown(targetRight);
        }
    }

    bool _targetLeftInput;
    public bool targetLeftInput {
        get {
            return Input.GetKey(targetLeft);
        }
    }
    bool _targetLeftPressed;
    public bool targetLeftPressed {
        get {
            return Input.GetKeyDown(targetLeft);
        }
    }

    /* --- SELECTION ---*/
    public bool selectUpInput {
        get {
            return Input.GetKey(selectUp);
        }
    }
    public bool selectUpPressed {
        get {
            return Input.GetKeyDown(selectUp);
        }
    }
    public bool selectDownInput {
        get {
            return Input.GetKey(selectDown);
        }
    }
    public bool selectDownPressed {
        get {
            return Input.GetKeyDown(selectDown);
        }
    }
    public bool selectRightInput {
        get {
            return Input.GetKey(selectRight);
        }
    }
    public bool selectRightPressed {
        get {
            return Input.GetKeyDown(selectRight);
        }
    }
    public bool selectLeftInput {
        get {
            return Input.GetKey(selectLeft);
        }
    }
    public bool selectLeftPressed {
        get {
            return Input.GetKeyDown(selectLeft);
        }
    }

    // Start is called before the first frame update
    void Awake() {
        
    }
    new void Start()
    {
        base.Start();
        mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        _moveInput = new Vector3(0,0,0);
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        forwardDir = mainCameraObject.transform.forward;
        forwardDir.y = 0;
        forwardDir.Normalize();
        rightDir = Vector3.Cross(Vector3.up, forwardDir);

        ReadInput();
    }

    // This is still good to have! For simple single button interactions its redundant, but this can be useful to code more
    // advanced interactions. Dubbel clicking, two button presses, more nuanced direction reading.
    protected override void ReadInput() { 
        
        _moveInput = forwardDir*Input.GetAxis("Vertical") + rightDir*Input.GetAxis("Horizontal");

        _confirmInput = Input.GetKey(confirm);
        _confirmPressed = Input.GetKeyDown(confirm);

        _cancelInput = Input.GetKey(cancel);
        _cancelPressed = Input.GetKeyDown(cancel);

        _targetRightInput = Input.GetKey(targetRight);
        _targetRightPressed = Input.GetKeyDown(targetRight);

        _targetLeftInput = Input.GetKey(targetLeft);
        _targetLeftPressed = Input.GetKeyDown(targetLeft);

        
    }

    //protected override void SendInputToBehaviour(){ }

}
