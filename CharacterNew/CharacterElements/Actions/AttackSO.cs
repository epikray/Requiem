using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Incorporate armament Influence, but this is dependent on adding armaments.
//      Go other that it is actually correct
//      Add some randomness into the calculations?
// IDEAS: dmg max/min, tied to accuracy. 
//      Status mods? And what to do with effects...
//      

[CreateAssetMenu (fileName = "AttackSO",  menuName = "Scriptable Objects/Action/Attack")]
public class AttackSO : CharActionSO
{   
    [SerializeField]
    public List<ImpactDelivery> payloads;

    public float armamentInfluence; //?

    void OnEnable() {
        _type = ACT_TYPE.ATTACK;
    }

    public override float GetAverageAccuracy()
    {
        float res = 0;
        foreach(ImpactDelivery impact in payloads) {
            res += impact.accuracy;
        }
        return res/(float)payloads.Count;
    }

    public override float GetAverageDamage()
    {
        float res = 0;
        foreach(ImpactDelivery impact in payloads) {
            res += impact.damage;
        }
        return res/(float)payloads.Count;
    }

    public override float GetDamageAdjustedByAccuracy()
    {
        float res = 0;
        float var = 0;
        foreach(ImpactDelivery impact in payloads) {
            var += impact.accuracy;
            res += impact.damage*var;
        }
        return res;
    }

    public override int GetImpactListCount()
    {
        return payloads.Count;
    }

    public override float GetTotalAccuracy()
    {
        float res = 0;
        foreach(ImpactDelivery impact in payloads) {
            res += impact.accuracy;
        }
        return res;
    }

    public override float GetTotalDamage()
    {
        float res = 0;
        foreach(ImpactDelivery impact in payloads) {
            res += impact.damage;
        }
        return res;
    }
    
}
