using System.Diagnostics;
using System.Text;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace youtube
{
    [Command(Description = "Download YouTube video.")]
    public class Download : ICommand
    {
        [CommandParameter(0, Description = "Video URL.")]
        public required string Url { get; init; }

        [CommandOption("resolution", 'r', Description = "Sets the resolution of the video.")]
        public string Quality { get; init; } = "1080p";

        [CommandOption("audio", 'a', Description = "Audio only.")]
        public bool AudioOnly { get; init; } = false;

        [CommandOption("output", 'o', Description = "Sets the output directory.")]
        public string? Output { get; init; } = null;

        [CommandOption("extension", 'e', Description = "Sets the extension of the output file.")]
        public string? Extension { get; init; } = null;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var output = Output?.ToLowerInvariant() switch
            {
                "desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "video" or "videos" => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                "music" or "musics" => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                "picture" or "pictures" or "photo" or "photos" => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                null => Environment.CurrentDirectory,
                _ => Output!
            };

            var extension = (Extension?.ToLowerInvariant(), AudioOnly) switch
            {
                (null, true) => "mp3",
                (null, false) => null,
                (_, _) => Extension.TrimStart('.')
            };

            var arguments = new StringBuilder()
                .AppendSpace(AudioOnly ? @"-f ""ba""" : $@"--audio-multistreams -S ""res:{new string(Quality.Where(char.IsDigit).ToArray())}""")
                .AppendSpace(!AudioOnly ? $"--merge-output-format {extension}" : string.Empty)
                .AppendSpace($@"-o ""{output}/%(title)s-%(id)s.{extension ?? "%(ext)s"}""")
                .Append(Url)
                .ToString();

            //var arguments = AudioOnly
            //    ? $@"-f ""ba"" {Url}"
            //    : $@"--audio-multistreams -S ""res:{new string(Quality.Where(char.IsDigit).ToArray())}"" {Url}";

            var process = new Process()
            {
                StartInfo =
                {
                    FileName = Dependency.Youtube,
                    Arguments = arguments,
                    UseShellExecute = false
                }
            };

            console.ResetColor();
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
