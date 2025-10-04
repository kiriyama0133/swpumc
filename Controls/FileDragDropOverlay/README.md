# 文件拖拽覆盖层控件

这是一个用于显示文件拖拽提示的Avalonia控件，当用户拖拽文件到窗口时会显示一个模糊背景的提示界面。

## 功能特性

- 🎨 **美观的UI设计** - 现代化的模糊背景和圆角设计
- 📁 **文件类型验证** - 支持 .zip 和 .mrpack 格式的整合包文件
- 📊 **文件信息显示** - 显示文件名和文件大小
- 🔄 **实时状态更新** - 根据拖拽状态动态显示内容
- 🎯 **事件处理** - 完整的拖拽事件处理机制

## 使用方法

### 1. 在XAML中使用

```xml
<dragDrop:FileDragDropOverlay Name="FileDragDropOverlay"
                              FileDrop="OnFileDrop"/>
```

### 2. 在代码中处理事件

```csharp
private async void OnFileDrop(object? sender, FileDragEventArgs e)
{
    var filePath = e.FilePath;
    if (!string.IsNullOrEmpty(filePath))
    {
        // 处理文件拖拽逻辑
        Console.WriteLine($"检测到文件: {filePath}");
        
        // 验证文件类型
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".zip" || extension == ".mrpack")
        {
            // 处理整合包文件
            await ProcessModpackFile(filePath);
        }
    }
}
```

### 3. 手动控制显示状态

```csharp
// 显示拖拽提示
FileDragDropOverlay.ShowDragDropOverlay();

// 隐藏拖拽提示
FileDragDropOverlay.HideDragDropOverlay();

// 设置文件信息
FileDragDropOverlay.SetFileInfo("example.zip");
```

## 控件结构

### FileDragDropOverlay.axaml
- 主控件XAML定义
- 包含模糊背景和提示内容
- 响应式布局设计

### FileDragDropOverlay.axaml.cs
- 控件代码后台
- 事件处理逻辑
- 公共API接口

### FileDragDropOverlayViewModel.cs
- 视图模型
- 数据绑定属性
- 业务逻辑处理

## 样式定制

控件使用以下主要样式：

- **背景模糊**: `BlurEffect Radius="15"`
- **主容器**: 白色半透明背景，圆角20px
- **阴影效果**: `DropShadowEffect` 提供深度感
- **颜色主题**: 绿色主题色 `#4CAF50`

## 事件说明

- `FileDragEnter` - 文件拖拽进入事件
- `FileDragLeave` - 文件拖拽离开事件  
- `FileDrop` - 文件放下事件

## 注意事项

1. 确保在主窗口中正确设置拖拽事件处理
2. 文件路径验证建议在事件处理中进行
3. 控件会自动处理拖拽状态的显示和隐藏
4. 支持的文件格式：.zip, .mrpack

## 集成示例

在主窗口中的完整集成示例：

```xml
<!-- 在主窗口XAML中 -->
<dragDrop:FileDragDropOverlay Name="FileDragDropOverlay"
                              FileDrop="OnFileDrop"/>
```

```csharp
// 在主窗口代码后台中
private async void OnFileDrop(object? sender, FileDragEventArgs e)
{
    try
    {
        var filePath = e.FilePath;
        if (string.IsNullOrEmpty(filePath)) return;

        // 验证文件格式
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".zip" && extension != ".mrpack")
        {
            ShowErrorToast("文件格式不支持，请拖拽 .zip 或 .mrpack 格式的整合包文件");
            return;
        }

        // 处理整合包文件
        ShowSuccessToast($"检测到整合包文件: {Path.GetFileName(filePath)}");
        
        // 这里可以调用整合包安装服务
        // await InstallModpackAsync(filePath);
    }
    catch (Exception ex)
    {
        ShowErrorToast($"处理文件时发生错误: {ex.Message}");
    }
}
```
