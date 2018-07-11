using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

// helper class for syncing references to networked behaviours
public class SyncRef<T> where T : KittyNetworkBehaviour {

  public MonoBehaviour owner { get; private set; }  // all SyncRef's must have an owner that they can use for their sync coroutines
  public T value { get; private set; }

  public Action<T> readCallback { get; private set; } // the callback to be executed when a value is recieved

  public SyncRef(MonoBehaviour owner, Action<T> readCallback) {
    this.owner = owner;
    this.readCallback = readCallback;
  }

  public void Sync(NetworkSync sync, T value) {
    if (sync.isWriting) {
      sync.Write(value);
    }
    else {
      NetworkInstanceId id = NetworkInstanceId.Invalid;
      sync.Read(ref id);
      if (id.Value <= 0) {
        this.value = null;
        readCallback(null);
      }
      else {
        value = new Id(id).Find<T>();
        if (value != null) {
          this.value = value;
          readCallback(value);
        }
        else {
          owner.StartCoroutine(SyncReadRoutine(new Id(id)));
        }
      }
    }
  }

  IEnumerator SyncReadRoutine(Id id) {
    for (;;) {
      T value = id.Find<T>();
      if (value != null) {
        this.value = value;
        readCallback(value);
        break;
      }
      yield return null;
    }
  }

}

// variant of SyncRef that lets you syncronize an additional piece of data
// the extra data is not automatically serialized
public class SyncRef<T, D> where T : KittyNetworkBehaviour {

  public MonoBehaviour owner { get; private set; }  // all SyncRef's must have an owner that they can use for their sync coroutines
  public T value { get; private set; }

  public Action<T, D> readCallback { get; private set; } // the callback to be executed when a value is recieved

  public SyncRef(MonoBehaviour owner, Action<T, D> readCallback) {
    this.owner = owner;
    this.readCallback = readCallback;
  }

  public void Sync(NetworkSync sync, T value, D data) {
    if (sync.isWriting) {
      sync.Write(value);
    }
    else {
      NetworkInstanceId id = NetworkInstanceId.Invalid;
      sync.Read(ref id);
      if (id.Value <= 0) {
        this.value = null;
        readCallback(null, data);
      }
      else {
        value = new Id(id).Find<T>();
        if (value != null) {
          this.value = value;
          readCallback(value, data);
        }
        else {
          owner.StartCoroutine(SyncReadRoutine(new Id(id), data));
        }
      }
    }
  }

  IEnumerator SyncReadRoutine(Id id, D data) {
    for (;;) {
      T value = id.Find<T>();
      if (value != null) {
        this.value = value;
        readCallback(value, data);
        break;
      }
      yield return null;
    }
  }

}
