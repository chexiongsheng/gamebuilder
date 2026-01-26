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
      public Func<object, object, ArrayBuffer> updateAgentFunc;
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
        cachedJsonStringifyFunc = scriptEngine.ExportsFunctions.Get<Func<object, string>>("jsonStringify");
        cachedJsonParseFunc = scriptEngine.ExportsFunctions.Get<Func<string, object>>("jsonParse");
        Debug.Log("[PuertsAdapter] Delegate cache initialized");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsAdapter] Failed to initialize delegate cache: {ex.Message}");
      }
    }

    /// <summary>
    /// 注册 VoosEngine 实例到 JS 环境
    /// </summary>
    public void RegisterVoosEngine(VoosEngine engine)
    {
      scriptEngine.RegisterVoosEngine(engine);
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

        //System.IO.File.WriteAllText("brain.js", javascript);
        //scriptEngine.RegisterModule($"brain_{brainUid}", javascript);
        //scriptEngine.ExecuteModule($"brain_{brainUid}");
        // 没大重构js前还只能通过Eval，主要是它的mem,card这种是通过全局变量的切换来实现私有的
        scriptEngine.Eval(javascript, $"brain_{brainUid}");

        // 获取updateAgent函数引用
        try
        {
          context.updateAgentFunc = scriptEngine.ExportsFunctions.Get< Func<object, object, ArrayBuffer>>("updateAgentPostMessageFlush");
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

        Debug.Log($"[PuertsAdapter] Brain {brainUid} reset successfully");
        return true;
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsAdapter] Failed to reset brain {brainUid}: {ex.Message}\n{ex.StackTrace}");
        return false;
      }
    }

    public void LoadAllBuiltinBehaviors()
    {
      scriptEngine.ExecuteModule("BehaviorLibrary/BehaviorLibraryIndex.mjs");
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
      if (!brainContexts.TryGetValue(brainUid, out BrainContext context))
      {
        Debug.LogError($"[PuertsAdapter] Brain not found: {brainUid}");
        return Util.Maybe<TResponse>.CreateEmpty();
      }

      if (context.updateAgentFunc == null)
      {
        Debug.LogError($"[PuertsAdapter] updateAgent function not found for brain: {brainUid}");
        return Util.Maybe<TResponse>.CreateEmpty();
      }

      try
      {
        // TODO: 根据puerts的特点进行优化
        // 序列化请求为JSON
        string inputJson = JsonUtility.ToJson(input, false);

        // 将JSON解析为JS对象
        object requestObj = cachedJsonParseFunc(inputJson);

        // 调用updateAgent函数
        // updateAgent会修改requestObj，添加响应数据
        if (bytes != null && bytes.Length > 0)
        {
          var arrayBuffer = new Puerts.ArrayBuffer(bytes);
          arrayBuffer = context.updateAgentFunc(requestObj, arrayBuffer);
          Array.Copy(arrayBuffer.Bytes, bytes, Math.Min(arrayBuffer.Count, bytes.Length));
        }
        else
        {
          context.updateAgentFunc(requestObj, null);
        }

        // 将修改后的JS对象序列化回JSON
        string outputJson = cachedJsonStringifyFunc(requestObj);

        // 反序列化为响应类型
        TResponse response = JsonUtility.FromJson<TResponse>(outputJson);

        return Util.Maybe<TResponse>.CreateWith(response);
      }
      catch (Exception e)
      {
        Debug.LogError($"[PuertsAdapter] UpdateAgent<T> failed: {e.Message}\n{e.StackTrace}");
        return Util.Maybe<TResponse>.CreateEmpty();
      }
    }
  }
}
