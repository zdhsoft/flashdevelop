using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectManager.Building
{
    public class ProcessRunner
    {
        public static bool Run(string fileName, string arguments, bool ignoreExitCode, bool mergeErrors)
        {
            // CrossOver native call
            bool isNative = fileName == "FDEXE.sh" || Path.GetExtension(fileName) == ".command";

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = Encoding.Default;
            process.StartInfo.StandardErrorEncoding = Encoding.Default;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(fileName);
            process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(arguments);
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.Start();
            
            // capture output in a separate thread
            LineFilter stdoutFilter = new LineFilter(process.StandardOutput, Console.Out, false);
            LineFilter stderrFilter = new LineFilter(process.StandardError, Console.Error, mergeErrors);

            Thread outThread = new Thread(stdoutFilter.Filter);
            Thread errThread = new Thread(stderrFilter.Filter);

            outThread.Start();
            errThread.Start();

            // Call is redirected, process is lost, will finish when done.
            if (isNative) return stderrFilter.Lines == 0;

            process.WaitForExit();
            outThread.Join(1000);
            errThread.Join(1000);
            
            return (ignoreExitCode) ? stderrFilter.Lines == 0 : process.ExitCode == 0;
        }
    }

    class LineFilter
    {
        static readonly Regex reSplitError = new Regex(@"\.[a-z]+:[0-9]+$");
        readonly TextReader reader;
        readonly TextWriter writer;
        readonly bool mergeErrors;
        string unsplit;
        public int Lines;

        public LineFilter(TextReader reader, TextWriter writer, bool mergeErrors)
        {
            this.reader = reader;
            this.writer = writer;
            this.mergeErrors = mergeErrors;
        }

        public void Filter()
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (line is null) break;

                if (mergeErrors)
                {
                    if (unsplit != null) { line = unsplit + ":" + line; unsplit = null; }
                    else if (reSplitError.IsMatch(line)) { unsplit = line; continue; }
                }
                writer.WriteLine(line);
                writer.Flush();
                
                if (line.Length > 0) Lines++;
            }
        }
    }
}