using System.Diagnostics;
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

        [CommandOption("resolution", 'r', Description = "Sets the resolution of the video. (default is 1080p)")]
        public string Quality { get; init; } = "1080p";

        [CommandOption("audio", 'a', Description = "Audio only (default is false)")]
        public bool AudioOnly { get; init; } = false;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync(AudioOnly.ToString());

            var arguments = AudioOnly
                ? $@"-f ""ba"" {Url}"
                : $@"--audio-multistreams -S ""res:{new string(Quality.Where(char.IsDigit).ToArray())}"" {Url}";

            var process = new Process()
            {
                StartInfo =
                {
                    FileName = Dependency.Youtube,
                    Arguments = arguments,
                    UseShellExecute = false
                }
            };

            Console.ResetColor();
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
