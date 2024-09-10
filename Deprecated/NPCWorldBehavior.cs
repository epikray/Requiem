
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCWorldBehavior : WorldBehavior
{
    // Start is called before the first frame update

    [SerializeField]
    private Transform defaultPos;

    //private GuardZone guardZone;

    [SerializeField]
    private VoidEventChannelSO startBattle;
    [SerializeField]
    private float triggerOnBattleDistance;
    [SerializeField]
    private bool chasePlayer;

    private Transform moveTo; 
    private GameObject target; //idK... if we have the object, we have the transform. we only need the tranform, hell, we only need the position

    private NavMeshAgent navMeshComp;

    [SerializeField]
    private List<CharacterDataSO> enemyGroup;

    

    public bool randomEnemyGroup;

    private bool touchedPlayer;

    [SerializeField]
    public bool defeated;
    
    void onAwake() {
        
    }

    new void Start()
    {
        /*
        if(defeated) {
            Destroy(this.gameObject);
        }
        */
        base.Start();
        touchedPlayer = false;
        navMeshComp = GetComponent<NavMeshAgent>();

        if(enemyGroup.Count == 0) {
            randomEnemyGroup = true;
        }
        /*
        CharacterDataSO cdata = (CharacterDataSO)ScriptableObject.Instantiate(data);
        Debug.Log("cdata.name " + cdata.name);
        */
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        Move();

        attemptEnterBattleOnClosePlayerTarget();
    }

    public override void Move() {
        //Debug.Log("A Move() function with no arguments? Where am I supposed to move to!?");
        //moveTo! MOVE() M O V E S T O 
        //Giving things good names is an art
        if(!chasePlayer) {
            return;
        }

        navMeshComp.SetDestination(moveTo.position);
    }

    void attemptEnterBattleOnClosePlayerTarget() {
        //Debug.Log("Attempt Enter Battle");
        if(touchedPlayer) {
            return;
        }
        //Debug.Log("We havent touched him before");
        if(!((moveTo.position - transform.position).magnitude < triggerOnBattleDistance)) {
            //touchedPlayer = false;
            return;
        }
        //Debug.Log("He is within trigger distance");
        if(!(target.tag == "Player")) {
            //touchedPlayer = false;
            return;
        }
        //Debug.Log("Target is a player");
        touchedPlayer = true;

        Debug.Log(this.name + " wants to fight " + target.name);
        startBattle.RaiseEvent(this.gameObject);
        //Director can change scenes, battleOrganizers duty is to connect NPCDatas to enemy party
        
        
    }

    public List<CharacterDataSO> GetEnemyGroup() {
        return enemyGroup;
    }

    public override void EnterBattle() {

    }

    //Unless Player is a rigidbody as well, this doesnt really work
    void OnCollisionEnter(Collision other) {
        Debug.Log("Collision");
        if(other.gameObject.tag == "Player") {
            //Debug.Log(name + " wants to init battle against " + other.gameObject.name);
            //startBattle.RaiseEvent();
        }
    }

    public void SetTarget(GameObject _target) {
        target = _target;
        moveTo = target.transform;
    }

    
}
