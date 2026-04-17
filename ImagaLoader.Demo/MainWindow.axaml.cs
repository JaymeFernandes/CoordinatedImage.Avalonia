using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ImagaLoader.Demo.ViewModels;

namespace ImagaLoader.Demo;

public partial class MainWindow : Window
{
    private static readonly string[] ImageExtensions =
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".webp"
    };

    public MainWindow()
    {
        InitializeComponent();
        
        
        

        //_ = SelectFolder();
    }

    private bool IsImage(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ImageExtensions.Contains(ext);
    }

    private async Task SelectFolder()
    {
        var result = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Escolha uma pasta"
            });

        var folder = result.FirstOrDefault();

        if (folder != null) Console.WriteLine(folder.Path.LocalPath);

        var files = Directory.GetFiles(folder.Path.LocalPath).Where(IsImage);

        if (DataContext is MainWindowViewModel vm)
        {
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }

            foreach (var file in files)
                vm.Paths.Add(file);
        }
    }
}