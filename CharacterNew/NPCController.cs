using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: Null-ref saftey for the DetectionFields?
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class NPCController : Controller
{

    //We can't return a null vector. But we can make an equivalent null vector

    private Vector3 _targetInput;
    public Vector3 targetInput {
        get {
            if(arrested) return Constants.NullVector;
            return _targetInput;
        }
    }

    // For the enemy to start agressing both need to be true at some point.
    // Agression stops once the character leaves the agrovolume.
    // Its precident that vision < agro
    // All enemies should have a visionField, but they might not have an agroField
    // Then they can resolve to a 'defualt' location: where they spawned.
    public DetectionField visionField;
    public DetectionField agroField;

    public bool playerIsTarget {
        get { 
            if (agroField)
                return visionField.playerInside && agroField.playerInside; 
            else 
                return visionField.playerInside;
        }
    }

    public GameObject playerTarget {
        get {
            //Debug.Log("PlayerTarget is " + visionField.playerRef.name);
            return visionField.playerRef;
        }
    }

    private List<GameObject> targetsInVision;
    private List<GameObject> targetsInAgro;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        if (agroField) {
            _targetInput = agroField.playerLastLocation;
        } else {
            _targetInput = visionField.playerLastLocation;
        }
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        ReadInput();
    }

    //public override void ArrestInput() 

    protected override void ReadInput() {
        if(agroField) {
            if(visionField.playerInside && agroField.playerInside) {
                _targetInput = visionField.playerLastLocation;
            } else {
                _targetInput = agroField.transform.position;
            }
        } else {
            _targetInput = visionField.playerLastLocation;
        }
            
    }


    void AttemptAttackTarget() {
        if((_targetInput - this.transform.position).magnitude < 3) {

        }
    }
}
