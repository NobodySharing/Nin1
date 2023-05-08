using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace VPE
{
	public class Generators
	{
		private Random R;
		private ushort Limit;
		/// <summary>Vytvoří novou instanci generátoru.</summary>
		/// <param name="Lim">Maximální hodnota (excluding!), po k</param>
		/// <param name="Seed">Seed pro vytvoření náhodného generátoru.</param>
		public Generators(ushort Lim, long Seed = 0)
		{
			Limit = Lim;
			UpdateSeed(Seed);
		}
		/// <summary>Vygeneruje celou třídu nastavení.</summary>
		/// <returns>Nastavení.</returns>
		public Settings GenerateSetts()
		{
			decimal[] ABM = GenerateABM();
			Settings settings = new()
			{
				Reflector = GeneratePairs(0),
				RandCharConstA = ABM[0],
				RandCharConstB = ABM[1],
				RandCharConstM = ABM[2],
			};
			int count = R.Next(12, 42);
			for (int i = 0; i < count; i++)
			{
				Table t = GenerateTable((uint)i);
				t.Pozition = GenerateNum();
				settings.Rotors.Add(t);
			}
			count = R.Next(6, 14);
			for (int i = 0; i < count; i++)
			{
				Table t = GeneratePairsWithSkips((ushort)i);
				settings.Swaps.Add(t);
			}
			settings.RandCharSpcMin = (ushort)R.Next(4, 10);
			settings.RandCharSpcMax = (ushort)(settings.RandCharSpcMin + R.Next(10, 20));
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
		public Table GeneratePairs(ushort index)
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
		public Table GeneratePairsWithSkips(ushort index, double fillPortion = 0.65234375)
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
				if (R.NextDouble() <= fillPortion)
				{

					Temp[SelA] = SelB;
					Temp[SelB] = SelA;
				}
			}
			T.MainTable = Temp.ToList();
			return T;
		}
		/// <summary>Vygeneruje náhodné číslo od 0 do Codepage.Limit.</summary>
		/// <returns>Náhodné číslo v limitu.</returns>
		public ushort GenerateNum()
		{
			return Convert.ToUInt16(R.Next(Limit));
		}
		/// <summary>Vygeneruje konstanty A, B a M pro generátor náhodných znaků.</summary>
		/// <returns>Pole: A, B a M.</returns>
		public decimal[] GenerateABM()
		{
			decimal[] result = { 1, 1, 1 };
			List<uint> AMprimes = new()
			{
				PrimeList.Primes[R.Next(0, 30)],
				PrimeList.Primes[R.Next(30, 169)],
				PrimeList.Primes[R.Next(169, 2653)],
			};
			List<uint> Bprimes = new();
			uint num;
			byte count = 0;
			while (count < 5)
			{
				num = PrimeList.Primes[R.Next(0, 9592)];
				if (!AMprimes.Contains(num))
				{
					Bprimes.Add(num);
					count++;
				}
			}
			List<byte> Aexps = new(), Mexps = new();
			byte exp;
			for (byte i = 0; i < 3; i++)
			{
				exp = (byte)R.Next(1, 5);
				Mexps.Add(exp);
				if (exp > 2)
				{
					Aexps.Add(2);
				}
				else
				{
					Aexps.Add(1);
				}
			}
			for (byte i = 0; i < 3; i++)
			{
				result[0] *= (decimal)Math.Pow(AMprimes[i], Aexps[i]);
			}
			result[0]++;
			for (byte i = 0; i < 5; i++)
			{
				result[1] *= Bprimes[i];
			}
			for (byte i = 0; i < 3; i++)
			{
				result[2] *= (decimal)Math.Pow(AMprimes[i], Mexps[i]);
			}
			return result;
		}

		public ushort[] GenerateSpaceMinMax(ushort MinFrom = 4, ushort MinTo = 14, ushort MaxFrom = 16, ushort MaxTo = 30)
		{
			return new ushort[2] { (ushort)R.Next(MinFrom, MinTo), (ushort)R.Next(MaxFrom, MaxTo) };
		}
	}
}