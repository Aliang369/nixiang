using System;

[Serializable]
public struct ProtectedInt : IEquatable<ProtectedInt>, IComparable<ProtectedInt>
{
	private int _salt;

	private int _encoded;

	private int _check;

	private const int CheckMagic = 1511506142;

	private static int _saltSeed = -1640531535;

	public int Value => Decode();

	private static int NextSalt()
	{
		_saltSeed = _saltSeed * 1664525 + 1013904223;
		return _saltSeed;
	}

	public ProtectedInt(int value)
	{
		_salt = NextSalt();
		_encoded = value ^ _salt;
		_check = value ^ _salt ^ 0x5A17C0DE;
	}

	private int Decode()
	{
		if (_salt == 0 && _encoded == 0 && _check == 0)
		{
			return 0;
		}
		int num = _encoded ^ _salt;
		if ((num ^ _salt ^ 0x5A17C0DE) != _check)
		{
			ProtectedValueGuard.ReportTamper(_check);
		}
		return num;
	}

	public static implicit operator int(ProtectedInt v)
	{
		return v.Decode();
	}

	public static implicit operator ProtectedInt(int v)
	{
		return new ProtectedInt(v);
	}

	public bool Equals(ProtectedInt other)
	{
		return Decode() == other.Decode();
	}

	public override bool Equals(object obj)
	{
		if (obj is ProtectedInt other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Decode();
	}

	public int CompareTo(ProtectedInt other)
	{
		return Decode().CompareTo(other.Decode());
	}

	public override string ToString()
	{
		return Decode().ToString();
	}
}
