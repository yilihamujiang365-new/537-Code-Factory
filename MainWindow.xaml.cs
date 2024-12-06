using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Security.Policy;
using System.Reflection;
using WpfMessageBox = System.Windows.MessageBox;

namespace _537_FSCF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private string folderPath = $@"C:\Users\{Environment.UserName}\537 Code Factory Project\";
        // private string ProjectName;
        private FileSystemWatcher fileSystemWatcher;
        public string _projectFolderPath;


        public MainWindow()
        {
            InitializeComponent();
            CreateProjectFolder();
            InitializeWebview2();
        }        


        private async void InitializeWebview2()
        {
            await webView21.EnsureCoreWebView2Async(null);
        }
        private void CreateProjectFolder()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            //ProjecttreeView.Nodes.Clear();
            ProjecttreeView.Items.Clear();
            string ProjectName = null;
            CreateProjectForm projectnameform = new CreateProjectForm();

            if (projectnameform.ShowDialog() == true)
            {
                ProjectName = projectnameform.ProjectName;
            }

            if (string.IsNullOrEmpty(ProjectName))
            {
                MessageBox.Show("文件名不能为空，请重新输入。");
                return;
            }

            _projectFolderPath = System.IO.Path.Combine(folderPath, ProjectName);

            try
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(_projectFolderPath);
                string htmlFileName = "index.html";
                string htmlFilePath = System.IO.Path.Combine(_projectFolderPath, htmlFileName);

                string htmlContent = @"<!DOCTYPE html>
<html>
<head>
    <title>537 Code Factory</title>
    <meta charset=""UTF-8"">
</head>
<body>
    <h1>537 Code Factory</h1>
