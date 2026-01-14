using UnityEngine;
using Puerts;

namespace PuertsTest
{
    public class Callback : MonoBehaviour
    {
        ScriptEnv jsEnv;

        // Use this for initialization
        void Start()
        {
            jsEnv = new ScriptEnv(new BackendV8());

            jsEnv.Eval(@"
                let obj = new CS.PuertsTest.TestClass();
                //如果你后续要remove，需要这样构建一个Delegate，后续可以用该Delegate引用去remove
                let delegate = new CS.PuertsTest.Callback1(o => o.Foo()); 
                obj.AddEventCallback1(delegate);
                obj.AddEventCallback2(i => console.log(i)); //如果不需要remove，直接传函数即可
                obj.Trigger();
                obj.RemoveEventCallback1(delegate);
                obj.Trigger();
            ");
        }

        void OnDestroy()
        {
            jsEnv.Dispose();
        }
    }
}
