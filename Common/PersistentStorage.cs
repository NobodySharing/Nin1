using System;

namespace Common
{
	public class PersistentStorage
	{
		private const string MyDirName = "Nin1";
		private const string BasicConfigFileName = "Config.txt";
		private string BasicConfigFileContent = "";

		private readonly DirectoryInfo MyDir;
		private readonly FileInfo MyConfig;
		public PersistentStorage()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + MyDirName + "\\";
			MyDir = new(path);
			MyConfig = new(path + BasicConfigFileName);
			BasicConfigFileContent = path;
		}

		public void CheckCreateMyFolderAndConfig()
		{
			if (MyDir.Exists)
			{
				if (MyConfig.Exists)
				{
					return;
				}
				else
				{
					CreateBasicConfig();
				}
			}
			else
			{
				MyDir.Create();
				CreateBasicConfig();
			}
		}

		private void CreateBasicConfig()
		{
			MyConfig.Create().Close();
			File.WriteAllText(MyConfig.FullName, BasicConfigFileContent);
		}
	}
}