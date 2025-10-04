using System;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Layout;
using Markdig;

namespace swpumc.Utils
{
    /// <summary>
    /// Markdown渲染工具类，使用纯文本渲染但保持格式化显示
    /// </summary>
    public static class MarkdownRenderer
    {
        /// <summary>
        /// 将Markdown文本渲染为Avalonia控件
        /// </summary>
        /// <param name="markdownText">Markdown文本</param>
        /// <returns>渲染后的控件</returns>
        /// <exception cref="ArgumentException">当markdownText为null或空白时抛出</exception>
        public static Control RenderMarkdown(string markdownText)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
            {
                throw new ArgumentException("Markdown text cannot be null or whitespace.", nameof(markdownText));
            }

            return CreateFormattedTextControl(markdownText);
        }

        /// <summary>
        /// 将Markdown文本渲染为Avalonia控件，带自定义样式
        /// </summary>
        /// <param name="markdownText">Markdown文本</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        /// <returns>渲染后的控件</returns>
        public static Control RenderMarkdown(string markdownText, double? maxWidth = null, double? maxHeight = null)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
            {
                throw new ArgumentException("Markdown text cannot be null or whitespace.", nameof(markdownText));
            }

            var control = CreateFormattedTextControl(markdownText);
            
            if (maxWidth.HasValue)
            {
                control.MaxWidth = maxWidth.Value;
            }

            if (maxHeight.HasValue)
            {
                control.MaxHeight = maxHeight.Value;
            }

