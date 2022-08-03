using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPE;
using Factorizator;
using NeueDT;
using DTcalc;
using Microsoft.Win32;

namespace GUI
{
	public class VPE_VM
	{
		private Generators Generator = new(Codepage.Limit, DateTime.Now.Ticks);
		public SettingsStorage SS = new();
		public Settings S;
		public Crypto C;
		private const string Invalid_File = "N/A";
		private const string VPESS_filter = "VPE Settings Storage files (*.vpess)|*.vpess";
		private const string VPES_filter = "VPE Settings files (*.vpes)|*.vpes";
		private const string TXT_filter = "Text files (*.txt)|*.txt";
		public ushort RotorCNTR = 0, ReflCNTR = 0, SwapCNTR = 0;
		public List<uint> Rotors = new();
		public List<ushort> Refls = new();
		public List<ushort> Swaps = new();
		/// <summary>Vygeneruje rotory.</summary>
		/// <param name="count">Kolik rotorů?</param>
		/// <returns>Seznam indexů vygenerovaných.</returns>
		public void GenerateRotors(uint count = 10)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				SS.Rotors.Add(Generator.GenerateTable(RotorCNTR));
				Rotors.Add(RotorCNTR);
				RotorCNTR++;
			}
		}

		public void GenerateReflector(uint count = 2)
		{
			for (uint i = 0; i < count; i++)
			{
				Generator.UpdateSeed(DateTime.Now.Ticks);
				SS.Reflectors.Add(Generator.GeneratePairs(ReflCNTR));
				ReflCNTR++;
			}
		}

		public void GenerateSwaps(uint count = 5)
		{
			for (uint i = 0; i < count; i++)
			{
				Generator.UpdateSeed(DateTime.Now.Ticks);
				SS.Swaps.Add(Generator.GeneratePairsWithSkips(SwapCNTR));
				SwapCNTR++;
			}
		}

		public void SelectSettings(List<ushort> tables, List<ushort> swaps, ushort reflector)
		{
			S = SS.Select(tables, swaps, reflector);
		}

		public void LoadAll()
		{
			SS = FileHandling.ReadAll(OpenFile(VPESS_filter));
		}

		public void LoadAndMerge()
		{
			SS.Merge(FileHandling.ReadAll(OpenFile(VPESS_filter)));
		}

		public void LoadSpecific()
		{
			S = FileHandling.ReadSpecific(OpenFile(VPES_filter));
		}

		public void SaveAll()
		{
			string folder = GetFolder(SaveFile(VPESS_filter));
			if (folder != "N/A")
			{
				FileHandling.Save(SS, folder);
			}
		}

		public void SaveSpecific()
		{
			string folder = GetFolder(SaveFile(VPES_filter));
			if (folder != "N/A")
			{
				FileHandling.Save(S, folder);
			}
		}

		public ushort GenerateRandNum()
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			return Generator.GenerateNum();
		}

		public decimal[] GenerateRNDConsts()
		{
			return Generator.GenerateABM();
		}

		public string Encrypt(string inText)
		{
			C = new(S);
			return C?.Encypt(inText);
		}

		public string Decrypt(string inText)
		{
			C = new(S);
			return C?.Decypt(inText);
		}

		public string OpenMsgFile()
		{
			return FileHandling.ReadText(OpenFile(TXT_filter));
		}

		public void SaveMsgFile(string text)
		{
			FileHandling.SaveText(SaveFile(TXT_filter), text);
		}

		public void QuickSettGen()
		{

		}

		public void QuickSettSave()
		{
			FileHandling.Save(S, SaveFile(VPES_filter));
		}

		public void QuickSettOpen()
		{
			S = FileHandling.ReadSpecific(OpenFile(VPES_filter));
		}
		#region Obslužné metody
		/// <summary>Zobrazí dialog otevření souboru a vrátí cestu k němu.</summary>
		/// <param name="ext">Extensiona.</param>
		/// <returns>Cesta k souboru.</returns>
		private string OpenFile(string ext)
		{
			OpenFileDialog OFD = new()
			{
				Filter = ext
			};
			bool? dia = OFD.ShowDialog();
			if (dia is not null)
			{
				if (dia.Value)
				{
					return OFD.FileName;
				}
			}
			return Invalid_File;
		}

		private string SaveFile(string ext)
		{
			SaveFileDialog SFD = new()
			{
				Filter = ext
			};
			bool? dia = SFD.ShowDialog();
			if (dia is not null)
			{
				if (dia.Value)
				{
					return SFD.FileName;
				}
			}
			return Invalid_File;
		}

		private string GetFolder(string path)
		{
			if (path != Invalid_File)
			{
				int idx = path.LastIndexOf('\\');
				return path[..(idx + 1)];
			}
			return path;
		}
		#endregion
	}

	public class NDT_VM
	{

	}

	public class DTC_VM
	{

	}

	public class Fact_VM
	{

	}
}