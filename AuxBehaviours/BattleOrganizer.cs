using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OnBattleDataObject {
    public Vector3 positionHit;
    public GameObject gameObjectHit;
    public DetectionField agroField;

    // Would it be cool if we could pre-define what characters are playing. 
    // So battleOrganizer won't need to search for a known quantity
    public List<PCBehaviour> playerBehaviours;
    public List<NPCBehaviour> enemyBehaviours; 

    public OnBattleDataObject(Vector3 _PositionHit, GameObject _GameObjectHit, DetectionField _AgroField, 
        List<PCBehaviour> _playerBehaviours = null, List<NPCBehaviour> _enemyBehaviours = null) 
    {
        positionHit = _PositionHit;
        gameObjectHit = _GameObjectHit;
        agroField = _AgroField;
        playerBehaviours = _playerBehaviours;
        enemyBehaviours = _enemyBehaviours;
    }

}   

// Class to store rewards from battles, etc...
[Serializable]
public class RewardStruct {
    public List<CharActionSO> skillRewards;
    //TODOs
    public List<Item> itemRewards;
    public float growthReward;
    //money lol?

    public RewardStruct(List<CharActionSO> _skillRewards = null, List<Item> _itemRewards = null, float _growthReward = 0) {
        skillRewards = _skillRewards;
        if(skillRewards == null) {
            skillRewards = new List<CharActionSO>();
        }
        itemRewards = _itemRewards;
        if(itemRewards == null) {
            itemRewards = new List<Item>();
        }
        growthReward = _growthReward;
    }

    public void Init() {
        if(skillRewards == null) {
            skillRewards = new List<CharActionSO>();
        }
        if(itemRewards == null) {
            itemRewards = new List<Item>();
        }
    }

    public void SumRewards(RewardStruct otherReward) {
        /*
        if(skillRewards == null) {
            skillRewards = new List<CharActionSO>();
        } 
        */
        skillRewards.AddRange(otherReward.skillRewards);
        /*
        if(itemRewards == null) {
            itemRewards = new List<Item>();
        }
        */
        itemRewards.AddRange(otherReward.itemRewards);
        growthReward += otherReward.growthReward;
    }
}


public class OnResolveBattleDataObject {
    public string data = "deez nutz";
}

public class BattleOrganizer
{
    public bool teleportEnemies = false;

    // Or the runtime set. I think the runtime set is in itself a valuable concept.
    // And we can use the rtSet for a great purpose; tracking enemy allegiances. 
    // WildAnimals, enemies and players. Who are fighting? If a character assigned to the WildAnimals team attacks the player team,
    // then only wildanimals and players will fight. Enemies and wildAnimals could, but won't trigger a fight with each other, 
    // but they will attack each other if the player is involved
    public RuntimeSet enemyTeam;
    public RuntimeSet playerTeam;
    private Vector3 battleCenter;

    public BattleOrganizer(RuntimeSet _playerTeam, RuntimeSet _enemyTeam) {
        playerTeam = _playerTeam;
        enemyTeam = _enemyTeam;
    }

    public void InitBattle(OnBattleDataObject dataObj) {
        //Debug.Log("Init Battle");

        if(enemyTeam.Count > 0 || playerTeam.Count > 0) {
            Debug.LogWarning("Already fixing a fight!");
            return;
        }

        

        Vector3 posHit = dataObj.positionHit;
        Vector3 fieldCenter = dataObj.agroField.transform.position;
        battleCenter = fieldCenter;
        
        List<GameObject> chars = dataObj.agroField.charsInField;

        string charString = "";
        foreach(GameObject G in chars) {
            charString += G.name + ", ";
        }

        foreach(GameObject G in chars) {
            CharDataManager dataManager = G.GetComponent<Behaviour>().GetDataManager();
            if(dataManager.mySet == playerTeam) {
                playerTeam.Add(G);
                dataManager.targeting.SetOpponents(enemyTeam);
            } else if (dataManager.mySet == enemyTeam) {
                enemyTeam.Add(G);
                dataManager.targeting.SetOpponents(playerTeam);
            }
        }
        int i = 0;
        Vector3 playerSide = new Vector3(0f, 0f, -8f);
        foreach(GameObject obj in playerTeam.set) {
            obj.GetComponent<Behaviour>().TrySwitchContext(BEHAVIOUR_CONTEXT.BATTLE);

            obj.GetComponent<Behaviour>().MoveTo(GetBattlePosSimple(battleCenter, i, playerSide, playerTeam.Count), 100);
            
            i++;
        }
        Vector3 enemySide = new Vector3(0f, 0f, 8f);
        i = 0;
        foreach(GameObject obj in enemyTeam.set) {
            obj.GetComponent<Behaviour>().TrySwitchContext(BEHAVIOUR_CONTEXT.BATTLE);

            obj.GetComponent<Behaviour>().MoveTo(GetBattlePosSimple(battleCenter, i, enemySide, enemyTeam.Count), 100);
            
            i++;
        }
    }

