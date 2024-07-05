using System;
using System.Diagnostics;
using System.Text;

namespace SandboxTest.Utils
{
    public static class CommandLineUtils
    {
        public static async Task<string> RunCommand(string commandToRun)
        {
            return await RunCommand(commandToRun, Directory.GetCurrentDirectory());
        }

        public static async Task<string> RunCommand(string commandToRun, string workingDirectory)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                using (var windowsCommandLineProcess = new Process())
                {
                    windowsCommandLineProcess.StartInfo.FileName = "cmd";
                    windowsCommandLineProcess.StartInfo.RedirectStandardOutput = true;
                    windowsCommandLineProcess.StartInfo.RedirectStandardInput = true;
                    windowsCommandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
                    windowsCommandLineProcess.Start();

                    await windowsCommandLineProcess.StandardInput.WriteLineAsync($"{commandToRun} & exit");
                    await windowsCommandLineProcess.WaitForExitAsync();

                    var output = await windowsCommandLineProcess.StandardOutput.ReadToEndAsync();
                    return output;
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

                    await linuxCommandLineProcess.WaitForExitAsync();

                    var output = await linuxCommandLineProcess.StandardOutput.ReadToEndAsync();
                    return output;
                }
        }

        public static async Task<Process> RunProcess(string commandToRun, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            return await RunProcess(commandToRun, Directory.GetCurrentDirectory(), outputReceived, errorReceived);
        }

        public static async Task<Process> RunProcess(string commandToRun, string workingDirectory, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var windowsCommandLineProcess = new Process();
                windowsCommandLineProcess.StartInfo.FileName = "cmd";
                windowsCommandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
                AddRunProcessCommonParameters(outputReceived, errorReceived, windowsCommandLineProcess);
                windowsCommandLineProcess.Start();
                windowsCommandLineProcess.BeginOutputReadLine();
                windowsCommandLineProcess.BeginErrorReadLine();

                await windowsCommandLineProcess.StandardInput.WriteLineAsync($"{commandToRun}");
                return windowsCommandLineProcess;
            }
            else
            {
                var linuxCommandLineProcess = new Process();
                linuxCommandLineProcess.StartInfo.FileName = "/bin/bash";
                linuxCommandLineProcess.StartInfo.Arguments = "-c \" " + commandToRun + " \"";
                AddRunProcessCommonParameters(outputReceived, errorReceived, linuxCommandLineProcess);
                linuxCommandLineProcess.Start();
                linuxCommandLineProcess.BeginOutputReadLine();
                linuxCommandLineProcess.BeginErrorReadLine();

                return linuxCommandLineProcess;
            }
        }

        private static void AddRunProcessCommonParameters(Action<string>? outputReceived, Action<string>? errorReceived, Process commandLineProcess)
        {
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
        }
    }
}