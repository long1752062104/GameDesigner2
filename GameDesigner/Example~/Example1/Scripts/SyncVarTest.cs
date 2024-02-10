using Net.Share;
using Net.UnityComponent;
using System;
using System.Collections.Generic;
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

    public SyncVariable<SyncVarClass> test4 = new SyncVariable<SyncVarClass>(new SyncVarClass());

    public SyncVariable<int> test5 = new SyncVariable<int>();

    public SyncVariable<string> test6 = new SyncVariable<string>();

    public SyncVariable<Vector2> test7 = new SyncVariable<Vector2>();

    public SyncVariable<Rect> test8 = new SyncVariable<Rect>();

    public SyncVariable<List<int>> test9 = new SyncVariable<List<int>>();

    [SyncVar(authorize = false)]
    public SyncVarClass[] test10 = new SyncVarClass[0];

    [SyncVar(authorize = false)]
    public List<SyncVarClass> test11 = new List<SyncVarClass>();

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
        test4.Value.value2.OnValueChanged = OnTest4Value;
        netObj.AddSyncVar(this, test4.Value);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //test = new SyncVarClass();
            //test1 = new SyncVarClass();
            //test2 = new SyncVarClass();
            //test5.Value = RandomHelper.Range(0, 1000);
            //test4.Value = new SyncVarClass() { value = RandomHelper.Range(0, 1000) };
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            test4.Value.value2.Value = RandomHelper.Range(0, 1000);
        }
    }
}

[Serializable]
public class SyncVarClass
{
    public int value;
    [Net.Serialize.NonSerialized] //过滤字段不进行序列化和判断
    public int value1;
    [Net.Serialize.NonSerialized] //过滤字段不进行序列化和判断
    public SyncVariable<int> value2 = new SyncVariable<int>();

    public int[] value3;
    public SyncVarClass[] value4;

    public int value44 = 5;

    public SyncVarClass value5;

    [SyncVar]
    public Rect value6;

    public override string ToString()
    {
        return $"value={value}";
    }
}