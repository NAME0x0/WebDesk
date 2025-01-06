using Microsoft.Win32;
using System.Windows;

namespace WebDesk
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsManager _settings;

        public SettingsWindow(SettingsManager settings)
        {
            InitializeComponent();
            _settings = settings;
            LoadSettings();
        }

        private void LoadSettings()
        {
            PathTextBox.Text = _settings.WallpaperPath;
            AutoStartCheckBox.IsChecked = _settings.AutoStart;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "HTML files (*.html;*.htm)|*.html;*.htm|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.WallpaperPath = PathTextBox.Text;
            _settings.AutoStart = AutoStartCheckBox.IsChecked ?? false;
            _settings.Save();
            Close();
        }
    }
}