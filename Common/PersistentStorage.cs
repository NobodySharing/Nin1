using System;

namespace Common
{
	public class PersistentStorageManager
	{
		#region Texts for config
		private const string HeaderCommon = "[Common]\r\n";
		private const string HeaderVPE = "[VPE]\r\n";
		private const string HeaderNDT = "[Neue date-time]\r\n";
		private const string HeaderDTC = "[Date-time calculator]\r\n";
		private const string HeaderF = "[Factorizator]\r\n";
		private const string HeaderPG = "[Password generator]\r\n";
		private const string HeaderMD = "[Map downloader]\r\n";
		private const string LineIsDefault = "Is default";
		private const string LineShowTab = "Default tab";
		private const string LineAutoloadTableLib = "Autoload Table Library";
		private const string LineAutoloadSettingsLib = "Autoload Settings Library";
		#endregion
		private const string MyDirName = "Nin1";
		private const string ConfigFileName = "Config.txt";
		private readonly FileInfo ConfigFile; // Path to config file directly.

		public PersistentStorageManager()
		{
			ConfigFile = new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + MyDirName + "\\" + ConfigFileName);
			CheckOrCreateMyConfig();
		}
		/// <summary></summary>
		public void CheckOrCreateMyConfig()
		{
			if (ConfigFile.Exists)
			{
				return;
			}
			else
			{
				Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + MyDirName);
				ConfigFile.Create().Close();
				CreateBasicConfig();
			}
		}

		private void CreateBasicConfig()
		{
			System.Text.StringBuilder sb = new();
			sb.Append(HeaderCommon);
			sb.Append("\t" + LineIsDefault + "=true\r\n");
			sb.Append("\t" + LineShowTab + "=0\r\n");
			sb.Append(HeaderVPE);
			sb.Append("\t" + LineAutoloadTableLib + "=\r\n");
			sb.Append("\t" + LineAutoloadSettingsLib + "=\r\n");
			sb.Append(HeaderNDT);
			sb.Append("\t\r\n");
			sb.Append(HeaderDTC);
			sb.Append("\t\r\n");
			sb.Append(HeaderF);
			sb.Append("\t\r\n");
			sb.Append(HeaderPG);
			sb.Append("\t\r\n");
			sb.Append(HeaderMD);
			sb.Append("\t");
			File.WriteAllText(ConfigFile.FullName, sb.ToString());
		}
		
		public PersistentStorage ReadConfig()
		{
			PersistentStorage ps = new();
			if (ConfigFile.Exists)
			{
				string[] content = File.ReadAllLines(ConfigFile.FullName);
				foreach (string line in content)
				{
					if (line.StartsWith("[") && line.EndsWith("]"))
					{ // No processing for headers needed (yet).
						continue;
					}
					else
					{
						string temp = line.Trim();
						string[] part = temp.Split("=");
						if (part.Length == 2)
						{
							switch (part[0])
							{
								case LineIsDefault:
									ps.IsDefault = part[1] == "true" || part[1] == "True";
									break;
								case LineShowTab:
									if (byte.TryParse(part[1].Trim(), out byte tabIdx))
									{
										ps.ShowTab = tabIdx;
									}
									else
									{
										ps.ShowTab = 0;
									}
									break;
								case LineAutoloadTableLib:
									ps.PathsToTableLib = part[1];
									break;
								case LineAutoloadSettingsLib:
									ps.PathsToSettLib = part[1];
									break;
								default:
									continue;
							}
						}
					}
				}
			}
			return ps;
		}

		public void WriteConfig(PersistentStorage ps)
		{
			System.Text.StringBuilder sb = new();
			sb.Append(HeaderCommon);
			sb.Append("\t" + LineIsDefault + "=" + (ps.IsDefault ? "true" : "false") + "\r\n");
			sb.Append("\t" + LineShowTab + "=" + "0\r\n"); // 0 is just placeholder.
			sb.Append(HeaderVPE);
			sb.Append("\t" + LineAutoloadTableLib + "=" +  ps.PathsToTableLib + "\r\n");
			sb.Append("\t" + LineAutoloadSettingsLib + "=" + ps.PathsToSettLib + "\r\n");
			sb.Append(HeaderNDT);
			sb.Append("\t\r\n");
			sb.Append(HeaderDTC);
			sb.Append("\t\r\n");
			sb.Append(HeaderF);
			sb.Append("\t\r\n");
			sb.Append(HeaderPG);
			sb.Append("\t\r\n");
			sb.Append(HeaderMD);
			sb.Append('\t');
			File.WriteAllText(ConfigFile.FullName, sb.ToString());
		}
	}

	public class PersistentStorage
	{
		#region Common
		public bool IsDefault = true;
		public byte ShowTab = 0;
		#endregion
		#region VPE
		public string PathsToSettLib = "";
		public string PathsToTableLib = "";
		#endregion
	}
}