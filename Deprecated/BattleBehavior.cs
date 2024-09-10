using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleBehavior : MonoBehaviour
{
    /*
        Defines what a character can do during battle 
    */

    //Queued action
    

    /*
              party
                ^
                |
        CD1, CD2, CD3, CD4       
         ^    ^    ^    ^
         |    |    |    | 
        BB1, BB2, BB3, BB4
                |
                v
             enemies

    */
    protected CharacterDataSO data; // data->party, 
    //public ActionChannel actionChannel;
    protected PartySO oponents;
    protected CharacterDataSO target;

    //public BattleOrganizer organizer;

    //protected ActionSO queuedAction;

    //[SerializeField]
    //protected AnimActionChannelSO aaChannel;

    protected AnimActionPort aaPort;
    
    //Grace period before proper fighting begins
    //Consider the Unity messaging system
    protected bool readyActive; //
    protected bool actionReady;
    protected float readyMeter; // c [0, 1]

    protected bool hasWB;

    protected int partyInd;

    // Start is called before the first frame update
    protected void Start()
    {
        data = GetComponent<Character>().GetCharacterData();
        target = oponents.members[0];
        
        //hasWB = hasCousinBehavior();
        Debug.Log(this.name + " BattleBehavoir.Start()");
        aaPort = GetComponentInChildren<AnimActionPort>();
        data.setAnimActionPort(aaPort);

        //readyActive = true;
        readyMeter = 0f;
        partyInd = data.GetPartyMemberIndex();
    }

    protected void onEnable() {
        Debug.Log(this + "onEnable()");
        //hasWB = hasCousinBehavior();
    }

    protected void onLoad() {
        Debug.Log(this + " onLoad()");
    }

    // Update is called once per frame
    protected void Update()
    {   
        /*
        if(data.dead) {
            Debug.Log(data.name + " is dead");
        }
        */
        if(readyActive) {
            statcharge();

            if(readyMeter > 1f /*&& queuedAction*/) {
                //queuedAction
            }
        }
    }

    protected void LateUpdate() {
        statclean();

        if(data.dead) {
            Destroy(gameObject);
        }
    }

    protected void SetTarget(CharacterDataSO C) {
        target = C;
    }

    public abstract void UseSkill(string actionname);

    public abstract void UseAttack(string actionname);

    public abstract void UseDefend(string actionname);

    public abstract void UseStrategy(string actionname);

    //performs all necesary charges
    protected void statcharge() {
        readyMeter_charge();
    }

    //clean up all necesary stats, reset to zero what not
    protected void statclean() {
        readyMeter_clean();
    }

    public CharacterDataSO GetFirstLivingEnemy() {
        CharacterDataSO C = oponents.members[0];
        if(C.dead) {
            return GetNextEnemyCharacter();
        }
        return C;
    }

    public bool TargetIsInOwnParty() {
        if(target.GetParty() != oponents) {
            return true;
        }
        return false;
    }

    public CharacterDataSO GetFirstLivingFriend() {
        CharacterDataSO C = data.GetParty().members[0];
        if(C.dead) {
            return GetNextPartyCharacter();
        }
        return C;
    }

    public CharacterDataSO GetNextPartyCharacter() {
        
        int i = data.GetParty().GetMemberIndex(data);
        CharacterDataSO C = data.GetParty().GetMemberByIndex(i+1);
        if(C == null) {
            return data;
        }
        return C;
    }

    public CharacterDataSO GetPrevPartyCharacter() {
        int i = data.GetParty().GetMemberIndex(data);
        CharacterDataSO C = data.GetParty().GetMemberByIndex(i-1);
        if(C == null) {
            return data;
        }

        return C;
    }
    
    public CharacterDataSO GetNextEnemyCharacter() {
        int i = oponents.GetMemberIndex(target);
        CharacterDataSO C = oponents.GetMemberByIndex(i+1);
        if(C == null) {
            return data;
        }
        return C;
    }

    
    public CharacterDataSO GetPrevEnemyCharacter() {
        int i = oponents.GetMemberIndex(target);
        CharacterDataSO C = oponents.GetMemberByIndex(i-1);
        if(C == null) {
            return data;
        }
        return C;
    }

    protected void readyMeter_charge() {
        if(!readyActive) {
            return;
         }
         if(readyMeter < 1f) {
            float readyChargeC = data.getReadyChargeC(); // derived from dexterity
            readyMeter += readyChargeC * Time.deltaTime;
         } 
    }

    protected void readyMeter_clean() {
        if(readyMeter > 1f) {
            readyMeter = 0f;
        }
    }

    public void SetOponents(PartySO oponents) {
        this.oponents = oponents;
    }

    //exchanges/activates itself [the script] for a WorldBehavior instead
    //public abstract void LeaveBattle();

    public bool tryChangeBehavior() {
        if(hasCousinBehavior()) {
            gameObject.GetComponent<WorldBehavior>().enabled = true;
            this.enabled = false;
            return true;
        } else {
            Debug.Log(this + " could not find an attached WorldBehavior");
            return false;
        }
    }

    private bool hasCousinBehavior() {
        if(gameObject.GetComponent<WorldBehavior>()) {
            return true;
        } else {
            return false;
        }
    }

    //public abstract void Idle();
}
