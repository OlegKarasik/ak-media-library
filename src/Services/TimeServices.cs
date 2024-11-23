using System.Collections.Concurrent;
using System.Diagnostics;

namespace MediaLibrary.Extensions.Services;

public static class TimeServices
{
  public class Measurement
  {
    public required TimeSpan Elapsed
    {
      get; init;
    }
  }

  public class Measurement<T> : Measurement
  {
    public required T Data
    {
      get; init;
    }
  }

  private static readonly ConcurrentBag<Stopwatch> watches;

  static TimeServices()
  {
    watches = [];
  }

  private static Stopwatch Rent()
  {
    return watches.TryTake(out var result) ? result : new Stopwatch();
  }

  private static void Return(
    Stopwatch watch)
  {
    watch.Reset();
    watches.Add(watch);
  }

  public static Measurement Measure(
    Action action)
  {
    var w = Rent();
    w.Start();
    try
    {
      action();
      return new Measurement { Elapsed = w.Elapsed };
    }
    finally
    {
      Return(w);
    }
  }

  public static Measurement<T> Measure<T>(
    Func<T> action)
  {
    var w = Rent();
    w.Start();
    try
    {
      return new Measurement<T>
      {
        Data = action(),
        Elapsed = w.Elapsed
      };
    }
    finally
    {
      Return(w);
    }
  }

  public static async Task<Measurement> MeasureAsync(
    Func<Task> action)
  {
    var w = Rent();
    w.Start();
    try
    {
      await action();
      return new Measurement { Elapsed = w.Elapsed };
    }
    finally
    {
      Return(w);
    }
  }

  public static async Task<Measurement<T>> MeasureAsync<T>(
    Func<Task<T>> action)
  {
    var w = Rent();
    w.Start();
    try
    {
      return new Measurement<T>
      {
        Data = await action(),
        Elapsed = w.Elapsed
      };
    }
    finally
    {
      Return(w);
    }
  }
}
