using System.Collections.ObjectModel;

namespace StudentCabinet.Views
{
    public partial class LogViewerPage : ContentPage
    {
        private List<string> _allLines = new();

        public LogViewerPage()
        {
            InitializeComponent();
            LoadLogs();
            LevelPicker.SelectedIndex = 0;
        }

        private void LoadLogs()
        {
            var logsDir = Path.Combine(FileSystem.AppDataDirectory, "logs");

            if (!Directory.Exists(logsDir))
            {
                _allLines = new List<string> { "Файл логів ще не створено." };
                return;
            }

            var latestFile = Directory.GetFiles(logsDir, "log-*.txt")
                .OrderByDescending(File.GetLastWriteTime)
                .FirstOrDefault();

            if (latestFile is null)
            {
                _allLines = new List<string> { "Файл логів не знайдено." };
                return;
            }

            var lines = new List<string>();
            using (var stream = new FileStream(latestFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                    lines.Add(reader.ReadLine()!);
            }

            _allLines = lines;
        }

        private void OnLevelChanged(object sender, EventArgs e)
        {
            var level = LevelPicker.SelectedItem?.ToString() ?? "Усі";
            ApplyFilter(level);
        }

        private void ApplyFilter(string level)
        {
            var filtered = level == "Усі"
                ? _allLines
                : _allLines.Where(l => l.Contains($"[{level}]")).ToList();

            LogsView.ItemsSource = new ObservableCollection<string>(filtered);
        }
    }
}