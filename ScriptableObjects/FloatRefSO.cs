using UnityEngine;

[CreateAssetMenu(fileName = "FloatRefSO", menuName = "ScriptableObjects/Variable/FloatRef")]
public class FloatRefSO : ScriptableObject
{
    public FloatVarSO Variable;

    public float Value {
        get { return Variable.Value; }
    }

    public float Init {
        get { return Variable.Init; }
    }
}
