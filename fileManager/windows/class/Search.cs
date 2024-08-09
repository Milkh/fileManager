using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using fileManager;




namespace FileManager.windows
{
    public partial class MainWindow : Window
    {
        private async void SearchAsync()
        {
            if (!_isDirectoryTreeLoaded && !_isLazyLoad)
            {
                MessageBox.Show("Дерево директорий еще загружается. Пожалуйста, подождите.");
                return;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _startTime = DateTime.Now;
            _timer.Start();

            ResultsTreeView.Items.Clear();
            CurrentDirectoryTextBlock.Text = "";
            FoundFilesTextBlock.Text = "0";
            ElapsedTimeTextBlock.Text = "0";

            string startDirectory = StartDirectoryTextBox.Text;
            string regexPattern = RegexTextBox.Text;

            SaveConfig(new SearchConfig { StartDirectory = startDirectory, RegexPattern = regexPattern });

            try
            {
                await Task.Run(() => SearchFiles(startDirectory, regexPattern, _cancellationTokenSource.Token));
                OpenFoundDirectoryOrFile(startDirectory);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Поиск был остановлен.");
            }
            finally
            {
                _timer.Stop();
            }
        }

        private void SearchFiles(string startDirectory, string regexPattern, CancellationToken token)
        {
            var regex = new Regex(regexPattern);
            int foundFiles = 0;
            int totalFiles = 0;

            void SearchDirectory(string directory)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    Dispatcher.Invoke(() =>
                    {
                        CurrentDirectoryTextBlock.Text = directory;
                        FoundFilesTextBlock.Text = foundFiles.ToString();
                    });

                    foreach (var file in Directory.GetFiles(directory))
                    {
                        totalFiles++;
                        if (regex.IsMatch(Path.GetFileName(file)))
                        {
                            foundFiles++;
                            Dispatcher.Invoke(() =>
                            {
                                ResultsTreeView.Items.Add(new TreeViewItem { Header = file, Tag = file });
                            });
                        }
                    }

                    foreach (var subDirectory in Directory.GetDirectories(directory))
                    {
                        SearchDirectory(subDirectory);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Директория не найдена:");
                    });
                }
                catch (FileNotFoundException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Файл не найден в директории: ");
                    });
                }
                catch (UnauthorizedAccessException) { }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show($"Файл не найден в директории: ");
                }
                catch (Exception ex) { }
            }

            SearchDirectory(startDirectory);
        }

        private void SaveConfig(SearchConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config);
                File.WriteAllText("config.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении конфигурации: {ex.Message}");
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    string json = File.ReadAllText("config.json");
                    var config = JsonSerializer.Deserialize<SearchConfig>(json);
                    StartDirectoryTextBox.Text = config.StartDirectory;
                    RegexTextBox.Text = config.RegexPattern;

                    UpdatePlaceholderVisibility(StartDirectoryTextBox, StartDirectoryPlaceholder);
                    UpdatePlaceholderVisibility(RegexTextBox, RegexPlaceholder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке конфигурации: {ex.Message}");
            }
        }

        private void OpenFoundDirectoryOrFile(string path)
        {
            try
            {
                var itemToSelect = FindNodeByPath((TreeViewItem)ResultsTreeView.Items[0], path);
                if (itemToSelect != null)
                {
                    itemToSelect.BringIntoView();
                    itemToSelect.Focus();
                    SelectTreeViewItem(itemToSelect);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Файл или директория не найден. \r\nВозможно вы указали вместо директории путь к файлу");
            }
        }
    }
}
