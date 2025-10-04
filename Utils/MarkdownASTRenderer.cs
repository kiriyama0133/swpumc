using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;

namespace swpumc.Utils
{
    /// <summary>
    /// 基于Markdig AST的Markdown渲染器
    /// 直接将AST节点映射到Avalonia控件
    /// </summary>
    public static class MarkdownASTRenderer
    {
        /// <summary>
        /// 将Markdig AST文档渲染为Avalonia控件
        /// </summary>
        /// <param name="document">Markdig AST文档</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        /// <returns>渲染后的控件</returns>
        public static Control RenderDocument(MarkdownDocument document, double? maxWidth = null, double? maxHeight = null)
        {
            try
            {
                var container = new StackPanel
                {
                    Spacing = 8,
                    Margin = new Avalonia.Thickness(15)
                };

                if (maxWidth.HasValue)
                {
                    container.MaxWidth = maxWidth.Value;
                }

                if (maxHeight.HasValue)
                {
                    container.MaxHeight = maxHeight.Value;
                }

                // 遍历文档中的所有块级元素
                foreach (var block in document)
                {
                    var control = RenderBlock(block);
                    if (control != null)
                    {
                        container.Children.Add(control);
                    }
                }

                return container;
            }
            catch (Exception ex)
            {
                return new TextBlock
                {
                    Text = $"AST渲染失败: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red
                };
            }
        }

        /// <summary>
        /// 渲染块级元素
        /// </summary>
        /// <param name="block">块级元素</param>
        /// <returns>渲染后的控件</returns>
        private static Control? RenderBlock(Block block)
        {
            return block switch
            {
                HeadingBlock heading => RenderHeading(heading),
                ParagraphBlock paragraph => RenderParagraph(paragraph),
                ListBlock list => RenderList(list),
                QuoteBlock quote => RenderQuote(quote),
                FencedCodeBlock code => RenderCodeBlock(code),
                ThematicBreakBlock => RenderHorizontalRule(),
                HtmlBlock html => RenderHtmlBlock(html),
                Table table => RenderTable(table),
                _ => null
            };
        }

        /// <summary>
        /// 渲染标题
        /// </summary>
        private static Control RenderHeading(HeadingBlock heading)
        {
            var fontSize = Math.Max(24 - (heading.Level * 2), 14);
            
            var textBlock = new TextBlock
            {
                FontSize = fontSize,
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 10, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };

            // 渲染标题的行内元素
            RenderInlines(textBlock.Inlines, heading.Inline);
            
            return textBlock;
        }

        /// <summary>
        /// 渲染段落
        /// </summary>
        private static Control RenderParagraph(ParagraphBlock paragraph)
        {
            var textBlock = new TextBlock
            {
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 3, 0, 3)
            };

            // 渲染段落的行内元素
            RenderInlines(textBlock.Inlines, paragraph.Inline);
            
            return textBlock;
        }

