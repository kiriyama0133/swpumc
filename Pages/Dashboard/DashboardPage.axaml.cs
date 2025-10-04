using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Pages.Dashboard;
using swpumc.Services;

namespace swpumc.Pages.Dashboard;

public partial class DashboardPage : BasePage
{
    public DashboardPage()
    {
        InitializeComponent();
        // 从依赖注入容器获取DialogService
        var app = App.Current as App;
        var dialogService = app?.Services?.GetService<IDialogService>();
        DataContext = new DashboardPageViewModel(dialogService!);
    }

    private void OnAnnouncementCardClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is SukiUI.Controls.GlassCard card && card.DataContext is Announcement announcement)
        {
            if (DataContext is DashboardPageViewModel viewModel)
            {
                viewModel.ShowAnnouncementDetail(announcement);
            }
        }
    }
}
