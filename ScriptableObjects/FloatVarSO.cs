// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using UnityEngine;

[CreateAssetMenu(fileName = "FloatVar", menuName = "ScriptableObjects/Variable/FloatVar")]
public class FloatVarSO : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline]
    public string DeveloperDescription = "";
#endif
    public float Value;
    public float Init;
    [SerializeField]
    private bool consistent;

    public void OnEnable() {
        if(consistent) { Value = Init; }
    }

    public void SetValue(float value)
    {
        Value = value;
    }

    public void SetValue(FloatVar value)
    {
        Value = value.Value;
    }

    public void SetInit(float value)
    {
        Init = value;
    }

    public void SetInit(FloatVar value)
    {
        Init = value.Value;
    }

    public void ApplyChange(float amount)
    {
        Value += amount;
    }

    public void ApplyChange(FloatVar amount)
    {
        Value += amount.Value;
    }
}
