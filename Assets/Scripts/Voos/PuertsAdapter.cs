using System;
using System.Collections.Generic;
using UnityEngine;
using Puerts;

namespace Voos
{
  /// <summary>
  /// Puerts适配器，提供与V8InUnity.Native兼容的接口
  /// </summary>
  public class PuertsAdapter
  {
    private PuertsScriptEngine scriptEngine;
    private Dictionary<string, BrainContext> brainContexts = new Dictionary<string, BrainContext>();
    private VoosEngine voosEngine;

    // 委托缓存，减少GC压力
    private Func<object, string> cachedJsonStringifyFunc;
    private Func<string, object> cachedJsonParseFunc;
    private Action<string, object> cachedSetGlobalFunc;

    // 性能监控
    private int updateAgentCallCount = 0;
    private float totalUpdateAgentTime = 0f;
    private System.Diagnostics.Stopwatch performanceTimer;

    /// <summary>
    /// Brain上下文，存储每个Brain的JS函数引用
    /// </summary>
    private class BrainContext
    {
      public string brainUid;
      public string javascript;
      public Action<object, object> updateAgentFunc;
      public Func<object> postMessageFlushFunc;
      public Dictionary<string, bool> compiledModules = new Dictionary<string, bool>();
    }

    public PuertsAdapter(VoosEngine engine)
    {
      this.voosEngine = engine;
      this.scriptEngine = PuertsScriptEngine.Instance;

      if (!scriptEngine.IsInitialized)
      {
        scriptEngine.Initialize();
      }

      // 初始化性能监控
      performanceTimer = new System.Diagnostics.Stopwatch();

      // 初始化委托缓存
      InitializeDelegateCache();
    }

    /// <summary>
    /// 初始化委托缓存
    /// </summary>
    private void InitializeDelegateCache()
    {
      try
      {
        cachedJsonStringifyFunc = scriptEngine.Eval<Func<object, string>>("(obj) => JSON.stringify(obj)");
        cachedJsonParseFunc = scriptEngine.Eval<Func<string, object>>("(json) => JSON.parse(json)");
        cachedSetGlobalFunc = scriptEngine.Eval<Action<string, object>>("(k, v) => globalThis[k] = v;");
        Debug.Log("[PuertsAdapter] Delegate cache initialized");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsAdapter] Failed to initialize delegate cache: {ex.Message}");
      }
    }

    /// <summary>
    /// 注册回调到JS环境
    /// </summary>
    public void RegisterCallbacks(
      PuertsCallbacks.GetActorBooleanDelegate getActorBoolean,
      PuertsCallbacks.SetActorBooleanDelegate setActorBoolean,
      PuertsCallbacks.GetActorFloatDelegate getActorFloat,
      PuertsCallbacks.SetActorFloatDelegate setActorFloat,
      PuertsCallbacks.GetActorVector3Delegate getActorVector3,
      PuertsCallbacks.SetActorVector3Delegate setActorVector3,
      PuertsCallbacks.GetActorQuaternionDelegate getActorQuaternion,
      PuertsCallbacks.SetActorQuaternionDelegate setActorQuaternion,
      PuertsCallbacks.GetActorStringDelegate getActorString,
      PuertsCallbacks.SetActorStringDelegate setActorString,
      PuertsCallbacks.CallServiceDelegate callService,
      PuertsCallbacks.HandleErrorDelegate handleError,
      PuertsCallbacks.HandleLogDelegate handleLog)
    {
      scriptEngine.RegisterCallbacks(
        getActorBoolean, setActorBoolean,
        getActorFloat, setActorFloat,
        getActorVector3, setActorVector3,
        getActorQuaternion, setActorQuaternion,
        getActorString, setActorString,
        callService, handleError, handleLog);
    }

