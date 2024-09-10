using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu (fileName = "CharActionSO",  menuName = "Scriptable Objects/Action/Strategy")]
public class StrategySO : CharActionSO
{   
    [SerializeField]
    public TriggerStateDelivery triggerState;

    void OnEnable() {
        _type = ACT_TYPE.STRATEGY;
    }
    
    public override float GetAverageAccuracy()
    {
        throw new System.NotImplementedException();
    }

    public override float GetAverageDamage()
    {
        throw new System.NotImplementedException();
    }

    public override float GetDamageAdjustedByAccuracy()
    {
        throw new System.NotImplementedException();
    }

    public override int GetImpactListCount()
    {
        throw new System.NotImplementedException();
    }

    public override float GetTotalAccuracy()
    {
        throw new System.NotImplementedException();
    }

    public override float GetTotalDamage()
    {
        throw new System.NotImplementedException();
    }
}