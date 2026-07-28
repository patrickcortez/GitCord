using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Gitbot2.Source.Core
{
    internal class Shell // Might use in the future, initially for /external <command> <args>
    {
        private Process shell;
        private ILogger logger;
        private StringBuilder error;
        public Shell(string Arguments,ILogger _logger)
        {
            shell = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd",
                    Arguments = $"/c {Arguments}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow=true,
                    ErrorDialog=true
                }
            };

        }

        public async Task<(int exc,string output)> ExecuteAsync()
        {
            shell.OutputDataReceived += (_,e) =>
            {
                if(e.Data is not null)
                {
                    // do nothing, just consume data
                }
            };

            shell.ErrorDataReceived += (_, e) =>
            {
                if(e.Data is not null)
                {
                    error.Append(e.Data);
                }
            };



            if (shell.Start())
            {
                shell.BeginErrorReadLine();
                shell.BeginOutputReadLine();

                await shell.WaitForExitAsync();

                int exc = shell.ExitCode;

                if(error.Length > 0)
                {
                    return (exc, error.ToString()); // grab exit-code and error message
                }

                return (exc,"success");
            }

            logger.LogError("Process failed to start");

            return (1,string.Empty);
        }
    }
}
