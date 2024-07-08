using System.Diagnostics;
using System.Text;

namespace SandboxTest.Utils
{
    public static class CommandLineUtils
    {
        public static async Task<string> RunCommandAsync(string commandToRun)
        {
            return await RunCommandAsync(commandToRun, Directory.GetCurrentDirectory());
        }

        public static async Task<string> RunCommandAsync(string commandToRun, string workingDirectory)
        {
            var output = new StringBuilder();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                using (var windowsCommandLineProcess = new Process())
                {
                    windowsCommandLineProcess.StartInfo.FileName = "cmd";
                    windowsCommandLineProcess.StartInfo.RedirectStandardOutput = true;
                    windowsCommandLineProcess.StartInfo.RedirectStandardInput = true;
                    windowsCommandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
                    windowsCommandLineProcess.Start();

                    windowsCommandLineProcess.OutputDataReceived += delegate (object sender, DataReceivedEventArgs args) {
                        output.AppendLine(args.Data);
                    };
                    windowsCommandLineProcess.BeginOutputReadLine();

                    await windowsCommandLineProcess.StandardInput.WriteLineAsync($"{commandToRun} & exit");
                    await windowsCommandLineProcess.WaitForExitAsync();

                    return output.ToString();
                }
            }
            else
                using (var linuxCommandLineProcess = new Process())
                {
                    linuxCommandLineProcess.StartInfo.FileName = "/bin/bash";
                    linuxCommandLineProcess.StartInfo.Arguments = "-c \" " + commandToRun + " \"";
                    linuxCommandLineProcess.StartInfo.UseShellExecute = false;
                    linuxCommandLineProcess.StartInfo.RedirectStandardOutput = true;
                    linuxCommandLineProcess.StartInfo.RedirectStandardError = true;
                    linuxCommandLineProcess.Start();

                    linuxCommandLineProcess.OutputDataReceived += delegate (object sender, DataReceivedEventArgs args) {
                        output.AppendLine(args.Data);
                    };
                    linuxCommandLineProcess.BeginOutputReadLine();

                    await linuxCommandLineProcess.WaitForExitAsync();

                    return output.ToString();
                }
        }

        public static Task<Process> RunCommandLineProcessAsync(string commandToRun, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            return RunCommandLineProcessAsync(commandToRun, Directory.GetCurrentDirectory(), outputReceived, errorReceived);
        }

        public static async Task<Process> RunCommandLineProcessAsync(string commandToRun, string workingDirectory, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            var commandLineProcess = new Process();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                commandLineProcess.StartInfo.FileName = "cmd";
                commandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
            }
            else
            {
                commandLineProcess.StartInfo.FileName = "/bin/bash";
                commandLineProcess.StartInfo.Arguments = "-c \" " + commandToRun + " \"";
            }
            commandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
            commandLineProcess.StartInfo.UseShellExecute = false;
            commandLineProcess.StartInfo.RedirectStandardError = true;
            commandLineProcess.StartInfo.RedirectStandardOutput = true;
            commandLineProcess.StartInfo.RedirectStandardInput = true;

            if (outputReceived != null)
            {
                commandLineProcess.OutputDataReceived += (sender, args) =>
                {
                    outputReceived(args.Data ?? string.Empty);
                };
            }
            if (errorReceived != null)
            {
                commandLineProcess.ErrorDataReceived += (sender, args) =>
                {
                    errorReceived(args.Data ?? string.Empty);
                };
            }
            commandLineProcess.Start();
            commandLineProcess.BeginOutputReadLine();
            commandLineProcess.BeginErrorReadLine();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                await commandLineProcess.StandardInput.WriteLineAsync($"{commandToRun}");
            }

            return commandLineProcess;
        }
    }
}