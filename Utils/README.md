# 动画管理系统

这是一个统一的动画管理类，支持弹簧动画和各种缓动函数，已注册到依赖注入容器中。

## 功能特性

- ✅ 弹簧动画支持
- ✅ 多种缓动函数
- ✅ 淡入淡出动画
- ✅ 缩放动画
- ✅ 位移动画
- ✅ 旋转动画
- ✅ 扩展方法支持
- ✅ 依赖注入集成

## 使用方法

### 1. 基本动画扩展方法

```csharp
// 淡入动画
await myControl.FadeInAsync(TimeSpan.FromMilliseconds(300));

// 淡出动画
await myControl.FadeOutAsync(TimeSpan.FromMilliseconds(300));

// 缩放动画
await myControl.ScaleAsync(1.0, 1.2, TimeSpan.FromMilliseconds(200));

// 位移动画
await myControl.TranslateAsync(0, 100, 0, 0, TimeSpan.FromMilliseconds(300));

// 旋转动画
await myControl.RotateAsync(0, 360, TimeSpan.FromSeconds(1));

// 弹簧动画
await myControl.SpringAsync(
    Visual.RenderTransformProperty,
    new ScaleTransform(1.0, 1.0),
    new ScaleTransform(1.1, 1.1),
    TimeSpan.FromMilliseconds(500)
);
```

### 2. 使用缓动函数

```csharp
// 使用预定义的缓动函数
var easing = EasingFactory.CreateEasing(EasingType.CubicEaseOut);
await myControl.FadeInAsync(TimeSpan.FromMilliseconds(300), easing);

// 可用的缓动函数类型：
// - Linear: 线性
// - EaseIn: 缓入
// - EaseOut: 缓出
// - EaseInOut: 缓入缓出
// - CubicEaseIn: 立方缓入
// - CubicEaseOut: 立方缓出
// - CubicEaseInOut: 立方缓入缓出
// - QuadraticEaseIn: 二次缓入
// - QuadraticEaseOut: 二次缓出
// - QuadraticEaseInOut: 二次缓入缓出
// - QuarticEaseIn: 四次缓入
// - QuarticEaseOut: 四次缓出
// - QuarticEaseInOut: 四次缓入缓出
// - ElasticEaseIn: 弹性缓入
// - ElasticEaseOut: 弹性缓出
// - ElasticEaseInOut: 弹性缓入缓出
// - BounceEaseIn: 反弹缓入
// - BounceEaseOut: 反弹缓出
// - BounceEaseInOut: 反弹缓入缓出
```

### 3. 使用动画服务

```csharp
// 通过依赖注入获取动画服务
var animationService = serviceProvider.GetService<IAnimationService>();

// 创建自定义动画
var animation = animationService.CreateEasingAnimation(
    Visual.OpacityProperty,
    0.0,
    1.0,
    TimeSpan.FromMilliseconds(300),
    new CubicEaseOut()
);

// 播放动画
await animationService.PlayAnimationAsync(myControl, animation);
```

### 4. 实际应用示例

```csharp
// 按钮点击动画
private async void Button_Click(object sender, RoutedEventArgs e)
{
    var button = sender as Button;
    await button.ScaleAsync(1.0, 0.95, TimeSpan.FromMilliseconds(100));
    await button.ScaleAsync(0.95, 1.0, TimeSpan.FromMilliseconds(100));
}

// 卡片悬停动画
private async void Card_PointerEnter(object sender, PointerEventArgs e)
{
    var card = sender as Control;
    await card.ScaleAsync(1.0, 1.05, TimeSpan.FromMilliseconds(200));
}

private async void Card_PointerLeave(object sender, PointerEventArgs e)
{
    var card = sender as Control;
    await card.ScaleAsync(1.05, 1.0, TimeSpan.FromMilliseconds(200));
}

// 页面切换动画
private async Task SwitchPage(Control fromPage, Control toPage)
{
    // 当前页面淡出
    await fromPage.FadeOutAsync(TimeSpan.FromMilliseconds(300));
    
    // 新页面淡入
    toPage.Opacity = 0;
    await toPage.FadeInAsync(TimeSpan.FromMilliseconds(300));
}

// 主题切换动画
private async Task SwitchTheme()
{
    await container.FadeOutAsync(TimeSpan.FromMilliseconds(200));
    // 切换主题逻辑
    await container.FadeInAsync(TimeSpan.FromMilliseconds(200));
}
```

## 文件结构

```
Utils/
├── IAnimationService.cs          # 动画服务接口
├── AnimationService.cs           # 动画服务实现
├── AnimationExtensions.cs        # 动画扩展方法
├── AnimationExamples.cs          # 使用示例
└── README.md                     # 说明文档
```

## 依赖注入

动画服务已注册到依赖注入容器中：

```csharp
// 在 App.axaml.cs 中
services.AddSingleton<IAnimationService, AnimationService>();
```

## 注意事项

1. 所有动画方法都是异步的，需要使用 `await` 关键字
2. 可以传递 `CancellationToken` 来取消动画
3. 动画会自动处理控件的 `RenderTransform` 属性
4. 建议在动画前设置初始状态，动画后清理状态
5. 弹簧动画使用 `KeySpline` 实现，参数可调整

## 性能优化建议

1. 避免同时运行过多动画
2. 使用 `CancellationToken` 及时取消不需要的动画
3. 对于复杂的动画序列，考虑使用 `Task.WhenAll` 并行执行
4. 在页面卸载时取消所有正在进行的动画
