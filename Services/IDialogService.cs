using System;
using Avalonia.Controls;
using SukiUI.Dialogs;

namespace swpumc.Services;

public interface IDialogService
{
    /// <summary>
    /// 创建并显示一个Dialog
    /// </summary>
    /// <param name="title">Dialog标题</param>
    /// <param name="content">Dialog内容</param>
    /// <param name="buttonText">按钮文本</param>
    /// <param name="onButtonClick">按钮点击回调</param>
    /// <param name="dismissOnClick">点击按钮是否关闭Dialog</param>
    /// <param name="buttonStyle">按钮样式</param>
    /// <param name="buttonVariant">按钮变体</param>
    /// <param name="dismissOnBackgroundClick">点击背景是否关闭Dialog</param>
    /// <returns>是否成功显示</returns>
    bool ShowDialog(
        string title,
        string content,
        string buttonText = "关闭",
        Action? onButtonClick = null,
        bool dismissOnClick = true,
        string buttonStyle = "Flat",
        string buttonVariant = "Accent",
        bool dismissOnBackgroundClick = true);

    /// <summary>
    /// 创建并显示一个包含自定义控件的Dialog
    /// </summary>
    /// <param name="title">Dialog标题</param>
    /// <param name="contentControl">自定义内容控件</param>
    /// <param name="buttonText">按钮文本</param>
    /// <param name="onButtonClick">按钮点击回调</param>
    /// <param name="dismissOnClick">点击按钮是否关闭Dialog</param>
    /// <param name="buttonStyle">按钮样式</param>
    /// <param name="buttonVariant">按钮变体</param>
    /// <param name="dismissOnBackgroundClick">点击背景是否关闭Dialog</param>
    /// <returns>是否成功显示</returns>
    bool ShowDialogWithControl(
        string title,
        Control contentControl,
        string buttonText = "关闭",
        Action? onButtonClick = null,
        bool dismissOnClick = true,
        string buttonStyle = "Flat",
        string buttonVariant = "Accent",
        bool dismissOnBackgroundClick = true);
}
