using System;
using UnityEngine;

namespace Voos
{
  /// <summary>
  /// Puerts回调委托定义
  /// 用于C#与JS之间的双向通信
  /// </summary>
  public static class PuertsCallbacks
  {
    // ==================== Actor属性访问委托 ====================

    /// <summary>
    /// 获取Actor的Boolean属性
    /// </summary>
    public delegate bool GetActorBooleanDelegate(string actorId, string fieldId);

    /// <summary>
    /// 设置Actor的Boolean属性
    /// </summary>
    public delegate void SetActorBooleanDelegate(string actorId, string fieldId, bool value);

    /// <summary>
    /// 获取Actor的Float属性
    /// </summary>
    public delegate float GetActorFloatDelegate(string actorId, string fieldId);

    /// <summary>
    /// 设置Actor的Float属性
    /// </summary>
    public delegate void SetActorFloatDelegate(string actorId, string fieldId, float value);

    /// <summary>
    /// 获取Actor的Vector3属性
    /// </summary>
    public delegate Vector3Dto GetActorVector3Delegate(string actorId, string fieldId);

    /// <summary>
    /// 设置Actor的Vector3属性
    /// </summary>
    public delegate void SetActorVector3Delegate(string actorId, string fieldId, Vector3Dto value);

    /// <summary>
    /// 获取Actor的Quaternion属性
    /// </summary>
    public delegate QuaternionDto GetActorQuaternionDelegate(string actorId, string fieldId);

    /// <summary>
    /// 设置Actor的Quaternion属性
    /// </summary>
    public delegate void SetActorQuaternionDelegate(string actorId, string fieldId, QuaternionDto value);

    /// <summary>
    /// 获取Actor的String属性
    /// </summary>
    public delegate string GetActorStringDelegate(string actorId, string fieldId);

    /// <summary>
    /// 设置Actor的String属性
    /// </summary>
    public delegate void SetActorStringDelegate(string actorId, string fieldId, string value);

    // ==================== 服务调用委托 ====================

    /// <summary>
    /// 调用服务
    /// </summary>
    public delegate string CallServiceDelegate(string serviceName, string argsJson);

    // ==================== 错误处理和日志委托 ====================

    /// <summary>
    /// 处理JS错误
    /// </summary>
    public delegate void HandleErrorDelegate(string errorMessage, string stackTrace);

    /// <summary>
    /// 处理JS日志
    /// </summary>
    public delegate void HandleLogDelegate(string level, string message);

    // ==================== 数据传输对象 ====================

    /// <summary>
    /// Vector3数据传输对象（用于JS互操作）
    /// </summary>
    [Serializable]
    public struct Vector3Dto
    {
      public float x;
      public float y;
      public float z;

      public Vector3Dto(float x, float y, float z)
      {
        this.x = x;
        this.y = y;
        this.z = z;
      }

      public Vector3Dto(Vector3 v)
      {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
      }

      public Vector3 ToVector3()
      {
        return new Vector3(x, y, z);
      }

      public static implicit operator Vector3(Vector3Dto dto)
      {
        return dto.ToVector3();
      }

      public static implicit operator Vector3Dto(Vector3 v)
      {
        return new Vector3Dto(v);
      }
    }

    /// <summary>
    /// Quaternion数据传输对象（用于JS互操作）
    /// </summary>
    [Serializable]
    public struct QuaternionDto
    {
      public float x;
      public float y;
      public float z;
      public float w;

      public QuaternionDto(float x, float y, float z, float w)
      {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
      }

      public QuaternionDto(Quaternion q)
      {
        this.x = q.x;
        this.y = q.y;
        this.z = q.z;
        this.w = q.w;
      }

      public Quaternion ToQuaternion()
      {
        return new Quaternion(x, y, z, w);
      }

      public static implicit operator Quaternion(QuaternionDto dto)
      {
        return dto.ToQuaternion();
      }

      public static implicit operator QuaternionDto(Quaternion q)
      {
        return new QuaternionDto(q);
      }
    }
  }
}
