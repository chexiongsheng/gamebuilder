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
  /// Puerts脚本引擎封装
  /// </summary>
  public class PuertsScriptEngine : IDisposable
  {
    private ScriptEnv jsEnv;
    private bool isInitialized = false;

    // 调试和性能监控
    public static bool DebugMode = false;
    public static bool EnablePerformanceMonitoring = false;
    private System.Diagnostics.Stopwatch performanceTimer;
    private int evalCount = 0;
    private int errorCount = 0;
    private float totalEvalTime = 0f;

    /// <summary>
    /// 获取JS环境实例
    /// </summary>
    public ScriptEnv JsEnv => jsEnv;

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => isInitialized;

    public PuertsScriptEngine()
    {
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

        jsEnv = new ScriptEnv(new BackendV8());

        jsEnv.ExecuteModule("puerts/module.mjs");
        ExportsFunctions = jsEnv.ExecuteModule("main.mjs");
        Debug.Log("[PuertsScriptEngine] main.mjs loaded successfully");

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

    internal ScriptObject ExportsFunctions = null;

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
        var setGlobal = ExportsFunctions.Get<Action<string, object>>("setGlobal");

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
  }
}
