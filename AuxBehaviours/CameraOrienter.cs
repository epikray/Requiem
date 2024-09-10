using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrienter : MonoBehaviour
{
    public Transform directionStart;
    public Transform directionEnd;
    Vector3 direction;
    public float distance;
    public CameraMovement cMovement;
    BoxCollider boxCollider;

    void Awake() {
        boxCollider = GetComponent<BoxCollider>();

        direction = Vector3.Normalize(directionStart.position - directionEnd.position);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag != "Player") {
            return;
        }

        cMovement.offsetDir = direction;
        cMovement.offsetDist = distance;
    }
}
