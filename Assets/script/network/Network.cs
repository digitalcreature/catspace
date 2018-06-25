using UnityEngine;
using UnityEngine.Networking;

public static class Network {

  public static NetworkClient client => NetworkManager.singleton.client;

  public static NetworkInstanceId Id(this KittyNetworkBehaviour component) {
    if (component == null) {
      return NetworkInstanceId.Invalid;
    }
    else {
      return component.netId;
    }
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

  public NetworkInstanceId id;

  public Id(NetworkInstanceId id) {
    this.id = id;
  }

  public static implicit operator Id (NetworkInstanceId id) => new Id(id);
  public static implicit operator NetworkInstanceId (Id id) => id.id;

  public T Find<T>() where T : KittyNetworkBehaviour {
    return id.Find<T>();
  }

}
