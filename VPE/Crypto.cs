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
		/// <summary>Sada čísel reprezentující zprávu. Proměnná, se kterou v celém procesu pracuji.</summary>
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
			ConvertToNums(Text); // OK
			FileHandling.DebugDump(Message);
			AddRandomChars(); // 
			FileHandling.DebugDump(Message);
			SwitchCharPoz(0); // 
			FileHandling.DebugDump(Message);
			OrderShift(); // 
			FileHandling.DebugDump(Message);
			MultithreadedPartEnc(); //
			FileHandling.DebugDump(Message);
			ConstantShift(); // 
			FileHandling.DebugDump(Message);
			VariableShift(); // 
			FileHandling.DebugDump(Message);
			AddRandomChars();
			FileHandling.DebugDump(Message);
			SwitchCharPoz(1);
			FileHandling.DebugDump(Message);
			FileHandling.DumpToDisk("Encryption");
			return ConvertToString();
		}
		/// <summary>Dešifruje text.</summary>
		/// <param name="Text">Zašifrovaný text.</param>
		/// <returns>Dešifrovaný text.</returns>
		public string Decypt(string Text)
		{
			ConvertToNums(Text);
			FileHandling.DebugDump(Message);
			UnSwitchCharPoz(1);
			FileHandling.DebugDump(Message);
			RemoveRandomChars();
			FileHandling.DebugDump(Message);
			UnVariableShift(); // 
			FileHandling.DebugDump(Message);
			UnConstantShift(); // 
			FileHandling.DebugDump(Message);
			MultithreadedPartDec(); // 
			FileHandling.DebugDump(Message);
			UnOrderShift(); // 
			FileHandling.DebugDump(Message);
			UnSwitchCharPoz(0); // 
			FileHandling.DebugDump(Message);
			RemoveRandomChars(); // 
			FileHandling.DebugDump(Message);
			FileHandling.DumpToDisk("Decryption");
			return ConvertToString(); // OK
		}
		/// <summary></summary>
		private void MultithreadedPartEnc()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Swap();
				FileHandling.DebugDump(Message);
				Scramble();
				FileHandling.DebugDump(Message);
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
				Swap();
				FileHandling.DebugDump(Message);
				Unscramble();
				FileHandling.DebugDump(Message);
				Unswap();
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
		/// <summary>Zkonvertuje textovou zprávu na číselnou reprezentaci podle tabulky znaků.</summary>
		/// <param name="Text">Textová zpráva.</param>
		private void ConvertToNums(string Text)
		{
			Message = new(Text.Length);
			for (int i = 0; i < Text.Length; i++)
			{
				char C = Text[i]; // Současný znak bokem.
				char N = i == (Text.Length - 1)? '\0' : Text[i + 1]; // Následující znak. Pokud jsem na konci, dávám \0, jako znamení toho stavu.
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
				{ // Pokud znak neznám, přeskakuji ho.
					continue;
				}
			}
		}
		/// <summary>Převede sadu čísel na text.</summary>
		/// <returns>Text.</returns>
		private string ConvertToString()
		{
			StringBuilder SB = new();
			foreach (ushort Num in Message)
			{
				if (Num < Codepage.Limit)
				{
					SB.Append(Codepage.CharSet[Num]);
				}
				else
				{
					SB.Append('✳');
				}
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
			for (int i = 0; i < Sett.Swaps.Count; i++)
			{
				for (int j = 0; j < Message.Count; j++)
				{
					swapped = Sett.Swaps[i].FindValueUsingIndex(Message[j]);
					if (swapped == Table.Outside || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[j] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		/// <param name="message"></param>
		private void Swap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = 0; i < Sett.Swaps.Count; i++)
			{
				for (int j = 0; j < message.Length; j++)
				{
					swapped = Sett.Swaps[i].FindValueUsingIndex(message[j]);
					if (swapped == Table.Outside || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						message[j] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		private void Unswap()
		{
			ushort swapped;
			for (int i = Sett.Swaps.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < Message.Count; j++)
				{
					swapped = Sett.Swaps[i].FindIndexUsingValue(Message[j]);
					if (swapped == Table.NotFound || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[j] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		/// <param name="message"></param>
		private void Unswap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = Sett.Swaps.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < message.Length; j++)
				{
					swapped = Sett.Swaps[i].FindIndexUsingValue(message[j]);
					if (swapped == Table.NotFound || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						message[j] = swapped;
					}
				}
			}
		}
		/// <summary></summary>
		private void Scramble()
		{
			List<ushort>[] logs = { new List<ushort>(), new List<ushort>() };
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i);
				logs[0].Add(Message[i]);
				Reflect(i);
				logs[1].Add(Message[i]);
				BackwardScramble(i);
				Message[i] = (ushort)((Message[i] + Sett.Rotors[0].Pozition) % Codepage.Limit);
				Sett.IncrementPozitions();
			}
			foreach (List<ushort> sub in logs)
			{
				FileHandling.DebugDump(sub);
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
			List<ushort>[] logs = { new List<ushort>(), new List<ushort>() };
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i);
				logs[0].Add(Message[i]);
				ReflectBack(i);
				logs[1].Add(Message[i]);
				BackwardScramble(i);
				Message[i] = (ushort)((Message[i] + Sett.Rotors[0].Pozition) % Codepage.Limit);
				Sett.IncrementPozitions();
			}
			foreach(List<ushort> sub in logs)
			{
				FileHandling.DebugDump(sub);
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
				int Sum = message[i] + sett.Rotors[0].Pozition;
				message[i] = (ushort)(Sum % Codepage.Limit);
				sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		private void ConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				long temp = Message[i];
				temp += Sett.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
			}
		}
		/// <summary></summary>
		private void UnConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				long temp = Message[i];
				temp -= Sett.ConstShift;
				temp %= Codepage.Limit;
				Message[i] = temp < 0 ? (ushort)(temp + Codepage.Limit) : (ushort)temp;
			}
		}
		/// <summary>Posune čísla podle pořadí.</summary>
		private void OrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] + i) % Codepage.Limit);
			}
		}
		/// <summary>Posune čísla zpět podle pořadí.</summary>
		private void UnOrderShift()
		{
			int temp;
			for (int i = 0; i < Message.Count; i++)
			{
				temp = (Message[i] - i) % Codepage.Limit;
				Message[i] = temp < 0 ? (ushort)(temp + Codepage.Limit) : (ushort)temp;
			}
		}
		/// <summary></summary>
		private void VariableShift ()
		{
			long v, c = Sett.ConstShift >= (Codepage.Limit / 2) ? Sett.ConstShift : Codepage.Limit - Sett.ConstShift;
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] + (Sett.VarShift * (i % c));
				v %= Codepage.Limit;
				Message[i] = v < 0 ? (ushort)(v + Codepage.Limit) : (ushort)v;
			}
		}
		/// <summary></summary>
		private void UnVariableShift()
		{
			long v, c = Sett.ConstShift >= (Codepage.Limit / 2) ? Sett.ConstShift : Codepage.Limit - Sett.ConstShift;
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] - (Sett.VarShift * (i % c));
				v %= Codepage.Limit;
				Message[i] = v < 0 ? (ushort)(v + Codepage.Limit) : (ushort)v;
			}
		}
		/// <summary></summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private void ForwardScramble(int i)
		{
			int sum = (Message[i] + Sett.Rotors[0].Pozition) % Codepage.Limit;
			Message[i] = Sett.Rotors[0].FindValueUsingIndex((ushort)sum);
			for (int j = 1; j < Sett.Rotors.Count; j++)
			{
				sum = (Message[i] + Sett.Rotors[j - 1].Pozition) % Codepage.Limit;
				sum = (sum + Sett.Rotors[j].Pozition) % Codepage.Limit;
				Message[i] = Sett.Rotors[j].FindValueUsingIndex((ushort)sum);
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
			int sum = (Message[i] + Sett.Rotors.Last().Pozition) % Codepage.Limit;
			Message[i] = Sett.Rotors.Last().FindIndexUsingValue((ushort)sum);
			for (int j = Sett.Rotors.Count - 2; j > -1; j--)
			{
				sum = (Message[i] + Sett.Rotors[j + 1].Pozition) % Codepage.Limit;
				sum = (sum + Sett.Rotors[j].Pozition) % Codepage.Limit;
				Message[i] = Sett.Rotors[j].FindIndexUsingValue((ushort)sum);
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
			int sum = (Message[i] + Sett.Rotors.Last().Pozition) % Codepage.Limit;
			Message[i] = Sett.Reflector.FindValueUsingIndex((ushort)sum);
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
			int sum = (Message[i] + Sett.Rotors.Last().Pozition) % Codepage.Limit;
			Message[i] = Sett.Reflector.FindIndexUsingValue((ushort)sum);
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
			Generators Gen = new (Codepage.Limit, (long)(Sett.RandCharConstB.ComputeConstant() % long.MaxValue));
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2; // Index inicialization, where I will add random char.
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin); // Inicializace mezery. V jádru „náhodný“ výpočet, pak dání do rozsahu a posunutí o minimum.
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant(); // I have to precompute the constants here, so I don't compute them every time.
			while (index < Message.Count)
			{
				Message.Insert(index, Gen.GenerateNum());
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
			if (index == Message.Count)
			{
				Message.Add(Gen.GenerateNum());
			}
		}
		/// <summary>Removes random chars from the message.</summary>
		private void RemoveRandomChars()
		{
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2;
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin);
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant();
			List<int> IdxToRemove = new()
			{
				index
			};
			IncrementSpace(ref index, ref space, gap, A, B, M);
			while (index < Message.Count)
			{
				IdxToRemove.Add(index);
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
			for (int i = IdxToRemove.Count - 1; i > -1; i--)
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