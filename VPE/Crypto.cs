using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.DirectoryServices.ActiveDirectory;

namespace VPE
{
	public class Crypto
	{
		private readonly Settings Sett;
		private const int MultithreadingThreshold = 16384;
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
			SwitchCharPoz(0); // 
			OrderShift(); // 
			MultithreadedPartEnc(); //
			ConstantShift(); // 
			VariableShift(); // 
			AddRandomChars(); // 
			SwitchCharPoz(1); //
			return ConvertToString(); // 
		}
		/// <summary>De�ifruje text.</summary>
		/// <param name="Text">Za�ifrovan� text.</param>
		/// <returns>De�ifrovan� text.</returns>
		public string Decypt(string Text)
		{
			ConvertToNums(Text);
			UnSwitchCharPoz(1);
			RemoveRandomChars();
			UnVariableShift();
			UnConstantShift();
			MultithreadedPartDec();
			UnOrderShift();
			UnSwitchCharPoz(0);
			RemoveRandomChars();
			return ConvertToString();
		}
		/// <summary></summary>
		private void MultithreadedPartEnc()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Swap();
				Scramble();
				Unswap();
			}
			else
			{
				ushort[][] parts = PrepareData(out Settings_Rotors[] rotors);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					ForwardPassThroughTables(ref parts[i], rotors[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
				}
				_ = Sett.SetPozitions(rotors.Last().GetPozitions());
			}
		}
		/// <summary></summary>
		private void MultithreadedPartDec()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Unswap();
				Unscramble();
				Swap();
			}
			else
			{
				ushort[][] parts = PrepareData(out Settings_Rotors[] rotors);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					BackwardPassThroughTables(ref parts[i], rotors[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
				}
				_ = Sett.SetPozitions(rotors.Last().GetPozitions());
			}
		}
		/// <summary>Divides the message to segments. Number of those is dependant on logical processor count.
		/// Last segment should be bit longer, of length of the message is not dividible by processor count.</summary>
		/// <returns>Divided message.</returns>
		private ushort[][] DivideMessage()
		{
			ushort[][] parts = new ushort[Environment.ProcessorCount][];
			int segLen = Message.Count / Environment.ProcessorCount;
			for (int j = 0; j < (Environment.ProcessorCount - 1); j++)
			{
				parts[j] = new ushort[segLen];
				Message.CopyTo(j * segLen, parts[j], 0, segLen);
			}
			int lastSize = Message.Count - ((Environment.ProcessorCount - 1) * segLen);
			ushort[] last = new ushort[lastSize];
			Message.CopyTo((Environment.ProcessorCount - 1) * segLen, last, 0, 30);
			parts[^1] = last;
			return parts;
		}
		/// <summary></summary>
		/// <param name="rotors"></param>
		/// <returns></returns>
		private ushort[][] PrepareData(out Settings_Rotors[] rotors)
		{
			ushort[][] parts = DivideMessage();
			rotors = Sett.CloneRotors(Environment.ProcessorCount);
			for (int i = 1; i < Environment.ProcessorCount; i++)
			{
				rotors[i].IncrementPozitions((uint)(parts[0].Length * i));
			}
			return parts;
		}
		/// <summary></summary>
		/// <param name="parts"></param>
		private void CombineMessage(ushort[][] parts)
		{
			Message.Clear();
			foreach (ushort[] part in parts)
			{
				Message.AddRange(part);
			}
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
		/// <summary></summary>
		/// <param name="message"></param>
		/// <param name="rotors"></param>
		private void ForwardPassThroughTables(ref ushort[] message, Settings_Rotors rotors)
		{
			Swap(ref message);
			Scramble(ref message, rotors);
			Unswap(ref message);
		}
		/// <summary></summary>
		/// <param name="message"></param>
		/// <param name="rotors"></param>
		private void BackwardPassThroughTables(ref ushort[] message, Settings_Rotors rotors)
		{
			Unswap(ref message);
			Unscramble(ref message, rotors);
			Swap(ref message);
		}
		/// <summary></summary>
		private void Swap()
		{
			ushort swapped;
			for (int i = 0; i < Message.Count; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					swapped = T.FindValueUsingIndex(Message[i]);
					if (swapped == Table.Outside || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[i] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		/// <param name="message"></param>
		private void Swap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = 0; i < message.Length; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					swapped = T.FindValueUsingIndex(message[i]);
					if (swapped == Table.Outside || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[i] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		private void Unswap()
		{
			ushort swapped;
			for (int j = Sett.Swaps.Count - 1; j >= 0; j--)
			{
				for (int i = Message.Count - 1; i >= 0; i--)
				{
					swapped = Sett.Swaps[j].FindIndexUsingValue(Message[i]);
					if (swapped == Table.NotFound || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[i] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		/// <param name="message"></param>
		private void Unswap(ref ushort[] message)
		{
			ushort swapped;
			for (int j = Sett.Swaps.Count - 1; j >= 0; j--)
			{
				for (int i = message.Length - 1; i >= 0; i--)
				{
					swapped = Sett.Swaps[j].FindIndexUsingValue(Message[i]);
					if (swapped == Table.NotFound || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[i] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		private void Scramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i);
				Reflect(i);
				BackwardScramble(i);
				int Sum = Message[i] + Sett.Rotors[0].Pozition;
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		/// <param name="message"></param>
		/// <param name="sett"></param>
		private void Scramble(ref ushort[] message, Settings_Rotors sett)
		{
			for (int i = 0; i < message.Length; i++)
			{
				ForwardScramble(i, ref message, sett);
				Reflect(i, ref message);
				BackwardScramble(i, ref message, sett);
				int Sum = message[i] + sett.Rotors[0].Pozition;
				message[i] = (ushort)(Sum % Codepage.Limit);
				sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		private void Unscramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				BackwardScramble(i);
				ReflectBack(i);
				ForwardScramble(i);
				int Sum = Message[i] + Sett.Rotors[^1].Pozition;
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		/// <param name="message"></param>
		/// <param name="sett"></param>
		private void Unscramble(ref ushort[] message, Settings_Rotors sett)
		{
			for (int i = 0; i < message.Length; i++)
			{
				BackwardScramble(i, ref message, sett);
				ReflectBack(i, ref message);
				ForwardScramble(i, ref message, sett);
				int Sum = message[i] + sett.Rotors[^1].Pozition;
				message[i] = (ushort)(Sum % Codepage.Limit);
				sett.IncrementPozitions();
			}
		}
		/// <summary>Posune ��sla podle posunov�ho parametru.</summary>
		private void ConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				uint temp = Message[i];
				temp += Sett.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
			}
		}
		/// <summary>Posune ��sla zp�t podle posunov�ho parametru.</summary>
		private void UnConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				uint temp = Message[i];
				temp -= Sett.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
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
		/// <summary></summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private void ForwardScramble(int i)
		{
			int sum = Message[i] + Sett.Rotors[0].Pozition;
			ushort remain = (ushort)(sum % Codepage.Limit);
			Message[i] = Sett.Rotors[0].FindValueUsingIndex(remain);
			for (int j = 1; j < Sett.Rotors.Count; j++)
			{
				sum = Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j - 1].Pozition);
				remain = (ushort)(sum % Codepage.Limit);
				Message[i] = Sett.Rotors[j].FindValueUsingIndex(remain);
			}
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <param name="message"></param>
		/// <param name="sett"></param>
		/// <returns></returns>
		private void ForwardScramble(int i, ref ushort[] message, Settings_Rotors sett)
		{
			int sum = message[i] + sett.Rotors[0].Pozition;
			ushort remain = (ushort)(sum % Codepage.Limit);
			message[i] = sett.Rotors[0].FindValueUsingIndex(remain);
			for (int j = 1; j < sett.Rotors.Count; j++)
			{
				sum = message[i] + (sett.Rotors[j].Pozition - sett.Rotors[j - 1].Pozition);
				remain = (ushort)(sum % Codepage.Limit);
				message[i] = sett.Rotors[j].FindValueUsingIndex(remain);
			}
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private void BackwardScramble(int i)
		{
			int sum = Message[i] + Sett.Rotors[^1].Pozition;
			ushort remain = (ushort)(sum % Codepage.Limit);
			Message[i] = Sett.Rotors[^1].FindIndexUsingValue(remain);
			for (int j = Sett.Rotors.Count - 2; j >= 0; j--)
			{
				sum = Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j + 1].Pozition);
				remain = (ushort)(sum % Codepage.Limit);
				Message[i] = Sett.Rotors[j].FindIndexUsingValue(remain);
			}
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <param name="message"></param>
		/// <param name="sett"></param>
		/// <returns></returns>
		private void BackwardScramble(int i, ref ushort[] message, Settings_Rotors sett)
		{
			int sum = message[i] + sett.Rotors[^1].Pozition;
			ushort remain = (ushort)(sum % Codepage.Limit);
			message[i] = sett.Rotors[^1].FindIndexUsingValue(remain);
			for (int j = sett.Rotors.Count - 2; j >= 0; j--)
			{
				sum = message[i] + (sett.Rotors[j].Pozition - sett.Rotors[j + 1].Pozition);
				remain = (ushort)(sum % Codepage.Limit);
				message[i] = sett.Rotors[j].FindIndexUsingValue(remain);
			}
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private void Reflect(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindValueUsingIndex(Remain);
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private void Reflect(int i, ref ushort[] message)
		{
			int Sum = message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			message[i] = Sett.Reflector.FindValueUsingIndex(Remain);
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private void ReflectBack(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindIndexUsingValue(Remain);
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private void ReflectBack(int i, ref ushort[] message)
		{
			int Sum = message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			message[i] = Sett.Reflector.FindIndexUsingValue(Remain);
		}
		/// <summary>Adds random chars to the message.</summary>
		private void AddRandomChars()
		{
			Generators Gen = new (Codepage.Limit, DateTime.Now.Ticks);
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2; // Index inicialization, where I will add random char.
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin); // Inicializace mezery. V j�dru �n�hodn�� v�po�et, pak d�n� do rozsahu a posunut� o minimum.
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant(); // I have to precompute the constants here, so I don't compute them every time.
			while (index < Message.Count)
			{
				Message.Insert(index, Gen.GenerateNum());
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
		}
		/// <summary>Removes random chars from the message.</summary>
		private void RemoveRandomChars()
		{
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2;
			Message.RemoveAt(index);
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin);
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant();
			List<int> IdxToRemove = new();
			while (index <= Message.Count)
			{
				IncrementSpace(ref index, ref space, gap, A, B, M);
				IdxToRemove.Add(index);
			}
			for (int i = IdxToRemove.Count - 1; i >= 0; i--)
			{
				Message.RemoveAt(IdxToRemove[i]);
			}
		}
		/// <summary>Increments index by new pseudorandom gap.</summary>
		/// <param name="index">Previous index.</param>
		/// <param name="space">Previous gap.</param>
		/// <param name="gap">Max gap size.</param>
		/// <param name="A">A in y = (Ax + B) % M.</param>
		/// <param name="B">B in y = (Ax + B) % M.</param>
		/// <param name="M">M in y = (Ax + B) % M.</param>
		private void IncrementSpace (ref int index, ref BigInteger space, int gap, BigInteger A, BigInteger B, BigInteger M)
		{
			space = (A * space + B) % M;
			index +=  (int)(space % gap) + Sett.RandCharSpcMin;
		}
		/// <summary></summary>
		/// <param name="stage"></param>
		private void SwitchCharPoz(byte stage)
		{
			List<int> switchTable = ConstructSwitchTable(stage);
			List<ushort> original = new();
			original.AddRange(Message);
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[i] = original[switchTable[i]];
			}
		}
		/// <summary></summary>
		/// <param name="stage"></param>
		private void UnSwitchCharPoz(byte stage)
		{
			List<int> switchTable = ConstructSwitchTable(stage);
			List<ushort> original = new();
			original.AddRange(Message);
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[switchTable[i]] = original[i];
			}
		}
		/// <summary></summary>
		/// <param name="stage"></param>
		/// <returns></returns>
		private List<int> ConstructSwitchTable(byte stage)
		{
			List<int> switchTable = new();
			BigInteger A, B;
			if (stage == 0)
			{
				A = Sett.SwitchConstA.ComputeConstant();
				B = Sett.SwitchConstB.ComputeConstant();
			}
			else
			{
				A = Sett.SwitchConstC.ComputeConstant();
				B = Sett.SwitchConstD.ComputeConstant();
			}
			uint m = (uint)Message.Count;
			if (m % (A * B) == 0)
			{
				m--;
			}
			for (int i = 0; i < m; i++)
			{
				switchTable.Add((int)((A * i + B) % m));
			}
			return switchTable;
		}
	}
}