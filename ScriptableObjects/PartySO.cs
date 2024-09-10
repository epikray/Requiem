using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PartySO", menuName = "ScriptableObjects/PartyData")]
public class PartySO : ScriptableObject
{
    public List<CharacterDataSO> members;

    private CharacterDataSO inWorldRepresentative;
    
    public bool fighting;
    public bool allDead;

    public void tryAddMember(CharacterDataSO C) {
        members.Add(C);
    }

    public void tryRemoveMember(CharacterDataSO C) {
        members.Remove(C);
    }

    public int GetMemberIndex(CharacterDataSO C) {
        return members.FindIndex(x => string.Equals(x.name, C.name));
    }

    public CharacterDataSO GetMemberByIndex(int index) {
        if(allDead) {
            return null;
        }

        int size = members.Count;
        int i = index%size;
        if(i < 0) {
            i = size - 1;
        }
        //Debug.Log("GetMemberByIndex test: " + i);
        if(members[i].dead) {
            CheckAndSetAllDead();
            if(allDead) {
                Debug.Log("All party members are dead");
                return null;
            }
            
            return GetMemberByIndex(i+1);
        }
        return members[i];
    }

    protected bool CheckAllDead() {
        foreach(CharacterDataSO C in members) {
            if (!C.dead) {
                return false;
            }
        }
        return true;
    }

    public void CheckAndSetAllDead() {

        if(CheckAllDead()) {
            allDead = true;
        } else {
            allDead = false;
        }
    }

    public void SetRepresentative(CharacterDataSO C) {
        inWorldRepresentative = C;
    }

    public void SetRepSpawnInWorld(bool val) {
        inWorldRepresentative.logic.spawnInWorld = val;
    }

}
