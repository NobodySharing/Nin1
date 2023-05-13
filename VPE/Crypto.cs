using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Common.PrimeList;

namespace VPE
{
	public class Crypto
	{
		private Settings Sett;
		/// <summary>Sada ��sel reprezentuj�c� zpr�vu. Prom�nn�, se kterou v cel�m procesu pracuji.</summary>
		private List<ushort> Message;
		public Crypto(Settings S)
		{
			Sett = S;
		}
		/// <summary>Za�ifruje text.</summary>
		/// <param name="Text">Text na za�ifrov�n�.</param>
		/// <returns>Za�ifrovan� text.</returns>
		public string Encypt(string Text)
		{
			ConvertToNums(Text); // 
			AddRandomChars(); // 
			SwitchCharPoz(); // 
			OrderShift(); // 
			Swap(); // 
			Scramble(); // 
			Swap(); // 
			ConstantShift(); // 
			VariableShift(); // 
			AddRandomChars(); // 
			SwitchCharPoz(); //
			return ConvertToString(); // 
		}
		/// <summary>De�ifruje text.</summary>
		/// <param name="Text">Za�ifrovan� text.</param>
		/// <returns>De�ifrovan� text.</returns>
		public string Decypt(string Text)
		{
			ConvertToNums(Text);
			UnSwitchCharPoz();
			RemoveRandomChars();
			UnVariableShift();
			UnConstantShift();
			Unswap();
			Unscramble();
			Unswap();
			UnOrderShift();
			UnSwitchCharPoz();
			RemoveRandomChars();
			return ConvertToString();
		}
		/// <summary>Zkonvertuje textovou zpr�vu na ��selnou reprezentaci podle tabulky znak�.</summary>
		/// <param name="Text">Textov� zpr�va.</param>
		private void ConvertToNums(string Text)
		{
			Message = new(Text.Length);
			for (int i = 0; i < Text.Length; i++)
			{
				char C = Text[i]; // Sou�asn� znak bokem.
				char N = i == (Text.Length - 1)? '\0' : Text[i + 1]; // N�sleduj�c� znak. Pokud jsem na konci, d�v�m \0, jako znamen� toho stavu.
				if (C == '\r')
				{
					if ((N == '\n') || (N == '\0'))
					{
						Message.Add((ushort)Codepage.CharSet.IndexOf("\r\n"));
						i++;
						continue;
					}
					else
					{
						Message.Add((ushort)Codepage.CharSet.IndexOf("\r\n"));
						continue;
					}
				}
				else if (C == '\n')
				{
					Message.Add((ushort)Codepage.CharSet.IndexOf("\r\n"));
					continue;
				}
				int index = Codepage.CharSet.IndexOf(Convert.ToString(C));
				if (index >= 0)
				{
					Message.Add((ushort)index);
				}
				else
				{ // Pokud znak nezn�m, p�eskakuji ho.
					continue;
				}
			}
		}
		/// <summary>P�evede sadu ��sel na text.</summary>
		/// <returns>Text.</returns>
		private string ConvertToString()
		{
			StringBuilder SB = new();
			foreach (ushort Num in Message)
			{
				SB.Append(Codepage.CharSet[Num]);
			}
			return SB.ToString();
		}
		/// <summary>Prohod� ��sla postupn� podle v�ech tabulek.</summary>
		private void Swap()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					if (T.FindValueUsingIndex(Message[i]) < ushort.MaxValue - 10) // Error k�dy.
					{
						ushort swapped = T.FindValueUsingIndex(Message[i]);
						Message[i] = swapped == (ushort.MaxValue - 1) ? Message[i] : swapped; // Pokud tam tahle hodnota nen�, neprohazuji.
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary>Zvr�t� prohozen� ��sel, zp�tn�.</summary>
		private void Unswap()
		{
			for (int j = Sett.Swaps.Count - 1; j >= 0; j--)
			{
				for (int i = Message.Count - 1; i >= 0; i--)
				{
					if (Sett.Swaps[j].FindIndexUsingValue(Message[i]) != ushort.MaxValue)
					{
						ushort swapped = Sett.Swaps[j].FindIndexUsingValue(Message[i]);
						Message[i] = swapped == (ushort.MaxValue - 1) ? Message[i] : swapped;
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary>Zam�ch� ��sla podle tabulek.</summary>
		private void Scramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i); // Dop�edn� pr�chod.
				Reflect(i); // Odraz s prohozen�m.
				BackwardScramble(i); // Zp�tn� pr�chod.
				int Sum = Message[i] + Sett.Rotors[0].Pozition; // Kv�li debugu v samostatn� prom�nn�.
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary>Odzam�ch� ��sla podle tabulek.</summary>
		private void Unscramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				BackwardScramble(i);
				ReflectBack(i);
				ForwardScramble(i);
				int Sum = Message[i] + Sett.Rotors[0].Pozition;
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary>Posune ��sla podle posunov�ho parametru.</summary>
		private void ConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				Message[i] += Sett.ConstShift;
				Message[i] %= Codepage.Limit;
			}
		}
		/// <summary>Posune ��sla zp�t podle posunov�ho parametru.</summary>
		private void UnConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				Message[i] -= Sett.ConstShift;
				Message[i] %= Codepage.Limit;
			}
		}
		/// <summary>Posune ��sla podle po�ad�.</summary>
		private void OrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] + i) % Codepage.Limit);
			}
		}
		/// <summary>Posune ��sla zp�t podle po�ad�.</summary>
		private void UnOrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] - i) % Codepage.Limit);
			}
		}
		/// <summary>Provede prom�nn� posun ��sel.</summary>
		private void VariableShift ()
		{
			uint v, c = Sett.ConstShift > (Codepage.Limit / 2) ? Sett.ConstShift : (uint)(Codepage.Limit - Sett.ConstShift);
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] + (uint)(Sett.VarShift * (i % c));
				Message[i] = (ushort)(v % Codepage.Limit);
			}
		}
		/// <summary>Provede prom�nn� posun zp�t ��sel.</summary>
		private void UnVariableShift()
		{
			uint v, c = Sett.ConstShift > (Codepage.Limit / 2) ? Sett.ConstShift : (uint)(Codepage.Limit - Sett.ConstShift);
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] - (uint)(Sett.VarShift * (i % c));
				Message[i] = (ushort)(v % Codepage.Limit);
			}
		}
		/// <summary>Dop�edn� pr�chod 1 znaku v�emi tabulkami.</summary>
		/// <param name="i">Index znaku.</param>
		private void ForwardScramble(int i)
		{
			for (int j = 0; j < Sett.Rotors.Count; j++)
			{
				int Sum = j == 0 ? Message[i] + Sett.Rotors[j].Pozition : Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j - 1].Pozition); // Kalkulace pozice po vstupu do tabulky.
				ushort Remain = (ushort)(Sum % Codepage.Limit); // Z�st�n� v rozsahu.
				Message[i] = Sett.Rotors[j].FindValueUsingIndex(Remain); // Pr�chod tabulkou.
			}
		}
		/// <summary>Odraz� znak dle tabulky.</summary>
		/// <param name="i">Index znaku.</param>
		/// <returns>Odra�en� znak.</returns>
		private void Reflect(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindValueUsingIndex(Remain);
		}
		/// <summary>Zp�tn� odraz� znak dle tabulky.</summary>
		/// <param name="i">Index znaku.</param>
		/// <returns>Odra�en� znak.</returns>
		private void ReflectBack(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindIndexUsingValue(Remain);
		}
		/// <summary>Zp�tn� pr�chod 1 znaku v�emi tabulkami.</summary>
		/// <param name="i">Index znaku.</param>
		private void BackwardScramble(int i)
		{
			for (int j = Sett.Rotors.Count - 1; j >= 0; j--)
			{
				int Sum = j == (Sett.Rotors.Count - 1) ? Message[i] + Sett.Rotors[j].Pozition : Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j + 1].Pozition);
				ushort Remain = (ushort)(Sum % Codepage.Limit);
				Message[i] = Sett.Rotors[j].FindIndexUsingValue(Remain);
			}
		}
		/// <summary>P�id� n�hodn� znaky do sady.</summary>
		private void AddRandomChars()
		{
			Generators Gen = new (Codepage.Limit, DateTime.Now.Ticks);
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2; // Inicializace indexu, kam budu p�id�vat n�hodn� znak.
			Message.Insert(index, Gen.GenerateNum()); // P�id�m n�hodn� znak, na 0. nebo 1. pozici podle sudosti/lichosti sou�tu minima a maxima rozsah� mezer.
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin; // Velikost rozsahu mezery.
			decimal space = (Math.Floor(Codepage.Limit / (decimal)gap) % gap) + Sett.RandCharSpcMin; // Inicializace mezery. V j�dru �n�hodn�� v�po�et, pak d�n� do rozsahu a posunut� o minimum.
			while (index <= Message.Count)
			{
				IncrementSpace(ref index, ref space, gap);
				if (index <= Message.Count - 1)
				{
					Message.Insert(index, Gen.GenerateNum());
				}
				else 
				{
					break;
				}
			}
		}
		/// <summary>Odebere n�hodn� znaky ze sady.</summary>
		private void RemoveRandomChars()
		{
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2;
			Message.RemoveAt(index);
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			decimal space = (Math.Floor(Codepage.Limit / (decimal)gap) % gap) + Sett.RandCharSpcMin;
			while (index <= Message.Count)
			{
				IncrementSpace(ref index, ref space, gap);
				Message.RemoveAt(index);
			}
		}
		/// <summary>Inkrementuje index o novou pseudon�hodnou mezeru.</summary>
		/// <param name="index">P�edchoz� index.</param>
		/// <param name="space">P�edchoz� mezera.</param>
		/// <param name="gap">Rozsah velikosti mezery.</param>
		private void IncrementSpace (ref int index, ref decimal space, int gap)
		{
			space = (Sett.RandCharConstA * space + Sett.RandCharConstB) % Sett.RandCharConstM;
			index += (int)((space % gap) + Sett.RandCharSpcMin);
		}
		/// <summary></summary>
		private void SwitchCharPoz()
		{
			List<int> switchTable = ConstructSwitchTable();
			List<ushort> original = new();
			original.AddRange(Message);
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[i] = original[switchTable[i]];
			}
		}
		/// <summary></summary>
		private void UnSwitchCharPoz()
		{
			List<int> switchTable = ConstructSwitchTable();
			List<ushort> original = new();
			original.AddRange(Message);
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[switchTable[i]] = original[i];
			}
		}
		/// <summary></summary>
		private List<int> ConstructSwitchTable()
		{
			List<int> switchTable = new();
			uint a = Primes[Sett.SwitchConstAIdx], b = Primes[Sett.SwitchConstBIdx], m = (uint)Message.Count;
			if (m % (a*b) == 0)
			{
				m--;
			}
			for (int i = 0; i < m; i++)
			{
				switchTable.Add(Convert.ToInt32((a * i + b) % m));
			}
			return switchTable;
		}
	}
}