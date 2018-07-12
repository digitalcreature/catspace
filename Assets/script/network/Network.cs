using UnityEngine;
using UnityEngine.Networking;

public static class Network {

  public static NetworkClient client => NetworkManager.singleton.client;

  public static Id Id(this KittyNetworkBehaviour component) {
    return new Id(component);
  }

  public static T Find<T>(this NetworkInstanceId id) where T : KittyNetworkBehaviour {
    GameObject obj;
    // because for some fucking reason unity has you call different
    // functions for this depending if youre on the client or the server
    if (NetworkClient.active) {
      obj = ClientScene.FindLocalObject(id);
    }
    else {
      obj = NetworkServer.FindLocalObject(id);
    }
    if (obj != null) {
      return obj.GetComponent<T>();
    }
    return null;
  }

}

// this struct lets you use shorthand instead of the cumbersome "NetworkInstanceId" everywhere
public struct Id {

  public uint value;

  public const uint nullValue = 0xFFFFFFFF;

  public bool isNull => value == nullValue;
  public bool isUnspawned => value == 0;

  public Id(uint value) {
    this.value = value;
  }

  public Id(KittyNetworkBehaviour obj) {
    if (obj == null) {
      value = nullValue;
    }
    else {
      NetworkInstanceId id = obj.netId;
      if (id.IsEmpty()) {
        value = 0;
      }
      else {
        value = id.Value;
      }
    }
  }


  public T Find<T>() where T : KittyNetworkBehaviour {
    if (isNull) return null;
    if (isUnspawned) return null;
    NetworkInstanceId id = new NetworkInstanceId(value);
    return id.Find<T>();
  }

}
