using Net.Share;
using Net.UnityComponent;
using System;
using UnityEngine;

public class SyncVarTest : NetworkBehaviour
{
    [SyncVar]
    public SyncVarClass test;
    [SyncVar(authorize = false)]
    public SyncVarClass test1;
    [SyncVar(authorize = false, hook = nameof(OnTest2Value))]
    public SyncVarClass test2;
    [SyncVar]
    public SyncVarClass test3;

    public SyncVariable<SyncVarClass> test4 = new SyncVariable<SyncVarClass>();

    public SyncVariable<int> test5  = new SyncVariable<int>();

    public SyncVariable<string> test6 = new SyncVariable<string>();

    public SyncVariable<Vector2> test7 = new SyncVariable<Vector2>();

    public SyncVariable<Rect> test8 = new SyncVariable<Rect>();

    public void OnTest4Value(int old, int value)
    {
        Debug.Log(value);
    }

    public void OnTest2Value(SyncVarClass old, SyncVarClass value) 
    {
        Debug.Log(value);
    }

    public override void Start()
    {
        base.Start();
        test5.OnValueChanged = OnTest4Value;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            test = new SyncVarClass();
            test1 = new SyncVarClass();
            test2 = new SyncVarClass();
            test5.Value = RandomHelper.Range(0, 1000);
            test4.Value = new SyncVarClass() { value = RandomHelper.Range(0, 1000) };
        }
    }
}

[Serializable]
public class SyncVarClass
{
    public int value;

    public override bool Equals(object obj)
    {
        if (!(obj is SyncVarClass obj1))
            return false;
        return obj1.value == value;
    }

    public override string ToString()
    {
        return $"value={value}";
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}