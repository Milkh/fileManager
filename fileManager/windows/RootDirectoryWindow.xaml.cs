using System.IO;
using System.Text.Json;
using System.Windows;
using fileManager;



namespace FileManager.windows
{
    
    /// <summary>
    /// Логика взаимодействия для RootDirectoryWindow.xaml
    /// </summary>
    public partial class RootDirectoryWindow : Window
    {
        
        private string _configFilePath = "config.json";
        private SearchConfig _config;
        public string SelectedDirectory { get; private set; }
        public bool IsLazyLoad { get; private set; } = false;

        public RootDirectoryWindow()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void LazyLoadButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDirectory = RootDirectoryTextBox.Text;
            if (Directory.Exists(SelectedDirectory))
            {
                IsLazyLoad = true;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Указанная директория не существует. Пожалуйста, выберите существующую директорию.");
            }
        }


        private void LoadConfig()
        {
            if (File.Exists(_configFilePath))
            {
                string json = File.ReadAllText(_configFilePath);
                _config = JsonSerializer.Deserialize<SearchConfig>(json);

                
                RootDirectoryTextBox.Text = _config.RootDirectory;
            }
            else
            {
                _config = new SearchConfig();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            
            SelectedDirectory = RootDirectoryTextBox.Text;
            if (Directory.Exists(SelectedDirectory))
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Указанная директория не существует. Пожалуйста, выберите существующую директорию.");
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
             DialogResult = true;
             Close();
        }


    }
}
