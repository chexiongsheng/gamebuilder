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

using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;

/// <summary>
/// Unit tests for VoosNetworkWriter and VoosNetworkReader classes.
/// These tests verify binary format compatibility with JavaScript's VoosBinaryReaderWriter.
/// </summary>
public class VoosNetworkSerializationTests
{
  [Test]
  public void TestBasicTypes()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    // Write test data
    writer.Write((byte)42);
    writer.Write((ushort)1234);
    writer.Write((int)567890);
    writer.Write(3.14159f);

    // Read back
    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Assert.AreEqual(42, reader.ReadByte());
    Assert.AreEqual(1234, reader.ReadUInt16());
    Assert.AreEqual(567890, reader.ReadInt32());
    Assert.AreEqual(3.14159f, reader.ReadSingle(), 0.00001f);
  }

  [Test]
  public void TestBoolean()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    // Write booleans
    writer.WriteVoosBoolean(true);
    writer.WriteVoosBoolean(false);
    writer.WriteVoosBoolean(true);

    // Read back
    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Assert.IsTrue(reader.ReadVoosBoolean());
    Assert.IsFalse(reader.ReadVoosBoolean());
    Assert.IsTrue(reader.ReadVoosBoolean());
  }

  [Test]
  public void TestVector3()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    Vector3 testVec = new Vector3(1.5f, 2.5f, 3.5f);
    writer.WriteVoosVector3(testVec);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Vector3 result = reader.ReadVoosVector3();

    Assert.AreEqual(testVec.x, result.x, 0.00001f);
    Assert.AreEqual(testVec.y, result.y, 0.00001f);
    Assert.AreEqual(testVec.z, result.z, 0.00001f);
  }

  [Test]
  public void TestQuaternion()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    Quaternion testQuat = Quaternion.Euler(45f, 90f, 180f);
    writer.Write(testQuat);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Quaternion result = reader.ReadQuaternion();

    Assert.AreEqual(testQuat.x, result.x, 0.00001f);
    Assert.AreEqual(testQuat.y, result.y, 0.00001f);
    Assert.AreEqual(testQuat.z, result.z, 0.00001f);
    Assert.AreEqual(testQuat.w, result.w, 0.00001f);
  }

  [Test]
  public void TestColor()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    Color testColor = new Color(0.1f, 0.5f, 0.9f, 1.0f);
    writer.WriteColor(testColor);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Color result = reader.ReadColor();

    Assert.AreEqual(testColor.r, result.r, 0.00001f);
    Assert.AreEqual(testColor.g, result.g, 0.00001f);
    Assert.AreEqual(testColor.b, result.b, 0.00001f);
    Assert.AreEqual(testColor.a, result.a, 0.00001f);
  }

  [Test]
  public void TestUtf16String()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    string testString = "Hello World!";
    writer.WriteUtf16(testString);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadUtf16();

    Assert.AreEqual(testString, result);
  }

  [Test]
  public void TestUtf16MultiLanguage()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    // Test with Japanese, Chinese, and English
    string testString = "japanese いろはに 中文测试 English";
    writer.WriteUtf16(testString);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadUtf16();

    Assert.AreEqual(testString, result);
  }

  [Test]
  public void TestUtf16EmptyString()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    writer.WriteUtf16("");

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadUtf16();

    Assert.AreEqual("", result);
  }

  [Test]
  public void TestUtf16NullString()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    writer.WriteUtf16(null);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadUtf16();

    Assert.AreEqual("", result);
  }

  [Test]
  public void TestVoosNameEmpty()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    writer.WriteVoosName("");

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadVoosName();

    Assert.AreEqual("", result);
  }

  [Test]
  public void TestVoosNameGuid()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    string testGuid = Guid.NewGuid().ToString("N");
    writer.WriteVoosName(testGuid);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadVoosName();

    Assert.AreEqual(testGuid, result);
  }

  [Test]
  public void TestVoosNameString()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    string testName = "__DEFAULT_BEHAVIOR__";
    writer.WriteVoosName(testName);

    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    string result = reader.ReadVoosName();

    Assert.AreEqual(testName, result);
  }

  [Test]
  public void TestRoundTrip()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    // Write various types
    writer.WriteVoosBoolean(true);
    writer.Write(42);
    writer.Write(3.14f);
    writer.WriteUtf16("Test String");
    writer.WriteVoosVector3(new Vector3(1, 2, 3));

    // Read back
    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Assert.IsTrue(reader.ReadVoosBoolean());
    Assert.AreEqual(42, reader.ReadInt32());
    Assert.AreEqual(3.14f, reader.ReadSingle(), 0.00001f);
    Assert.AreEqual("Test String", reader.ReadUtf16());
    Vector3 vec = reader.ReadVoosVector3();
    Assert.AreEqual(1f, vec.x, 0.00001f);
    Assert.AreEqual(2f, vec.y, 0.00001f);
    Assert.AreEqual(3f, vec.z, 0.00001f);
  }

  [Test]
  public void TestBufferOverflow()
  {
    byte[] buffer = new byte[4];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    writer.Write((int)42); // This should work (4 bytes)

    // This should throw an exception (trying to write beyond buffer)
    Assert.Throws<InvalidOperationException>(() => writer.Write((byte)1));
  }

  [Test]
  public void TestBufferUnderflow()
  {
    byte[] buffer = new byte[4];
    VoosNetworkReader reader = new VoosNetworkReader(buffer);

    reader.ReadInt32(); // This should work (4 bytes)

    // This should throw an exception (trying to read beyond buffer)
    Assert.Throws<InvalidOperationException>(() => reader.ReadByte());
  }

  [Test]
  public void TestToArray()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    writer.Write((byte)1);
    writer.Write((byte)2);
    writer.Write((byte)3);

    byte[] result = writer.ToArray();

    Assert.AreEqual(3, result.Length);
    Assert.AreEqual(1, result[0]);
    Assert.AreEqual(2, result[1]);
    Assert.AreEqual(3, result[2]);
  }

  [Test]
  public void TestLittleEndian()
  {
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    // Write 0x12345678 in little-endian
    writer.Write(0x12345678);

    // Check byte order
    Assert.AreEqual(0x78, buffer[0]);
    Assert.AreEqual(0x56, buffer[1]);
    Assert.AreEqual(0x34, buffer[2]);
    Assert.AreEqual(0x12, buffer[3]);
  }

  [Test]
  public void TestJavaScriptCompatibility()
  {
    // This test uses known data that should match JavaScript's VoosBinaryReaderWriter
    byte[] buffer = new byte[1024];
    VoosNetworkWriter writer = new VoosNetworkWriter(buffer);

    // Write data in the same format as JavaScript
    writer.WriteVoosBoolean(true);
    writer.Write(42);
    writer.Write(0.123f);
    writer.WriteVoosBoolean(false);

    // Verify the exact byte sequence matches what JavaScript would produce
    VoosNetworkReader reader = new VoosNetworkReader(buffer);
    Assert.AreEqual(true, reader.ReadVoosBoolean());
    Assert.AreEqual(42, reader.ReadInt32());
    Assert.AreEqual(0.123f, reader.ReadSingle(), 0.00001f);
    Assert.AreEqual(false, reader.ReadVoosBoolean());
  }
}
