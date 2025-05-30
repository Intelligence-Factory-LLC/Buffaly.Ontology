namespace ProtoScript.Extensions.SolutionItems
{
	public class Project
	{
		public string ProjectName;
		public string FileName;
		public string RootDirectory;

		public List<ProjectFile> Files = new List<ProjectFile>();

		public bool HasProjectChangedOnDisk()
		{
			bool bChanged = false;

			foreach (ProjectFile projectFile in this.Files)
			{

				//>check the file length against the length of the file on disk
				string strFileOnDisk = projectFile.FileName;
				if (!System.IO.File.Exists(strFileOnDisk))
				{
					bChanged = true;
					break;
				}

				long lengthOnDisk = new System.IO.FileInfo(strFileOnDisk).Length;
				if (lengthOnDisk != projectFile.Length)
				{
					bChanged = true;
					break;
				}
			}

			return bChanged;
		}
	}
}
