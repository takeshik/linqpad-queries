<Query Kind="Program">
  <NuGetReference>UniRx</NuGetReference>
  <Namespace>UniRx</Namespace>
</Query>

/*
 * Rx-UniRx-Bridge, written by takeshik
 * See: https://github.com/takeshik/linqpad-queries
 */

void Main()
{
    Observable.Interval(TimeSpan.FromSeconds(1))
        .Take(3)
        .DoOnCompleted(() => "completed!".Dump())
        .AsSystemObservable()
        .Dump();
}

public static class UniRxBridgeExtensions
{
    private class SystemObservable<T> : System.IObservable<T>
    {
        private readonly UniRx.IObservable<T> _inner;

        public SystemObservable(UniRx.IObservable<T> inner)
        {
            this._inner = inner;
        }

        public IDisposable Subscribe(System.IObserver<T> observer)
            => this._inner.Subscribe(new UniRxObserver<T>(observer));
    }

    private class UniRxObservable<T> : UniRx.IObservable<T>
    {
        private readonly System.IObservable<T> _inner;

        public UniRxObservable(System.IObservable<T> inner)
        {
            this._inner = inner;
        }

        public IDisposable Subscribe(UniRx.IObserver<T> observer)
            => this._inner.Subscribe(new SystemObserver<T>(observer));
    }

    private class SystemObserver<T> : System.IObserver<T>
    {
        private readonly UniRx.IObserver<T> _inner;

        public SystemObserver(UniRx.IObserver<T> inner)
        {
            this._inner = inner;
        }

        public void OnNext(T value)
            => this._inner.OnNext(value);

        public void OnError(Exception error)
            => this._inner.OnError(error);

        public void OnCompleted()
            => this._inner.OnCompleted();
    }

    private class UniRxObserver<T> : UniRx.IObserver<T>
    {
        private readonly System.IObserver<T> _inner;

        public UniRxObserver(System.IObserver<T> inner)
        {
            this._inner = inner;
        }

        public void OnNext(T value)
            => this._inner.OnNext(value);

        public void OnError(Exception error)
            => this._inner.OnError(error);

        public void OnCompleted()
            => this._inner.OnCompleted();
    }

    public static System.IObserver<TSource> AsSystemObserver<TSource>
        (this UniRx.IObserver<TSource> observer)
        => new SystemObserver<TSource>(observer);

    public static UniRx.IObserver<TSource> AsUniRxObserver<TSource>
        (this System.IObserver<TSource> observer)
        => new UniRxObserver<TSource>(observer);

    public static System.IObservable<TSource> AsSystemObservable<TSource>
        (this UniRx.IObservable<TSource> source)
        => new SystemObservable<TSource>(source);

    public static UniRx.IObservable<TSource> AsUniRxObservable<TSource>
        (this System.IObservable<TSource> source)
        => new UniRxObservable<TSource>(source);
}