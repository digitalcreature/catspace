using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

public static class TaskManager {

  public static bool workersAreRunning => workers != null;

  static ConcurrentPriorityQueue<float, ITask> tasks = new ConcurrentPriorityQueue<float, ITask>();

  static ManualResetEvent tasksAvailableEvent = new ManualResetEvent(false);

  static bool joinRequested;

  static Thread[] workers;

  public static Task<T> Schedule<T>(float priority, Func<T> f) {
    if (f == null) throw new ArgumentException();
    Task<T> task = new Task<T>(f);
    tasks.Enqueue(priority, task);
    tasksAvailableEvent.Set();
    return task;
  }

  public static void StartWorkers(int n = 8) {
    JoinWorkers();
    workers = new Thread[n];
    joinRequested = false;
    tasksAvailableEvent.Reset();
    for (int i = 0; i < n; i ++) {
      Thread worker = new Thread(Worker);
      workers[i] = worker;
      worker.Start();
    }
  }

  public static void JoinWorkers() {
    if (workers != null) {
      joinRequested = true;
      foreach (Thread worker in workers) {
        worker.Join();
      }
      workers = null;
    }
  }

  public static void AbortWorkers() {
    if (workers != null) {
      foreach (Thread worker in workers) {
        worker.Abort();
      }
      workers = null;
    }
  }

  static void Worker() {
    while (true) {
      KeyValuePair<float, ITask> pair;
      while (tasks.TryDequeue(out pair)) {
        pair.Value.Run();
      }
      if (joinRequested) {
        return;
      }
      else {
        tasksAvailableEvent.Reset();
        tasksAvailableEvent.WaitOne();
      }
    }
  }

}

public class Task<T> : ITask {

  public bool isDone { get; private set; } = false;

  public T result { get; private set; }

  Func<T> task;    // the task to perform

  public Task(Func<T> task) {
    this.task = task;
  }

  public void Run() {
    result = task();
    isDone = true;
  }

}

interface ITask {

  void Run();

}
