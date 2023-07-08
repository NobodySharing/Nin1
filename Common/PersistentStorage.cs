using System;

namespace Common
{
	public class PersistentStorageManager
	{
		private const string MyDirName = "Nin1";
		private const string ConfigFileName = "Config.txt";
		private readonly FileInfo ConfigFile; // Path to config file directly.

		public PersistentStorageManager()
		{
			ConfigFile = new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + MyDirName + "\\" + ConfigFileName);
			CheckOrCreateMyConfig();
		}
		/// <summary></summary>
		public void CheckOrCreateMyConfig()
		{
			if (ConfigFile.Exists)
			{
				if (ConfigFile.Exists)
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
				ConfigFile.Create();
				CreateBasicConfig();
			}
		}

		private void CreateBasicConfig()
		{
			System.Text.StringBuilder sb = new();
			sb.Append("[Common]\r\n");
			sb.Append("\tIsDefault=true\r\n");
			sb.Append("[VPE]\r\n");
			sb.Append("\tAutoload Table Library=\r\n");
			sb.Append("\tAutoload Settings Library=\r\n");
			sb.Append("\tSelect Settings=\r\n");
			sb.Append("[Neue date-time]\r\n");
			sb.Append(" \r\n");
			sb.Append("[Date-time calculator]\r\n");
			sb.Append(" \r\n");
			sb.Append("[Factorizator]\r\n");
			sb.Append(" \r\n");
			sb.Append("[Password generator]\r\n");
			sb.Append(" \r\n");
			sb.Append("[Map downloader]\r\n");
			sb.Append(" \r\n");
			File.WriteAllText(ConfigFile.FullName, sb.ToString());
		}
		
		public PersistentStorage ReadConfig()
		{
			PersistentStorage ps = new();
			if (ConfigFile.Exists)
			{
				string[] content = File.ReadAllLines(ConfigFile.FullName);

			}
			return ps;
		}

		public void WriteConfig(PersistentStorage ps)
		{

		}
	}

	public class PersistentStorage
	{
		#region Common
		bool IsDefault = true;
		#endregion
		#region VPE
		string PathsToSettLib = "";
		string PathsToTableLib = "";
		int ActiveSett = -1;
		#endregion
	}
}