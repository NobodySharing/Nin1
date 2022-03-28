using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VPE
{
	public class Crypto
	{
		private Settings Sett;

		public Crypto(Settings S)
		{
			Sett = S;
		}
		/// <summary>Za�ifruje text.</summary>
		/// <param name="Text">Text na za�ifrov�n�.</param>
		/// <returns>Za�ifrovan� text.</returns>
		public string Encypt(string Text)
		{
			List<ushort> Working = ConvertToNums(Text); // P�evod textu na ��sla.
			AddRandomChars(ref Working); // P�id�v�m n�hodn� znaky.
			VariableShift(ref Working); // Provede jednoduch� posun cel� sady.
			Swap(ref Working); // Prohod� znaky podle (��ste�n� vypln�n�) tabulky.
			Scramble(ref Working); // Zam�ch� znaky postupn� dle tabulek, odraz� a pak zp�tn� dle tabulek.
			Swap(ref Working);
			VariableShift(ref Working);
			ConstantShift(ref Working); // Posune ka�d� znak o konstantu.
			return ConvertToString(Working); // P�evede ��sla na text.
		}
		/// <summary>De�ifruje text.</summary>
		/// <param name="Text">Za�ifrovan� text.</param>
		/// <returns>De�ifrovan� text.</returns>
		public string Decypt(string Text)
		{
			List<ushort> Working = ConvertToNums(Text);
			RemoveRandomChars(ref Working);
			UnVariableShift(ref Working);
			Unswap(ref Working);
			Unscramble(ref Working);
			Unswap(ref Working);
			UnVariableShift(ref Working);
			UnConstantShift(ref Working);
			return ConvertToString(Working);
		}
		/// <summary>Zkonvertuje textovou zpr�vu na ��selnou reprezentaci podle tabulky znak�.</summary>
		/// <param name="Text">Textov� zpr�va.</param>
		/// <returns>��seln� reprezentace.</returns>
		private List<ushort> ConvertToNums(string Text)
		{
			List<ushort> Result = new(Text.Length);
			foreach (char C in Text)
			{
				if (C == '\r')
				{ // Beru rovnou \r\n.
					Result.Add((ushort)Codepage.Letters.IndexOf("\r\n"));
					continue;
				}
				if (C == '\n')
				{ // Ignoruji, \n o samot� nem� v�znam.
					continue;
				}
				string Letter = Convert.ToString(C);
				int index = Codepage.Letters.IndexOf(Letter);
				if (index >= 0)
				{
					Result.Add((ushort)index);
				}
				else
				{ // Pokud znak nezn�m, p�eskakuji ho.
					continue;
				}
			}
			return Result;
		}
		/// <summary>P�evede sadu ��sel na text.</summary>
		/// <param name="Numbers">Sada ��sel.</param>
		/// <returns>Text.</returns>
		private string ConvertToString(List<ushort> Numbers)
		{
			StringBuilder SB = new();
			foreach (ushort Num in Numbers)
			{
				SB.Append(Codepage.Letters[Num]);
			}
			return SB.ToString();
		}
		/// <summary>Prohod� ��sla postupn� podle v�ech tabulek.</summary>
		/// <param name="Numbers">��sla na prohozen�.</param>
		private void Swap(ref List<ushort> Numbers)
		{
			for (int i = 0; i < Numbers.Count; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					if (T.FindValueUsingIndex(Numbers[i]) < ushort.MaxValue - 10) // Error k�dy.
					{
						ushort swapped = T.FindValueUsingIndex(Numbers[i]);
						Numbers[i] = swapped == (ushort.MaxValue - 1) ? Numbers[i] : swapped; // Pokud tam tahle hodnota nen�, neprohazuji.
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary>Zvr�t� prohozen� ��sel, zp�tn�.</summary>
		/// <param name="Numbers">��sla na odprohozen�.</param>
		private void Unswap(ref List<ushort> Numbers)
		{
			for (int j = Sett.Swaps.Count - 1; j >= 0; j--)
			{
				for (int i = Numbers.Count - 1; i >= 0; i--)
				{
					if (Sett.Swaps[j].FindIndexUsingValue(Numbers[i]) != ushort.MaxValue)
					{
						ushort swapped = Sett.Swaps[j].FindIndexUsingValue(Numbers[i]);
						Numbers[i] = swapped == (ushort.MaxValue - 1) ? Numbers[i] : swapped;
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary>Zam�ch� ��sla podle tabulek.</summary>
		/// <param name="Numbers">��sla na zam�ch�n�.</param>
		private void Scramble(ref List<ushort> Numbers)
		{
			ushort[] Positions = Sett.Positions;
			for (int i = 0; i < Numbers.Count; i++)
			{
				Numbers[i] = ForwardScramble(Numbers[i], Positions); // Dop�edn� pr�chod.
				Numbers[i] = Reflect(Numbers[i], Positions.Last()); // Odraz s prohozen�m.
				Numbers[i] = BackwardScramble(Numbers[i], Positions); // Zp�tn� pr�chod.
				int Sum = Numbers[i] + Positions[0];
				Numbers[i] = (ushort)(Sum % Codepage.Limit);
				Increment(ref Positions);
			}
		}
		/// <summary>Odzam�ch� ��sla podle tabulek.</summary>
		/// <param name="Numbers">��sla na odzam�ch�n�.</param>
		private void Unscramble(ref List<ushort> Numbers)
		{
			ushort[] Positions = Sett.Positions;
			for (int i = 0; i < Numbers.Count; i++)
			{
				Numbers[i] = BackwardScramble(Numbers[i], Positions);
				Numbers[i] = ReflectBack(Numbers[i], Positions.Last());
				Numbers[i] = ForwardScramble(Numbers[i], Positions);
				int Sum = Numbers[i] + Positions[0];
				Numbers[i] = (ushort)(Sum % Codepage.Limit);
				Increment(ref Positions);
			}
		}
		/// <summary>Posune ��sla podle posunov�ho parametru.</summary>
		/// <param name="Numbers">��sla na posunut�.</param>
		private void ConstantShift(ref List<ushort> Numbers)
		{
			for (int i = 0; Numbers.Count > 0; i++)
			{
				Numbers[i] += Sett.Shift;
				Numbers[i] %= Codepage.Limit;
			}
		}
		/// <summary>Posune ��sla zp�t podle posunov�ho parametru.</summary>
		/// <param name="Numbers">��sla na posunut� zp�t.</param>
		private void UnConstantShift(ref List<ushort> Numbers)
		{
			for (int i = 0; Numbers.Count > 0; i++)
			{
				Numbers[i] -= Sett.Shift;
				Numbers[i] %= Codepage.Limit;
			}
		}
		/// <summary>Posune ��sla podle po�ad�.</summary>
		/// <param name="Numbers">��sla na posunut�.</param>
		private void VariableShift(ref List<ushort> Numbers)
		{
			int temp;
			for (int i = 0; i < Numbers.Count; i++)
			{
				temp = Numbers[i] + i;
				Numbers[i] = (ushort)(temp % Codepage.Limit);
			}
		}
		/// <summary>Posune ��sla zp�t podle po�ad�.</summary>
		/// <param name="Numbers">��sla na posunut� zp�t.</param>
		private void UnVariableShift(ref List<ushort> Numbers)
		{
			int temp;
			for (ushort i = 0; i < Numbers.Count; i++)
			{
				temp = Numbers[i] - i;
				Numbers[i] = (ushort)(temp % Codepage.Limit);
			}
		}
		/// <summary>Dop�edn� pr�chod 1 znaku v�emi tabulkami.</summary>
		/// <param name="Number">Znak.</param>
		/// <param name="Positions">Pozice v tabulk�ch.</param>
		/// <returns>Znak po pr�chodu v�emi tabulkami.</returns>
		private ushort ForwardScramble(ushort Number, ushort[] Positions)
		{
			for (int i = 0; i < Sett.Tables.Count; i++)
			{
				int Sum = i == 0 ? Number + Positions[i] : Number + (Positions[i] - Positions[i - 1]); // Kalkulace pozice po vstupu do tabulky.
				ushort Remain = (ushort)(Sum % Codepage.Limit); // Z�st�n� v rozsahu.
				Number = Sett.Tables[i].FindValueUsingIndex(Remain); // Pr�chod tabulkou.
			}
			return Number;
		}
		/// <summary>Odraz� znak dle tabulky.</summary>
		/// <param name="Number">Znak.</param>
		/// <param name="LastPosition">Pozice v posledn� tabulce.</param>
		/// <returns>Odra�en� znak.</returns>
		private ushort Reflect(ushort Number, ushort LastPosition)
		{
			int Sum = Number + LastPosition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			ushort Counterpart = Sett.Reflector.FindValueUsingIndex(Remain);
			return Counterpart;
		}
		/// <summary>Zp�tn� odraz� znak dle tabulky.</summary>
		/// <param name="Number">Znak.</param>
		/// <param name="LastPosition">Pozice v posledn� tabulce.</param>
		/// <returns>Odra�en� znak.</returns>
		private ushort ReflectBack(ushort Number, ushort LastPosition)
		{
			int Sum = Number + LastPosition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			ushort Counterpart = Sett.Reflector.FindIndexUsingValue(Remain);
			return Counterpart;
		}
		/// <summary>Zp�tn� pr�chod 1 znaku v�emi tabulkami.</summary>
		/// <param name="Number">Znak.</param>
		/// <param name="Positions">Pozice v tabulk�ch.</param>
		/// <returns>Znak po pr�chodu v�emi tabulkami.</returns>
		private ushort BackwardScramble(ushort Number, ushort[] Positions)
		{
			for (int i = Sett.Tables.Count - 1; i >= 0; i--)
			{
				int Sum = i == (Sett.Tables.Count - 1) ? Number + Positions[i] : Number + (Positions[i] - Positions[i + 1]);
				ushort Remain = (ushort)(Sum % Codepage.Limit);
				Number = Sett.Tables[i].FindIndexUsingValue(Remain);
			}
			return Number;
		}
		/// <summary>Inkrementuje ��sla v poli. Nejni��� index zna�� nejvy��� ��d.</summary>
		/// <param name="Numbers">Pole ��sel.</param>
		private void Increment(ref ushort[] Numbers)
		{
			int Last = Numbers.Length - 1;
			Numbers[Last]++;
			for (int i = Last; i >= 0; i--)
			{
				if (Numbers[i] >= Codepage.Limit)
				{
					Numbers[i] = 0;
					if (i > 0)
					{
						Numbers[i - 1]++;
					}
				}
			}
		}
		/// <summary>Vypo��t� frekvenci vkl�d�n� n�hodn�ch znak�, jak velk� maj� b�t mezery mezi n�hodn�mi znaky.</summary>
		/// <returns>Velikost mezery.</returns>
		private ushort CalculateRandomFrequency ()
		{
			ushort Frequency = Sett.Shift;
			while (Frequency < (Codepage.Limit / 2) + 1)
			{
				Frequency = Convert.ToUInt16 (Math.Pow(Frequency, 2) % Codepage.Limit);
			}
			return Frequency;
		}
		/// <summary>P�id� n�hodn� znaky do sady.</summary>
		/// <param name="Numbers">Sada.</param>
		private void AddRandomChars(ref List<ushort> Numbers)
		{
			Generators Gen = new (Codepage.Limit, DateTime.Now.Ticks);
			ushort Space = CalculateRandomFrequency();
			for (int i = Sett.Shift; i <= Numbers.Count; i += Space)
			{
				Numbers.Insert(i, Gen.GenerateNum());
			}
		}
		/// <summary>Odebere n�hodn� znaky ze sady.</summary>
		/// <param name="Numbers">Sada.</param>
		private void RemoveRandomChars(ref List<ushort> Numbers)
		{
			ushort Space = CalculateRandomFrequency();
			for (int i = Sett.Shift; i <= Numbers.Count; i += Space)
			{
				Numbers.RemoveAt(i);
			}
		}
	}
}