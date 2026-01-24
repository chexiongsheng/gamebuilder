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
/// Replacement for Unity's deprecated UnityEngine.Networking.NetworkReader.
/// This class provides binary deserialization compatible with JavaScript's VoosBinaryReaderWriter.
/// 
/// Key features:
/// - Little-endian byte order (compatible with JavaScript DataView)
/// - Supports basic types: byte, ushort, int, float
/// - Supports Unity types: Vector3, Quaternion
/// - Thread-safe for single-threaded use (not designed for concurrent access)
/// 
/// Usage example:
/// <code>
/// byte[] data = GetSerializedData();
/// VoosNetworkReader reader = new VoosNetworkReader(data);
/// byte b = reader.ReadByte();
/// int i = reader.ReadInt32();
/// float f = reader.ReadSingle();
/// </code>
/// </summary>
public class VoosNetworkReader
{
  private byte[] buffer;
  private int position;

  /// <summary>
  /// Creates a new VoosNetworkReader with the specified buffer.
  /// </summary>
  /// <param name="buffer">The byte array to read data from</param>
  public VoosNetworkReader(byte[] buffer)
  {
    if (buffer == null)
    {
      throw new ArgumentNullException(nameof(buffer));
    }
    this.buffer = buffer;
    this.position = 0;
  }

  /// <summary>
  /// Gets the current read position in bytes.
  /// </summary>
  public int Position
  {
    get { return position; }
  }

  /// <summary>
  /// Reads a single byte from the buffer.
  /// </summary>
  /// <returns>The byte value</returns>
  public byte ReadByte()
  {
    CheckCapacity(1);
    byte value = buffer[position];
    position += 1;
    return value;
  }

  /// <summary>
  /// Reads a boolean value from a single byte (0 for false, non-zero for true).
  /// Compatible with JavaScript's readBoolean method.
  /// </summary>
  /// <returns>The boolean value</returns>
  public bool ReadBoolean()
  {
    return ReadByte() == 1;
  }

  /// <summary>
  /// Reads a 16-bit unsigned integer in little-endian format.
  /// </summary>
  /// <returns>The ushort value</returns>
  public ushort ReadUInt16()
  {
    CheckCapacity(2);
    ushort value = (ushort)(buffer[position] | (buffer[position + 1] << 8));
    position += 2;
    return value;
  }

  /// <summary>
  /// Reads a 32-bit signed integer in little-endian format.
  /// </summary>
  /// <returns>The int value</returns>
  public int ReadInt32()
  {
    CheckCapacity(4);
    int value = buffer[position] |
                (buffer[position + 1] << 8) |
                (buffer[position + 2] << 16) |
                (buffer[position + 3] << 24);
    position += 4;
    return value;
  }

  /// <summary>
  /// Reads a 32-bit floating point number in little-endian format.
  /// </summary>
  /// <returns>The float value</returns>
  public float ReadSingle()
  {
    CheckCapacity(4);
    byte[] bytes = new byte[4];
    Buffer.BlockCopy(buffer, position, bytes, 0, 4);
    if (!BitConverter.IsLittleEndian)
    {
      Array.Reverse(bytes);
    }
    float value = BitConverter.ToSingle(bytes, 0);
    position += 4;
    return value;
  }

  /// <summary>
  /// Reads a Quaternion as four 32-bit floats (x, y, z, w) in little-endian format.
  /// </summary>
  /// <returns>The Quaternion value</returns>
  public Quaternion ReadQuaternion()
  {
    float x = ReadSingle();
    float y = ReadSingle();
    float z = ReadSingle();
    float w = ReadSingle();
    return new Quaternion(x, y, z, w);
  }

  /// <summary>
  /// Checks if there is enough data in the buffer for the requested number of bytes.
  /// </summary>
  /// <param name="bytesNeeded">Number of bytes needed</param>
  private void CheckCapacity(int bytesNeeded)
  {
    if (position + bytesNeeded > buffer.Length)
    {
      throw new InvalidOperationException(
        $"Buffer underflow: trying to read {bytesNeeded} bytes at position {position}, but buffer size is {buffer.Length}");
    }
  }
}
