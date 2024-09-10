using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Finer precision automated camera precision is necasary
//       Control needs to use info for target size to better fit the whole target on screen.
public class CameraMovement : MonoBehaviour
{
    GameObject playerObject;
    Behaviour owner;
    Transform posTarget;
    Transform rotTarget;
    // Offset is
    
    public Vector3 offsetDir_combat;
    public float offsetDist_combat;
    public Vector3 offsetDir;
    public float offsetDist;
    
    public bool usingCombatOffset;
    public bool offset_is_relative;

    public float translationSpeed;
    public float tugMagnitude;
    public float rotationSpeed;
    
    void Awake() {
        offsetDir = Vector3.Normalize(offsetDir);
        offsetDir_combat = Vector3.Normalize(offsetDir);
        usingCombatOffset = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        owner = playerObject.GetComponent<Behaviour>();

    }

    // Update is called once per frame
    void Update()
    {
        // When the player is fighting, there are two selections that are being made.
        // What action to do, and what character to do that action to.
        // I would like to communicate with the camera what selections have been and need to be made.
        // What the camera points at communicates selection. Confirmation marker needs to be visible at all times.
        // Position indicates whever an action needs to be selected.
        // 
        // And in the future we will definitely need some sort of ownership of the camera. 
        // When I switch players to act, the camera context will switch aswell

        Transform ownerCurTarget = owner.GetDataManager().targeting.GetTarget(true).gameObject.transform;
        bool ownerHasActionSelected = owner.GetDataManager().aSelector.HasSelection();

        posTarget = playerObject.transform;
        rotTarget = ownerCurTarget;

        Vector3 posOffset = offsetDir*offsetDist;
        Vector3 rotOffset = rotTarget.forward;
        float speedMod = 1f;
        if(usingCombatOffset) {
            speedMod = 10f;
            posOffset = offsetDir_combat*offsetDist_combat;
            rotOffset = new Vector3(0f, 0f, 0f);
        }
        if(ownerHasActionSelected) {
            posOffset *= 2f;
        }


        PositionCamera(posOffset, speedMod);
        RotateCamera(rotOffset, speedMod);
    }

    void PositionCamera(Vector3 offset, float speedMod) {
        
        Vector3 posVector = posTarget.position;
        /*
        if(offset_is_relative) {
            // rotate posTarget around the y_axis at the point target.transform.position
            // so that 
            float angles = Vector3.SignedAngle(offset, -posTarget.forward, Vector3.up);

            Vector3 newOffset = new Vector3(
                offset.x*Mathf.Cos(angles) - offset.z*Mathf.Sin(angles),
                offset.y,
                offset.x*Mathf.Sin(angles) + offset.z*Mathf.Cos(angles)
            );

            posVector = posVector + newOffset;
            Debug.Log(
                "offset = " + offset + "\n"
                + "-targe.forward = " + (-posTarget.forward) + "\n"
                + "angles = " + angles);
        } else {
            
        }
        */
        posVector += offset;        

        float tug = (posVector - transform.position).magnitude*tugMagnitude;
        if(tug < translationSpeed) tug = translationSpeed;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            posVector, 
            (tug + translationSpeed*speedMod)*Time.deltaTime
        );
    }

    void RotateCamera(Vector3 offset, float speedMod) {
        Vector3 rotVector = (rotTarget.position + offset) - transform.position;
        
        transform.rotation = Quaternion.LookRotation(
            Vector3.RotateTowards(
                transform.forward, 
                rotVector, 
                rotationSpeed*speedMod*Time.deltaTime, 
                0.0f
            )
        );
    }
}
