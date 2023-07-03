using System;
using UnityEngine;

public class AOTHook : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var types = new Type[]
        {
            typeof(Binding.BaseBind<bool>)
        };
    }
}
