using System;
using System.IO;
using UnityEngine;
using Puerts;

namespace Voos
{
  /// <summary>
  /// Puerts脚本引擎管理器
  /// 负责Puerts ScriptEnv的生命周期管理
  /// </summary>
  public class PuertsScriptEngine : IDisposable
  {
    private static PuertsScriptEngine instance;
    private ScriptEnv jsEnv;
    private bool isInitialized = false;
    private string polyfillCode;

    // 调试和性能监控
    public static bool DebugMode = false;
    public static bool EnablePerformanceMonitoring = false;
    private System.Diagnostics.Stopwatch performanceTimer;
    private int evalCount = 0;
    private int errorCount = 0;
    private float totalEvalTime = 0f;

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static PuertsScriptEngine Instance
    {
      get
      {
        if (instance == null)
        {
          instance = new PuertsScriptEngine();
        }
        return instance;
      }
    }

    /// <summary>
    /// 获取JS环境实例
    /// </summary>
    public ScriptEnv JsEnv => jsEnv;

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => isInitialized;

    private PuertsScriptEngine()
    {
      // 私有构造函数，确保单例
      performanceTimer = new System.Diagnostics.Stopwatch();
    }

    /// <summary>
    /// 初始化Puerts环境
    /// </summary>
    public bool Initialize()
    {
      if (isInitialized)
      {
        Debug.LogWarning("[PuertsScriptEngine] Already initialized");
        return true;
      }

      try
      {
        Debug.Log("[PuertsScriptEngine] Initializing Puerts environment...");

        // 创建JS环境，使用V8后端
        jsEnv = new ScriptEnv(new BackendV8());

        // 加载polyfill代码
        LoadPolyfill();

        isInitialized = true;
        Debug.Log("[PuertsScriptEngine] Puerts environment initialized successfully");
        return true;
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsScriptEngine] Failed to initialize: {ex.Message}\n{ex.StackTrace}");
        isInitialized = false;
        return false;
      }
    }

