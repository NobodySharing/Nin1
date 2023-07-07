using System;

namespace Common
{
	public class PersistentStorageManager
	{
		private const string MyDirName = "Nin1";
		private const string ConfigFileName = "Config.txt";
		private string DefaultConfigFileContent =
			"[VPE]\r\n" +
			"\tAutoload Table Library=\r\n" +
			"\tAutoload Settings Library=\r\n" +
			"\tSelect Settings=\r\n" +
			"[Neue date-time]\r\n" +
			" \r\n" +
			"[Date-time calculator]\r\n" +
			" \r\n" +
			"[Factorizator]\r\n" +
			" \r\n" +
			"[Password generator]\r\n" +
			" \r\n" +
			"[Map downloader]\r\n" +
			" \r\n";
		private readonly DirectoryInfo MyDir; // Path to folder with config file.
		private readonly FileInfo MyConfig; // Path to config file directly.

		public PersistentStorageManager()
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
		
		internal PersistentStorage ReadConfig()
		{

		}

		internal void WriteConfig(PersistentStorage ps)
		{

		}
	}

	public class PersistentStorage
	{
		string PathsToSettLib;
		string PathsToTableLib;
		int ActiveSett;
	}
}