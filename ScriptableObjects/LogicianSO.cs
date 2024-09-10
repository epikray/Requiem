using System.Collections;
using System.Collections.Generic;
using UnityEngine;





/*
    Whats the point of this class?
    To solve the 'previous state logic' problem in Director
    As far as I can see, we have no good solution for keeping track of what we want to change across scene changes
    The intention here is to have a list (most probably in then form List<Pair<Character, Logic>>) that connects characters to whatever
*/


[CreateAssetMenu(fileName = "LogicianSO", menuName = "ScriptableObjects/LogicianSO")]
public class LogicianSO : ScriptableObject
{

    public Logic[] CharacterManager = new Logic[500];

    //public V


}

[System.Serializable]
public struct Logic {
    public bool spawnInWorld;
    
    public Vector3 spawnLocation;

    public float val;
}