    /// <summary>
    /// 加载Polyfill适配层
    /// </summary>
    private void LoadPolyfill()
    {
      if (string.IsNullOrEmpty(polyfillCode))
      {
        // 尝试从文件加载polyfill
        string polyfillPath = Path.Combine(Application.dataPath, "Scripts", "Behaviors", "JavaScript", "polyfill.js.txt");
        if (File.Exists(polyfillPath))
        {
          polyfillCode = File.ReadAllText(polyfillPath);
          Debug.Log($"[PuertsScriptEngine] Loaded polyfill from {polyfillPath}");
        }
        else
        {
          // 使用内嵌的最小polyfill
          polyfillCode = @"
// Minimal polyfill
if (typeof globalThis === 'undefined') {
  var globalThis = this;
}
console = console || {};
console.log = console.log || function() {};
console.warn = console.warn || function() {};
console.error = console.error || function() {};
";
          Debug.LogWarning($"[PuertsScriptEngine] Polyfill file not found at {polyfillPath}, using minimal polyfill");
        }
      }

      try
      {
        jsEnv.Eval(polyfillCode, "polyfill.js");
        Debug.Log("[PuertsScriptEngine] Polyfill loaded successfully");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsScriptEngine] Failed to load polyfill: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// 设置Polyfill代码
    /// </summary>
    public void SetPolyfillCode(string code)
    {
      polyfillCode = code;
      if (isInitialized)
      {
        LoadPolyfill();
      }
    }

    /// <summary>
    /// 注册C#回调到JS全局环境
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
      if (!isInitialized)
      {
        throw new InvalidOperationException("PuertsScriptEngine not initialized");
      }

      try
      {
        // 导出设置全局变量的函数
        var setGlobal = jsEnv.Eval<Action<string, object>>("(k, v) => globalThis[k] = v;");

        // 注册委托到全局对象
        setGlobal("__getActorBoolean", getActorBoolean);
        setGlobal("__setActorBoolean", setActorBoolean);
        setGlobal("__getActorFloat", getActorFloat);
        setGlobal("__setActorFloat", setActorFloat);
        setGlobal("__getActorVector3", getActorVector3);
        setGlobal("__setActorVector3", setActorVector3);
        setGlobal("__getActorQuaternion", getActorQuaternion);
        setGlobal("__setActorQuaternion", setActorQuaternion);
        setGlobal("__getActorString", getActorString);
        setGlobal("__setActorString", setActorString);
        setGlobal("__callService", callService);
        setGlobal("__handleError", handleError);
        setGlobal("__handleLog", handleLog);

        Debug.Log("[PuertsScriptEngine] Callbacks registered successfully");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsScriptEngine] Failed to register callbacks: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// 执行字符串脚本
    /// </summary>
    public void Eval(string code, string chunkName = "chunk")
    {
      if (!isInitialized)
      {
        throw new InvalidOperationException("PuertsScriptEngine not initialized");
      }

      if (EnablePerformanceMonitoring)
      {
        performanceTimer.Restart();
      }

      try
      {
        if (DebugMode)
        {
          Debug.Log($"[PuertsScriptEngine] Evaluating {chunkName} ({code.Length} chars)");
        }

        jsEnv.Eval(code, chunkName);
        evalCount++;

        if (EnablePerformanceMonitoring)
        {
          performanceTimer.Stop();
          float elapsed = (float)performanceTimer.Elapsed.TotalMilliseconds;
          totalEvalTime += elapsed;
          Debug.Log($"[PuertsScriptEngine] Eval {chunkName} took {elapsed:F2}ms (avg: {totalEvalTime / evalCount:F2}ms)");
        }
      }
      catch (Exception ex)
      {
        errorCount++;
        string errorMsg = ExtractErrorMessage(ex, chunkName);
        Debug.LogError($"[PuertsScriptEngine] Eval error in {chunkName}: {errorMsg}");

        if (DebugMode)
        {
          Debug.LogError($"[PuertsScriptEngine] Stack trace: {ex.StackTrace}");
        }
        throw;
      }
    }

    /// <summary>
    /// 执行字符串脚本并返回结果
    /// </summary>
    public T Eval<T>(string code, string chunkName = "chunk")
    {
      if (!isInitialized)
      {
        throw new InvalidOperationException("PuertsScriptEngine not initialized");
      }

      if (EnablePerformanceMonitoring)
      {
        performanceTimer.Restart();
      }

      try
      {
        if (DebugMode)
        {
          Debug.Log($"[PuertsScriptEngine] Evaluating<{typeof(T).Name}> {chunkName} ({code.Length} chars)");
        }

        T result = jsEnv.Eval<T>(code, chunkName);
        evalCount++;

        if (EnablePerformanceMonitoring)
        {
          performanceTimer.Stop();
          float elapsed = (float)performanceTimer.Elapsed.TotalMilliseconds;
          totalEvalTime += elapsed;
          Debug.Log($"[PuertsScriptEngine] Eval<{typeof(T).Name}> {chunkName} took {elapsed:F2}ms (avg: {totalEvalTime / evalCount:F2}ms)");
        }

        return result;
      }
      catch (Exception ex)
      {
        errorCount++;
        string errorMsg = ExtractErrorMessage(ex, chunkName);
        Debug.LogError($"[PuertsScriptEngine] Eval<{typeof(T).Name}> error in {chunkName}: {errorMsg}");

        if (DebugMode)
        {
          Debug.LogError($"[PuertsScriptEngine] Stack trace: {ex.StackTrace}");
        }
        throw;
      }
    }

    /// <summary>
    /// 执行模块脚本
    /// </summary>
    public object ExecuteModule(string moduleName)
    {
      if (!isInitialized)
      {
        throw new InvalidOperationException("PuertsScriptEngine not initialized");
      }

      try
      {
        return jsEnv.ExecuteModule(moduleName);
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsScriptEngine] ExecuteModule error for {moduleName}: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// 每帧更新，处理JS异步任务
    /// </summary>
    public void Tick()
    {
      if (isInitialized && jsEnv != null)
      {
        try
        {
          jsEnv.Tick();
        }
        catch (Exception ex)
        {
          Debug.LogError($"[PuertsScriptEngine] Tick error: {ex.Message}");
        }
      }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
      if (jsEnv != null)
      {
        Debug.Log("[PuertsScriptEngine] Disposing Puerts environment...");
        jsEnv.Dispose();
        jsEnv = null;
      }
      isInitialized = false;
      instance = null;
    }

    /// <summary>
    /// 重置环境（用于测试或重新加载）
    /// </summary>
    public void Reset()
    {
      Dispose();
      Initialize();
    }

    /// <summary>
    /// 提取错误消息，包含行号信息
    /// </summary>
    private string ExtractErrorMessage(Exception ex, string chunkName)
    {
      string message = ex.Message;

      // 尝试提取行号信息
      // Puerts错误格式通常包含 "at line X" 或 "line X"
      var lineMatch = System.Text.RegularExpressions.Regex.Match(message, @"line\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      if (lineMatch.Success)
      {
        int lineNum = int.Parse(lineMatch.Groups[1].Value);
        return $"{message} (in {chunkName} at line {lineNum})";
      }

      return $"{message} (in {chunkName})";
    }

    /// <summary>
    /// 获取性能统计信息
    /// </summary>
    public string GetPerformanceStats()
    {
      if (evalCount == 0)
      {
        return "No evaluations yet";
      }

      float avgTime = totalEvalTime / evalCount;
      return $"Evals: {evalCount}, Errors: {errorCount}, Avg Time: {avgTime:F2}ms, Total Time: {totalEvalTime:F2}ms";
    }

    /// <summary>
    /// 重置性能统计
    /// </summary>
    public void ResetPerformanceStats()
    {
      evalCount = 0;
      errorCount = 0;
      totalEvalTime = 0f;
      Debug.Log("[PuertsScriptEngine] Performance stats reset");
    }
  }
}
