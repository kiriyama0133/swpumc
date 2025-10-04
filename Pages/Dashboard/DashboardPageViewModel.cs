using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Material.Icons;
using swpumc.ViewModels;
using swpumc.Services;
using swpumc.Utils;

namespace swpumc.Pages.Dashboard;

public class ServerInfo : BaseViewModel
{
    private string _name = string.Empty;
    private string _type = string.Empty;
    private int _playerCount;
    private int _maxPlayers;
    private string _status = string.Empty;
    private string _lastUpdate = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public int PlayerCount
    {
        get => _playerCount;
        set => SetProperty(ref _playerCount, value);
    }

    public int MaxPlayers
    {
        get => _maxPlayers;
        set => SetProperty(ref _maxPlayers, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string LastUpdate
    {
        get => _lastUpdate;
        set => SetProperty(ref _lastUpdate, value);
    }

    public string PlayerInfo => $"{PlayerCount}/{MaxPlayers} 玩家在线";

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

public class Announcement : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _content = string.Empty;
    private string _author = string.Empty;
    private DateTime _publishTime;
    private string _type = string.Empty;
    private string _markdownContent = string.Empty;
    private string _fileName = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public string Author
    {
        get => _author;
        set => SetProperty(ref _author, value);
    }

    public DateTime PublishTime
    {
        get => _publishTime;
        set => SetProperty(ref _publishTime, value);
    }

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public string MarkdownContent
    {
        get => _markdownContent;
        set => SetProperty(ref _markdownContent, value);
    }

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string TimeAgo => GetTimeAgo(PublishTime);

    private string GetTimeAgo(DateTime time)
    {
        var timeSpan = DateTime.Now - time;
        if (timeSpan.TotalMinutes < 1) return "刚刚";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}分钟前";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}小时前";
        return $"{(int)timeSpan.TotalDays}天前";
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

public class DashboardPageViewModel : BaseViewModel
{
    public static string Title { get; set; } = "主页";
    public static string Description { get; set; } = "服务器监控面板";
    public static MaterialIconKind Icon { get; set; } = MaterialIconKind.ViewDashboard;
    public static int Index { get; set; } = 0;

    private readonly IDialogService _dialogService;
    private ObservableCollection<ServerInfo> _servers = new();
    private ObservableCollection<Announcement> _announcements = new();

    public ObservableCollection<ServerInfo> Servers
    {
        get => _servers;
        set => SetProperty(ref _servers, value);
    }

    public ObservableCollection<Announcement> Announcements
    {
        get => _announcements;
        set => SetProperty(ref _announcements, value);
    }

    public DashboardPageViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        // 初始化服务器数据
        Servers.Add(new ServerInfo
        {
            Name = "生存服务器",
            Type = "Survival",
            PlayerCount = 24,
            MaxPlayers = 50,
            Status = "在线",
            LastUpdate = "2分钟前"
        });

        Servers.Add(new ServerInfo
        {
            Name = "创造服务器",
            Type = "Creative",
            PlayerCount = 8,
            MaxPlayers = 30,
            Status = "在线",
            LastUpdate = "1分钟前"
        });

        Servers.Add(new ServerInfo
        {
            Name = "GTNH服务器",
            Type = "GTNH",
            PlayerCount = 12,
            MaxPlayers = 20,
            Status = "在线",
            LastUpdate = "3分钟前"
        });

        // 初始化Markdown公告数据
        LoadMarkdownAnnouncements();
    }

    private void LoadMarkdownAnnouncements()
    {
        try
        {
            var announcementPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Announcement");
            
            if (Directory.Exists(announcementPath))
            {
                var markdownFiles = Directory.GetFiles(announcementPath, "*.md");
                
                foreach (var file in markdownFiles)
                {
                    try
                    {
                        // 使用通用解析器解析Markdown文件
                        var announcementInfo = MarkdownAnnouncementParser.ParseMarkdownFile(file);
                        
                        // 转换为Announcement对象
                        var announcement = new Announcement
                        {
                            FileName = announcementInfo.FileName,
                            Title = announcementInfo.Title,
                            Content = announcementInfo.Preview,
                            Author = announcementInfo.Author,
                            PublishTime = announcementInfo.PublishTime,
                            Type = announcementInfo.Type,
                            MarkdownContent = announcementInfo.FullContent
                        };
                        
                        Announcements.Add(announcement);
                        
                    }
                    catch (Exception)
                    {
                    }
                }
                
                
                // 按发布时间排序（最新的在前）
                var sortedAnnouncements = new ObservableCollection<Announcement>(
                    Announcements.OrderByDescending(a => a.PublishTime));
                Announcements = sortedAnnouncements;
                
            }
            else
            {
            }
        }
        catch (Exception)
        {
        }
    }



        public void ShowAnnouncementDetail(Announcement announcement)
        {
            // 如果有Markdown内容，使用Markdown渲染
            if (!string.IsNullOrEmpty(announcement.MarkdownContent))
            {
                ShowMarkdownAnnouncementDetail(announcement);
            }
            else
            {
                // 使用普通文本显示
                _dialogService.ShowDialog(
                    title: announcement.Title,
                    content: $"{announcement.Content}\n\n发布者: {announcement.Author}\n发布时间: {announcement.TimeAgo}",
                    buttonText: "关闭",
                    buttonStyle: "Flat",
                    buttonVariant: "Accent"
                );
            }
        }

        private void ShowMarkdownAnnouncementDetail(Announcement announcement)
        {
            try
            {
                // 使用Markdown渲染器创建控件，不限制尺寸让内容自然展开
                var markdownControl = MarkdownRenderer.RenderMarkdownWithAST(announcement.MarkdownContent);
                
                // 创建ScrollViewer直接包含Markdown内容，设置合适的最大尺寸
                var scrollViewer = new ScrollViewer
                {
                    Content = markdownControl,
                    MaxHeight = 700,
                    MaxWidth = 900
                };
                
                // 使用自定义控件显示Dialog，不显示标题
                _dialogService.ShowDialogWithControl(
                    title: "",
                    contentControl: scrollViewer,
                    buttonText: "关闭",
                    buttonStyle: "Flat",
                    buttonVariant: "Accent"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardPageViewModel] Markdown渲染失败: {ex.Message}");
                
                // 降级到普通文本显示，不显示标题
                _dialogService.ShowDialog(
                    title: "",
                    content: announcement.Content,
                    buttonText: "关闭",
                    buttonStyle: "Flat",
                    buttonVariant: "Accent"
                );
            }
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
