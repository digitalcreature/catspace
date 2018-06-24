using UnityEngine;
using UnityEngine.Networking;
using System;

// helper struct to make serialization code shorter and less of a pain in the ass to write and read
public struct NetworkSync {

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

  public void SyncBehaviour<T>(ref T value) where T : KittyNetworkBehaviour {
    if (isWriting) {
      writer.Write(value == null ? null : value.gameObject);
    }
    else {
      GameObject obj = reader.ReadGameObject();
      if (obj == null)
        value = null;
      else
        value = obj.GetComponent<T>();
    }
  }

  public void Sync<T>(ref T value) where T : INetworkSyncable {
    value.OnSync(this);
  }

  // unity supported types

  public void Sync(ref char value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadChar(); }

  public void Sync(ref byte value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadByte(); }

  public void Sync(ref sbyte value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadSByte(); }

  public void Sync(ref short value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadInt16(); }

  public void Sync(ref ushort value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadUInt16(); }

  public void Sync(ref int value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadInt32(); }

  public void Sync(ref uint value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadUInt32(); }

  public void Sync(ref long value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadInt64(); }

  public void Sync(ref ulong value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadUInt64(); }

  public void Sync(ref float value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadSingle(); }

  public void Sync(ref double value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadDouble(); }

  public void Sync(ref Decimal value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadDecimal(); }

  public void Sync(ref string value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadString(); }

  public void Sync(ref bool value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadBoolean(); }

  public void Sync(ref byte[] buffer, int count) {
    if (isWriting) writer.Write(buffer, count);
    else buffer = reader.ReadBytes(count); }

  public void Sync(ref byte[] buffer, int offset, int count) {
    if (isWriting) writer.Write(buffer, offset, count);
    else buffer = reader.ReadBytes(count); }

  public void Sync(ref Vector2 value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadVector2(); }

  public void Sync(ref Vector3 value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadVector3(); }

  public void Sync(ref Vector4 value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadVector4(); }

  public void Sync(ref Color value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadColor(); }

  public void Sync(ref Color32 value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadColor32(); }

  public void Sync(ref GameObject value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadGameObject(); }

  public void Sync(ref Quaternion value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadQuaternion(); }

  public void Sync(ref Rect value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadRect(); }

  public void Sync(ref Plane value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadPlane(); }

  public void Sync(ref Ray value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadRay(); }

  public void Sync(ref Matrix4x4 value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadMatrix4x4(); }

  public void SyncMessage<T>(ref T msg) where T : MessageBase, new() {
    if (isWriting) writer.Write(msg);
    else msg = reader.ReadMessage<T>(); }

  public void Sync(ref NetworkHash128 value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadNetworkHash128(); }

  public void Sync(ref NetworkIdentity value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadNetworkIdentity(); }

  public void Sync(ref NetworkInstanceId value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadNetworkId(); }

  public void Sync(ref NetworkSceneId value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadSceneId(); }

  public void Sync(ref Transform value) {
    if (isWriting) writer.Write(value);
    else value = reader.ReadTransform(); }


}

// implement in custom types to make them serializable over the network
public interface INetworkSyncable {

  void OnSync(NetworkSync sync);

}
