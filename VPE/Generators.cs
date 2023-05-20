using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace VPE
{
	public class Generators
	{
		private Random R;
		private readonly ushort Limit;
		/// <summary>Vytvoří novou instanci generátoru.</summary>
		/// <param name="Lim">Maximální hodnota (excluding!), po k</param>
		/// <param name="Seed">Seed pro vytvoření náhodného generátoru.</param>
		public Generators(ushort Lim, long Seed = 0)
		{
			Limit = Lim;
			UpdateSeed(Seed);
		}
		/// <summary>Generates complete instance of Settings class.</summary>
		/// <param name="idxs">Set of indexies. 0: rotors, 1: swaps, 2: refls, 3: setts.</param>
		/// <returns>Settings.</returns>
		public Settings GenerateSetts(uint[] idxs)
		{
			PrimeDefinedConstant[] ABM = GenerateABM();
			PrimeDefinedConstant[] ABCD = GenerateABCD();
			ushort[] spaces = GenerateSpaceMinMax();
			Settings settings = new()
			{
				Name = DateTime.Now.ToString("u") + " (automatically generated)",
				Idx = idxs[3],
				Reflector = GeneratePairs(idxs[2]),
				RandCharConstA = ABM[0],
				RandCharConstB = ABM[1],
				RandCharConstM = ABM[2],
				SwitchConstA = ABCD[0],
				SwitchConstB = ABCD[1],
				SwitchConstC = ABCD[2],
				SwitchConstD = ABCD[3],
				RandCharSpcMin = spaces[0],
				RandCharSpcMax = spaces[1],
			};
			int count = R.Next(12, 42);
			for (int i = 0; i < count; i++)
			{
				Table t = GenerateTable((uint)(idxs[0] + i));
				t.Pozition = GenerateNum();
				settings.Rotors.Add(t);
			}
			count = R.Next(6, 14);
			for (int i = 0; i < count; i++)
			{
				Table t = GeneratePairsWithSkips((uint)(idxs[1] + i), GenerateDoubleInRange(9d / 16d, 950d / 1024d));
				settings.Swaps.Add(t);
			}
			settings.ConstShift = GenerateNum();
			settings.VarShift = GenerateNum();
			return settings;
		}
		/// <summary>Aktualizuje seed generátoru čísel.</summary>
		/// <param name="NewSeed">Seed, nepovinný. Pokud nezadán, bere se podle současného času.</param>
		public void UpdateSeed(long NewSeed = 0)
		{
			DateTime Now;
			if (NewSeed == 0)
			{
				Now = DateTime.Now;
			}
			else
			{
				Now = DateTime.FromBinary(NewSeed);
			}
			ulong ms = (ulong)(Now.Millisecond + Now.Second * 1000 + Now.Minute * 60000) + (ulong)(Now.Hour * 3600000) + (ulong)((Now.DayOfYear - 1) * 86400000) + (ulong)(Now.Year * 365.25 * 86400000);
			int remainder = (int)(ms % int.MaxValue);
			R = new Random(remainder);
		}
		/// <summary>Vygeneruje tabulku. Pro rotory.</summary>
		/// <param name="index">Index tabulky, použit jako pseudonázev.</param>
		/// <returns>Výsledná tabulka.</returns>
		public Table GenerateTable(uint index)
		{
			Table T = new()
			{
				Idx = index,
				HasPozition = true,
			};
			List<ushort> Remains = new();
			for (ushort u = 0; u < Limit; u++)
			{
				Remains.Add(u);
			}
			int RandIndex;
			ushort Selected;
			for (int i = 0; i < Limit; i++)
			{
				if (i < Limit - 2)
				{
					RandIndex = R.Next(Remains.Count);
				}
				else
				{
					RandIndex = 0;
				}
				Selected = Remains[RandIndex];
				T.MainTable.Add(Selected);
				Remains.Remove(Selected);
			}
			return T;
		}
		/// <summary>Vygeneruje párovou tabulku, na reflektory.</summary>
		/// <param name="index">Index tabulky, použit jako pseudonázev.</param>
		/// <returns>Výsledná tabulka.</returns>
		public Table GeneratePairs(uint index)
		{
			Table T = new()
			{
				Idx = index,
				IsPaired = true
			};
			ushort[] Temp = new ushort[Codepage.Limit];
			List<ushort> Remains = new();
			for (ushort u = 0; u < Limit; u++)
			{
				Remains.Add(u);
			}
			int RandIndex;
			ushort SelA, SelB;
			while (Remains.Count > 0)
			{
				RandIndex = R.Next(Remains.Count);
				SelA = Remains[RandIndex];
				Remains.RemoveAt(RandIndex);
				RandIndex = R.Next(Remains.Count);
				SelB = Remains[RandIndex];
				Remains.RemoveAt(RandIndex);
				Temp[SelA] = SelB;
				Temp[SelB] = SelA;
			}
			T.MainTable = Temp.ToList();
			return T;
		}
		/// <summary>Vygeneruje párovou tabulku, kde nejsou všechny hodnoty. Na swapy.</summary>
		/// <param name="index">Index tabulky, použit jako pseudonázev.</param>
		/// <param name="fillPortion">Jaký podíl záměn má být vyplněn? Nevyplněné položky nebudou zaměňovány.</param>
		/// <returns>Výsledná tabulka.</returns>
		public Table GeneratePairsWithSkips(uint index, double fillPortion = 0.65234375)
		{
			Table T = new()
			{
				Idx = index,
				IsPaired = true,
				IsIncomplete = true,
			};
			ushort[] temp = new ushort[Codepage.Limit];
			List<ushort> remains = new();
			for (ushort u = 0; u < Limit; u++)
			{
				remains.Add(u);
			}
			int randIndex;
			ushort SelA, SelB;
			while (remains.Count > 0)
			{
				randIndex = R.Next(remains.Count);
				SelA = remains[randIndex];
				remains.RemoveAt(randIndex);
				randIndex = R.Next(remains.Count);
				SelB = remains[randIndex];
				remains.RemoveAt(randIndex);
				if (R.NextDouble() <= fillPortion)
				{
					temp[SelA] = SelB;
					temp[SelB] = SelA;
				}
				else
				{
					temp[SelA] = Table.Blank; // Marking empty spots.
					temp[SelB] = Table.Blank;
				}
			}
			T.MainTable = temp.ToList();
			return T;
		}
		/// <summary>Vygeneruje náhodné číslo od 0 do Codepage.Limit.</summary>
		/// <returns>Náhodné číslo v limitu.</returns>
		public ushort GenerateNum()
		{
			return Convert.ToUInt16(R.Next(Limit));
		}

		public double GenerateDoubleInRange(double from, double to)
		{
			if (from > to)
			{
				(to, from) = (from, to);
			}
			double range = to - from;
			double rand = R.NextDouble();
			return from + rand * range;
		}
		/// <summary>Vygeneruje konstanty A, B a M pro generátor délky mezer mezi náhodnými znaky.</summary>
		/// <returns>Pole: A, B a M.</returns>
		public PrimeDefinedConstant[] GenerateABM()
		{
			PrimeDefinedConstant[] result = new PrimeDefinedConstant[3];
			List<int>[] idxs = new List<int>[] { new List<int>(), new List<int>(), new List<int>() }; // A, B, M.
			List<byte>[] exps = new List<byte>[] { new List<byte>(), new List<byte>(), new List<byte>() }; // A, B, M.
			int num;
			for (byte i  = 0; i < 5; i++)
			{
				num = R.Next(0, PrimeList.Total);
				idxs[0].Add(num);
				idxs[1].Add(num);
				idxs[2].Add(R.Next(0, PrimeList.Total));
			}
			for (byte i = 0; i < 2; i++)
			{
				idxs[0].Add(R.Next(0, PrimeList.Total));
				idxs[1].Add(R.Next(0, PrimeList.Total));
			}
			for (byte i = 0; i < 7; i++)
			{
				exps[0].Add(Convert.ToByte(R.Next(1, GetSizeBasedMax(idxs[0][i]))));
				exps[1].Add(Convert.ToByte(R.Next(1, GetSizeBasedMax(idxs[1][i]))));
				if (i < 5)
				{
					exps[2].Add(Convert.ToByte(R.Next(1, GetSizeBasedMax(idxs[2][i]))));
				}
			}
			for (int i = 0; i < 3; i++)
			{
				result[i] = new();
				for (int j = 0; j < idxs[i].Count; j++)
				{
					result[i].PrimeIdxs[j] = idxs[i][j];
					result[i].Exponents[j] = exps[i][j];
				}
			}
			return result;
		}

		private static int GetSizeBasedMax(int num)
		{
			return num switch
			{
				<= PrimeList.Last1Digit => 9,
				<= PrimeList.Last2Digit => 8,
				<= PrimeList.Last3Digit => 7,
				<= PrimeList.Last4Digit => 6,
				<= PrimeList.Last5Digit => 5,
				<= PrimeList.Last6Digit => 4,
				_ => 0,
			};
		}

		public PrimeDefinedConstant[] GenerateABCD()
		{
			PrimeDefinedConstant[] result = new PrimeDefinedConstant[4];
			for(byte i = 0; i < result.Length; i++)
			{
				result[i] = new();
				for (byte j = 0; j < R.Next(3, 6); j++)
				{
					int num = R.Next(PrimeList.First3Digit, PrimeList.Total);
					byte exp = Convert.ToByte(R.Next(1, GetSizeBasedMax(num)));
					result[i].PrimeIdxs[j] = num;
					result[i].Exponents[j] = exp;
				}
			}
			return result;
		}

		public ushort[] GenerateSpaceMinMax(ushort MinFrom = 2, ushort MinTo = 8, ushort MaxFrom = 10, ushort MaxTo = 20)
		{
			return new ushort[2] { (ushort)R.Next(MinFrom, MinTo), (ushort)R.Next(MaxFrom, MaxTo) };
		}
	}
}