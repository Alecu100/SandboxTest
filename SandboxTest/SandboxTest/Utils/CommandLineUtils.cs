using System.Diagnostics;

namespace SandboxTest.Utils
{
    public static class CommandLineUtils
    {
        static async Task<string> RunCommand(string commandToRun)
        {
            return await RunCommand(commandToRun, Directory.GetCurrentDirectory());
        }

        private static async Task<string> RunCommand(string commandToRun, string workingDirectory)
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
    }
}