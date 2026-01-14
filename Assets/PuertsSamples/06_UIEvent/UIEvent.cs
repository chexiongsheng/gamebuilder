using System;
using UnityEngine;
using Puerts;

public class UIEvent : MonoBehaviour
{
    static ScriptEnv jsEnv;

    void Start()
    {
        if (jsEnv == null)
        {
            jsEnv = new ScriptEnv(new BackendV8());
        }

        var init = jsEnv.ExecuteModule("UIEvent.mjs").Get<Action<MonoBehaviour>>("init");

        if (init != null) init(this);
    }
}
