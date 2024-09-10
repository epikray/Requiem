using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleResolution : int{
    VICTORY,
    DEFEAT,
    ESCAPE
}

public class Director : MonoBehaviour
{
    public KeyCode forceEndBattle;
    //
    public Transform[] monsterSpawns;

    public RuntimeSet playerTeam;
    public RuntimeSet enemyTeam;

    public VoidEventChannelSO eventInitBattle;
    public VoidEventChannelSO eventBeginBattle;
    
    public VoidEventChannelSO eventEscapeBattle;
    public VoidEventChannelSO eventBattleDefeat;
    public VoidEventChannelSO eventBattleVictory;

    BattleOrganizer battleOrganizer;
    // Start is called before the first frame update
    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
        battleOrganizer = new BattleOrganizer(playerTeam, enemyTeam);
        
    }

    void Start()
    {
        eventInitBattle.OnEventRaised += SetUpBattle;
        eventBeginBattle.OnEventRaised += BeginBattle;;
        eventEscapeBattle.OnEventRaised += ResolveBattleEscape;
        eventBattleDefeat.OnEventRaised += ResolveBattleDefeat;
        eventBattleVictory.OnEventRaised += ResolveBattleVictory;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(forceEndBattle))
            eventEscapeBattle.RaiseEvent();

        if(enemyTeam != null && enemyTeam.Count > 0) {
            bool victory = true;
            foreach(GameObject G in enemyTeam.set) {
                victory = G.GetComponent<Behaviour>().GetDataManager().lifeState != LIFE_STATE.ALIVE;
                if(!victory) break;
            }
            if(victory) {
                eventBattleVictory.RaiseEvent();
            } 
        }
        if(playerTeam != null && playerTeam.Count > 0) {
            bool defeat = true;
            foreach(GameObject G in playerTeam.set) {
                defeat = G.GetComponent<Behaviour>().GetDataManager().lifeState != LIFE_STATE.ALIVE;
                if(!defeat) break;
            }
            if(defeat) eventBattleDefeat.RaiseEvent();
        }
    }

    void SetUpBattle() {
        
        
        // Use Enemy runtimeSet as a list of all enemies to fight

        GameObject caller = eventInitBattle.caller;
        OnBattleDataObject onBattleDataObject = (OnBattleDataObject)eventInitBattle.data;
        battleOrganizer.InitBattle(onBattleDataObject);



    }

    void BeginBattle() {
        // for all objs in each runtimeset, set them to battle battleOrganizer
    }

    void ResolveBattleEscape() {
        GameObject caller = eventEscapeBattle.caller;
        OnResolveBattleDataObject onResolveBattleDataObject = (OnResolveBattleDataObject)eventEscapeBattle.data;
        battleOrganizer.ResolveBattle(BattleResolution.ESCAPE);
    }

    void ResolveBattleDefeat() {
        battleOrganizer.ResolveBattle(BattleResolution.DEFEAT);
    }

    void ResolveBattleVictory() {
        battleOrganizer.ResolveBattle(BattleResolution.VICTORY);
    }

}
