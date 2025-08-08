using BasicUtilities;
using System;
using System.Diagnostics;

namespace Ontology.Agents.Actions
{
	public class SystemOperations
	{
		static public string RunCommandLine(string strCommand, string strArgs)
		{
			return CmdUtil.RunCmdWithLogging(strCommand, strArgs);
		}
		static public string RunCommandLineAndResume(string strCommand, string strArgs)
		{
			return CmdUtil.RunCmdWithLogging(strCommand, strArgs, false);
		}

		//>+ create a version of RunCommandLine that can be used to launch a file directly using the default app
		static public string LaunchFile(string strFilePath)
		// Launches a file using the command line by executing a 'start' command with the specified file path.
		{
			string CommandName = System.IO.Path.Combine(Environment.SystemDirectory, "cmd.exe");
			//> use the full path to cmd.exe for CommandName

			string Args = $"/c start \"\" \"{strFilePath}\"";

			return RunCommandLine(CommandName, Args);
		}

		//> add a method to get all files from a directory that match a pattern
		static public List<string> GetFilesMatchingPattern(string strDirectory, string strPattern)
		{
			return FileUtil.GetPathsMatching(Path.Combine(strDirectory, strPattern));
		}

		//> add a method to get the subdirectories under a parent directory
		static public List<string> GetSubdirectories(string strParentDirectory)
		{
			try
			{
				string[] arrSubdirectories = Directory.GetDirectories(strParentDirectory);
				List<string> lstSubdirectories = new List<string>(arrSubdirectories);
				return lstSubdirectories;
			}
			catch (Exception ex)
			{
				Logs.DebugLog.WriteEvent("Error Fetching Subdirectories", $"Directory: {strParentDirectory} Exception: {ex.Message}");
				return new List<string>();
			}
		}

		//> add a method to search for a file recursively		
		static public List<string> SearchFilesRecursively(string strDirectory, string strFileName)		
		// Searches for files in a specified directory and its subdirectories that match a given name and returns a list of their paths.
		{
			List<string> lstMatchingFiles = new List<string>();
			string[] files = Directory.GetFiles(strDirectory, strFileName, SearchOption.AllDirectories);
			lstMatchingFiles.AddRange(files);
			return lstMatchingFiles;
		}

		//> add method to copy a file from one path to a target directory
		static public void CopyFile(string strSourcePath, string strTargetDirectory)
		{
			string strFileName = Path.GetFileName(strSourcePath);
			string strDestinationPath = FileUtil.BuildPath(strTargetDirectory, strFileName);
			File.Copy(strSourcePath, strDestinationPath, true);
		}

		//> add a method to take a list of files and copy them to a target directory
		static public void CopyFilesToDirectory(List<string> lstSourceFiles, string strTargetDirectory)
		{
			DirectoryUtil.CreateDirectoryIfMissing(strTargetDirectory);
			foreach (string strSourceFile in lstSourceFiles)
			{
				CopyFile(strSourceFile, strTargetDirectory);
			}
		}

		//>+ create a method to get the last 3 events from the Windows Event Log in the Application log
		//>and return them as a list of strings
                static public List<string> GetLast3EventsFromApplicationLog()
                // Retrieves the last three log entries from the application event log and formats them into a list of strings.
                {
                        List<string> lstEvents = new List<string>();
                        if (OperatingSystem.IsWindows())
                        {
                                EventLog eventLog = new EventLog("Application");
                                EventLogEntryCollection eventEntries = eventLog.Entries;
                                for (int i = eventEntries.Count - 1; i >= 0 && lstEvents.Count < 3; i--)
                                {
                                        EventLogEntry entry = eventEntries[i];
                                        //>+ serialize the most important fields to string
                                        lstEvents.Add($"Timestamp: {entry.TimeGenerated}, Source: {entry.Source}, Message: {entry.Message}");

                                }
                        }
                        return lstEvents;
                }

	}
}
