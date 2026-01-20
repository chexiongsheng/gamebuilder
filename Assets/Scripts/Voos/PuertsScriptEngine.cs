using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Puerts;
using Debug = UnityEngine.Debug;

namespace Voos
{
  /// <summary>
  /// 内存模块加载器 - 支持从内存中加载JavaScript模块
  /// </summary>
  public class MemoryModuleLoader : ILoader, IModuleChecker
  {
    private Dictionary<string, string> modules = new Dictionary<string, string>();
    private ILoader fallbackLoader;

    public MemoryModuleLoader(ILoader fallbackLoader = null)
    {
      this.fallbackLoader = fallbackLoader ?? new DefaultLoader();
    }

    /// <summary>
    /// 注册一个内存模块
    /// </summary>
    public void RegisterModule(string modulePath, string code)
    {
      modules[modulePath] = code;
      if (PuertsScriptEngine.DebugMode)
      {
        Debug.Log($"[MemoryModuleLoader] Registered module: {modulePath} ({code.Length} chars)");
      }
    }

    /// <summary>
    /// 移除一个内存模块
    /// </summary>
    public void UnregisterModule(string modulePath)
    {
      modules.Remove(modulePath);
    }

    /// <summary>
    /// 清空所有内存模块
    /// </summary>
    public void Clear()
    {
      modules.Clear();
    }

    public bool FileExists(string filepath)
    {
      // 先检查内存中是否有该模块
      if (modules.ContainsKey(filepath))
      {
        return true;
      }
      // 回退到默认加载器
      return fallbackLoader?.FileExists(filepath) ?? false;
    }

    public string ReadFile(string filepath, out string debugpath)
    {
      // 先从内存中读取
      if (modules.TryGetValue(filepath, out string code))
      {
        debugpath = $"memory://{filepath}";
        return code;
      }
      // 回退到默认加载器
      if (fallbackLoader != null)
      {
        return fallbackLoader.ReadFile(filepath, out debugpath);
      }
      debugpath = filepath;
      return null;
    }

    public bool IsESM(string filepath)
    {
      // 内存模块默认都是ESM
      if (modules.ContainsKey(filepath))
      {
        return true;
      }
      // 回退到默认加载器
      if (fallbackLoader is IModuleChecker checker)
      {
        return checker.IsESM(filepath);
      }
      // 默认判断：不是.cjs结尾的都是ESM
      return !filepath.EndsWith(".cjs");
    }
  }

  /// <summary>
  /// Puerts脚本引擎封装
  /// </summary>
  public class PuertsScriptEngine : IDisposable
  {
    private static PuertsScriptEngine instance;
    private ScriptEnv jsEnv;
    private MemoryModuleLoader memoryLoader;
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

        // 创建内存模块加载器
        memoryLoader = new MemoryModuleLoader();

        // 创建JS环境，使用V8后端和自定义加载器
        jsEnv = new ScriptEnv(new BackendV8(memoryLoader));

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
          throw new Exception("Polyfill code is not set and polyfill.js.txt file not found");
        }
      }

      try
      {
        memoryLoader.RegisterModule("polyfill.js", polyfillCode);
        jsEnv.ExecuteModule("polyfill.js");
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
    /// 注册 VoosEngine 实例到 JS 全局环境
    /// </summary>
    public void RegisterVoosEngine(VoosEngine engine)
    {
      if (!isInitialized)
      {
        throw new InvalidOperationException("PuertsScriptEngine not initialized");
      }

      try
      {
        // 导出设置全局变量的函数
        var setGlobal = jsEnv.Eval<Action<string, object>>("(k, v) => globalThis[k] = v;");

        // 直接注册 VoosEngine 实例到全局对象
        // JS 可以直接调用 engine 的公共方法
        setGlobal("__voosEngine", engine);

        Debug.Log("[PuertsScriptEngine] VoosEngine registered successfully");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[PuertsScriptEngine] Failed to register VoosEngine: {ex.Message}");
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

    /// <summary>
    /// 注册内存模块（用于ExecuteModule）
    /// </summary>
    public void RegisterModule(string modulePath, string code)
    {
      if (!isInitialized)
      {
        throw new InvalidOperationException("PuertsScriptEngine not initialized");
      }

      if (memoryLoader == null)
      {
        throw new InvalidOperationException("MemoryModuleLoader not available");
      }

      memoryLoader.RegisterModule(modulePath, code);
    }

    /// <summary>
    /// 执行ES模块（支持export语法）
    /// </summary>
    public void ExecuteModule(string modulePath)
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
          Debug.Log($"[PuertsScriptEngine] Executing module {modulePath}");
        }

        jsEnv.ExecuteModule(modulePath);
        evalCount++;

        if (EnablePerformanceMonitoring)
        {
          performanceTimer.Stop();
          float elapsed = (float)performanceTimer.Elapsed.TotalMilliseconds;
          totalEvalTime += elapsed;
          Debug.Log($"[PuertsScriptEngine] ExecuteModule {modulePath} took {elapsed:F2}ms (avg: {totalEvalTime / evalCount:F2}ms)");
        }
      }
      catch (Exception ex)
      {
        errorCount++;
        string errorMsg = ExtractErrorMessage(ex, modulePath);
        Debug.LogError($"[PuertsScriptEngine] ExecuteModule error in {modulePath}: {errorMsg}");

        if (DebugMode)
        {
          Debug.LogError($"[PuertsScriptEngine] Stack trace: {ex.StackTrace}");
        }
        throw;
      }
    }

    /// <summary>
    /// 移除内存模块
    /// </summary>
    public void UnregisterModule(string modulePath)
    {
      if (memoryLoader != null)
      {
        memoryLoader.UnregisterModule(modulePath);
      }
    }
  }
}
