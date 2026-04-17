using Autofac;
using Autofac.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ImagaLoader.Demo.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ImagaLoader.Demo;

public class App : Application
{
    public static IContainer Container { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeContainer()
    {
        var builder = new ContainerBuilder();
        var servers = new ServiceCollection();


        builder.Populate(servers);
        Container = builder.Build();
    }
}