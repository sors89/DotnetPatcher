using ICSharpCode.Decompiler.Metadata;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace DotnetPatcher
{
	public class ModuleReader
	{
		public static PEFile ReadModule(string path, bool createBackup)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException($"Could not find file {path}");
			}

			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				PEFile module = new PEFile(path, fileStream, PEStreamOptions.PrefetchEntireImage);
				AssemblyName assemblyName = new AssemblyName(module.FullName);


				string versionedPath = $"{path}_Backup";
				if (!File.Exists(versionedPath))
				{
					File.Copy(path, versionedPath);
				}


				return module;
			}
		}
	}
}
