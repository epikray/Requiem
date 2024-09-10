using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu (fileName = "SkillSO",  menuName = "Scriptable Objects/Action/Skill")]
public class SkillSO : CharActionSO
{   
    [SerializeField]
    public List<ImpactDelivery> payloads;

    public float armamentInfluence; //?

    public ItemID consumedItem;

    void OnEnable() {
        _type = ACT_TYPE.SKILL;
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
