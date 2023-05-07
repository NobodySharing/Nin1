using System;

namespace Common
{
	public class PersistentStorage
	{
		private const string MyDirName = "Nin1";
		private const string ConfigFileName = "Config.txt";
		private string DefaultConfigFileContent =
			"VPE:\r\n" +
			" \r\n" +
			
			"Password generator:\r\n" +
			" \r\n" +
			
			"Date-time calculator:\r\n" +
			" \r\n";

		private readonly DirectoryInfo MyDir; // Cesta ke složce s konfigurákem.
		private readonly FileInfo MyConfig; // Cesta ke konfiguráku.

		public PersistentStorage()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + MyDirName + "\\";
			MyDir = new(path);
			MyConfig = new(path + ConfigFileName);
			CheckOrCreateMyFolderAndConfig();
		}

		public void CheckOrCreateMyFolderAndConfig()
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
			File.WriteAllText(MyConfig.FullName, DefaultConfigFileContent);
		}
		/*
		internal string ReadConfig()
		{

		}

		internal void WriteConfig()
		{

		}*/
	}
}