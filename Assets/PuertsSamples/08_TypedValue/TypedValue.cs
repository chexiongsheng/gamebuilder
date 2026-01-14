using UnityEngine;
using Puerts;

namespace PuertsTest
{
    public class TypedValue : MonoBehaviour
    {
        ScriptEnv jsEnv;

        // Use this for initialization
        void Start()
        {
            jsEnv = new ScriptEnv(new BackendV8());

            jsEnv.Eval(@"
                let value = new CS.Puerts.Int64Value(512n);
                CS.PuertsTest.TypedValue.CallSomeFunction(value);
            ");
        }

        void OnDestroy()
        {
            jsEnv.Dispose();
        }

        public static void CallSomeFunction(object o) {
            UnityEngine.Debug.Log("value type:" + o.GetType());
        }
    }
}
