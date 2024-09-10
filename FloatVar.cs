using System;
using UnityEngine;

[Serializable]
public class FloatVar
{
    public float Value;
    public float Init;

    public FloatVar(float _value, float _init) {
        Value = _value;
        Init = _init;
    }

    public FloatVar() {
        Value = 0f;
        Init = 0f;
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

    public void ResetValue() {
        Value = Init;
    }

    //operator overloads
    public static float operator+(float rhs, FloatVar lhs) {
        return rhs + lhs.Value;
    }

    public static float operator*(float rhs, FloatVar lhs) {
        return rhs * lhs.Value;
    }

    public static float operator-(float rhs, FloatVar lhs) {
        return rhs - lhs.Value;
    }

}

[Serializable]
public class IntVar
{
    public int Value;
    public int Init;

    public IntVar(int _value, int _init) {
        Value = _value;
        Init = _init;
    }

    public IntVar() {
        Value = 0;
        Init = 0;
    }

    public void SetValue(int value)
    {
        Value = value;
    }

    public void SetValue(IntVar value)
    {
        Value = value.Value;
    }

    public void SetInit(int value)
    {
        Init = value;
    }

    public void SetInit(IntVar value)
    {
        Init = value.Value;
    }

    public void ApplyChange(int amount)
    {
        Value += amount;
    }

    public void ApplyChange(IntVar amount)
    {
        Value += amount.Value;
    }

    public void ResetValue() {
        Value = Init;
    }
}
