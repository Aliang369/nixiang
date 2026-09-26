using System;
using System.Text;

public sealed class zbBinReader
{
	private readonly byte[] m_data;

	private readonly string[] m_strings;

	private int m_pos;

	private static readonly uint[] s_crcTable = BuildCrcTable();

	public bool IsMap { get; }

	public int RecordCount { get; }

	public static zbBinReader Create(byte[] bytes)
	{
		return new zbBinReader(bytes);
	}

	private zbBinReader(byte[] bytes)
	{
		if (bytes == null || bytes.Length < 13)
		{
			throw new InvalidOperationException("zbBinReader: 数据为空或长度不足");
		}
		uint num = ComputeCrc32(bytes, bytes.Length - 4);
		int num2 = bytes.Length - 4;
		uint num3 = (uint)(bytes[num2] | (bytes[num2 + 1] << 8) | (bytes[num2 + 2] << 16) | (bytes[num2 + 3] << 24));
		if (num != num3)
		{
			throw new InvalidOperationException($"zbBinReader: CRC 校验失败 expect=0x{num:X8} actual=0x{num3:X8}");
		}
		if (bytes[0] != 90 || bytes[1] != 66 || bytes[2] != 67 || bytes[3] != 49)
		{
			throw new InvalidOperationException("zbBinReader: 魔数不匹配（非 ZBC1 格式）");
		}
		m_data = bytes;
		m_pos = 4;
		int num4 = m_data[m_pos++];
		IsMap = (num4 & 1) != 0;
		m_pos++;
		int num5 = (int)ReadVarIntRaw();
		for (int i = 0; i < num5; i++)
		{
			ReadVarIntRaw();
			EnsureRemaining(1);
			m_pos++;
		}
		int num6 = (int)ReadVarIntRaw();
		m_strings = new string[num6];
		for (int j = 0; j < num6; j++)
		{
			int num7 = (int)ReadVarIntRaw();
			EnsureRemaining(num7);
			m_strings[j] = ((num7 == 0) ? string.Empty : Encoding.UTF8.GetString(m_data, m_pos, num7));
			m_pos += num7;
		}
		RecordCount = (int)ReadVarIntRaw();
	}

	public int ReadInt()
	{
		long num = ReadVarIntRaw();
		return (int)((num >> 1) ^ -(num & 1));
	}

	public long ReadLong()
	{
		long num = ReadVarIntRaw();
		return (num >> 1) ^ -(num & 1);
	}

	public float ReadFloat()
	{
		EnsureRemaining(4);
		float result = BitConverter.ToSingle(m_data, m_pos);
		m_pos += 4;
		return result;
	}

	public string ReadString()
	{
		int num = (int)ReadVarIntRaw();
		if (num < 0 || num >= m_strings.Length)
		{
			throw new InvalidOperationException($"zbBinReader: 字符串池越界 idx={num} pos={m_pos}");
		}
		return m_strings[num];
	}

	public int[] ReadIntArray()
	{
		int num = (int)ReadVarIntRaw();
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ReadInt();
		}
		return array;
	}

	public long[] ReadLongArray()
	{
		int num = (int)ReadVarIntRaw();
		long[] array = new long[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ReadLong();
		}
		return array;
	}

	public float[] ReadFloatArray()
	{
		int num = (int)ReadVarIntRaw();
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ReadFloat();
		}
		return array;
	}

	public string[] ReadStringArray()
	{
		int num = (int)ReadVarIntRaw();
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ReadString();
		}
		return array;
	}

	public int[][] ReadIntArray2()
	{
		int num = (int)ReadVarIntRaw();
		int[][] array = new int[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)ReadVarIntRaw();
			int[] array2 = new int[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = ReadInt();
			}
			array[i] = array2;
		}
		return array;
	}

	public long[][] ReadLongArray2()
	{
		int num = (int)ReadVarIntRaw();
		long[][] array = new long[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)ReadVarIntRaw();
			long[] array2 = new long[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = ReadLong();
			}
			array[i] = array2;
		}
		return array;
	}

	public float[][] ReadFloatArray2()
	{
		int num = (int)ReadVarIntRaw();
		float[][] array = new float[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)ReadVarIntRaw();
			float[] array2 = new float[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = ReadFloat();
			}
			array[i] = array2;
		}
		return array;
	}

	public string[][] ReadStringArray2()
	{
		int num = (int)ReadVarIntRaw();
		string[][] array = new string[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)ReadVarIntRaw();
			string[] array2 = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = ReadString();
			}
			array[i] = array2;
		}
		return array;
	}

	public void ValidateEnd()
	{
		if (m_pos != m_data.Length - 4)
		{
			throw new InvalidOperationException($"zbBinReader: 记录体长度不符 pos={m_pos} 期望={m_data.Length - 4}");
		}
	}

	private long ReadVarIntRaw()
	{
		long num = 0L;
		int num2 = 0;
		while (true)
		{
			EnsureRemaining(1);
			byte b = m_data[m_pos++];
			num |= (long)(b & 0x7F) << num2;
			if ((b & 0x80) == 0)
			{
				break;
			}
			num2 += 7;
			if (num2 > 63)
			{
				throw new InvalidOperationException($"zbBinReader: varint 超长 pos={m_pos}");
			}
		}
		return num;
	}

	private void EnsureRemaining(int count)
	{
		if (m_pos + count > m_data.Length - 4)
		{
			throw new InvalidOperationException($"zbBinReader: 越界读取 pos={m_pos} need={count}");
		}
	}

	private static uint[] BuildCrcTable()
	{
		uint[] array = new uint[256];
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = num;
			for (int i = 0; i < 8; i++)
			{
				num2 = (((num2 & 1) != 0) ? (0xEDB88320u ^ (num2 >> 1)) : (num2 >> 1));
			}
			array[num] = num2;
		}
		return array;
	}

	private static uint ComputeCrc32(byte[] data, int length)
	{
		uint num = uint.MaxValue;
		for (int i = 0; i < length; i++)
		{
			num = s_crcTable[(num ^ data[i]) & 0xFF] ^ (num >> 8);
		}
		return num ^ 0xFFFFFFFFu;
	}
}
