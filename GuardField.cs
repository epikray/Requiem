using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: GaurdField will be a DetectionField but with added data. The implication is enemies will be gaurding this zone.
// So there will be a reward for defeating enemies in this area. At the moment, new actions.
// The problem to solve here is that I need to be able to give PC Characters new skills, perhaps items, perhaps grow their base states.
// Most important is giving new skills. The rest will prolly follow the pipeline.
// To better fit the previous BattleOrganizer::ResolveBattle function, the reward info needs to be within the enemy.
// NPCBehaviour when Initing battle needs to grab and store their reward from the pool.


// TODOS: NPC Characters needs a reward pool, to get the game working it can just be empty, but it can be added and removed from.
//      NPC Characters rewards needs to be supplied by guardfield when battle is started. Perhaps put back if the battle resolution fails due
//      to player excaping or dieing. 
//      PC Characters needs a way to interface with ID Instance to add skills to their toolkit. Later add other items and growth if I feel the need.
//      
public class GuardField : DetectionField
{
    [SerializeField]
    RewardStruct rewards;
    bool rewardsExtracted;
    

    new void Awake() {
        base.Awake();
        rewards.Init();
        rewardsExtracted = false;
    }
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    public RewardStruct ExtractRewards() {
        RewardStruct reward = new RewardStruct();
        if(rewardsExtracted) {
            return reward;
        }
        rewardsExtracted = true;
        reward.SumRewards(rewards);
        return reward;
    }

    public void InsertRewards() {
        rewardsExtracted = false;
    }

    // TODO: InsertReward that removes this.rewards from another RewardStruct.
}
