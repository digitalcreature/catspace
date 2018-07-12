using UnityEngine;
using UnityEngine.Networking;
using System;

// helper class to make serialization code shorter and less of a pain in the ass to write and read
public class NetworkSync {

  public NetworkWriter writer { get; private set; }
  public NetworkReader reader { get; private set; }

  public bool isWriting => writer != null;
  public bool isReading => reader != null;

  public NetworkSync(NetworkWriter writer) {
    this.writer = writer;
    this.reader = null;
  }

  public NetworkSync(NetworkReader reader) {
    this.writer = null;
    this.reader = reader;
  }

  public NetworkSync Sync<T>(ref T value) where T : struct, INetworkSyncable {
    value.OnSync(this);
    return this;
  }

  public NetworkSync Sync<T>(T value) where T : class, INetworkSyncable {
    value.OnSync(this);
    return this;
  }

  // unity supported types

  public NetworkSync Sync(ref Id value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref char value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref byte value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref sbyte value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref short value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref ushort value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref int value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref uint value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref long value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref ulong value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref float value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref double value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Decimal value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref string value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref bool value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref byte[] buffer, int count) {
    if (isWriting) return Write(buffer, count);
    else return Read(ref buffer, count); }
  public NetworkSync Sync(ref byte[] buffer, int offset, int count) {
    if (isWriting) return Write(buffer, offset, count);
    else return Read(ref buffer, offset, count); }
  public NetworkSync Sync(ref Vector2 value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Vector3 value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Vector4 value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Color value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Color32 value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref GameObject value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Quaternion value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Rect value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Plane value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Ray value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Matrix4x4 value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync SyncMessage<T>(ref T msg) where T : MessageBase, new() {
    if (isWriting) return Write(msg);
    else return ReadMessage<T>(ref msg); }
  public NetworkSync Sync(ref NetworkHash128 value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref NetworkIdentity value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref NetworkInstanceId value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref NetworkSceneId value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref Transform value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }
  public NetworkSync Sync(ref KeyCode value) {
    if (isWriting) return Write(value);
    else return Read(ref value); }

  // write
  public NetworkSync Write(Id value) { writer.Write(value.value); return this; }
  public NetworkSync Write(char value) { writer.Write(value); return this; }
  public NetworkSync Write(byte value) { writer.Write(value); return this; }
  public NetworkSync Write(sbyte value) { writer.Write(value); return this; }
  public NetworkSync Write(short value) { writer.Write(value); return this; }
  public NetworkSync Write(ushort value) { writer.Write(value); return this; }
  public NetworkSync Write(int value) { writer.Write(value); return this; }
  public NetworkSync Write(uint value) { writer.Write(value); return this; }
  public NetworkSync Write(long value) { writer.Write(value); return this; }
  public NetworkSync Write(ulong value) { writer.Write(value); return this; }
  public NetworkSync Write(float value) { writer.Write(value); return this; }
  public NetworkSync Write(double value) { writer.Write(value); return this; }
  public NetworkSync Write(Decimal value) { writer.Write(value); return this; }
  public NetworkSync Write(string value) { writer.Write(value); return this; }
  public NetworkSync Write(bool value) { writer.Write(value); return this; }
  public NetworkSync Write(byte[] buffer, int count) { writer.Write(buffer, count); return this; }
  public NetworkSync Write(byte[] buffer, int offset, int count) { writer.Write(buffer, offset, count); return this; }
  public NetworkSync Write(Vector2 value) { writer.Write(value); return this; }
  public NetworkSync Write(Vector3 value) { writer.Write(value); return this; }
  public NetworkSync Write(Vector4 value) { writer.Write(value); return this; }
  public NetworkSync Write(Color value) { writer.Write(value); return this; }
  public NetworkSync Write(Color32 value) { writer.Write(value); return this; }
  public NetworkSync Write(GameObject value) { writer.Write(value); return this; }
  public NetworkSync Write(Quaternion value) { writer.Write(value); return this; }
  public NetworkSync Write(Rect value) { writer.Write(value); return this; }
  public NetworkSync Write(Plane value) { writer.Write(value); return this; }
  public NetworkSync Write(Ray value) { writer.Write(value); return this; }
  public NetworkSync Write(Matrix4x4 value) { writer.Write(value); return this; }
  public NetworkSync Write(MessageBase msg) { writer.Write(msg); return this; }
  public NetworkSync Write(NetworkHash128 value) { writer.Write(value); return this; }
  public NetworkSync Write(NetworkIdentity value) { writer.Write(value); return this; }
  public NetworkSync Write(NetworkInstanceId value) { writer.Write(value); return this; }
  public NetworkSync Write(NetworkSceneId value) { writer.Write(value); return this; }
  public NetworkSync Write(Transform value) { writer.Write(value); return this; }
  public NetworkSync Write(KeyCode value) { writer.Write((int) value); return this; }

  // read
  public NetworkSync Read(ref Id value) { value = new Id(reader.ReadUInt32()); return this; }
  public NetworkSync Read(ref char value) { value = reader.ReadChar(); return this; }
  public NetworkSync Read(ref byte value) { value = reader.ReadByte(); return this; }
  public NetworkSync Read(ref sbyte value) { value = reader.ReadSByte(); return this; }
  public NetworkSync Read(ref short value) { value = reader.ReadInt16(); return this; }
  public NetworkSync Read(ref ushort value) { value = reader.ReadUInt16(); return this; }
  public NetworkSync Read(ref int value) { value = reader.ReadInt32(); return this; }
  public NetworkSync Read(ref uint value) { value = reader.ReadUInt32(); return this; }
  public NetworkSync Read(ref long value) { value = reader.ReadInt64(); return this; }
  public NetworkSync Read(ref ulong value) { value = reader.ReadUInt64(); return this; }
  public NetworkSync Read(ref float value) { value = reader.ReadSingle(); return this; }
  public NetworkSync Read(ref double value) { value = reader.ReadDouble(); return this; }
  public NetworkSync Read(ref Decimal value) { value = reader.ReadDecimal(); return this; }
  public NetworkSync Read(ref string value) { value = reader.ReadString(); return this; }
  public NetworkSync Read(ref bool value) { value = reader.ReadBoolean(); return this; }
  public NetworkSync Read(ref byte[] buffer, int count) { buffer = reader.ReadBytes(count); return this; }
  public NetworkSync Read(ref byte[] buffer, int offset, int count) { buffer = reader.ReadBytes(count); return this; }
  public NetworkSync Read(ref Vector2 value) { value = reader.ReadVector2(); return this; }
  public NetworkSync Read(ref Vector3 value) { value = reader.ReadVector3(); return this; }
  public NetworkSync Read(ref Vector4 value) { value = reader.ReadVector4(); return this; }
  public NetworkSync Read(ref Color value) { value = reader.ReadColor(); return this; }
  public NetworkSync Read(ref Color32 value) { value = reader.ReadColor32(); return this; }
  public NetworkSync Read(ref GameObject value) { value = reader.ReadGameObject(); return this; }
  public NetworkSync Read(ref Quaternion value) { value = reader.ReadQuaternion(); return this; }
  public NetworkSync Read(ref Rect value) { value = reader.ReadRect(); return this; }
  public NetworkSync Read(ref Plane value) { value = reader.ReadPlane(); return this; }
  public NetworkSync Read(ref Ray value) { value = reader.ReadRay(); return this; }
  public NetworkSync Read(ref Matrix4x4 value) { value = reader.ReadMatrix4x4(); return this; }
  public NetworkSync ReadMessage<T>(ref T msg) where T : MessageBase, new() { msg = reader.ReadMessage<T>(); return this; }
  public NetworkSync Read(ref NetworkHash128 value) { value = reader.ReadNetworkHash128(); return this; }
  public NetworkSync Read(ref NetworkIdentity value) { value = reader.ReadNetworkIdentity(); return this; }
  public NetworkSync Read(ref NetworkInstanceId value) { value = reader.ReadNetworkId(); return this; }
  public NetworkSync Read(ref NetworkSceneId value) { value = reader.ReadSceneId(); return this; }
  public NetworkSync Read(ref Transform value) { value = reader.ReadTransform(); return this; }
  public NetworkSync Read(ref KeyCode value) { value = (KeyCode) reader.ReadInt32(); return this; }

}

// implement in custom types to make them serializable over the network
public interface INetworkSyncable {

  void OnSync(NetworkSync sync);

}
