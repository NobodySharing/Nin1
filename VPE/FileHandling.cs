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
		public const string FileExtKL = ".vpekl"; // Very primitive encryption keyes library.
		public const string FileExtTL = ".vpetl"; // Very primitive encryption table library.
		public const string FileExtK = ".vpek"; // Very primitive encryption key.
		public const string FileExtM = ".vpem"; // Very primitive/primitively encryption/encrypted message.
		public const string FileExtTXT = ".txt";

		/// <summary>Saves an instance of KeyesLibrary class.</summary>
		/// <param name="what">KeyesLibrary class.</param>
		/// <param name="where">Path to a file to be created.</param>
		public static void Save(KeyesLibrary what, string where)
		{
			File.WriteAllBytes(where, what.ToBytes());
		}
		/// <summary>Saves an instance of TableLibrary class.</summary>
		/// <param name="what">TableLibrary class.</param>
		/// <param name="where">Path to a file to be created.</param>
		public static void Save(TableLibrary what, string where)
		{
			File.WriteAllBytes(where, what.ToBytes());
		}
		/// <summary>Saves an instance of key class.</summary>
		/// <param name="what">Key class.</param>
		/// <param name="where">Path to a file.</param>
		public static void Save(Key what, string where)
		{
			File.WriteAllBytes(where, what.ToBytes());
		}
		/// <summary>Loads KeyesLibrary from a location.</summary>
		/// <param name="filename">Path to KeyesLibrary.</param>
		/// <param name="kl">KeyesLibrary loaded from disk.</param>
		public static void Load(string filename, out KeyesLibrary kl)
		{
			kl = new(File.ReadAllBytes(filename));
		}
		/// <summary>Loads TableLibrary from a location.</summary>
		/// <param name="filename">Path to TableLibrary.</param>
		/// <param name="tl">TableLibrary loaded from disk.</param>
		public static void Load(string filename, out TableLibrary tl)
		{
			tl = new(File.ReadAllBytes(filename));
		}
		/// <summary>Loads Key from a location.</summary>
		/// <param name="filename">Path to Key.</param>
		/// <param name="k">Key loaded from disk.</param>
		public static void Load(string filename, out Key k)
		{
			k = new(File.ReadAllBytes(filename));
		}
		/// <summary>Saves text to specified path, as UTF-8.</summary>
		/// <param name="path">Where to save the text.</param>
		/// <param name="text">Text to save.</param>
		public static void SaveText(string path, string text)
		{
			File.WriteAllText(path, text, Encoding.UTF8);
		}
		/// <summary>Loads text from a pecified path, assumes UTF-8.</summary>
		/// <param name="path">Where to load the text from.</param>
		/// <returns>Text.</returns>
		public static string LoadText(string path)
		{
			return File.ReadAllText (path, Encoding.UTF8);
		}
		/// <summary>Saves the message in custom binary format.</summary>
		/// <param name="path">Where to save.</param>
		/// <param name="message">Message.</param>
		public static void SaveMessageInBinary(string path, byte[] message)
		{
			File.WriteAllBytes(path, message);
		}
		/// <summary>Loads binary message in custom format and converts it into a list of ushorts.</summary>
		/// <param name="path">Where to load from.</param>
		/// <returns>Message.</returns>
		public static byte[] LoadMessageFromBinary(string path)
		{
			return File.ReadAllBytes(path);
		}
	}
}