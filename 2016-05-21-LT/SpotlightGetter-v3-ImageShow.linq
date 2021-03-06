<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var destinationPath = @"TODO: 画像の保存先ディレクトリのパスを指定";
    var sourcePath = Environment.ExpandEnvironmentVariables(
        @"%LOCALAPPDATA%\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets");

    var sourceDirectory = new DirectoryInfo(sourcePath.Dump("Source"));
    if (!sourceDirectory.Exists)
    {
    	$"Directory not found.".Dump();
    	return;
    }
    
    var destinationDirectory = new DirectoryInfo(destinationPath.Dump("Destination"));
    destinationDirectory.Create();
    
    var landscape = destinationDirectory.CreateSubdirectory("Landscape");
    var portrait = destinationDirectory.CreateSubdirectory("Portrait");

    var files = sourceDirectory.EnumerateFiles().Where(x => x.Length > 100000).ToArray();
    var bar = new ProgressBar("Copying:", max: files.Length).Dump();
    var dcLog = new AppendableDumpContainer().Dump();
    var dcImg = new DumpContainer();
    Util.WithStyle(dcImg, "display: block; zoom: 0.5").Dump();

    for (var i = 0; i < files.Length; ++i)
    {
        bar.Caption = $"Copying: ({i + 1} of {files.Length}):";
        bar.Value = i + 1;
        var file = files[i];
    	using (var image = Image.FromFile(file.FullName))
    	{
    		if (image.Size.Width >= 1920)
    		{
    			var path = Path.Combine(landscape.FullName, $"{file.Name}.jpg");
    			if (!File.Exists(path))
                {
                    dcLog.Append(new Hyperlinq(
                        () => dcImg.Content = Util.Image(path),
                        $"Landscape ({image.Size.Width} x {image.Size.Height}) {path}"));
    				file.CopyTo(path);
    			}
    		}
    		else if (image.Size.Height >= 1920)
    		{
    			var path = Path.Combine(portrait.FullName, $"{file.Name}.jpg");
    			if (!File.Exists(path))
                {
                    dcLog.Append(new Hyperlinq(
                        () => dcImg.Content = Util.Image(path),
                        $"Portrait ({image.Size.Width} x {image.Size.Height}) {path}"));
    				file.CopyTo(path);
    			}
    		}
        }
    }
    bar.Caption = $"Copying: ({files.Length} of {files.Length}) done.";
    bar.Value = bar.Max;
}

public class ProgressBar : Util.ProgressBar
{
    private int _min, _max, _value;
    public int Min
    {
        get { return this._min; }
        set
        {
            this._min = value;
            this.Refresh();
        }
    }

    public int Max
    {
        get { return this._max; }
        set
        {
            this._max = value;
            this.Refresh();
        }
    }

    public int Value
    {
        get { return this._value; }
        set
        {
            this._value = value;
            this.Refresh();
        }
    }

    public ProgressBar(string caption = "", bool hideWhenCompleted = false, int min = 0, int max = 100)
        : base(caption, hideWhenCompleted)
    {
        this.Min = min;
        this.Max = max;
    }

    public void Refresh()
    {
        this.Fraction
            = this.Max == 0 ? 0 : Math.Max(0, Math.Min((double)(this.Value - this.Min) / this.Max, 1));
    }
}

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
