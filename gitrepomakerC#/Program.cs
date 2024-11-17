using System;
using System.Diagnostics;

namespace PowerShellExecution
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("git repo maker (git needed)");
            Console.ResetColor();

            string command = "git init repoexample";

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "/bin/bash";
            processInfo.Arguments = $"-c \"{command}\"";
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;

            try
            {
                using (Process process = Process.Start(processInfo))
                {
                    using (System.IO.StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        Console.WriteLine(result);
                    }
                }

                Console.WriteLine("repo has been made, it's called repoexample, please rename it if you are going to make more repos with this tool.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
