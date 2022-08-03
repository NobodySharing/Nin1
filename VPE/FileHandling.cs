using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VPE
{
	public static class FileHandling
	{
		public const string FileExtSS = ".vpess"; // Very primitive encryption settings storage.
		public const string FileExtS = ".vpes"; // Very primitive encryption settings.
		public const string FileExtTXT = ".txt";
		/// <summary>Uloží třídu úložiště nastavení.</summary>
		/// <param name="what">Třída úložiště nastavení.</param>
		/// <param name="where">Kam – složka!</param>
		public static void Save(SettingsStorage what, string where)
		{
			IEnumerable<string> myFiles = Directory.EnumerateFiles(where, "*" + FileExtSS);
			string finalFileName;
			if (myFiles.Count() == 0)
			{
				finalFileName = where.EndsWith('\\') ? where + "00000" + FileExtSS : where + '\\' + "00000" + FileExtSS;
				WriteAll(what, finalFileName);
				return;
			}
			else
			{
				string fileName;
				if (myFiles.Count() == 1)
				{
					fileName = Path.GetFileName(myFiles.First());
					if (uint.TryParse(fileName.AsSpan(0, fileName.Length - 6), out uint name))
					{
						if (name == 0)
						{
							finalFileName = where.EndsWith('\\') ? where + "00001" + FileExtSS : where + '\\' + "00001" + FileExtSS;
							WriteAll(what, finalFileName);
							return;
						}
						else
						{
							finalFileName = where.EndsWith('\\') ? where + "00000" + FileExtSS : where + '\\' + "00000" + FileExtSS;
							WriteAll(what, finalFileName);
							return;
						}
					}
				}
				else
				{
					List<uint> fileNums = new();
					foreach (string file in myFiles)
					{
						fileName = Path.GetFileName(file);
						string onlyFileName = fileName.AsSpan(0, fileName.Length - 6).ToString();
						if (uint.TryParse(onlyFileName, out uint name))
						{
							fileNums.Add(name);
						}
					}
					fileNums = fileNums.OrderBy(x => x).ToList();
					for (uint i = 0; i <= fileNums.Max() + 1; i++)
					{
						if (fileNums.Contains(i))
						{
							continue;
						}
						else
						{
							finalFileName = where.EndsWith('\\') ? where + i.ToString("00000") + FileExtSS : where + '\\' + i.ToString("00000") + FileExtSS;
							WriteAll(what, finalFileName);
							return;
						}
					}
					finalFileName = where.EndsWith('\\') ? where + fileNums.Max().ToString("00000") + FileExtSS : where + '\\' + fileNums.Max().ToString("00000") + FileExtSS;
					WriteAll(what, finalFileName);
					return;
				}
			}
		}
		/// <summary>Uloží třídu nastavení.</summary>
		/// <param name="what">Třída nastavení.</param>
		/// <param name="where">Kam – složka!</param>
		public static void Save(Settings what, string where)
		{
			IEnumerable<string> myFiles = Directory.EnumerateFiles(where, "*" + FileExtS);
			string finalFileName;
			if (myFiles.Count() == 0)
			{
				finalFileName = where.EndsWith('\\') ? where + "00000" + FileExtS : where + '\\' + "00000" + FileExtS;
				WriteAll(what, finalFileName);
				return;
			}
			else
			{
				string fileName;
				if (myFiles.Count() == 1)
				{
					fileName = Path.GetFileName(myFiles.First());
					if (uint.TryParse(fileName.AsSpan(0, fileName.Length - 6), out uint name))
					{
						if (name == 0)
						{
							finalFileName = where.EndsWith('\\') ? where + "00001" + FileExtS : where + '\\' + "00001" + FileExtS;
							WriteAll(what, finalFileName);
							return;
						}
						else
						{
							finalFileName = where.EndsWith('\\') ? where + "00000" + FileExtS : where + '\\' + "00000" + FileExtS;
							WriteAll(what, finalFileName);
							return;
						}
					}
				}
				else
				{
					List<uint> fileNums = new();
					foreach (string file in myFiles)
					{
						fileName = Path.GetFileName(file);
						string onlyFileName = fileName.AsSpan(0, fileName.Length - 6).ToString();
						if (uint.TryParse(onlyFileName, out uint name))
						{
							fileNums.Add(name);
						}
					}
					fileNums = fileNums.OrderBy(x => x).ToList();
					for (uint i = 0; i <= fileNums.Max() + 1; i++)
					{
						if (fileNums.Contains(i))
						{
							continue;
						}
						else
						{
							finalFileName = where.EndsWith('\\') ? where + i.ToString("00000") + FileExtS : where + '\\' + i.ToString("00000") + FileExtS;
							WriteAll(what, finalFileName);
							return;
						}
					}
					finalFileName = where.EndsWith('\\') ? where + fileNums.Max().ToString("00000") + FileExtS : where + '\\' + fileNums.Max().ToString("00000") + FileExtS;
					WriteAll(what, finalFileName);
					return;
				}
			}
		}

		public static SettingsStorage ReadAll(string path)
		{
			return new SettingsStorage(File.ReadAllBytes(path));
		}

		public static Settings ReadSpecific(string path)
		{
			return new Settings(File.ReadAllBytes(path));
		}

		public static string ReadText(string path)
		{
			return File.ReadAllText (path, Encoding.UTF8);
		}

		public static void SaveText(string path, string text)
		{
			File.WriteAllText (path, text);
		}

		private static void WriteAll(SettingsStorage storage, string path)
		{
			File.WriteAllBytes(path, storage.ToBytes());
		}

		private static void WriteAll(Settings storage, string path)
		{
			File.WriteAllBytes(path, storage.ToBytes());
		}
	}
}