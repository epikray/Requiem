using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Interface to communicate with another object

    Communication Chart:
    Anything and everything else
      |         |
      v         v
    CData <-> Character -> BattleBehaviour
                       -> WorldBehaviour

    CData acts a cross scene representation of Character.
    Character is an instance of CData, every Character needs one and only one CData.

*/

public class Character : MonoBehaviour
{
    [SerializeField]
    private WorldBehavior wBehavior;
    [SerializeField]
    private BattleBehavior bBehavior;
    [SerializeField]
    public CharacterDataSO data;

    [SerializeField]
    public LogicianSO logician;

    public bool PlayerControlled; //PC or NPC
    public bool Fighting; //World or Battle


    // Start is called before the first frame update
    void Start()
    {
        //Add a characterController Component
        wBehavior = GetComponent<WorldBehavior>();
        bBehavior = GetComponent<BattleBehavior>();

        OnStart_HandleLogic();

        Debug.Log("Character.Start() : " + data.name);
    }

    // Update is called once per frame
    void Update()
    {
        OnUpdate_HandleLogic();
    }

    public void SetBattle() {
        
        if(!bBehavior.enabled) {
            bool res = bBehavior.tryChangeBehavior();
            if(!res) {
                Debug.Log(name + " failed to change to BattleBehavior");
            }
        }
        
    }

    public void SetWorld() {
        if(!wBehavior.enabled) {
            bool res = wBehavior.tryChangeBehavior();
            if(!res) {
                Debug.Log(name + " failed to change to WorldBehavior");
            }
        }
        
    }

    public CharacterDataSO GetCharacterData() {
        return data;
    }

    public void SetOponents(PartySO oponents) {
        bBehavior.SetOponents(oponents);
        
    }

    public void SetCharacterData(CharacterDataSO CD) {
        data = CD;
    }

    private void OnStart_HandleLogic() {
        //Update toSpawn boolean based on whether the character is dead or not
        if(data.logic.spawnInWorld == false) {
            Destroy(this.gameObject);
        }
        
    }

    private void OnUpdate_HandleLogic() {

    }

    public void DisableThis() {
        wBehavior.enabled = false;
        bBehavior.enabled = true;
        //GetComponent<MeshRenderer>().enabled = false;
    }
}
