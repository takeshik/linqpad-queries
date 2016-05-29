<Query Kind="Program" />

void Main()
{
    const string imagePath1 = "TODO: 画像ファイルのパスを指定";
    const string imagePath2 = "TODO: 別の画像ファイルのパスを指定";
    // クリックして処理を実行・画像表示・プレースホルダのサンプル
    new Hyperlinq(() =>
    {
        var dc = new DumpContainer().Dump();
        // 雑な実装してるので、止めるときはメニューの [Query] > [Cancel All Threads and Reset] で
        while (true)
        {
            dc.Content = Util.Image(imagePath1);
            Thread.Sleep(500);
            dc.Content = Util.Image(imagePath2);
            Thread.Sleep(500);
        }
    }, "LET'S DANCE!").Dump();
}

#region Extend...

public class AppendableDumpContainer : DumpContainer
{
    private DumpContainer _head;
    private DumpContainer _current;

    public AppendableDumpContainer()
    {
        this._head = this._current = new DumpContainer();
    }

    public void Append(object obj)
    {
        var dc = new DumpContainer();
        this._current.Content = Util.VerticalRun(obj, dc);
        this._current = dc;
        this.Content = this._head;
    }
}
#endregion