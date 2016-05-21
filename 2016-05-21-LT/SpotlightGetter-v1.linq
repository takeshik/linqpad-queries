<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
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

    // いろいろゴミが混ざるので、100 KB 以上かつ Full-HD 以上の画像を引っ張ってくるようにしてみる
    foreach (var file in sourceDirectory.GetFiles().Where(x => x.Length > 100000))
    {
        using (var image = Image.FromFile(file.FullName))
        {
            if (image.Size.Width >= 1920)
            {
                var path = Path.Combine(landscape.FullName, $"{file.Name}.jpg");
                if (!File.Exists(path))
                {
                    $"Landscape ({image.Size.Width} x {image.Size.Height}) {path}".Dump();
                    file.CopyTo(path);
                }
            }
            else if (image.Size.Height >= 1920)
            {
                var path = Path.Combine(portrait.FullName, $"{file.Name}.jpg");
                if (!File.Exists(path))
                {
                    $"Portrait ({image.Size.Width} x {image.Size.Height}) {path}".Dump();
                    file.CopyTo(path);
                }
            }
        }
    }
}