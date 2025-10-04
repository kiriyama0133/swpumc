using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Material.Icons;
using swpumc.ViewModels;

namespace swpumc.Pages.AnimationTest;

public class AnimationTestPageViewModel : BaseViewModel
{
    public static string Title { get; set; } = "动画测试";
    public static string Description { get; set; } = "测试各种动画效果";
    public static MaterialIconKind Icon { get; set; } = MaterialIconKind.Animation;
    public static int Index { get; set; } = 3;

    private string _statusMessage = "准备就绪";
    private int _animationCount = 0;
    private bool _isAnimating = false;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int AnimationCount
    {
        get => _animationCount;
        set => SetProperty(ref _animationCount, value);
    }

    public bool IsAnimating
    {
        get => _isAnimating;
        set => SetProperty(ref _isAnimating, value);
    }

    public AnimationTestPageViewModel()
    {
        // 初始化
    }

    public void UpdateStatus(string message)
    {
        StatusMessage = message;
    }

    public void IncrementAnimationCount()
    {
        AnimationCount++;
    }

    public void SetAnimatingState(bool animating)
    {
        IsAnimating = animating;
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
