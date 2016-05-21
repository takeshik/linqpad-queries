<Query Kind="Program" />

void Main()
{
    // LINQPad のプログレス バー機能のサンプル
    var bar = new Util.ProgressBar("Progress Bar").Dump();
    for (var i = 0; i <= 100; i++)
    {
        Thread.Sleep(25);
        bar.Caption = $"Progress Bar (i = {i})";
        bar.Fraction = i / 100d;
    }
}

#region Extend...

// 拡張例: 割合ではなく min / max / value ベースの値指定
public class ProgressBar : Util.ProgressBar
{
    private int _min, _max, _value;
    public int Min
    {
        get { return this._min; }
        set { this._min = value; this.Refresh(); }
    }

    public int Max
    {
        get { return this._max; }
        set { this._max = value; this.Refresh(); }
    }

    public int Value
    {
        get { return this._value; }
        set { this._value = value; this.Refresh(); }
    }

    public ProgressBar(string caption = "", bool hideWhenCompleted = false, int min = 0, int max = 100)
        : base(caption, hideWhenCompleted)
    {
        this.Min = min;
        this.Max = max;
    }

    public void Refresh()
        => this.Fraction
            = this.Max == 0 ? 0 : Math.Max(0, Math.Min((double)(this.Value - this.Min) / this.Max, 1));
}

#endregion