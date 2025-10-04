using System;
using Avalonia.Controls;
using swpumc.Models;
using swpumc.Services;

namespace swpumc.Controls.VersionCard;

public partial class VersionCard : UserControl
{
    private VersionServiceFactory? _versionServiceFactory;

    public VersionCard()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (DataContext is MinecraftVersion version)
        {
            // 从依赖注入容器获取服务
            var serviceProvider = (Avalonia.Application.Current as App)?.Services;
            if (serviceProvider != null)
            {
                var configService = serviceProvider.GetService(typeof(IConfigService)) as IConfigService;
                var minecraftVersionService = serviceProvider.GetService(typeof(MinecraftVersionService)) as MinecraftVersionService;
                
                if (configService != null && minecraftVersionService != null)
                {
                    var javaEnvironmentService = serviceProvider.GetService(typeof(IJavaEnvironmentService)) as IJavaEnvironmentService;
                    if (javaEnvironmentService != null)
                    {
                        _versionServiceFactory ??= new VersionServiceFactory(configService, javaEnvironmentService, minecraftVersionService);
                    }
                }
            }
            
            DataContext = new VersionCardViewModel(version, _versionServiceFactory);
        }
    }
}
