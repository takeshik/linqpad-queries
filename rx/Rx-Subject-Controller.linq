<Query Kind="Program">
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
</Query>

/*
 * Rx-Subject-Controller, written by takeshik
 * See: https://github.com/takeshik/linqpad-queries
 */

void Main()
{
    var xs = new Subject<int>();
    var ys = new Subject<int>();

    Util.HorizontalRun("xs, ys", xs.WithController(), ys.WithController())
        .Dump();

    Util.HorizontalRun("xs.Zip(ys), ys.CombineLatest(ys)",
        xs.Zip(ys, (x, y) => new { x, y, }),
        xs.CombineLatest(ys, (x, y) => new { x, y, })
    ).Dump();
}

public static class ObserverExtensions
{
    public static object Controller<T>(
        this IObserver<T> observer,
        Func<T> nextValueGenerator = null,
        Func<Exception> errorGenerator = null)
    {
        return Util.HorizontalRun(true,
            new Hyperlinq(() => observer.OnNext((nextValueGenerator ?? (() =>
            {
                var t = typeof(T);
                if (t.Name == "Unit" && t.IsValueType)
                {
                    return default(T);
                }
                return Util.ReadLine<T>("Next value:", default(T));
            }))()), "Next"),
            new Hyperlinq(() => observer.OnError((errorGenerator ?? (() =>
                new Exception(Util.ReadLine("Error message:")))
            )()), "Error"),
            new Hyperlinq(() => observer.OnCompleted(), "Completed")
        );
    }

    public static object WithController<T>(
        this IObserver<T> observer,
        Func<T> nextValueGenerator = null,
        Func<Exception> errorGenerator = null)
    {
        return Util.VerticalRun(
            observer.Controller(),
            observer
        );
    }

    public static IObserver<T> DumpWithController<T>(
        this IObserver<T> observer,
        string header = null)
    {
        observer.WithController().Dump(header);
        return observer;
    }
}