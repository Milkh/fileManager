using System;
using System.IO;
using System.Windows;

namespace FileManager.windows
{
    public partial class EditFileWindow : Window
    {
        private string _filePath;

        public EditFileWindow(string filePath)
        {
            InitializeComponent();
            _filePath = filePath;
            FileContentTextBox.Text = File.ReadAllText(filePath);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(_filePath, FileContentTextBox.Text);
                MessageBox.Show("Файл сохранен.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}");
            }
        }
    }
}