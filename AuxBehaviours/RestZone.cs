using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    If the Player Character is in the zone =>
    They get a boost to their regen. Later they can be allowed to switch up skills, add perks, etc.

    Downside is it triggers monsters to respawn. And heals any who are hurt.
*/
public class RestZone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other) {
        if(other.tag != "Player") {
            return;
        }
        
        //Debug.Log("TriggerStay: Found " + other.gameObject.tag + ", healing them");
        PCBehaviour PCB = other.gameObject.GetComponent<PCBehaviour>();
        PCB.GetDataManager().cProperties.HurtPool(STAT.HEALTH, -100f*Time.deltaTime, true, true);
    }
}