        /// <summary>
        /// 渲染列表
        /// </summary>
        private static Control RenderList(ListBlock list)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20, 2, 0, 2)
            };

            int itemIndex = 1; // 用于有序列表的序号
            foreach (var item in list)
            {
                if (item is ListItemBlock listItem)
                {
                    var itemControl = RenderListItem(listItem, list.BulletType, itemIndex);
                    if (itemControl != null)
                    {
                        stackPanel.Children.Add(itemControl);
                    }
                    itemIndex++; // 递增序号
                }
            }

            return stackPanel;
        }

        /// <summary>
        /// 渲染列表项
        /// </summary>
        private static Control? RenderListItem(ListItemBlock listItem, char bulletType, int itemIndex)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Avalonia.Thickness(0, 2, 0, 2)
            };

            // 添加列表标记
            var bullet = bulletType switch
            {
                '-' or '*' or '+' => "• ",
                '1' => $"{itemIndex}. ", // 使用递增的序号
                _ => "• "
            };

            var bulletText = new TextBlock
            {
                Text = bullet,
                FontSize = 14,
                Width = 20
            };

            stackPanel.Children.Add(bulletText);

            // 渲染列表项内容
            var contentPanel = new StackPanel();
            foreach (var block in listItem)
            {
                var control = RenderBlock(block);
                if (control != null)
                {
                    contentPanel.Children.Add(control);
                }
            }

            stackPanel.Children.Add(contentPanel);
            return stackPanel;
        }

        /// <summary>
        /// 渲染引用块
        /// </summary>
        private static Control RenderQuote(QuoteBlock quote)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Avalonia.Thickness(2, 0, 0, 0),
                Padding = new Avalonia.Thickness(10, 5, 0, 5),
                Margin = new Avalonia.Thickness(10, 5, 0, 5)
            };

            var stackPanel = new StackPanel();
            foreach (var block in quote)
            {
                var control = RenderBlock(block);
                if (control != null)
                {
                    stackPanel.Children.Add(control);
                }
            }

            border.Child = stackPanel;
            return border;
        }

        /// <summary>
        /// 渲染代码块
        /// </summary>
        private static Control RenderCodeBlock(FencedCodeBlock code)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Avalonia.Thickness(1),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 5, 0, 5),
                CornerRadius = new Avalonia.CornerRadius(4)
            };

            var textBlock = new TextBlock
            {
                Text = string.Join("\n", code.Lines),
                FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            return border;
        }

        /// <summary>
        /// 渲染水平分割线
        /// </summary>
        private static Control RenderHorizontalRule()
        {
            return new Border
            {
                Height = 1,
                Background = Brushes.Gray,
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
        }

        /// <summary>
        /// 渲染HTML块
        /// </summary>
        private static Control RenderHtmlBlock(HtmlBlock html)
        {
            var textBlock = new TextBlock
            {
                Text = string.Join("\n", html.Lines),
                FontSize = 12,
                FontStyle = FontStyle.Italic,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Gray
            };

            return textBlock;
        }

        /// <summary>
        /// 渲染行内元素
        /// </summary>
        /// <param name="inlines">目标Inline集合</param>
        /// <param name="markdigInline">Markdig行内元素</param>
        private static void RenderInlines(InlineCollection inlines, ContainerInline? markdigInline)
        {
            if (markdigInline?.Any() != true) return;

            foreach (var child in markdigInline)
            {
                switch (child)
                {
                    case LiteralInline literal:
                        // 纯文本内容
                        inlines.Add(new Run(literal.Content.ToString()));
                        break;

                    case EmphasisInline emphasis:
                        // 处理粗体或斜体
                        var span = new Span
                        {
                            FontWeight = emphasis.DelimiterCount == 2 ? FontWeight.Bold : FontWeight.Normal,
                            FontStyle = emphasis.DelimiterCount == 1 ? FontStyle.Italic : FontStyle.Normal
                        };
                        
                        // 递归处理 EmphasisInline 内部的子节点
                        RenderInlines(span.Inlines, emphasis);
                        inlines.Add(span);
                        break;

                    case LinkInline link:
                        // 处理链接 - 使用Span模拟链接样式
                        var linkSpan = new Span
                        {
                            Foreground = Brushes.Blue,
                            TextDecorations = TextDecorations.Underline
                        };

                        // 递归处理链接文本
                        RenderInlines(linkSpan.Inlines, link);
                        inlines.Add(linkSpan);
                        break;

                    case CodeInline code:
                        // 处理行内代码
                        var codeSpan = new Span
                        {
                            FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                            Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0))
                        };
                        codeSpan.Inlines.Add(new Run(code.Content.ToString()));
                        inlines.Add(codeSpan);
                        break;

                    case ContainerInline container:
                        // 如果是其他类型的容器，递归处理其内部
                        RenderInlines(inlines, container);
                        break;
                }
            }
        }

        /// <summary>
        /// 渲染表格
        /// </summary>
        private static Control RenderTable(Table table)
        {
            try
            {
                var grid = new Grid
                {
                    Margin = new Avalonia.Thickness(0, 10, 0, 10)
                };

                // 设置列定义
                var columnCount = GetTableColumnCount(table);
                for (int i = 0; i < columnCount; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // 设置行定义
                var rowCount = table.Count;
                for (int i = 0; i < rowCount; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                int currentRow = 0;
                foreach (var tableRow in table)
                {
                    if (tableRow is TableRow row)
                    {
                        int currentColumn = 0;
                        foreach (var cell in row)
                        {
                            if (cell is TableCell tableCell)
                            {
                                var cellControl = RenderTableCell(tableCell, currentRow == 0);
                                Grid.SetRow(cellControl, currentRow);
                                Grid.SetColumn(cellControl, currentColumn);
                                grid.Children.Add(cellControl);
                                currentColumn++;
                            }
                        }
                        currentRow++;
                    }
                }

                // 添加边框
                var border = new Border
                {
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Avalonia.Thickness(1),
                    Child = grid
                };

                return border;
            }
            catch (Exception ex)
            {
                return new TextBlock
                {
                    Text = $"表格渲染失败: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red
                };
            }
        }

        /// <summary>
        /// 渲染表格单元格
        /// </summary>
        private static Control RenderTableCell(TableCell cell, bool isHeader)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Avalonia.Thickness(0.5),
                Padding = new Avalonia.Thickness(8, 4)
            };

            var textBlock = new TextBlock
            {
                FontSize = isHeader ? 14 : 12,
                FontWeight = isHeader ? FontWeight.Bold : FontWeight.Normal,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 渲染单元格内容
            if (cell.Count > 0)
            {
                var firstBlock = cell[0];
                if (firstBlock is ParagraphBlock paragraph)
                {
                    RenderInlines(textBlock.Inlines, paragraph.Inline!);
                }
                else
                {
                    textBlock.Text = GetCellText(cell);
                }
            }
            else
            {
                textBlock.Text = "";
            }

            border.Child = textBlock;
            return border;
        }

        /// <summary>
        /// 获取表格列数
        /// </summary>
        private static int GetTableColumnCount(Table table)
        {
            if (table.Count == 0) return 0;
            
            var firstRow = table[0] as TableRow;
            return firstRow?.Count ?? 0;
        }

        /// <summary>
        /// 获取单元格文本内容
        /// </summary>
        private static string GetCellText(TableCell cell)
        {
            var text = "";
            foreach (var block in cell)
            {
                if (block is ParagraphBlock paragraph)
                {
                    text += GetInlineText(paragraph.Inline!);
                }
                else
                {
                    text += block.ToString();
                }
            }
            return text.Trim();
        }

        /// <summary>
        /// 获取内联文本内容
        /// </summary>
        private static string GetInlineText(Markdig.Syntax.Inlines.Inline inline)
        {
            if (inline == null) return "";
            
            var text = "";
            var current = inline;
            while (current != null)
            {
                if (current is LiteralInline literal)
                {
                    text += literal.Content;
                }
                else if (current is CodeInline code)
                {
                    text += code.Content;
                }
                else if (current is EmphasisInline emphasis)
                {
                    text += GetInlineText(emphasis.FirstChild!);
                }
                else if (current is LinkInline link)
                {
                    text += GetInlineText(link.FirstChild!);
                }
                else if (current is ContainerInline container)
                {
                    text += GetInlineText(container.FirstChild!);
                }
                
                current = current.NextSibling;
            }
            
            return text;
        }
    }
}