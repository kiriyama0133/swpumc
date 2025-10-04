using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace swpumc.Utils
{
    /// <summary>
    /// Markdown公告解析器，用于自动解析Markdown文件中的元数据
    /// </summary>
    public static class MarkdownAnnouncementParser
    {
        /// <summary>
        /// 解析Markdown文件并提取公告信息
        /// </summary>
        /// <param name="filePath">Markdown文件路径</param>
        /// <returns>解析后的公告信息</returns>
        public static AnnouncementInfo ParseMarkdownFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Markdown文件不存在: {filePath}");
            }

            var content = File.ReadAllText(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileInfo = new FileInfo(filePath);

            return ParseMarkdownContent(content, fileName, fileInfo.CreationTime, fileInfo.LastWriteTime);
        }

        /// <summary>
        /// 使用Markdig解析Markdown并打印AST结构
        /// </summary>
        /// <param name="filePath">Markdown文件路径</param>
        public static void ParseAndPrintAST(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Markdown文件不存在: {filePath}");
                return;
            }

            var content = File.ReadAllText(filePath);

            // 使用Markdig解析Markdown
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var document = Markdown.Parse(content, pipeline);
            PrintASTNode(document, 0);
        }

        /// <summary>
        /// 递归打印AST节点结构
        /// </summary>
        /// <param name="node">AST节点</param>
        /// <param name="depth">缩进深度</param>
        private static void PrintASTNode(MarkdownObject node, int depth)
        {
            if (node == null) return;

            var indent = new string(' ', depth * 2);
            var nodeType = node.GetType().Name;
            
            // 获取节点的基本信息
            var info = GetNodeInfo(node);
            
            // 递归打印子节点
            if (node is ContainerBlock containerBlock)
            {
                foreach (var child in containerBlock)
                {
                    PrintASTNode(child, depth + 1);
                }
            }
            else if (node is ContainerInline containerInline)
            {
                foreach (var child in containerInline)
                {
                    PrintASTNode(child, depth + 1);
                }
            }
        }

        /// <summary>
        /// 获取节点的详细信息
        /// </summary>
        /// <param name="node">AST节点</param>
        /// <returns>节点信息字符串</returns>
        private static string GetNodeInfo(MarkdownObject node)
        {
            switch (node)
            {
                case HeadingBlock heading:
                    return $"(Level: {heading.Level}, Text: '{GetInlineText(heading.Inline)}')";
                
                case ParagraphBlock paragraph:
                    return $"(Text: '{GetInlineText(paragraph.Inline)}')";
                
                case FencedCodeBlock code:
                    return $"(Language: '{code.Info}', Lines: {code.Lines.Count})";
                
                case ListBlock list:
                    return $"(Type: {list.BulletType}, Items: {list.Count})";
                
                case ListItemBlock listItem:
                    return $"(Items: {listItem.Count})";
                
                case QuoteBlock quote:
                    return $"(Items: {quote.Count})";
                
                case ThematicBreakBlock:
                    return "(Horizontal Rule)";
                
                case LinkInline link:
                    return $"(Url: '{link.Url}', Text: '{GetInlineText(link)}')";
                
                case EmphasisInline emphasis:
                    return $"(Type: {emphasis.DelimiterChar}, Text: '{GetInlineText(emphasis)}')";
                
                case CodeInline code:
                    return $"(Text: '{code.Content}')";
                
                case LiteralInline literal:
                    return $"(Text: '{literal.Content}')";
                
                case LineBreakInline:
                    return "(Line Break)";
                
                case HtmlBlock html:
                    return $"(Type: {html.Type}, Lines: {html.Lines.Count})";
                
                default:
                    return $"({node.GetType().Name})";
            }
        }

        /// <summary>
        /// 获取内联文本内容
        /// </summary>
        /// <param name="inline">内联元素</param>
        /// <returns>文本内容</returns>
        private static string GetInlineText(Inline inline)
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
                    // 递归获取强调文本内的内容
                    text += GetInlineText(emphasis.FirstChild);
                }
                else if (current is LinkInline link)
                {
                    // 获取链接文本
                    text += GetInlineText(link.FirstChild);
                }
                else if (current is ContainerInline container)
                {
                    // 递归处理容器内联元素
                    text += GetInlineText(container.FirstChild);
                }
                
                current = current.NextSibling;
            }
            
            return text.Length > 50 ? text.Substring(0, 50) + "..." : text;
        }

        /// <summary>
        /// 解析Markdown内容并提取公告信息
        /// </summary>
        /// <param name="content">Markdown内容</param>
        /// <param name="fileName">文件名</param>
        /// <param name="creationTime">创建时间</param>
        /// <param name="lastWriteTime">最后修改时间</param>
        /// <returns>解析后的公告信息</returns>
        public static AnnouncementInfo ParseMarkdownContent(string content, string fileName, DateTime creationTime, DateTime lastWriteTime)
        {
            var info = new AnnouncementInfo
            {
                FileName = fileName,
                FullContent = content,
                CreationTime = creationTime,
                LastWriteTime = lastWriteTime
            };

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var document = Markdown.Parse(content, pipeline);
            PrintASTNode(document, 0);
            Console.WriteLine();

            // 解析Front Matter (YAML前置元数据)
            ParseFrontMatter(content, info);

            // 如果没有找到Front Matter，尝试从内容中解析
            if (string.IsNullOrEmpty(info.Title))
            {
                ParseFromContent(content, info);
            }

            // 生成预览内容
            info.Preview = MarkdownRenderer.GetPreview(content, 150);

            return info;
        }

        /// <summary>
        /// 解析Front Matter (YAML前置元数据)
        /// </summary>
        private static void ParseFrontMatter(string content, AnnouncementInfo info)
        {
            var frontMatterMatch = Regex.Match(content, @"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline);
            if (frontMatterMatch.Success)
            {
                var yamlContent = frontMatterMatch.Groups[1].Value;
                var lines = yamlContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var key = line.Substring(0, colonIndex).Trim();
                        var value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');

                        switch (key.ToLower())
                        {
                            case "title":
                                info.Title = value;
                                break;
                            case "author":
                                info.Author = value;
                                break;
                            case "type":
                                info.Type = value;
                                break;
                            case "publishdate":
                            case "date":
                                if (DateTime.TryParse(value, out var publishDate))
                                {
                                    info.PublishTime = publishDate;
                                }
                                break;
                            case "tags":
                                info.Tags = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从Markdown内容中解析信息
        /// </summary>
        private static void ParseFromContent(string content, AnnouncementInfo info)
        {
            // 提取第一个标题作为标题
            var titleMatch = Regex.Match(content, @"^#\s+(.+)$", RegexOptions.Multiline);
            if (titleMatch.Success)
            {
                info.Title = titleMatch.Groups[1].Value.Trim();
            }
            else
            {
                info.Title = info.FileName;
            }

            // 根据文件名推断类型
            info.Type = InferTypeFromFileName(info.FileName);

            // 根据内容推断作者
            info.Author = InferAuthorFromContent(content);

            // 使用文件修改时间作为发布时间
            info.PublishTime = info.LastWriteTime;
        }

        /// <summary>
        /// 根据文件名推断公告类型
        /// </summary>
        private static string InferTypeFromFileName(string fileName)
        {
            var lowerFileName = fileName.ToLower();
            
            if (lowerFileName.Contains("maintenance") || lowerFileName.Contains("维护"))
                return "维护";
            if (lowerFileName.Contains("update") || lowerFileName.Contains("更新"))
                return "更新";
            if (lowerFileName.Contains("contest") || lowerFileName.Contains("活动") || lowerFileName.Contains("比赛"))
                return "活动";
            if (lowerFileName.Contains("news") || lowerFileName.Contains("新闻"))
                return "新闻";
            if (lowerFileName.Contains("notice") || lowerFileName.Contains("通知"))
                return "通知";
            
            return "其他";
        }

        /// <summary>
        /// 根据内容推断作者
        /// </summary>
        private static string InferAuthorFromContent(string content)
        {
            // 查找常见的作者标识
            var authorPatterns = new[]
            {
                @"发布者[：:]\s*(.+)",
                @"作者[：:]\s*(.+)",
                @"管理员[：:]\s*(.+)",
                @"技术团队[：:]\s*(.+)",
                @"活动组[：:]\s*(.+)"
            };

            foreach (var pattern in authorPatterns)
            {
                var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return "系统";
        }
    }

    /// <summary>
    /// 公告信息数据模型
    /// </summary>
    public class AnnouncementInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string FullContent { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}
