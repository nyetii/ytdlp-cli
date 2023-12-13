using System.Diagnostics;
using System;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace youtube
{
    internal static class Dependency
    {
        //private static string _dir = string.Empty;
        //public static string FileDirectory
        //{
        //    get => _dir;
        //    set
        //    {
        //        value = SetDirectory();
        //        _dir = value;
        //    }
        //}
        public static string FileDirectory => SetDirectory();

        public static string Ffmpeg { get; private set; } = string.Empty;
        public static string Youtube { get; private set; } = string.Empty;

        public static DateTime? LastUpdated { get; private set; }

        private const EnvironmentVariableTarget Scope = EnvironmentVariableTarget.User;
        private static string _path = Environment.GetEnvironmentVariable("PATH", Scope) 
                                     ?? throw new NotSupportedException("Variável de Ambiente \"PATH\" não foi encontrada.");

        public static async Task InitializeAsync()
        {
            var paths = _path.Split(';');

            Youtube = paths.Select(x => Path.Combine(x, "yt-dlp.exe"))
                .Where(File.Exists)
                .FirstOrDefault() ?? await DownloadYoutubeAsync();
            
            
            var lastUpdatedStr = await File.ReadAllTextAsync($@"{FileDirectory}\lastupdated.txt");

            DateTime? lastUpdated = string.IsNullOrWhiteSpace(lastUpdatedStr) ? null : DateTime.Parse(lastUpdatedStr);

            LastUpdated = lastUpdated;

            if (LastUpdated - DateTime.Today >= TimeSpan.FromDays(7) || lastUpdated is null)
                await UpdateAsync();

            Ffmpeg = paths.Select(x => Path.Combine(x, "ffmpeg.exe"))
                .Where(File.Exists)
                .FirstOrDefault() ?? await DownloadFfmpegAsync();
        }

        public static async Task UpdateAsync(bool forced = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Checking for updates");
            Console.ResetColor();
            var process = new Process()
            {
                StartInfo =
                {
                    FileName = Dependency.Youtube,
                    Arguments = $@"--update",
                    UseShellExecute = false
                }
            };
            Console.ResetColor();
            process.Start();
            await process.WaitForExitAsync();

            if(!forced)
                Console.Clear();

            LastUpdated = DateTime.Today;
            await File.WriteAllTextAsync($@"{FileDirectory}\lastupdated.txt", LastUpdated.ToString());
        }

        private static async Task<string> DownloadYoutubeAsync()
        {
            using var http = new HttpClient();
            await File.WriteAllBytesAsync($"{FileDirectory}/yt-dlp.exe",
                await http.GetByteArrayAsync(@"https://github.com/yt-dlp/yt-dlp/releases/download/2023.03.04/yt-dlp.exe"));

            LastUpdated = DateTime.Today;
            await File.WriteAllTextAsync($@"{FileDirectory}\lastupdated.txt", LastUpdated.ToString());

            return $"{FileDirectory}\\yt-dlp.exe";
        }

        private static async Task<string> DownloadFfmpegAsync()
        {
            using var http = new HttpClient();

            await File.WriteAllBytesAsync("ffmpeg.zip",
                await http.GetByteArrayAsync(@"https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"));

            ZipFile.ExtractToDirectory("ffmpeg.zip", $@"{Environment.CurrentDirectory}/ffmpeg");
            File.Move(@$"{Environment.CurrentDirectory}/ffmpeg/ffmpeg-6.0-essentials_build/bin/ffmpeg.exe",
                $"{FileDirectory}/ffmpeg.exe");

            Directory.Delete($@"{Environment.CurrentDirectory}/ffmpeg", true);
            File.Delete("ffmpeg.zip");

            return $"{FileDirectory}\\ffmpeg.exe";
        }

        private static string SetDirectory()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            directory += $"\\YouTube";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine(directory);
            }

            if (!File.Exists($"{directory}/youtube.exe"))
                File.Copy($"{Environment.CurrentDirectory}\\youtube.exe", $"{directory}\\youtube.exe");

            if (!File.Exists($"{directory}/youtube.dll"))
                File.Copy($"{Environment.CurrentDirectory}\\youtube.dll", $"{directory}\\youtube.dll");

            if (!File.Exists($"{directory}/youtube.deps.json"))
                File.Copy($"{Environment.CurrentDirectory}\\youtube.deps.json", $"{directory}\\youtube.deps.json");

            if (!File.Exists($"{directory}/youtube.runtimeconfig.json"))
                File.Copy($"{Environment.CurrentDirectory}\\youtube.runtimeconfig.json", $"{directory}\\youtube.runtimeconfig.json");

            var paths = _path.Split(';');

            if(paths.Any(x => x == directory))
                return directory;

            _path += $";{directory}";

            Environment.SetEnvironmentVariable("PATH", _path, Scope);

            return directory;
        }
    }
}
