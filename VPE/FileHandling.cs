using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Shapes;

namespace VPE
{
	public static class FileHandling
	{
		public const string FileExtSL = ".vpesl"; // Very primitive encryption settings library.
		public const string FileExtTL = ".vpetl"; // Very primitive encryption table library.
		public const string FileExtS = ".vpes"; // Very primitive encryption settings.
		public const string FileExtTXT = ".txt";

		/// <summary>Saves an instance of SettingsLibrary class.</summary>
		/// <param name="what">SettingsLibrary class.</param>
		/// <param name="where">Path to a file to be created.</param>
		public static void Save(SettingsLibrary what, string where)
		{
			if(where.EndsWith(FileExtSL))
			{
				File.WriteAllBytes(where, what.ToBytes());
			}
			else
			{
				//DirectoryInfo di = new(where);
				// ToDo: everyhting.
			}
		}
		/// <summary>Saves an instance of TableLibrary class.</summary>
		/// <param name="what">TableLibrary class.</param>
		/// <param name="where">Path to a file to be created.</param>
		public static void Save(TableLibrary what, string where)
		{
			if (where.EndsWith(FileExtTL))
			{
				File.WriteAllBytes(where, what.ToBytes());
			}
			else
			{
				//DirectoryInfo di = new(where);
				// ToDo: everyhting.
			}
		}
		/// <summary>Saves an instance of settings class. Creates a new file or deletes existing and writes a new one.</summary>
		/// <param name="what">Settings class.</param>
		/// <param name="where">Path to a file.</param>
		public static void SaveOrUpdate(Settings what, string where)
		{
			FileInfo fi = new(where);
			if (fi.Exists)
			{
				File.Delete(where);
				File.WriteAllBytes(where, what.ToBytes());
			}
			else
			{
				if (where.EndsWith(FileExtS))
				{
					File.WriteAllBytes(where, what.ToBytes());
				}
				else
				{

				}
			}
			{
				//DirectoryInfo di = new(where);
				// ToDo: everyhting.
			}
		}
		/// <summary>Loads SettingsLibrary from a location.</summary>
		/// <param name="filename">Path to SettingsLibrary.</param>
		/// <param name="sl">SettingsLibrary loaded from disk.</param>
		public static void Load(string filename, out SettingsLibrary sl)
		{
			sl = new(File.ReadAllBytes(filename));
		}
		/// <summary>Loads TableLibrary from a location.</summary>
		/// <param name="filename">Path to TableLibrary.</param>
		/// <param name="tl">TableLibrary loaded from disk.</param>
		public static void Load(string filename, out TableLibrary tl)
		{
			tl = new(File.ReadAllBytes(filename));
		}
		/// <summary>Loads Settings from a location.</summary>
		/// <param name="filename">Path to Settings.</param>
		/// <param name="s">Settings loaded from disk.</param>
		public static void Load(string filename, out Settings s)
		{
			s = new(File.ReadAllBytes(filename));
		}

		public static void SaveText(string path, string text)
		{
			File.WriteAllText(path, text, Encoding.UTF8);
		}

		public static string LoadText(string path)
		{
			return File.ReadAllText (path, Encoding.UTF8);
		}
	}
}