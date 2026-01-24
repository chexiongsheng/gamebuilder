/*
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using UnityEngine;

/// <summary>
/// Replacement for Unity's deprecated UnityEngine.Networking.NetworkWriter.
/// This class provides binary serialization compatible with JavaScript's VoosBinaryReaderWriter.
/// 
/// Key features:
/// - Little-endian byte order (compatible with JavaScript DataView)
/// - Supports basic types: byte, ushort, int, float
/// - Supports Unity types: Vector3, Quaternion
/// - Thread-safe for single-threaded use (not designed for concurrent access)
/// 
/// Usage example:
/// <code>
/// byte[] buffer = new byte[1024];
/// VoosNetworkWriter writer = new VoosNetworkWriter(buffer);
/// writer.Write((byte)1);
/// writer.Write(42);
/// writer.Write(3.14f);
/// byte[] data = writer.ToArray();
/// </code>
/// </summary>
public class VoosNetworkWriter
{
  private byte[] buffer;
  private int position;

  /// <summary>
  /// Creates a new VoosNetworkWriter with the specified buffer.
  /// </summary>
  /// <param name="buffer">The byte array to write data into</param>
  public VoosNetworkWriter(byte[] buffer)
  {
    if (buffer == null)
    {
      throw new ArgumentNullException(nameof(buffer));
    }
    this.buffer = buffer;
    this.position = 0;
  }

  /// <summary>
  /// Gets the current write position in bytes.
  /// </summary>
  public int Position
  {
    get { return position; }
  }

  /// <summary>
  /// Writes a single byte to the buffer.
  /// </summary>
  /// <param name="value">The byte value to write</param>
  public void Write(byte value)
  {
    CheckCapacity(1);
    buffer[position] = value;
    position += 1;
  }

  /// <summary>
  /// Writes a boolean value as a single byte (0 for false, 1 for true).
  /// Compatible with JavaScript's writeBoolean method.
  /// </summary>
  /// <param name="value">The boolean value to write</param>
  public void Write(bool value)
  {
    Write((byte)(value ? 1 : 0));
  }

  /// <summary>
  /// Writes a 16-bit unsigned integer in little-endian format.
  /// </summary>
  /// <param name="value">The ushort value to write</param>
  public void Write(ushort value)
  {
    CheckCapacity(2);
    buffer[position] = (byte)(value & 0xFF);
    buffer[position + 1] = (byte)((value >> 8) & 0xFF);
    position += 2;
  }

  /// <summary>
  /// Writes a 32-bit signed integer in little-endian format.
  /// </summary>
  /// <param name="value">The int value to write</param>
  public void Write(int value)
  {
    CheckCapacity(4);
    buffer[position] = (byte)(value & 0xFF);
    buffer[position + 1] = (byte)((value >> 8) & 0xFF);
    buffer[position + 2] = (byte)((value >> 16) & 0xFF);
    buffer[position + 3] = (byte)((value >> 24) & 0xFF);
    position += 4;
  }

  /// <summary>
  /// Writes a 32-bit floating point number in little-endian format.
  /// </summary>
  /// <param name="value">The float value to write</param>
  public void Write(float value)
  {
    CheckCapacity(4);
    byte[] bytes = BitConverter.GetBytes(value);
    if (!BitConverter.IsLittleEndian)
    {
      Array.Reverse(bytes);
    }
    Buffer.BlockCopy(bytes, 0, buffer, position, 4);
    position += 4;
  }

  /// <summary>
  /// Writes a Quaternion as four 32-bit floats (x, y, z, w) in little-endian format.
  /// </summary>
  /// <param name="value">The Quaternion to write</param>
  public void Write(Quaternion value)
  {
    Write(value.x);
    Write(value.y);
    Write(value.z);
    Write(value.w);
  }

  /// <summary>
  /// Returns a byte array containing all data written so far.
  /// </summary>
  /// <returns>A new byte array with the written data</returns>
  public byte[] ToArray()
  {
    byte[] result = new byte[position];
    Buffer.BlockCopy(buffer, 0, result, 0, position);
    return result;
  }

  /// <summary>
  /// Checks if there is enough capacity in the buffer for the requested number of bytes.
  /// </summary>
  /// <param name="bytesNeeded">Number of bytes needed</param>
  private void CheckCapacity(int bytesNeeded)
  {
    if (position + bytesNeeded > buffer.Length)
    {
      throw new InvalidOperationException(
        $"Buffer overflow: trying to write {bytesNeeded} bytes at position {position}, but buffer size is {buffer.Length}");
    }
  }
}