    /// <summary>
    /// 重置Brain（编译主脚本）
    /// </summary>
    public bool ResetBrain(string brainUid, string javascript)
    {
      try
      {
        Debug.Log($"[PuertsAdapter] ResetBrain for {brainUid}");

        // 创建或更新Brain上下文
        if (!brainContexts.ContainsKey(brainUid))
        {
          brainContexts[brainUid] = new BrainContext { brainUid = brainUid };
        }

        var context = brainContexts[brainUid];
        context.javascript = javascript;

        // 编译脚本
        scriptEngine.Eval(javascript, $"brain_{brainUid}");

        // 获取updateAgent函数引用
        try
        {
          context.updateAgentFunc = scriptEngine.Eval<Action<object, object>>("globalThis.updateAgent");
          if (context.updateAgentFunc == null)
          {
            Debug.LogError($"[PuertsAdapter] updateAgent function not found in brain {brainUid}");
            return false;
          }
        }
        catch (Exception ex)
        {
          Debug.LogError($"[PuertsAdapter] Failed to get updateAgent function: {ex.Message}");
          return false;
        }

        // 获取postMessageFlush函数引用（可选）
        try
        {
          context.postMessageFlushFunc = scriptEngine.Eval<Func<object>>("globalThis.postMessageFlush");
        }
        catch
        {
          // postMessageFlush是可选的
          context.postMessageFlushFunc = null;
        }

        Debug.Log($"[PuertsAdapter] Brain {brainUid} reset successfully");
        return true;
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsAdapter] Failed to reset brain {brainUid}: {ex.Message}\n{ex.StackTrace}");
        return false;
      }
    }

    /// <summary>
    /// 设置模块（编译ES6模块）
    /// </summary>
    public bool SetModule(string brainUid, string moduleKey, string javascript, Action<string> handleCompileError = null)
    {
      try
      {
        Debug.Log($"[PuertsAdapter] SetModule {moduleKey} for brain {brainUid}");

        if (!brainContexts.ContainsKey(brainUid))
        {
          Debug.LogError($"[PuertsAdapter] Brain {brainUid} not found. Call ResetBrain first.");
          return false;
        }

        var context = brainContexts[brainUid];

        // 使用Puerts的模块系统
        // 注意：这里需要将模块注册到全局的getVoosModule函数中
        string moduleWrapper = $@"
(function() {{
  if (typeof globalThis.__voosModules === 'undefined') {{
    globalThis.__voosModules = {{}};
  }}
  
  // 创建模块作用域
  const module = {{ exports: {{}} }};
  const exports = module.exports;
  
  // 执行模块代码
  {javascript}
  
  // 注册模块
  globalThis.__voosModules['{moduleKey}'] = module.exports;
  
  // 提供getVoosModule函数
  if (typeof globalThis.getVoosModule === 'undefined') {{
    globalThis.getVoosModule = function(moduleName) {{
      if (!globalThis.__voosModules[moduleName]) {{
        throw new Error('Module not found: ' + moduleName);
      }}
      return globalThis.__voosModules[moduleName];
    }};
  }}
}})();
";

        scriptEngine.Eval(moduleWrapper, $"module_{moduleKey}");
        context.compiledModules[moduleKey] = true;

        Debug.Log($"[PuertsAdapter] Module {moduleKey} compiled successfully");
        return true;
      }
      catch (Exception ex)
      {
        string errorMsg = $"Failed to compile module {moduleKey}: {ex.Message}";
        Debug.LogError($"[PuertsAdapter] {errorMsg}\n{ex.StackTrace}");
        handleCompileError?.Invoke(errorMsg);
        return false;
      }
    }

    /// <summary>
    /// 更新Agent（调用updateAgent函数）
    /// </summary>
    public bool UpdateAgent(string brainUid, string agentUid, string inputJson, out string outputJson)
    {
      outputJson = null;

      if (PuertsScriptEngine.EnablePerformanceMonitoring)
      {
        performanceTimer.Restart();
      }

      try
      {
        if (!brainContexts.ContainsKey(brainUid))
        {
          Debug.LogError($"[PuertsAdapter] Brain {brainUid} not found");
          return false;
        }

        var context = brainContexts[brainUid];
        if (context.updateAgentFunc == null)
        {
          Debug.LogError($"[PuertsAdapter] updateAgent function not available for brain {brainUid}");
          return false;
        }

        // 使用缓存的JSON解析函数
        object stateObj;
        if (cachedJsonParseFunc != null)
        {
          stateObj = cachedJsonParseFunc(inputJson);
        }
        else
        {
          stateObj = scriptEngine.Eval<object>($"({inputJson})");
        }
    
        // 调用updateAgent函数（第二个参数为null，表示没有arrayBuffer）
        context.updateAgentFunc(stateObj, null);
    
        // 使用缓存的JSON序列化函数
        if (cachedJsonStringifyFunc != null)
        {
          outputJson = cachedJsonStringifyFunc(stateObj);
        }
        else
        {
          var jsonStringifyFunc = scriptEngine.Eval<Func<object, string>>("(obj) => JSON.stringify(obj)");
          outputJson = jsonStringifyFunc(stateObj);
        }

        // 调用postMessageFlush（如果存在）
        if (context.postMessageFlushFunc != null)
        {
          try
          {
            context.postMessageFlushFunc();
          }
          catch (Exception ex)
          {
            Debug.LogWarning($"[PuertsAdapter] postMessageFlush error: {ex.Message}");
          }
        }

        updateAgentCallCount++;

        if (PuertsScriptEngine.EnablePerformanceMonitoring)
        {
          performanceTimer.Stop();
          float elapsed = (float)performanceTimer.Elapsed.TotalMilliseconds;
          totalUpdateAgentTime += elapsed;

          if (updateAgentCallCount % 100 == 0)
          {
            Debug.Log($"[PuertsAdapter] UpdateAgent avg time: {totalUpdateAgentTime / updateAgentCallCount:F2}ms ({updateAgentCallCount} calls)");
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsAdapter] UpdateAgent error for brain {brainUid}, agent {agentUid}: {ex.Message}\n{ex.StackTrace}");
        return false;
      }
    }

    /// <summary>
    /// 检查模块是否已编译
    /// </summary>
    public bool HasModuleCompiled(string brainUid, string moduleKey)
    {
      if (!brainContexts.ContainsKey(brainUid))
      {
        return false;
      }
      return brainContexts[brainUid].compiledModules.ContainsKey(moduleKey);
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    public void Tick()
    {
      scriptEngine.Tick();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
      brainContexts.Clear();
      cachedJsonStringifyFunc = null;
      cachedJsonParseFunc = null;
      cachedSetGlobalFunc = null;
      // 注意：不要在这里Dispose scriptEngine，因为它是单例
    }

    /// <summary>
    /// 获取性能统计
    /// </summary>
    public string GetPerformanceStats()
    {
      if (updateAgentCallCount == 0)
      {
        return "No UpdateAgent calls yet";
      }

      float avgTime = totalUpdateAgentTime / updateAgentCallCount;
      return $"UpdateAgent calls: {updateAgentCallCount}, Avg time: {avgTime:F2}ms, Total time: {totalUpdateAgentTime:F2}ms";
    }

    /// <summary>
    /// 更新Agent（带字节数组）
    /// </summary>
    public bool UpdateAgentWithBytes(string brainUid, string agentUid, string json, byte[] bytes)
    {
      if (!brainContexts.TryGetValue(brainUid, out BrainContext context))
      {
        Debug.LogError($"[PuertsAdapter] Brain not found: {brainUid}");
        return false;
      }

      if (context.updateAgentFunc == null)
      {
        Debug.LogError($"[PuertsAdapter] updateAgent function not found for brain: {brainUid}");
        return false;
      }

      try
      {
        // 将byte[]转换为Puerts.ArrayBuffer
        var arrayBuffer = new Puerts.ArrayBuffer(bytes);

        // 调用updateAgent(json, arrayBuffer)
        // 注意：updateAgent函数签名应该是 function updateAgent(state, buffer)
        context.updateAgentFunc(json, arrayBuffer);

        return true;
      }
      catch (Exception e)
      {
        Debug.LogError($"[PuertsAdapter] UpdateAgentWithBytes failed: {e.Message}\n{e.StackTrace}");
        return false;
      }
    }

    /// <summary>
    /// 更新Agent（泛型版本，带字节数组）
    /// </summary>
    public Util.Maybe<TResponse> UpdateAgent<TRequest, TResponse>(
      string brainUid, string agentUid, TRequest input, byte[] bytes)
    {
      // 序列化请求
      string inputJson = JsonUtility.ToJson(input, false);

      // 调用带字节数组的版本
      if (!UpdateAgentWithBytes(brainUid, agentUid, inputJson, bytes))
      {
        return Util.Maybe<TResponse>.CreateEmpty();
      }

      // 获取响应（从updateAgent的返回值）
      if (!brainContexts.TryGetValue(brainUid, out BrainContext context))
      {
        return Util.Maybe<TResponse>.CreateEmpty();
      }

      try
      {
        // 调用JSON.stringify获取结果
        if (cachedJsonStringifyFunc == null)
        {
          cachedJsonStringifyFunc = scriptEngine.Eval<Func<object, string>>("JSON.stringify");
        }

        // updateAgent应该返回结果对象
        // 但当前实现中updateAgent没有返回值，需要修改
        // 暂时返回空结果
        Debug.LogWarning("[PuertsAdapter] UpdateAgent<T> with bytes: response parsing not yet implemented");
        return Util.Maybe<TResponse>.CreateEmpty();
      }
      catch (Exception e)
      {
        Debug.LogError($"[PuertsAdapter] Failed to parse response: {e.Message}");
        return Util.Maybe<TResponse>.CreateEmpty();
      }
    }
  }
}
