using System;
using System.IO;

public class zbFileEncrypUtil
{
	private static byte m_key = 90;

	private static readonly byte[] m_encryptMark = new byte[4] { 222, 173, 190, 239 };

	public static bool IsEncrypted(byte[] data)
	{
		if (data.Length < m_encryptMark.Length)
		{
			return false;
		}
		for (int i = 0; i < m_encryptMark.Length; i++)
		{
			if (data[i] != m_encryptMark[i])
			{
				return false;
			}
		}
		return true;
	}

	public static void EncryptFile(string filePath)
	{
		byte[] data = File.ReadAllBytes(filePath);
		if (!IsEncrypted(data))
		{
			byte[] bytes = EncryptData(data);
			File.WriteAllBytes(filePath, bytes);
		}
	}

	public static byte[] EncryptData(byte[] data)
	{
		if (IsEncrypted(data))
		{
			return data;
		}
		byte[] array = new byte[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = (byte)(data[i] ^ m_key);
		}
		byte[] array2 = new byte[m_encryptMark.Length + array.Length];
		Array.Copy(m_encryptMark, 0, array2, 0, m_encryptMark.Length);
		Array.Copy(array, 0, array2, m_encryptMark.Length, array.Length);
		return array2;
	}

	public static byte[] DecryptFile(string filePath)
	{
		byte[] array = File.ReadAllBytes(filePath);
		if (!IsEncrypted(array))
		{
			return array;
		}
		byte[] array2 = new byte[array.Length - m_encryptMark.Length];
		Array.Copy(array, m_encryptMark.Length, array2, 0, array2.Length);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] ^= m_key;
		}
		return array2;
	}
}
