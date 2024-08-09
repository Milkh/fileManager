using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using fileManager;

namespace FileManager.windows
{
    public partial class MainWindow : Window
    {
        #region Поля================================================================================================================================================================================


        private bool _isDirectoryTreeLoaded = false;
        private CancellationTokenSource _cancellationTokenSource;
        private DispatcherTimer _timer;
        private DateTime _startTime;
        private string _rootPath;
        private bool _isLazyLoad;

        public MainWindow()
        {
            InitializeComponent();

            var rootDirectoryWindow = new RootDirectoryWindow();
            if (rootDirectoryWindow.ShowDialog() == true)
            {
                _rootPath = rootDirectoryWindow.SelectedDirectory;
                _isLazyLoad = rootDirectoryWindow.IsLazyLoad;

                if (_isLazyLoad)
                {
                    LoadLazyDirectoryTree();
                }
                else
                {
                    LoadDirectoryTree();
                }
            }
            else
            {
                Close();
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            UpdatePlaceholderVisibility(StartDirectoryTextBox, StartDirectoryPlaceholder);
            UpdatePlaceholderVisibility(RegexTextBox, RegexPlaceholder);

            ResultsTreeView.SelectedItemChanged += ResultsTreeView_SelectedItemChanged;
        }

        private async void LoadDirectoryTree()
        {
            ResultsTreeView.Items.Clear();
            string rootPath = _rootPath;

            var rootNode = new TreeViewItem
            {
                Header = rootPath,
                Tag = rootPath,
                IsExpanded = true
            };

            ResultsTreeView.Items.Add(rootNode);

            try
            {
                if (_isLazyLoad)
                {
                    await LoadNodesLazilyAsync(rootNode);
                }
                else
                {
                    await LoadNodesAsync(rootNode);
                }

                _isDirectoryTreeLoaded = true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Нет доступа к указанной директории.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке дерева директорий: {ex.Message}");
            }

            SynchronizeButton_Click(null, null);
        }

        private async void LoadDirectoryTree(object sender, RoutedEventArgs e)
        {
            ResultsTreeView.Items.Clear();
            string rootPath = _rootPath;

            var rootNode = new TreeViewItem
            {
                Header = rootPath,
                Tag = rootPath,
                IsExpanded = true
            };

            ResultsTreeView.Items.Add(rootNode);

            try
            {
                if (_isLazyLoad)
                {
                    await LoadNodesLazilyAsync(rootNode);
                }
                else
                {
                    await LoadNodesAsync(rootNode);
                }

                _isDirectoryTreeLoaded = true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Нет доступа к указанной директории.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке дерева директорий: {ex.Message}");
            }

            SynchronizeButton_Click(null, null);
        }

        #endregion



        #region События================================================================================================================================================================================
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var config = new SearchConfig
            {
                StartDirectory = StartDirectoryTextBox.Text,
                RegexPattern = RegexTextBox.Text,
                RootDirectory = _rootPath
            };
            SaveConfig(config);
        }

        private void ResultsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ResultsTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                StartDirectoryTextBox.Text = selectedItem.Tag as string;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchAsync();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ElapsedTimeTextBlock.Text = (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ResultsTreeView.SelectedItem is TreeViewItem selectedItem)
                {
                    string filePath = selectedItem.Header.ToString();
                    if (MessageBox.Show($"Вы уверены, что хотите удалить файл {filePath}?", "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            File.Delete(filePath);
                            ResultsTreeView.Items.Remove(selectedItem);
                            MessageBox.Show("Файл удален.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении файла: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении файла: {ex.Message}");
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ResultsTreeView.SelectedItem is TreeViewItem selectedItem)
                {
                    string filePath = selectedItem.Header.ToString();
                    var editWindow = new EditFileWindow(filePath);
                    editWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменение файла: {ex.Message}");
            }
        }

        private void SynchronizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = StartDirectoryTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(path))
                {
                    MessageBox.Show("Путь не указан.");
                    return;
                }

                if (File.Exists(path))
                {
                    string directory = Path.GetDirectoryName(path);
                    if (Directory.Exists(directory))
                    {
                        var rootItem = (TreeViewItem)ResultsTreeView.Items[0];
                        var itemToSelect = FindNodeByPath(rootItem, path);
                        if (itemToSelect != null)
                        {
                            SelectTreeViewItem(itemToSelect);
                        }
                        else
                        {
                            MessageBox.Show("Файл не найден в дереве.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Директория не существует.");
                    }
                }
                else if (Directory.Exists(path))
                {
                    var rootItem = (TreeViewItem)ResultsTreeView.Items[0];
                    var itemToSelect = FindNodeByPath(rootItem, path);
                    if (itemToSelect != null)
                    {
                        SelectTreeViewItem(itemToSelect);
                    }
                    else
                    {
                        MessageBox.Show("Директория не найдена в дереве.");
                    }
                }
                else
                {
                    MessageBox.Show("Указанный путь не существует.");
                }
            }
            catch
            {
                MessageBox.Show($"Вернитесь сначала в список директорий. \r\nВ поиске файлов нельзя найти директорию");
            }
        }
        #endregion


        #region Вспомогательные методы================================================================================================================================================================================
        private void UpdatePlaceholderVisibility(TextBox textBox, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion
    }
}
