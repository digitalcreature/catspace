using UnityEngine;
using UnityEngine.Networking;
using System;

public abstract class KittyNetworkBehaviour : NetworkBehaviour {

  bool syncRequested;

  public NetworkIdentity identity { get; private set; }

  protected virtual void Awake() {
    identity = GetComponent<NetworkIdentity>();
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll) {
    writer.Write(syncRequested);
    if (syncRequested || forceAll) {
      OnSync(new NetworkSync(writer));
      syncRequested = false;
      return true;
    }
    return false;
  }
  public override void OnDeserialize(NetworkReader reader, bool forceAll) {
    bool syncRequested = reader.ReadBoolean();
    if (syncRequested || forceAll) {
      OnSync(new NetworkSync(reader));
    }
  }

  // override and implement to customize syncronization
  protected virtual void OnSync(NetworkSync sync) {}

  // call this on the server to send syncronization data to the clients
  public void ServerSync() {
    if (isServer) {
      syncRequested = true;
      SetDirtyBit(1u);
    }
  }

  // assign local authority to this object
  // can only be called from the server
  // given identity must have local authority
  // if the object already has local authority, then its previous owner will have its ownership revoked
  // if the passed identity is null, the object will return to server authority
  // DO NOT USE: IS BROKEN
  public void AssignClientOwner(NetworkIdentity authority) {
    if (isServer) {
      if (identity.localPlayerAuthority) {
        if (authority != identity) {
          identity.RemoveClientAuthority(identity.clientAuthorityOwner);
        }
      }
      if (authority != null) {
        if (!identity.localPlayerAuthority) {
          identity.localPlayerAuthority = true;
        }
        identity.AssignClientAuthority(authority.connectionToClient);
      }
      else {
        if (identity.localPlayerAuthority) {
          identity.localPlayerAuthority = false;
        }
      }
    }
  }

  public void RevokeClientOwner() => AssignClientOwner(null);

}

