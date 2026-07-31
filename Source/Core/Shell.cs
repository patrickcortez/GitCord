using Gitbot2.Source.Commands;
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
        public Shell(string Arguments, ILogger _logger)
        {
            logger = _logger;
            error = new();
            string shellname = "",commandflag="";
            if (OperatingSystem.IsWindows())
            {
                shellname = "cmd";
                commandflag = "/c";
            }else if (OperatingSystem.IsLinux())
            {
                shellname = "bash";
                commandflag = "-c";
            }else if (OperatingSystem.IsMacOS())
            {
                shellname = "zsh";
                commandflag = "-c";
            }

            shell = new()
            {
                StartInfo = new()
                {
                    FileName = shellname,
                    Arguments = $"{commandflag} \"{Arguments}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    ErrorDialog = true,
                    WorkingDirectory = RepoCache.GetCurrentRepo().Info.WorkingDirectory
                }
            };

        }

        public async Task<(int exc,string output)> ExecuteAsync()
        {
            try
            {
                StringBuilder DataOutput = new();
                shell.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
                    {
                        DataOutput.AppendLine(e.Data);
                    }
                };

                shell.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null)
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

                    string output = DataOutput.ToString();

                    if (error.Length > 0)
                    {
                        return (exc, error.ToString()); // grab exit-code and error message
                    }

                    return (exc, (!string.IsNullOrEmpty(output))? output : "Command Successfully ran");
                }

                logger.LogError("Process failed to start");

                return (1, error.ToString());
            }catch(Exception ex)
            {
                RepoCache.SetException(ex);
                logger.LogError(ex, "Something went wrong while sending command");
                return (1,"Command failed to run");
            }
        }
    }
}
