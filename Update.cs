using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace youtube
{
    [Command("update", Description = "Force the updating of dependencies.")]
    public class Update : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync("Forcing update on YT-DLP...");
            await Dependency.UpdateAsync(true);
        }
    }
}
