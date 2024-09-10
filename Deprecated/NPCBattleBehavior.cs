using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBattleBehavior : BattleBehavior
{
    // Start is called before the first frame update
    
    float timertobattle;

    new void Start()
    {
        base.Start();
        timertobattle = 0f;
        //actionChannel.transactionDelegate += readTransactions;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

    }

    public void readTransactions() {
        Debug.Log(name + " got a transaction");
    }

    public override void UseAttack(string actionname) {

    }
    public override void UseDefend(string actionname) {
        
    }
    public override void UseSkill(string skillname) {
        
    }
    public override void UseStrategy(string actionname) {
        
    }

    //We can extract this an turn it into an SO for finding over parties
    /*
    void declareBattle(string enemyparty) {
        //PartySO playerparty = PlayerParty;
        
        enemies = (PartySO)Util.Objects.find<PartySO>(enemyparty);

        Debug.Log(enemies.members[0].name);
    }
    */
}
