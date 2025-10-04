using CommunityToolkit.Mvvm.ComponentModel;

namespace swpumc.Controls.GameLauncher;

/// <summary>
/// 启动配置Tab的ViewModel
/// </summary>
public class LaunchConfigTabViewModel : ObservableObject
{
    public string Header { get; }
    public string Content { get; }
    public bool IsEnabled { get; } = true;

    public LaunchConfigTabViewModel(string header, string content)
    {
        Header = header;
        Content = content;
    }
}