// public struct Message {
//
//   public short messageId { get; private set; }
//   public NetworkSync sync { get; private set; }
//
//   public bool isOutgoing => sync.isWriting;
//   public bool isIncoming => sync.isReading;
//
//   public Message(short messageId, NetworkSync sync) {
//     this.messageId = messageId;
//     this.sync = sync;
//   }
//
//   public bool SendToServer() {
//     if (isOutgoing) {
//       NetworkWriter writer = sync.writer;
//       sync = null;  // were done with this message! it isnt valid any more
//       writer.FinishMessage();
//       // only send if we have a valid id
//       if (messageId > 0) {
//         return Network.client.SendWriter(writer, Channels.DefaultReliable);
//       }
//     }
//     return false;
//   }
//
//   public void SendToClients() {
//     if (isOutgoing) {
//       NetworkWriter writer = sync.writer;
//       sync = null;  // were done with this message! it isnt valid any more
//       writer.FinishMessage();
//       // only send if we have a valid id
//       if (messageId > 0) {
//         NetworkServer.SendWriterToReady(null, writer, Channels.DefaultReliable);
//       }
//     }
//   }
//
//   // write
//   public Message Write(KittyNetworkBehaviour value) { sync.Write(value); return this; }
//   public Message Write(INetworkSyncable value) { sync.Write(value); return this; }
//   public Message Write(char value) { sync.Write(value); return this; }
//   public Message Write(byte value) { sync.Write(value); return this; }
//   public Message Write(sbyte value) { sync.Write(value); return this; }
//   public Message Write(short value) { sync.Write(value); return this; }
//   public Message Write(ushort value) { sync.Write(value); return this; }
//   public Message Write(int value) { sync.Write(value); return this; }
//   public Message Write(uint value) { sync.Write(value); return this; }
//   public Message Write(long value) { sync.Write(value); return this; }
//   public Message Write(ulong value) { sync.Write(value); return this; }
//   public Message Write(float value) { sync.Write(value); return this; }
//   public Message Write(double value) { sync.Write(value); return this; }
//   public Message Write(Decimal value) { sync.Write(value); return this; }
//   public Message Write(string value) { sync.Write(value); return this; }
//   public Message Write(bool value) { sync.Write(value); return this; }
//   public Message Write(byte[] buffer, int count) { sync.Write(buffer, count); return this; }
//   public Message Write(byte[] buffer, int offset, int count) { sync.Write(buffer, offset, count); return this; }
//   public Message Write(Vector2 value) { sync.Write(value); return this; }
//   public Message Write(Vector3 value) { sync.Write(value); return this; }
//   public Message Write(Vector4 value) { sync.Write(value); return this; }
//   public Message Write(Color value) { sync.Write(value); return this; }
//   public Message Write(Color32 value) { sync.Write(value); return this; }
//   public Message Write(GameObject value) { sync.Write(value); return this; }
//   public Message Write(Quaternion value) { sync.Write(value); return this; }
//   public Message Write(Rect value) { sync.Write(value); return this; }
//   public Message Write(Plane value) { sync.Write(value); return this; }
//   public Message Write(Ray value) { sync.Write(value); return this; }
//   public Message Write(Matrix4x4 value) { sync.Write(value); return this; }
//   public Message Write(MessageBase msg) { sync.Write(msg); return this; }
//   public Message Write(NetworkHash128 value) { sync.Write(value); return this; }
//   public Message Write(NetworkIdentity value) { sync.Write(value); return this; }
//   public Message Write(NetworkInstanceId value) { sync.Write(value); return this; }
//   public Message Write(NetworkSceneId value) { sync.Write(value); return this; }
//   public Message Write(Transform value) { sync.Write(value); return this; }
//
//   // read
//   public Message ReadBehaviour<T>(ref T value) where T : KittyNetworkBehaviour { sync.ReadBehaviour(ref value); return this; }
//   public Message Read<T>(ref T value) where T: INetworkSyncable { sync.Read(ref value); return this; }
//   public Message Read(ref char value) { sync.Read(ref value); return this; }
//   public Message Read(ref byte value) { sync.Read(ref value); return this; }
//   public Message Read(ref sbyte value) { sync.Read(ref value); return this; }
//   public Message Read(ref short value) { sync.Read(ref value); return this; }
//   public Message Read(ref ushort value) { sync.Read(ref value); return this; }
//   public Message Read(ref int value) { sync.Read(ref value); return this; }
//   public Message Read(ref uint value) { sync.Read(ref value); return this; }
//   public Message Read(ref long value) { sync.Read(ref value); return this; }
//   public Message Read(ref ulong value) { sync.Read(ref value); return this; }
//   public Message Read(ref float value) { sync.Read(ref value); return this; }
//   public Message Read(ref double value) { sync.Read(ref value); return this; }
//   public Message Read(ref Decimal value) { sync.Read(ref value); return this; }
//   public Message Read(ref string value) { sync.Read(ref value); return this; }
//   public Message Read(ref bool value) { sync.Read(ref value); return this; }
//   public Message Read(ref byte[] buffer, int count) { sync.Read(ref buffer, count); return this; }
//   public Message Read(ref byte[] buffer, int offset, int count) { sync.Read(ref buffer, offset, count); return this; }
//   public Message Read(ref Vector2 value) { sync.Read(ref value); return this; }
//   public Message Read(ref Vector3 value) { sync.Read(ref value); return this; }
//   public Message Read(ref Vector4 value) { sync.Read(ref value); return this; }
//   public Message Read(ref Color value) { sync.Read(ref value); return this; }
//   public Message Read(ref Color32 value) { sync.Read(ref value); return this; }
//   public Message Read(ref GameObject value) { sync.Read(ref value); return this; }
//   public Message Read(ref Quaternion value) { sync.Read(ref value); return this; }
//   public Message Read(ref Rect value) { sync.Read(ref value); return this; }
//   public Message Read(ref Plane value) { sync.Read(ref value); return this; }
//   public Message Read(ref Ray value) { sync.Read(ref value); return this; }
//   public Message Read(ref Matrix4x4 value) { sync.Read(ref value); return this; }
//   public Message ReadMessage<T>(ref T msg) where T : MessageBase, new() { sync.ReadMessage(ref msg); return this; }
//   public Message Read(ref NetworkHash128 value) { sync.Read(ref value); return this; }
//   public Message Read(ref NetworkIdentity value) { sync.Read(ref value); return this; }
//   public Message Read(ref NetworkInstanceId value) { sync.Read(ref value); return this; }
//   public Message Read(ref NetworkSceneId value) { sync.Read(ref value); return this; }
//   public Message Read(ref Transform value) { sync.Read(ref value); return this; }
//
// }