</body>
</html>";

                File.WriteAllText(htmlFilePath, htmlContent);
                LoadTreeView(_projectFolderPath);

                MessageBox.Show($"目录已创建: {directoryInfo.FullName}");
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"权限不足: {ex.Message}");
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IO 错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"未知错误: {ex.Message}");
            }
            string newnew = _projectFolderPath + "\\";
            LocalHost(newnew);

            Console.WriteLine(_projectFolderPath);
            Console.WriteLine(newnew);

        }

        private void LoadTreeView(string folderPath)
        {
            ProjecttreeView.Items.Clear();

            if (Directory.Exists(folderPath))
            {
                // 创建根节点
                TreeViewItem rootNode = new TreeViewItem();
                rootNode.Header = new DirectoryInfo(folderPath).Name; // 设置Header属性
                rootNode.Tag = folderPath; // 设置Tag属性

                // 添加子节点
                AddNodes(rootNode, folderPath);

                // 将根节点添加到TreeView中
                ProjecttreeView.Items.Add(rootNode);

                // 开始文件系统监视
                StartFileSystemWatcher(folderPath);
            }
            else
            {
                MessageBox.Show("指定的文件夹不存在");
            }
        }

        private void AddNodes(TreeViewItem parentItem, string folderPath)
        {
            try
            {
                // 添加子目录节点
                foreach (string subDir in Directory.GetDirectories(folderPath))
                {
                    string dirName = new DirectoryInfo(subDir).Name;
                    TreeViewItem subNode = new TreeViewItem();
                    subNode.Header = dirName; // 设置Header属性
                    subNode.Tag = subDir; // 设置Tag属性
                    parentItem.Items.Add(subNode); // 将子节点添加到父节点的Items集合中
                    AddNodes(subNode, subDir); // 递归添加子节点
                }

                // 添加文件节点
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    TreeViewItem fileNode = new TreeViewItem();
                    fileNode.Header = fileName; // 设置Header属性
                    fileNode.Tag = file; // 设置Tag属性
                    parentItem.Items.Add(fileNode); // 将文件节点添加到父节点的Items集合中
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("无法访问某些文件夹");
            }
        }

        private void StartFileSystemWatcher(string folderPath)
        {
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Dispose();
            }

            fileSystemWatcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.*",
                IncludeSubdirectories = true
            };

            fileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);
            fileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            fileSystemWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // 检查是否在 UI 线程上，如果不是，则使用 Dispatcher 更新 UI
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    LoadTreeView(_projectFolderPath);
                });
            }
            else
            {
                LoadTreeView(_projectFolderPath);
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // 检查是否在 UI 线程上，如果不是，则使用 Dispatcher 更新 UI
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    LoadTreeView(_projectFolderPath);
                });
            }
            else
            {
                LoadTreeView(_projectFolderPath);
            }
        }

        private void ProjecttreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // 获取选中的节点
                TreeViewItem selectedItem = ProjecttreeView.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    string nodetext = selectedItem.Header.ToString();
                    if (nodetext.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    {
                        string filepath = selectedItem.Tag.ToString();
                        // 在 WPF 中，你需要在 XAML 中定义 FastColoredTextBox 控件或者使用其他文本编辑器控件
                        // 以下代码假设你有一个名为 htmlTextBox 的 TextBox 控件在 XAML 中定义

                        ICSharpCode.AvalonEdit.TextEditor htmlTextBox = new ICSharpCode.AvalonEdit.TextEditor

                        {
                            Text = File.ReadAllText(filepath, System.Text.Encoding.UTF8),
                            ShowLineNumbers = true,
                            FontFamily = new FontFamily("Consolas"),
                            // 设置字体大小
                            FontSize = 18,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".html"),
                        };

                        // 添加键盘事件监听器
                        htmlTextBox.PreviewKeyDown += HtmlTextBox_PreviewKeyDown;


                        // 在 WPF 中，你需要使用 TabControl 和 TabItem 控件
                        TabItem tabPage = new TabItem
                        {
                            Header = nodetext,
                            Content = htmlTextBox
                        };
                        tabControl1.Items.Add(tabPage);
                    }
                    else if (nodetext.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    {
                        string filepath = selectedItem.Tag.ToString();
                        // 同上，假设你有一个名为 cssTextBox 的 TextBox 控件在 XAML 中定义
                        ICSharpCode.AvalonEdit.TextEditor cssTextBox = new ICSharpCode.AvalonEdit.TextEditor
                        {
                            //Language = FastColoredTextBoxNS.Language.JS,
                            Text = File.ReadAllText(filepath, System.Text.Encoding.UTF8),
                            ShowLineNumbers = true,
                            FontFamily = new FontFamily("Consolas"),
                            // 设置字体大小
                            FontSize = 18,
                            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".css"),
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                        };

                        // 添加键盘事件监听器
                        cssTextBox.PreviewKeyDown += CssTextBox_PreviewKeyDown;

                        // 使用 TabItem 控件
                        TabItem tabPage = new TabItem
                        {
                            Header = nodetext,
                            Content = cssTextBox
                        };
                        tabControl1.Items.Add(tabPage);
                    }
                    else if (nodetext.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    {
                        string filepath = selectedItem.Tag.ToString();
                        // 同上，假设你有一个名为 cssTextBox 的 TextBox 控件在 XAML 中定义
                        ICSharpCode.AvalonEdit.TextEditor JSTextBox = new ICSharpCode.AvalonEdit.TextEditor
                        {
                            //Language = FastColoredTextBoxNS.Language.JS,
                            Text = File.ReadAllText(filepath, System.Text.Encoding.UTF8),
                            ShowLineNumbers = true,
                            FontFamily = new FontFamily("Consolas"),
                            // 设置字体大小
                            FontSize = 18,
                            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".css"),
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                        };

                        // 添加键盘事件监听器
                        JSTextBox.PreviewKeyDown += JSTextBox_PreviewKeyDown;

                        // 使用 TabItem 控件
                        TabItem tabPage = new TabItem
                        {
                            Header = nodetext,
                            Content = JSTextBox
                        };
                        tabControl1.Items.Add(tabPage);
                    }
                }
            }
        }
        private void LocalHost(string _projectFolderPath)
        {
            if (!Directory.Exists(_projectFolderPath))
            {
                Console.WriteLine("文件夹不存在");
                return;
            }
            Thread serverThread = new Thread(() =>
            {
                try
                {
                    HttpListener listener = new HttpListener();
                    listener.Prefixes.Add("http://localhost:8080/");
                    listener.Start();
                    Console.WriteLine("服务启动");
                    while (true)
                    {
                        HttpListenerContext context = listener.GetContext();

                        HttpListenerRequest request = context.Request;
                        string relativePath = request.Url.AbsolutePath.Substring(1); // 移除开头的斜杠
                        string filepath = System.IO.Path.Combine(_projectFolderPath, relativePath);

                        // 规范化路径并检查是否在_projectFolderPath目录下
                        filepath = System.IO.Path.GetFullPath(filepath);
                        if (!filepath.StartsWith(_projectFolderPath, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = 403;
                            byte[] errorResponse = Encoding.UTF8.GetBytes("403 Forbidden");
                            context.Response.OutputStream.Write(errorResponse, 0, errorResponse.Length);
                            context.Response.Close();
                            return;
                        }

                        if (File.Exists(filepath))
                        {
                            byte[] buffer = File.ReadAllBytes(filepath);
                            context.Response.ContentType = GetContentType(filepath);
                            context.Response.ContentEncoding = System.Text.Encoding.UTF8; // 设置编码为UTF-8
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            string responseString = "<HTML><BODY>文件未找到。</BODY></HTML>";
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法启动服务器: {ex.Message}");
                    return;
                }
            });
            serverThread.IsBackground = true;
            serverThread.Start();

            // 确保Webview2导航到本地服务器的根目录
            webView21.CoreWebView2.Navigate("http://localhost:8080/index.html");
            IEbrower.Navigate("http://localhost:8080/index.html");
        }

        private string GetContentType(string filepath)
        {  // 根据文件扩展名返回内容类型
            switch (System.IO.Path.GetExtension(filepath).ToLower())
            {
                case ".html": return "text/html; charset=utf-8";
                case ".css": return "text/css; charset=utf-8";
                case ".js": return "application/javascript; charset=utf-8";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".gif": return "image/gif";
                // 添加更多文件类型和内容类型映射
                default: return "application/octet-stream";
            }
        }


        private void HtmlTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ICSharpCode.AvalonEdit.TextEditor editor = sender as ICSharpCode.AvalonEdit.TextEditor;
                if (editor != null)
                {
                    // 获取当前选中的TreeViewItem
                    TreeViewItem selectedItem = ProjecttreeView.SelectedItem as TreeViewItem;
                    if (selectedItem != null)
                    {
                        string filepath = selectedItem.Tag.ToString();
                        SaveAndRefresh(editor, filepath);
                    }
                    e.Handled = true;
                }
            }
        }
        private void CssTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ICSharpCode.AvalonEdit.TextEditor editor = sender as ICSharpCode.AvalonEdit.TextEditor;
                if (editor != null)
                {
                    // 获取当前选中的TreeViewItem
                    TreeViewItem selectedItem = ProjecttreeView.SelectedItem as TreeViewItem;
                    if (selectedItem != null)
                    {
                        string filepath = selectedItem.Tag.ToString();
                        SaveAndRefresh(editor, filepath);
                    }
                    e.Handled = true;
                }
            }
        }
        private void JSTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ICSharpCode.AvalonEdit.TextEditor editor = sender as ICSharpCode.AvalonEdit.TextEditor;
                if (editor != null)
                {
                    // 获取当前选中的TreeViewItem
                    TreeViewItem selectedItem = ProjecttreeView.SelectedItem as TreeViewItem;
                    if (selectedItem != null)
                    {
                        string filepath = selectedItem.Tag.ToString();
                        SaveAndRefresh(editor, filepath);
                    }
                    e.Handled = true;
                }
            }
        }
        private void TextEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ICSharpCode.AvalonEdit.TextEditor editor = sender as ICSharpCode.AvalonEdit.TextEditor;
                if (editor != null)
                {
                    // 获取当前选中的TreeViewItem
                    TreeViewItem selectedItem = ProjecttreeView.SelectedItem as TreeViewItem;
                    if (selectedItem != null)
                    {
                        string filepath = selectedItem.Tag.ToString();
                        SaveAndRefresh(editor, filepath);
                    }
                    e.Handled = true;
                }
            }
        }

        private void SaveAndRefresh(ICSharpCode.AvalonEdit.TextEditor editor, string filepath)
        {
            string content = editor.Text;
            File.WriteAllText(filepath, content, System.Text.Encoding.UTF8);

            string fileExtension = System.IO.Path.GetExtension(filepath).ToLowerInvariant();
            if (fileExtension == ".html" || fileExtension == ".css" || fileExtension == ".js")
            {
                webView21.CoreWebView2.Reload();
                IEbrower.Refresh();
            }
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button closeButton = sender as Button;
            TabItem tabItem = closeButton.TemplatedParent as TabItem;

            // 获取当前TabItem中的TextEditor
            ICSharpCode.AvalonEdit.TextEditor editor = tabItem.Content as ICSharpCode.AvalonEdit.TextEditor;
            if (editor != null)
            {
                // 获取当前选中的TreeViewItem
                TreeViewItem selectedItem = ProjecttreeView.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    string filepath = selectedItem.Tag.ToString();
                    SaveAndRefresh(editor, filepath);
                }
            }

            // 关闭TabItem
            tabControl1.Items.Remove(tabItem);
        }
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个浏览文件夹对话框
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "打开项目文件夹";

            // 显示打开文件夹对话框
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // 获取用户选择的项目文件夹路径
                string projectPath = folderBrowserDialog.SelectedPath;

                // 检查项目文件夹是否有效
                if (!string.IsNullOrEmpty(projectPath) && Directory.Exists(projectPath))
                {
                    _projectFolderPath = projectPath;
                    LoadTreeView(_projectFolderPath);
                    LocalHost(_projectFolderPath);
                    MessageBox.Show($"项目已加载: {_projectFolderPath}");
                }
                else
                {
                    MessageBox.Show("请选择一个有效的文件夹");
                }
            }
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取应用程序名称
            string Appname = assembly.GetName().Name;
            string AppVersion=assembly.GetName().Version.ToString();
            Version dotNetVersion = Environment.Version;
            MessageBox.Show($"537代码工厂(537 Code Factory 简称{Appname})\n" +
                            $"当前版本为:{AppVersion}\n" +
                            $"使用的.NET版本:{dotNetVersion}\n" +
                            $"框架：WPF\n" +
                            $"开发者:\n\tyilihamujiang365@outlook.com\n" +
                            $"使用的Nuget包列表如下：\n\t" +
                            $"Microsoft.Web.WebView2\n\t" +
                            $"AvalonEdit","关于",MessageBoxButton.OK,MessageBoxImage.Information);
        }
    }
    }
