using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Common;

namespace VPE
{
	/// <summary>Class for RNG, Codepage.Limit is used heavily here.</summary>
	public class Generators
	{
		/// <summary>Creates a new instance of the generator.</summary>
		public Generators()
		{
			RandomNumberGenerator.Create();
		}
		/// <summary>Generates complete instance of Key class.</summary>
		/// <param name="idxs">Set of indexies. 0: rotors, 1: swaps, 2: refls, 3: scramblers, 4: setts.</param>
		/// <param name="name"></param>
		/// <returns>Key.</returns>
		public static Key GenerateKey(uint[] idxs, string name = "")
		{
			PrimeDefinedConstant[] RotorShiftABM = GenerateABM();
			PrimeDefinedConstant[] RandCharABM = GenerateABM();
			PrimeDefinedConstant[] ABCD = GenerateABCD();
			ushort[] spaces = GenerateSpaceMinMax();
			Key settings = new()
			{
				Name = name == "" ? DateTime.Now.ToString("u") + " (automatically generated)" : name,
				Idx = idxs[4],
				Reflector = GeneratePairs(idxs[2]),
				InputScrambler = GenerateTableWithoutPoss(idxs[3]),
				OutputScrambler = GenerateTableWithoutPoss(idxs[3] + 1),
				RotorShiftConstN = GenerateUlong(),
				RotorShiftConstA = RotorShiftABM[0],
				RotorShiftConstB = RotorShiftABM[1],
				RotorShiftConstM = RotorShiftABM[2],
				RandCharConstA = RandCharABM[0],
				RandCharConstB = RandCharABM[1],
				RandCharConstM = RandCharABM[2],
				SwitchConstA = ABCD[0],
				SwitchConstB = ABCD[1],
				SwitchConstC = ABCD[2],
				SwitchConstD = ABCD[3],
				RandCharSpcMin = spaces[0],
				RandCharSpcMax = spaces[1],
			};
			int count = RandomNumberGenerator.GetInt32(48, 81);
			for (int i = 0; i < count; i++)
			{
				Table t = GenerateTable((uint)(idxs[0] + i));
				settings.Rotors.Add(t);
			}
			count = RandomNumberGenerator.GetInt32(8, 33);
			for (int i = 0; i < count; i++)
			{
				Table t = GeneratePairsWithSkips((uint)(idxs[1] + i), GenerateDoubleInRange(9d / 16d, 950d / 1024d));
				settings.Swaps.Add(t);
			}
			settings.ConstShift = GenerateNum();
			settings.VarShift = GenerateNum();
			return settings;
		}
		/// <summary>Generates full non-paired table with positions.</summary>
		/// <param name="index">Index of this table.</param>
		/// <returns>Table. Rotor.</returns>
		public static Table GenerateTable(uint index)
		{
			Table T = GenerateTableWithoutPoss(index);
			T.HasPosition = true;
			T.Positions.Add(GenerateNum());
			return T;
		}
		/// <summary>Generates full paired table, for reflectors.</summary>
		/// <param name="index">Index of this table.</param>
		/// <returns>Table. Reflector.</returns>
		public static Table GeneratePairs(uint index)
		{
			Table T = new()
			{
				Idx = index,
				IsPaired = true
			};
			ushort[] Temp = new ushort[Codepage.Limit];
			List<ushort> Remains = [];
			for (ushort u = 0; u < Codepage.Limit; u++)
			{
				Remains.Add(u);
			}
			int RandIndex;
			ushort SelA, SelB;
			while (Remains.Count > 0)
			{
				RandIndex = RandomNumberGenerator.GetInt32(Remains.Count);
				SelA = Remains[RandIndex];
				Remains.RemoveAt(RandIndex);
				RandIndex = RandomNumberGenerator.GetInt32(Remains.Count);
				SelB = Remains[RandIndex];
				Remains.RemoveAt(RandIndex);
				Temp[SelA] = SelB;
				Temp[SelB] = SelA;
			}
			T.MainTable = [.. Temp];
			return T;
		}
		/// <summary>Generates partially filled paired table. For swaps.</summary>
		/// <param name="index">Index of this table.</param>
		/// <param name="fillPortion">What ratio should be filled? Non-filled will get special numxber indicating their non-fill-ness.</param>
		/// <returns>Table. Swap.</returns>
		public static Table GeneratePairsWithSkips(uint index, double fillPortion = 0.65234375)
		{
			Table T = new()
			{
				Idx = index,
				IsPaired = true,
				IsIncomplete = true,
			};
			ushort[] temp = new ushort[Codepage.Limit];
			List<ushort> remains = [];
			for (ushort u = 0; u < Codepage.Limit; u++)
			{
				remains.Add(u);
			}
			int randIndex;
			ushort SelA, SelB;
			while (remains.Count > 0)
			{
				randIndex = RandomNumberGenerator.GetInt32(remains.Count);
				SelA = remains[randIndex];
				remains.RemoveAt(randIndex);
				randIndex = RandomNumberGenerator.GetInt32(remains.Count);
				SelB = remains[randIndex];
				remains.RemoveAt(randIndex);
				if (GenerateDoubleInRange(0, 1) <= fillPortion)
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
			T.MainTable = [.. temp];
			return T;
		}
		/// <summary>Generates full non-paired table without positions.</summary>
		/// <param name="index">Index of this table.</param>
		/// <returns>Table. Scrambler.</returns>
		public static Table GenerateTableWithoutPoss(uint index)
		{
			Table t = new()
			{
				Idx = index,
				HasPosition = false,
				IsPaired = false,
				IsIncomplete = false,
			};
			List<ushort> Remains = [];
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
					RandIndex = RandomNumberGenerator.GetInt32(Remains.Count);
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
		public static ushort GenerateNum()
		{
			return Convert.ToUInt16(RandomNumberGenerator.GetInt32(Codepage.Limit));
		}
		/// <summary>Generates a random ulong.</summary>
		/// <returns>Random ulong.</returns>
		public static ulong GenerateUlong()
		{
			return Helpers.ToUInt64(RandomNumberGenerator.GetBytes(8));
		}
		/// <summary>Generates decimal number in provided bounds.</summary>
		/// <param name="from">Lower bound, including.</param>
		/// <param name="to">Upper bound, excluding.</param>
		/// <returns>Decimal number.</returns>
		public static double GenerateDoubleInRange(double from, double to)
		{
			if (from > to)
			{
				(to, from) = (from, to);
			}
			double range = to - from;
			ulong rand = (ulong)RandomNumberGenerator.GetInt32(0, int.MaxValue);
			double randDecimal = rand / (double)((ulong)int.MaxValue + 1);
			return from + randDecimal * range;
		}
		/// <summary>Generates constants A, B a M for pseudorandom number generating, for space between random chars and how many positions to turn rotors after each char. Generating these constants follows special rules for best results.</summary>
		/// <returns>Array: A, B, M.</returns>
		public static PrimeDefinedConstant[] GenerateABM()
		{
			PrimeDefinedConstant[] result = new PrimeDefinedConstant[3];
			List<int>[] idxs = [[], [], []]; // A, B, M.
			List<byte>[] exps = [[], [], []]; // A, B, M.
			int num;
			for (byte i  = 0; i < 5; i++)
			{
				num = RandomNumberGenerator.GetInt32(0, PrimeList.Total);
				idxs[0].Add(num);
				idxs[1].Add(num);
				idxs[2].Add(RandomNumberGenerator.GetInt32(0, PrimeList.Total));
			}
			for (byte i = 0; i < 2; i++)
			{
				idxs[0].Add(RandomNumberGenerator.GetInt32(0, PrimeList.Total));
				idxs[1].Add(RandomNumberGenerator.GetInt32(0, PrimeList.Total));
			}
			for (byte i = 0; i < 7; i++)
			{
				exps[0].Add(Convert.ToByte(RandomNumberGenerator.GetInt32(1, GetSizeBasedMax(idxs[0][i]))));
				exps[1].Add(Convert.ToByte(RandomNumberGenerator.GetInt32(1, GetSizeBasedMax(idxs[1][i]))));
				if (i < 5)
				{
					exps[2].Add(Convert.ToByte(RandomNumberGenerator.GetInt32(1, GetSizeBasedMax(idxs[2][i]))));
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
		public static PrimeDefinedConstant[] GenerateABCD()
		{
			PrimeDefinedConstant[] result = new PrimeDefinedConstant[4];
			for(byte i = 0; i < result.Length; i++)
			{
				result[i] = new();
				for (byte j = 0; j < RandomNumberGenerator.GetInt32(3, 6); j++)
				{
					int num = RandomNumberGenerator.GetInt32(PrimeList.First3Digit, PrimeList.Total);
					byte exp = Convert.ToByte(RandomNumberGenerator.GetInt32(1, GetSizeBasedMax(num)));
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
		public static ushort[] GenerateSpaceMinMax(ushort MinFrom = 2, ushort MinTo = 8, ushort MaxFrom = 10, ushort MaxTo = 20)
		{
			return [(ushort)RandomNumberGenerator.GetInt32(MinFrom, MinTo), (ushort)RandomNumberGenerator.GetInt32(MaxFrom, MaxTo)];
		}
	}
}