            return control;
        }

        /// <summary>
        /// 使用AST渲染器将Markdown文本渲染为Avalonia控件
        /// </summary>
        /// <param name="markdownText">Markdown文本</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        /// <returns>渲染后的控件</returns>
        public static Control RenderMarkdownWithAST(string markdownText, double? maxWidth = null, double? maxHeight = null)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
            {
                throw new ArgumentException("Markdown text cannot be null or whitespace.", nameof(markdownText));
            }

            try
            {
                // 使用Markdig解析Markdown为AST
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var document = Markdown.Parse(markdownText, pipeline);

                // 使用AST渲染器渲染
                return MarkdownASTRenderer.RenderDocument(document, maxWidth, maxHeight);
            }
            catch (Exception)
            {
                
                // 如果AST渲染失败，回退到传统渲染
                return RenderMarkdown(markdownText, maxWidth, maxHeight);
            }
        }

        /// <summary>
        /// 创建格式化的文本控件
        /// </summary>
        /// <param name="markdownText">Markdown文本</param>
        /// <returns>格式化的控件</returns>
        private static Control CreateFormattedTextControl(string markdownText)
        {
            try
            {
                var stackPanel = new StackPanel
                {
                    Spacing = 8,
                    Margin = new Avalonia.Thickness(15)
                };

                // 移除Front Matter
                var contentWithoutFrontMatter = RemoveFrontMatter(markdownText);
                
                // 按行分割文本
                var lines = contentWithoutFrontMatter.Split('\n');
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // 空行
                        stackPanel.Children.Add(new TextBlock { Height = 5 });
                        continue;
                    }

                    var trimmedLine = line.Trim();
                    
                    // 处理标题
                    if (trimmedLine.StartsWith("#"))
                    {
                        var headerLevel = 0;
                        while (headerLevel < trimmedLine.Length && trimmedLine[headerLevel] == '#')
                        {
                            headerLevel++;
                        }
                        
                        var headerText = trimmedLine.Substring(headerLevel).Trim();
                        var fontSize = 24 - (headerLevel * 2); // 标题字体大小递减
                        
                        var headerBlock = new TextBlock
                        {
                            Text = headerText,
                            FontSize = Math.Max(fontSize, 14),
                            FontWeight = Avalonia.Media.FontWeight.Bold,
                            Margin = new Avalonia.Thickness(0, 10, 0, 5)
                        };
                        
                        stackPanel.Children.Add(headerBlock);
                        continue;
                    }
                    
                    // 处理列表项
                    if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* ") || trimmedLine.StartsWith("+ "))
                    {
                        var listText = trimmedLine.Substring(2);
                        var listBlock = new TextBlock
                        {
                            Text = "• " + listText,
                            FontSize = 14,
                            Margin = new Avalonia.Thickness(20, 2, 0, 2)
                        };
                        
                        stackPanel.Children.Add(listBlock);
                        continue;
                    }
                    
                    // 处理数字列表
                    var numberMatch = Regex.Match(trimmedLine, @"^\d+\.\s+(.*)");
                    if (numberMatch.Success)
                    {
                        var listText = numberMatch.Groups[1].Value;
                        var listBlock = new TextBlock
                        {
                            Text = "• " + listText,
                            FontSize = 14,
                            Margin = new Avalonia.Thickness(20, 2, 0, 2)
                        };
                        
                        stackPanel.Children.Add(listBlock);
                        continue;
                    }
                    
                    // 处理粗体文本
                    var boldText = ProcessBoldText(trimmedLine);
                    if (boldText != null)
                    {
                        stackPanel.Children.Add(boldText);
                        continue;
                    }
                    
                    // 普通文本
                    var textBlock = new TextBlock
                    {
                        Text = trimmedLine,
                        FontSize = 14,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, 3, 0, 3),
                        MaxWidth = 700 // 限制最大宽度，确保换行
                    };
                    
                    stackPanel.Children.Add(textBlock);
                }

                return stackPanel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MarkdownRenderer] Markdown渲染失败: {ex.Message}");
                
                return new TextBlock
                {
                    Text = $"Markdown渲染失败: {ex.Message}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = Avalonia.Media.Brushes.Red
                };
            }
        }

        /// <summary>
        /// 处理粗体文本
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>处理后的控件</returns>
        private static Control? ProcessBoldText(string text)
        {
            var boldMatch = Regex.Match(text, @"\*\*(.*?)\*\*");
            if (boldMatch.Success)
            {
                var beforeBold = text.Substring(0, boldMatch.Index);
                var boldText = boldMatch.Groups[1].Value;
                var afterBold = text.Substring(boldMatch.Index + boldMatch.Length);
                
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                
                if (!string.IsNullOrEmpty(beforeBold))
                {
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = beforeBold,
                        FontSize = 14,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    });
                }
                
                stackPanel.Children.Add(new TextBlock
                {
                    Text = boldText,
                    FontSize = 14,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                });
                
                if (!string.IsNullOrEmpty(afterBold))
                {
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = afterBold,
                        FontSize = 14,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    });
                }
                
                return stackPanel;
            }
            
            return null;
        }


        /// <summary>
        /// 获取Markdown文本的预览（移除Markdown语法，只保留纯文本）
        /// </summary>
        /// <param name="markdownText">Markdown文本</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>预览文本</returns>
        public static string GetPreview(string markdownText, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
            {
                return string.Empty;
            }

            // 移除Front Matter
            var contentWithoutFrontMatter = RemoveFrontMatter(markdownText);

            // 移除Markdown语法，只保留纯文本
            var plainText = RemoveMarkdownSyntax(contentWithoutFrontMatter);

            if (plainText.Length <= maxLength)
            {
                return plainText;
            }
            else
            {
                return plainText.Substring(0, maxLength) + "...";
            }
        }

        /// <summary>
        /// 移除Markdown文本中的Front Matter部分
        /// </summary>
        /// <param name="markdownText">包含Front Matter的Markdown文本</param>
        /// <returns>移除Front Matter后的Markdown文本</returns>
        private static string RemoveFrontMatter(string markdownText)
        {
            var frontMatterRegex = new Regex(@"^---\s*$(.*?)^---\s*$(.*)", RegexOptions.Singleline | RegexOptions.Multiline);
            var match = frontMatterRegex.Match(markdownText);
            if (match.Success)
            {
                return match.Groups[2].Value.Trim();
            }
            return markdownText;
        }

        /// <summary>
        /// 移除Markdown语法，只保留纯文本
        /// </summary>
        /// <param name="markdown">包含Markdown语法的字符串</param>
        /// <returns>移除Markdown语法后的纯文本</returns>
        private static string RemoveMarkdownSyntax(string markdown)
        {
            // 移除标题
            markdown = Regex.Replace(markdown, @"^#+\s*(.*)$", "$1", RegexOptions.Multiline);
            // 移除粗体和斜体
            markdown = Regex.Replace(markdown, @"(\*\*|__)(.*?)\1", "$2");
            markdown = Regex.Replace(markdown, @"(\*|_)(.*?)\1", "$2");
            // 移除链接
            markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)", "$1");
            // 移除图片
            markdown = Regex.Replace(markdown, @"!\[(.*?)\]\((.*?)\)", "$1");
            // 移除代码块
            markdown = Regex.Replace(markdown, @"`{3}[\s\S]*?`{3}", "");
            markdown = Regex.Replace(markdown, @"`([^`]+)`", "$1");
            // 移除引用
            markdown = Regex.Replace(markdown, @"^>\s*(.*)$", "$1", RegexOptions.Multiline);
            // 移除列表
            markdown = Regex.Replace(markdown, @"^(\s*[-*+]|\d+\.)\s*(.*)$", "$2", RegexOptions.Multiline);
            // 移除水平线
            markdown = Regex.Replace(markdown, @"^(\s*[-*_]){3,}\s*$", "", RegexOptions.Multiline);
            // 移除HTML标签
            markdown = Regex.Replace(markdown, @"<[^>]*>", "");
            // 移除多余的空行
            markdown = Regex.Replace(markdown, @"\n\s*\n", "\n");
            // 移除开头和结尾的空白
            markdown = markdown.Trim();

            return markdown;
        }
    }
}
