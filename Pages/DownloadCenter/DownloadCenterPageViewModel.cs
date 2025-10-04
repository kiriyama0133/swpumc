using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using swpumc.Models;
using swpumc.Services;
using swpumc.ViewModels;

namespace swpumc.Pages.DownloadCenter;

public class DownloadCenterPageViewModel : BaseViewModel, INotifyPropertyChanged
{
    public static string Title { get; set; } = "下载中心";
    public static string Description { get; set; } = "Download Center";
    public static MaterialIconKind Icon { get; set; } = MaterialIconKind.Download;
    public static int Index { get; set; } = 2;

    private readonly IMinecraftVersionService _versionService;
    private ObservableCollection<MinecraftVersion> _versions = new();
    private ObservableCollection<MinecraftVersion> _displayedVersions = new();
    private int _releaseCount;
    private int _snapshotCount;
    private int _totalCount;
    private bool _isLoading;
    private const int PageSize = 20; // 每页显示20个版本，减少初始渲染负担
    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMorePages = true;
    public ObservableCollection<MinecraftVersion> Versions
    {
        get => _displayedVersions;
        set => SetProperty(ref _displayedVersions, value);
    }

    public int ReleaseCount
    {
        get => _releaseCount;
        set => SetProperty(ref _releaseCount, value);
    }

    public int SnapshotCount
    {
        get => _snapshotCount;
        set => SetProperty(ref _snapshotCount, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsLoadingMore
    {
        get => _isLoadingMore;
        set => SetProperty(ref _isLoadingMore, value);
    }

    public bool HasMorePages
    {
        get => _hasMorePages;
        set => SetProperty(ref _hasMorePages, value);
    }

    public ICommand RefreshVersionsCommand { get; }
    public ICommand FilterVersionsCommand { get; }
    public ICommand LoadMoreVersionsCommand { get; }

    public DownloadCenterPageViewModel(IMinecraftVersionService versionService)
    {
        Console.WriteLine($"[DownloadCenterPageViewModel] 构造函数开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        _versionService = versionService;
        RefreshVersionsCommand = new AsyncRelayCommand(RefreshVersionsAsync);
        FilterVersionsCommand = new RelayCommand(FilterVersions);
        LoadMoreVersionsCommand = new AsyncRelayCommand(LoadMoreVersionsAsync);
        
        Console.WriteLine($"[DownloadCenterPageViewModel] 命令创建完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        // 异步初始化，不阻塞构造函数
        Console.WriteLine($"[DownloadCenterPageViewModel] 开始异步初始化 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        _ = Task.Run(async () => await InitializeAsync());
        Console.WriteLine($"[DownloadCenterPageViewModel] 构造函数结束 - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task InitializeAsync()
    {
        Console.WriteLine($"[DownloadCenterPageViewModel] InitializeAsync开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        await LoadVersionsAsync();
        Console.WriteLine($"[DownloadCenterPageViewModel] InitializeAsync结束 - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task LoadVersionsAsync()
    {
        Console.WriteLine($"[LoadVersionsAsync] 方法开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        try
        {
            // 立即在UI线程设置加载状态
            Console.WriteLine($"[LoadVersionsAsync] 设置IsLoading=true - 时间: {DateTime.Now:HH:mm:ss.fff}");
            IsLoading = true;
            
            // 在后台线程执行网络请求
            Console.WriteLine($"[LoadVersionsAsync] 开始Task.Run网络请求 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            var versions = await Task.Run(async () => await _versionService.GetAllVersionsAsync());
            Console.WriteLine($"[LoadVersionsAsync] Task.Run网络请求完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            
            // 在UI线程更新UI
            Console.WriteLine($"[LoadVersionsAsync] 开始UI线程更新 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Console.WriteLine($"[LoadVersionsAsync] UI线程更新开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
                
                // 保存所有版本到内部集合
                _versions.Clear();
                foreach (var version in versions)
                {
                    _versions.Add(version);
                }
                
                // 只显示第一页的版本
                LoadPage(0);
                
                UpdateCounts();
                IsLoading = false;
                Console.WriteLine($"[LoadVersionsAsync] UI线程更新完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            });
            Console.WriteLine($"[LoadVersionsAsync] UI线程更新调用完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadVersionsAsync] 异常: {ex.Message} - 时间: {DateTime.Now:HH:mm:ss.fff}");
            // 确保在异常情况下也重置加载状态
            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = false);
        }
        Console.WriteLine($"[LoadVersionsAsync] 方法结束 - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task RefreshVersionsAsync()
    {
        try
        {
            // 立即在UI线程设置加载状态
            IsLoading = true;
            
            // 在后台线程执行刷新请求
            await Task.Run(async () => await _versionService.RefreshVersionsAsync());
            
            // 重新获取版本数据
            var versions = await Task.Run(async () => await _versionService.GetAllVersionsAsync());
            
            // 在UI线程更新UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Versions.Clear();
                
                foreach (var version in versions)
                {
                    Versions.Add(version);
                }
                
                UpdateCounts();
                IsLoading = false;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"刷新版本失败: {ex.Message}");
            // 确保在异常情况下也重置加载状态
            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = false);
        }
    }

    private async Task LoadMoreVersionsAsync()
    {
        if (_isLoadingMore || !_hasMorePages)
        {
            Console.WriteLine($"[LoadMoreVersionsAsync] 跳过加载 - 正在加载:{_isLoadingMore}, 有更多页面:{_hasMorePages}");
            return;
        }

        try
        {
            Console.WriteLine($"[LoadMoreVersionsAsync] 开始加载更多版本 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            IsLoadingMore = true;
            
            // 在后台线程执行加载
            await Task.Run(async () =>
            {
                await Task.Delay(500); // 模拟加载延迟，让用户看到加载状态
            });
            
            // 在UI线程更新UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var nextPage = _currentPage + 1;
                LoadPage(nextPage, append: true); // 使用追加模式
                Console.WriteLine($"[LoadMoreVersionsAsync] 加载第{nextPage}页完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadMoreVersionsAsync] 异常: {ex.Message} - 时间: {DateTime.Now:HH:mm:ss.fff}");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private void FilterVersions()
    {
        // 实现版本筛选逻辑
        Console.WriteLine("筛选版本");
    }

    private void LoadPage(int page, bool append = false)
    {
        Console.WriteLine($"[LoadPage] 加载第{page}页，追加模式:{append} - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        // 如果不是追加模式，清空现有项目
        if (!append)
        {
            _displayedVersions.Clear();
        }
        
        var startIndex = page * PageSize;
        var endIndex = Math.Min(startIndex + PageSize, _versions.Count);
        
        // 批量添加，减少UI更新次数
        var itemsToAdd = new List<MinecraftVersion>();
        for (int i = startIndex; i < endIndex; i++)
        {
            itemsToAdd.Add(_versions[i]);
        }
        
        // 一次性添加所有项目
        foreach (var item in itemsToAdd)
        {
            _displayedVersions.Add(item);
        }
        
        _currentPage = page;
        
        // 检查是否还有更多页面
        var totalPages = (int)Math.Ceiling((double)_versions.Count / PageSize);
        HasMorePages = page < totalPages - 1;
        
        Console.WriteLine($"[LoadPage] 第{page}页加载完成，显示{_displayedVersions.Count}个版本，总页数:{totalPages}，还有更多:{HasMorePages} - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }

    private void UpdateCounts()
    {
        ReleaseCount = _versions.Count(v => v.Type == "release");
        SnapshotCount = _versions.Count(v => v.Type == "snapshot");
        TotalCount = _versions.Count;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
