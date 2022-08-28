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
		/// <summary>Sada èísel reprezentující zprávu. Promìnná, se kterou v celém procesu pracuji.</summary>
		private List<ushort> Message;
		public Crypto(Settings S)
		{
			Sett = S;
		}
		/// <summary>Zašifruje text.</summary>
		/// <param name="Text">Text na zašifrování.</param>
		/// <returns>Zašifrovaný text.</returns>
		public string Encypt(string Text)
		{
			ConvertToNums(Text); // Pøevod textu na èísla.
			AddRandomChars(); // Pøidávám náhodné znaky, co se budou šifrovat.
			OrderShift(); // Provede jednoduchý posun celé sady podle poøadí znaku v sadì.
			Swap(); // Prohodí znaky podle (èásteènì vyplnìné) tabulky.
			Scramble(); // Zamíchá znaky postupnì dle tabulek, odrazí a pak zpìtnì dle tabulek.
			Swap();
			ConstantShift(); // Posune každý znak o konstantu.
			VariableShift(); // Posune každý znak o promìnné èíslo závislé na seedu a poøadí.
			AddRandomChars(); // Pøidávám náhodné znaky, znovu, nešifrované.
			return ConvertToString(); // Pøevede èísla na text.
		}
		/// <summary>Dešifruje text.</summary>
		/// <param name="Text">Zašifrovaný text.</param>
		/// <returns>Dešifrovaný text.</returns>
		public string Decypt(string Text)
		{
			ConvertToNums(Text);
			RemoveRandomChars();
			UnOrderShift();
			Unswap();
			Unscramble();
			Unswap();
			UnConstantShift();
			UnVariableShift();
			RemoveRandomChars();
			return ConvertToString();
		}
		/// <summary>Zkonvertuje textovou zprávu na èíselnou reprezentaci podle tabulky znakù.</summary>
		/// <param name="Text">Textová zpráva.</param>
		private void ConvertToNums(string Text)
		{
			Message = new(Text.Length);
			for (int i = 0; i < Text.Length; i++)
			{
				char C = Text[i]; // Souèasný znak bokem.
				char N = i == (Text.Length - 1)? '\0' : Text[i + 1]; // Následující znak. Pokud jsem na konci, dávám \0, jako znamení toho stavu.
				if (C == '\r')
				{
					if ((N == '\n') || (N == '\0'))
					{
						Message.Add((ushort)Codepage.Letters.IndexOf("\r\n"));
						i++;
						continue;
					}
					else
					{
						Message.Add((ushort)Codepage.Letters.IndexOf("\r\n"));
						continue;
					}
				}
				else if (C == '\n')
				{
					Message.Add((ushort)Codepage.Letters.IndexOf("\r\n"));
					continue;
				}
				int index = Codepage.Letters.IndexOf(Convert.ToString(C));
				if (index >= 0)
				{
					Message.Add((ushort)index);
				}
				else
				{ // Pokud znak neznám, pøeskakuji ho.
					continue;
				}
			}
		}
		/// <summary>Pøevede sadu èísel na text.</summary>
		/// <returns>Text.</returns>
		private string ConvertToString()
		{
			StringBuilder SB = new();
			foreach (ushort Num in Message)
			{
				SB.Append(Codepage.Letters[Num]);
			}
			return SB.ToString();
		}
		/// <summary>Prohodí èísla postupnì podle všech tabulek.</summary>
		private void Swap()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					if (T.FindValueUsingIndex(Message[i]) < ushort.MaxValue - 10) // Error kódy.
					{
						ushort swapped = T.FindValueUsingIndex(Message[i]);
						Message[i] = swapped == (ushort.MaxValue - 1) ? Message[i] : swapped; // Pokud tam tahle hodnota není, neprohazuji.
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary>Zvrátí prohození èísel, zpìtnì.</summary>
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
		/// <summary>Zamíchá èísla podle tabulek.</summary>
		private void Scramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i); // Dopøedný prùchod.
				Reflect(i); // Odraz s prohozením.
				BackwardScramble(i); // Zpìtný prùchod.
				int Sum = Message[i] + Sett.Rotors[0].Pozition; // Kvùli debugu v samostatné promìnné.
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary>Odzamíchá èísla podle tabulek.</summary>
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
		/// <summary>Posune èísla podle posunového parametru.</summary>
		private void ConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				Message[i] += Sett.ConstShift;
				Message[i] %= Codepage.Limit;
			}
		}
		/// <summary>Posune èísla zpìt podle posunového parametru.</summary>
		private void UnConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				Message[i] -= Sett.ConstShift;
				Message[i] %= Codepage.Limit;
			}
		}
		/// <summary>Posune èísla podle poøadí.</summary>
		private void OrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] + i) % Codepage.Limit);
			}
		}
		/// <summary>Posune èísla zpìt podle poøadí.</summary>
		private void UnOrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] - i) % Codepage.Limit);
			}
		}
		/// <summary>Provede promìnný posun èísel.</summary>
		private void VariableShift ()
		{
			uint v, c = Sett.ConstShift > (Codepage.Limit / 2) ? Sett.ConstShift : (uint)(Codepage.Limit - Sett.ConstShift);
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] + (uint)(Sett.VarShift * (i % c));
				Message[i] = (ushort)(v % Codepage.Limit);
			}
		}
		/// <summary>Provede promìnný posun zpìt èísel.</summary>
		private void UnVariableShift()
		{
			uint v, c = Sett.ConstShift > (Codepage.Limit / 2) ? Sett.ConstShift : (uint)(Codepage.Limit - Sett.ConstShift);
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] - (uint)(Sett.VarShift * (i % c));
				Message[i] = (ushort)(v % Codepage.Limit);
			}
		}
		/// <summary>Dopøedný prùchod 1 znaku všemi tabulkami.</summary>
		/// <param name="i">Index znaku.</param>
		private void ForwardScramble(int i)
		{
			for (int j = 0; j < Sett.Rotors.Count; j++)
			{
				int Sum = j == 0 ? Message[i] + Sett.Rotors[j].Pozition : Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j - 1].Pozition); // Kalkulace pozice po vstupu do tabulky.
				ushort Remain = (ushort)(Sum % Codepage.Limit); // Zùstání v rozsahu.
				Message[i] = Sett.Rotors[j].FindValueUsingIndex(Remain); // Prùchod tabulkou.
			}
		}
		/// <summary>Odrazí znak dle tabulky.</summary>
		/// <param name="i">Index znaku.</param>
		/// <returns>Odražený znak.</returns>
		private void Reflect(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindValueUsingIndex(Remain);
		}
		/// <summary>Zpìtnì odrazí znak dle tabulky.</summary>
		/// <param name="i">Index znaku.</param>
		/// <returns>Odražený znak.</returns>
		private void ReflectBack(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindIndexUsingValue(Remain);
		}
		/// <summary>Zpìtný prùchod 1 znaku všemi tabulkami.</summary>
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
		/// <summary>Pøidá náhodné znaky do sady.</summary>
		private void AddRandomChars()
		{
			Generators Gen = new (Codepage.Limit, DateTime.Now.Ticks);
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2; // Inicializace indexu, kam budu pøidávat náhodný znak.
			Message.Insert(index, Gen.GenerateNum()); // Pøidám náhodný znak, na 0. nebo 1. pozici podle sudosti/lichosti souètu minima a maxima rozsahù mezer.
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin; // Velikost rozsahu mezery.
			decimal space = (Math.Floor(Codepage.Limit / (decimal)gap) % gap) + Sett.RandCharSpcMin; // Inicializace mezery. V jádru „náhodný“ výpoèet, pak dání do rozsahu a posunutí o minimum.
			while (index <= Message.Count)
			{
				IncrementSpace(ref index, ref space, gap);
				Message.Insert(index, Gen.GenerateNum());
			}
		}
		/// <summary>Odebere náhodné znaky ze sady.</summary>
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
		/// <summary>Inkrementuje index o novou pseudonáhodnou mezeru.</summary>
		/// <param name="index">Pøedchozí index.</param>
		/// <param name="space">Pøedchozí mezera.</param>
		/// <param name="gap">Rozsah velikosti mezery.</param>
		private void IncrementSpace (ref int index, ref decimal space, int gap)
		{
			space = (Sett.RandCharConstA * space + Sett.RandCharConstB) % Sett.RandCharConstM;
			index += (int)((space % gap) + Sett.RandCharSpcMin);
		}
	}
}