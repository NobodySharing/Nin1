using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace VPE
{
	/// <summary>Class for RNG, Codepage.Limit is used heavily here.</summary>
	public class Generators
	{
		private Random R;
		/// <summary>Creates a new instance of the generator.</summary>
		/// <param name="Lim">Maximal value (excluding!), up to which numbers can be generated.</param>
		/// <param name="Seed">Seed for this instance of RNG.</param>
		public Generators(int? seed = null)
		{
			UpdateSeed(seed);
		}
		/// <summary>Generates complete instance of Settings class.</summary>
		/// <param name="idxs">Set of indexies. 0: rotors, 1: swaps, 2: refls, 3: scramblers, 4: setts.</param>
		/// <returns>Settings.</returns>
		public Settings GenerateSetts(uint[] idxs, string name = "")
		{
			PrimeDefinedConstant[] ABM = GenerateABM();
			PrimeDefinedConstant[] ABCD = GenerateABCD();
			ushort[] spaces = GenerateSpaceMinMax();
			Settings settings = new()
			{
				Name = name == "" ? DateTime.Now.ToString("u") + " (automatically generated)" : name,
				Idx = idxs[4],
				Reflector = GeneratePairs(idxs[2]),
				InputScrambler = GenerateTableWithoutPoz(idxs[3]),
				OutputScrambler = GenerateTableWithoutPoz(idxs[3] + 1),
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
			int count = R.Next(16, 48);
			for (int i = 0; i < count; i++)
			{
				Table t = GenerateTable((uint)(idxs[0] + i));
				settings.Rotors.Add(t);
			}
			count = R.Next(8, 16);
			for (int i = 0; i < count; i++)
			{
				Table t = GeneratePairsWithSkips((uint)(idxs[1] + i), GenerateDoubleInRange(9d / 16d, 950d / 1024d));
				settings.Swaps.Add(t);
			}
			settings.ConstShift = GenerateNum();
			settings.VarShift = GenerateNum();
			return settings;
		}
		/// <summary>Updates the seed of RNG.</summary>
		/// <param name="NewSeed">Seed, nod mandatory. If not given, takes the current time.</param>
		public void UpdateSeed(int? seed = null)
		{
			seed ??= (int)(DateTime.Now.Ticks / int.MaxValue);
			R = new Random(seed.Value);
		}
		/// <summary>Generates full non-paired table with pozitions.</summary>
		/// <param name="index">Index of this table.</param>
		/// <returns>Table. Rotor.</returns>
		public Table GenerateTable(uint index)
		{
			Table T = GenerateTableWithoutPoz(index);
			T.HasPozition = true;
			T.Pozitions.Add(GenerateNum());
			return T;
		}
		/// <summary>Generates full paired table, for reflectors.</summary>
		/// <param name="index">Index of this table.</param>
		/// <returns>Table. Reflector.</returns>
		public Table GeneratePairs(uint index)
		{
			Table T = new()
			{
				Idx = index,
				IsPaired = true
			};
			ushort[] Temp = new ushort[Codepage.Limit];
			List<ushort> Remains = new();
			for (ushort u = 0; u < Codepage.Limit; u++)
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
		/// <summary>Generates partially filled paired table. For swaps.</summary>
		/// <param name="index">Index of this table.</param>
		/// <param name="fillPortion">What ratio should be filled? Non-filled will get special numxber indicating their non-fill-ness.</param>
		/// <returns>Table. Swap.</returns>
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
			for (ushort u = 0; u < Codepage.Limit; u++)
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
		/// <summary>Generates full non-paired table without pozitions.</summary>
		/// <param name="index">Index of this table.</param>
		/// <returns>Table. Scrambler.</returns>
		public Table GenerateTableWithoutPoz(uint index)
		{
			Table t = new()
			{
				Idx = index,
				HasPozition = false,
				IsPaired = false,
				IsIncomplete = false,
			};
			List<ushort> Remains = new();
			for (ushort u = 0; u < Codepage.Limit; u++)
			{
				Remains.Add(u);
			}
			int RandIndex;
			ushort Selected;
			for (int i = 0; i < Codepage.Limit; i++)
			{
				if (i < Codepage.Limit - 2)
				{
					RandIndex = R.Next(Remains.Count);
				}
				else
				{
					RandIndex = 0;
				}
				Selected = Remains[RandIndex];
				t.MainTable.Add(Selected);
				Remains.Remove(Selected);
			}
			return t;
		}
		/// <summary>Generates random number from 0 to Codepage.Limit, excluding.</summary>
		/// <returns>Random number in bounds.</returns>
		public ushort GenerateNum()
		{
			return Convert.ToUInt16(R.Next(Codepage.Limit));
		}
		/// <summary>Generates decimal number in provided bounds.</summary>
		/// <param name="from">Lower bound, including.</param>
		/// <param name="to">Upper bound, excluding.</param>
		/// <returns>Decimal number.</returns>
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
		/// <summary>Generates constants A, B a M for calculation of the space between 2 rand chars. Generating these constants follows special rules.</summary>
		/// <returns>Array: A, B, M.</returns>
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
		/// <summary>Gets a number based on size of provided number's translation to prime digits. For exponents.</summary>
		/// <param name="num">Number, index on the prime list.</param>
		/// <returns>Max exponent.</returns>
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
		/// <summary>Generates the A, B, C, D constants.</summary>
		/// <returns>Array of PDC, A, B, C, D in order.</returns>
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
		/// <summary>Generates min and max length of space, both based on given bounds.</summary>
		/// <param name="MinFrom">Lower bound of minumum, including.</param>
		/// <param name="MinTo">Upper bound of minumum, excluding.</param>
		/// <param name="MaxFrom">Lower bound of maximum, including.</param>
		/// <param name="MaxTo">Upper bound of maximum, excluding.</param>
		/// <returns>Min and max length of space (between random chars), in array.</returns>
		public ushort[] GenerateSpaceMinMax(ushort MinFrom = 2, ushort MinTo = 8, ushort MaxFrom = 10, ushort MaxTo = 20)
		{
			return new ushort[2] { (ushort)R.Next(MinFrom, MinTo), (ushort)R.Next(MaxFrom, MaxTo) };
		}
	}
}