using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileManager.windows
{
    public partial class MainWindow : Window
    {

        private async Task LoadNodesLazilyAsync(TreeViewItem parentNode)
        {
            string parentPath = parentNode.Tag.ToString();

            try
            {
                var directories = await Task.Run(() => Directory.GetDirectories(parentPath));
                var files = await Task.Run(() => Directory.GetFiles(parentPath));

                foreach (var directory in directories)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        var directoryNode = new TreeViewItem
                        {
                            Header = Path.GetFileName(directory),
                            Tag = directory,
                            IsExpanded = false
                        };

                        directoryNode.Expanded += async (s, e) => await LoadNodesLazilyAsync(directoryNode);
                        parentNode.Items.Add(directoryNode);
                    });
                }

                foreach (var file in files)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        var fileNode = new TreeViewItem
                        {
                            Header = Path.GetFileName(file),
                            Tag = file,
                            IsExpanded = false,
                            Foreground = Brushes.Blue
                        };

                        parentNode.Items.Add(fileNode);
                    });
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке директории {parentPath}: {ex.Message}");
                });
            }
        }

        private async Task LoadNodesAsync(TreeViewItem parentNode)
        {
            string parentPath = parentNode.Tag.ToString();

            try
            {
                var directories = await Task.Run(() => Directory.GetDirectories(parentPath));
                var files = await Task.Run(() => Directory.GetFiles(parentPath));

                var tasks = directories.Select(async directory =>
                {
                    var directoryNode = new TreeViewItem
                    {
                        Header = Path.GetFileName(directory),
                        Tag = directory,
                        IsExpanded = false
                    };

                    Dispatcher.Invoke(() => parentNode.Items.Add(directoryNode));

                    await LoadNodesAsync(directoryNode);
                });

                await Task.WhenAll(tasks);

                foreach (var file in files)
                {
                    var fileNode = new TreeViewItem
                    {
                        Header = Path.GetFileName(file),
                        Tag = file,
                        IsExpanded = false,
                        Foreground = Brushes.Blue
                    };

                    Dispatcher.Invoke(() => parentNode.Items.Add(fileNode));
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке директории {parentPath}: {ex.Message}");
            }
        }

        private async void LoadLazyDirectoryTree()
        {
            ResultsTreeView.Items.Clear();

            var rootNode = new TreeViewItem
            {
                Header = _rootPath,
                Tag = _rootPath,
                IsExpanded = false
            };

            rootNode.Expanded += DirectoryNode_Expanded;
            ResultsTreeView.Items.Add(rootNode);
        }

        private async void DirectoryNode_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem expandedNode && expandedNode.Items.Count == 0)
            {
                await LoadNodesLazilyAsync(expandedNode);
            }
        }
    }
}
