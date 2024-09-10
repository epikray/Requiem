using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Vec3SO toFollow;
    private Vector3 centering;
    [SerializeField]
    private Transform controlPoint1;
    [SerializeField]
    private Transform controlPoint2;
    private Vector3 slider;
    public Vector3 stiffness;
    
    public bool still;

    public Vector3 rotation;
    private Vector3 delVec; 

    // Start is called before the first frame update
    void Start()
    {
        slider = (controlPoint2.position - controlPoint1.position);
        centering = (controlPoint1.position + controlPoint2.position)* 0.5f;
        transform.eulerAngles = rotation;
        MoveTo(centering);
        delVec = toFollow.get();

    }

    // Update is called once per frame
    void Update()
    {
        delVec = toFollow.get() - delVec;
        
        if(!still)
            FollowAlongCP();
        
        LookAt_toFollow();
    }

    void StandAt(Vector3 pos) {

    }

    void MoveTo(Vector3 v) {
        transform.Translate(v - transform.position);
    }

    void Follow() {
        Vector3 v = toFollow.get() - transform.position;

        transform.Translate(v);
    }

    void FollowAlongCP() {
        //The appropriate pos is the one where the vector from pos to the target 'toFollow'
        //is perpendicular to the 'slider' vector
        //this hueristic only works for lines tho

        //project
        Vector3 toProject = toFollow.get() - controlPoint2.position;
        
        Vector3 projection = Vector3.Project(toProject, slider);

        transform.Translate((projection + controlPoint2.position) - transform.position);
    }
    

    void LookAt_toFollow() {
        transform.LookAt(toFollow.get());
    }

    void moveAndLook() {
        Vector3 newpos = toFollow.get();
        newpos += new Vector3(0, 5, -15);
        transform.position = newpos;
        
    }

}
