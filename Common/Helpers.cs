using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Common
{
	/// <summary>Class for all kinds of utility methods. Static, no data here.</summary>
	public static class Helpers
	{
		/// <summary>Converts a number to a custom base.</summary>
		/// <param name="num">Number to convert.</param>
		/// <param name="newBase">Base, 2 to 65535.</param>
		/// <returns>Array of newBase numbers. lowest index is lowest order.</returns>
		public static ushort[] BaseConvertor (BigInteger num, ushort newBase)
		{
			if (newBase < 2)
			{
				return [];
			}
			List<ushort> result = [];
			while (num > 0)
			{
				result.Add((ushort)(num % newBase));
				num /= newBase;
			}
			if (result.Count == 0)
			{
				result.Add(0);
			}
			return [.. result];
		}
		/// <summary>Converts a number to a byte array in big endian.</summary>
		/// <param name="number">A number to to convert.</param>
		/// <returns>Big endian byte array.</returns>
		public static byte[] GetBytes (ushort number)
		{
			byte[] converted = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(converted);
				return converted;
			}
			else
			{
				return converted;
			}
		}
		/// <summary>Converts a number to a byte array in big endian.</summary>
		/// <param name="number">A number to to convert.</param>
		/// <returns>Big endian byte array.</returns>
		public static byte[] GetBytes(uint number)
		{
			byte[] converted = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(converted);
				return converted;
			}
			else
			{
				return converted;
			}
		}
		/// <summary>Converts a number to a byte array in big endian.</summary>
		/// <param name="number">A number to to convert.</param>
		/// <returns>Big endian byte array.</returns>
		public static byte[] GetBytes(ulong number)
		{
			byte[] converted = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(converted);
				return converted;
			}
			else
			{
				return converted;
			}
		}
		/// <summary>Converts a number to a byte array in big endian.</summary>
		/// <param name="number">A number to to convert.</param>
		/// <returns>Big endian byte array.</returns>
		public static byte[] GetBytes(short number)
		{
			byte[] converted = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(converted);
				return converted;
			}
			else
			{
				return converted;
			}
		}
		/// <summary>Converts a number to a byte array in big endian.</summary>
		/// <param name="number">A number to to convert.</param>
		/// <returns>Big endian byte array.</returns>
		public static byte[] GetBytes(int number)
		{
			byte[] converted = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(converted);
				return converted;
			}
			else
			{
				return converted;
			}
		}
		/// <summary>Converts a number to a byte array in big endian.</summary>
		/// <param name="number">A number to to convert.</param>
		/// <returns>Big endian byte array.</returns>
		public static byte[] GetBytes(long number)
		{
			byte[] converted = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(converted);
				return converted;
			}
			else
			{
				return converted;
			}
		}
		/// <summary>Converts a byte array in big endian to a number.</summary>
		/// <param name="value">Byte array in big endian representing a number.</param>
		/// <param name="startIndex">Where to start in the array.</param>
		/// <returns>A number.</returns>
		public static ushort ToUInt16(byte[] value, int startIndex = 0)
		{
			if ((startIndex + 2) <= value.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					return BitConverter.ToUInt16([value[startIndex + 1], value[startIndex]]);
				}
				else
				{
					return BitConverter.ToUInt16(value, startIndex);
				}
			}
			else
			{
				return ushort.MaxValue;
			}
		}
		/// <summary>Converts a byte array in big endian to a number.</summary>
		/// <param name="value">Byte array in big endian representing a number.</param>
		/// <param name="startIndex">Where to start in the array.</param>
		/// <returns>A number.</returns>
		public static uint ToUInt32(byte[] value, int startIndex = 0)
		{
			if ((startIndex + 2) <= value.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					return BitConverter.ToUInt32([value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex]]);
				}
				else
				{
					return BitConverter.ToUInt32(value, startIndex);
				}
			}
			else
			{
				return uint.MaxValue;
			}
		}
		/// <summary>Converts a byte array in big endian to a number.</summary>
		/// <param name="value">Byte array in big endian representing a number.</param>
		/// <param name="startIndex">Where to start in the array.</param>
		/// <returns>A number.</returns>
		public static ulong ToUInt64(byte[] value, int startIndex = 0)
		{
			if ((startIndex + 2) <= value.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					return BitConverter.ToUInt64([value[startIndex + 7], value[startIndex + 6], value[startIndex + 5], value[startIndex + 4], value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex]]);
				}
				else
				{
					return BitConverter.ToUInt64(value, startIndex);
				}
			}
			else
			{
				return ulong.MaxValue;
			}
		}
		/// <summary>Converts a byte array in big endian to a number.</summary>
		/// <param name="value">Byte array in big endian representing a number.</param>
		/// <param name="startIndex">Where to start in the array.</param>
		/// <returns>A number.</returns>
		public static short ToInt16(byte[] value, int startIndex = 0)
		{
			if ((startIndex + 2) <= value.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					return BitConverter.ToInt16([value[startIndex + 1], value[startIndex]]);
				}
				else
				{
					return BitConverter.ToInt16(value, startIndex);
				}
			}
			else
			{
				return short.MaxValue;
			}
		}
		/// <summary>Converts a byte array in big endian to a number.</summary>
		/// <param name="value">Byte array in big endian representing a number.</param>
		/// <param name="startIndex">Where to start in the array.</param>
		/// <returns>A number.</returns>
		public static int ToInt32(byte[] value, int startIndex = 0)
		{
			if ((startIndex + 2) <= value.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					return BitConverter.ToInt32([value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex]]);
				}
				else
				{
					return BitConverter.ToInt32(value, startIndex);
				}
			}
			else
			{
				return int.MaxValue;
			}
		}
		/// <summary>Converts a byte array in big endian to a number.</summary>
		/// <param name="value">Byte array in big endian representing a number.</param>
		/// <param name="startIndex">Where to start in the array.</param>
		/// <returns>A number.</returns>
		public static long ToInt64(byte[] value, int startIndex = 0)
		{
			if ((startIndex + 2) <= value.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					return BitConverter.ToInt64([value[startIndex + 7], value[startIndex + 6], value[startIndex + 5], value[startIndex + 4], value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex]]);
				}
				else
				{
					return BitConverter.ToInt64(value, startIndex);
				}
			}
			else
			{
				return long.MaxValue;
			}
		}
	}
}