using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace swpumc.Models;

public class MinecraftVersion : INotifyPropertyChanged
{
    private string _id = string.Empty;
    private string _type = string.Empty;
    private DateTime _releaseTime;
    private DateTime _time;
    private string _url = string.Empty;
    private bool _isLatest;
    private bool _isRecommended;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public DateTime ReleaseTime
    {
        get => _releaseTime;
        set => SetProperty(ref _releaseTime, value);
    }

    public DateTime Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public bool IsLatest
    {
        get => _isLatest;
        set => SetProperty(ref _isLatest, value);
    }

    public bool IsRecommended
    {
        get => _isRecommended;
        set => SetProperty(ref _isRecommended, value);
    }

    public string DisplayName => Id;
    public string TypeDisplayName => GetTypeDisplayName();
    public string TimeAgo => GetTimeAgo(ReleaseTime);
    public string VersionTypeColor => GetVersionTypeColor();

    private string GetTypeDisplayName()
    {
        return Type switch
        {
            "release" => "正式版",
            "snapshot" => "快照版",
            "old_beta" => "Beta版",
            "old_alpha" => "Alpha版",
            _ => "未知"
        };
    }

    private string GetTimeAgo(DateTime time)
    {
        var timeSpan = DateTime.Now - time;
        if (timeSpan.TotalMinutes < 1) return "刚刚";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}分钟前";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}小时前";
        if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}天前";
        if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)}个月前";
        return $"{(int)(timeSpan.TotalDays / 365)}年前";
    }

    private string GetVersionTypeColor()
    {
        return Type switch
        {
            "release" => "Green",
            "snapshot" => "Orange", 
            "old_beta" => "Blue",
            "old_alpha" => "Purple",
            _ => "Gray"
        };
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
