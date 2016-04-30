<Query Kind="Program">
  <NuGetReference>Rx-Main</NuGetReference>
  <NuGetReference>Rx-Testing</NuGetReference>
  <Namespace>System</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Concurrency</Namespace>
  <Namespace>System.Reactive.Disposables</Namespace>
  <Namespace>System.Reactive.Joins</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive.PlatformServices</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>System.Reactive.Threading.Tasks</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Microsoft.Reactive.Testing</Namespace>
</Query>

void Main()
{
    // 他のシーケンス止める用
    var stopper = new Subject<Unit>();
    new Hyperlinq(() => stopper.OnNext(Unit.Default), "stop!").Dump();
    
    // スケジューラ
    var scheduler = new TestScheduler();
    scheduler.WithController().Dump();
    
    // シーケンスの用意
    var xs = Observable.Interval(TimeSpan.FromSeconds(0.3), scheduler)
        .TakeUntil(stopper)
        .Record(scheduler, "X");
    var ys = Observable.Interval(TimeSpan.FromSeconds(0.4), scheduler)
        .TakeUntil(stopper)
        .Record(scheduler, "Y");
    var zs = Observable.Interval(TimeSpan.FromSeconds(0.5), scheduler)
        .TakeUntil(stopper)
        .Record(scheduler, "Z");
    var xyzs = Observable.Merge(xs, ys, zs);
    
    // 表示
    var dc = new DumpContainer();
    Util.HorizontalRun(false, xyzs, dc).Dump(); // 水平配置用
    xyzs.ToArray().Subscribe(recs => dc.Content = recs.Visualize());
}

public class RecordEntry : IEquatable<RecordEntry>
{
    public long Time { get; private set; }
    public NotificationKind Kind { get; private set; }
    public object Value { get; private set; }
    public bool HasValue { get; private set; }
    public Exception Exception { get; private set; }
    public string Tag { get; private set; }

    public static RecordEntry Create<T>(long time, Notification<T> notification, string tag = null)
        => new RecordEntry()
        {
            Time = time,
            Kind = notification.Kind,
            Value = notification.HasValue ? (object)notification.Value : null,
            HasValue = notification.HasValue,
            Exception = notification.Exception,
            Tag = tag,
        };

    public bool Equals(RecordEntry other)
        => this.Time == other.Time
            && this.Kind == other.Kind
            && this.Value.Equals(other.Value)
            && this.Exception == other.Exception
            && this.Tag == other.Tag;
}

public class TypedDumpContainer<T> : DumpContainer
{
    public new T Content
    {
        get { return (T)base.Content; }
        set { base.Content = value; }
    }

    public TypedDumpContainer() { }
    public TypedDumpContainer(T initialContent) : base(initialContent) { }

    public void Refresh(Action<T> action)
    {
        action(this.Content);
        this.Refresh();
    }
}

public static class Extensions
{
    public static IObservable<RecordEntry> Record<TSource>(
        this IObservable<TSource> source,
        IScheduler scheduler,
        string tag = null)
        => source
            .Materialize()
            .Select(x => RecordEntry.Create(scheduler.Now.Ticks, x, tag));

    public static IEnumerable<object> Visualize(this IEnumerable<RecordEntry> records)
        => records.GroupBy(x => x.Time, (k, xs) => new
        {
            Time = k,
            Events = Util.HorizontalRun(true, xs
                .OrderBy(x => x.Kind)
                .ThenBy(x => x.Tag)
                .Select(x => new { x.Kind, x.Value, }
                    .Popup(x.Kind == NotificationKind.OnNext ? x.Tag : $"{x.Kind}({x.Tag})"))
            ),
        });

    public static object WithController(this TestScheduler sched)
    {
        var dc = sched.ToDumpContainer();
        return Util.VerticalRun(
            Util.HorizontalRun(true,
                new Hyperlinq(() => dc.Refresh(x => x.AdvanceBy(TimeSpan.FromMilliseconds(100).Ticks)), "+0.1s"),
                new Hyperlinq(() => dc.Refresh(x => x.AdvanceBy(TimeSpan.FromMilliseconds(250).Ticks)), "+0.25s"),
                new Hyperlinq(() => dc.Refresh(x => x.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)), "+0.5s"),
                new Hyperlinq(() => dc.Refresh(x => x.AdvanceBy(TimeSpan.FromMilliseconds(1000).Ticks)), "+1.0s"),
                new Hyperlinq(() => dc.Refresh(x => x.AdvanceBy(TimeSpan.FromMilliseconds(10000).Ticks)), "+10s")
            ),
            dc
        );
    }

    public static TypedDumpContainer<T> ToDumpContainer<T>(this T self)
        => new TypedDumpContainer<T>(self);

    public static object Popup(this object obj, string text)
    {
        var visible = false;
        var dc = new DumpContainer(obj);
        dc.Style = "display: none";
        return Util.VerticalRun(
            new Hyperlinq(() => dc.Style = (visible = !visible) ? "" : "display: none", text),
            dc);
    }
}