    public void ResolveBattle(BattleResolution resolution) {
        switch(resolution) {
            case BattleResolution.VICTORY :
                RewardStruct totalReward = new RewardStruct();


                foreach (GameObject obj in enemyTeam.set) {
                    NPCBehaviour NPCB = obj.GetComponent<NPCBehaviour>();
                    
                    // Gather up reward roll
                    totalReward.SumRewards(NPCB.defeatRewards);
                    NPCB.TrySwitchContext(BEHAVIOUR_CONTEXT.WORLD);
                }
                foreach (GameObject obj in playerTeam.set) {
                    PCBehaviour PCB = obj.GetComponent<PCBehaviour>();

                    PCB.RecieveReward(totalReward);
                    PCB.TrySwitchContext(BEHAVIOUR_CONTEXT.WORLD);
                }
                // Sum reward rolls
                // Add them to Player Inventory


                
                break;
            case BattleResolution.DEFEAT :
                foreach (GameObject obj in enemyTeam.set) {
                    obj.GetComponent<Behaviour>().TrySwitchContext(BEHAVIOUR_CONTEXT.WORLD);
                    //RelieveReward()
                }
                foreach (GameObject obj in playerTeam.set) {
                    obj.GetComponent<Behaviour>().TrySwitchContext(BEHAVIOUR_CONTEXT.WORLD);
                }



                break; 
            case BattleResolution.ESCAPE :
                foreach (GameObject obj in playerTeam.set) {
                    obj.GetComponent<Behaviour>().TrySwitchContext(BEHAVIOUR_CONTEXT.WORLD);
                    //RelieveReward()
                }
                foreach (GameObject obj in enemyTeam.set) {
                    obj.GetComponent<Behaviour>().TrySwitchContext(BEHAVIOUR_CONTEXT.WORLD);
                }


                
                break;
        }

        enemyTeam.Clear();
        playerTeam.Clear();
    }
    Vector3 GetBattlePosSimple(Vector3 center, int pos, Vector3 side, int positions) { 
        Vector3 res = new Vector3(0f, 0f, 0f);
        res = center + side;
        Vector3 tright = Vector3.Cross(side, Vector3.up);

        float interpDenominator = (float)(positions + 1);
        float interpNomirator = (float)(pos + 1);
        // 0< -> <1
        float interpVal = (interpNomirator/interpDenominator);

        res += tright*(1f - 2*interpVal);

        return res;
    }
    Vector3 GetBattlePosV(Vector3 center, int pos, Vector3 side, int positions) {
        Vector3 res = new Vector3(0f, 0f, 0f);
        
        res = center + side;
        Vector3 tright = Vector3.Cross(side, Vector3.up);
        Vector3 tup = side/2f;
        
        float interpDenominator = (float)(positions/2) + 1f;
        float interpNomirator = (float)((pos + 1)/2);

        // odd
        if(positions%2 == 1) {
            /*
                1 => 0 + 1 = 1
                3 => 1 + 1 = 2
                5 => 2 + 1 = 3 
            */
            

            res += tup*(1f - interpNomirator/interpDenominator);
            if(pos%2 == 0) {
                res -= tright*(interpNomirator/interpDenominator);
            } else {
                res += tright*(interpNomirator/interpDenominator);
            }

            //Debug.Log("GetBatlePos for " + center + ", " + pos + ", " + side + ", " + positions + ": Yearn res " + res);

            return res;
        } 
        // even
        //interpDenominator = (float)(positions/2) + 1f;
        interpNomirator = (float)((pos)/2 + 1);

        res += tup*(1f - interpNomirator/interpDenominator);
        if(pos%2 == 0) {
            res -= tright*(interpNomirator/interpDenominator);
        } else {
            res += tright*(interpNomirator/interpDenominator);
        }

        
        return res;
    }
}
