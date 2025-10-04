using System;
using Avalonia.Controls;
using SukiUI.Dialogs;

namespace swpumc.Services;

public class DialogService : IDialogService
{
    private readonly ISukiDialogManager _dialogManager;
    private readonly IThemeService _themeService;

    public DialogService(ISukiDialogManager dialogManager, IThemeService themeService)
    {
        _dialogManager = dialogManager;
        _themeService = themeService;
    }

    public bool ShowDialog(
        string title,
        string content,
        string buttonText = "关闭",
        Action? onButtonClick = null,
        bool dismissOnClick = true,
        string buttonStyle = "Flat",
        string buttonVariant = "Accent",
        bool dismissOnBackgroundClick = true)
    {
        try
        {
            var dialogBuilder = _dialogManager.CreateDialog()
                .WithTitle(title)
                .WithContent(content)
                .WithActionButton(buttonText, _ => onButtonClick?.Invoke(), dismissOnClick, buttonStyle, buttonVariant);

            if (dismissOnBackgroundClick)
            {
                dialogBuilder.Dismiss().ByClickingBackground();
            }

            return dialogBuilder.TryShow();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DialogService] Dialog显示异常: {ex.Message}");
            return false;
        }
    }

    public bool ShowDialogWithControl(
        string title,
        Control contentControl,
        string buttonText = "关闭",
        Action? onButtonClick = null,
        bool dismissOnClick = true,
        string buttonStyle = "Flat",
        string buttonVariant = "Accent",
        bool dismissOnBackgroundClick = true)
    {
        try
        {
            var dialogBuilder = _dialogManager.CreateDialog()
                .WithTitle(title)
                .WithContent(contentControl)
                .WithActionButton(buttonText, _ => onButtonClick?.Invoke(), dismissOnClick, buttonStyle, buttonVariant);

            if (dismissOnBackgroundClick)
            {
                dialogBuilder.Dismiss().ByClickingBackground();
            }

            return dialogBuilder.TryShow();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DialogService] Dialog显示异常: {ex.Message}");
            return false;
        }
    }
}
