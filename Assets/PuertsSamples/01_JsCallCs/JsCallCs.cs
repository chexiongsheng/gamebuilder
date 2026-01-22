using UnityEngine;
using Puerts;

namespace PuertsTest
{
    public class JsCallCs : MonoBehaviour
    {
        ScriptEnv jsEnv;

        void Start()
        {
            jsEnv = new ScriptEnv(new BackendV8());

            jsEnv.Eval(@"
                let gameObject = new CS.UnityEngine.GameObject('testObject');
                CS.UnityEngine.Debug.Log(gameObject.name);
            ");
        }

        void OnDestroy()
        {
            jsEnv.Dispose();
        }
    }
}
