using UnityEngine;
using UnityEngine.Networking;

public static class Network {

  public static NetworkInstanceId GetInstanceId(this NetworkBehaviour component) {
    if (component == null) {
      return NetworkInstanceId.Invalid;
    }
    else {
      return component.netId;
    }
  }

  public static T FindLocalObject<T>(this NetworkInstanceId id) where T : NetworkBehaviour {
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
