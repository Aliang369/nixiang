using System;

[Serializable]
public struct ProtectedLong : IEquatable<ProtectedLong>, IComparable<ProtectedLong>
{
	private long _salt;

	private long _encoded;

	private long _check;

	private const long CheckMagic = 6491869450247567793L;

	private static long _saltSeed = -7046029254386353131L;

	public long Value => Decode();

	private static long NextSalt()
	{
		_saltSeed = _saltSeed * 6364136223846793005L + 1442695040888963407L;
		return _saltSeed;
	}

	public ProtectedLong(long value)
	{
		_salt = NextSalt();
		_encoded = value ^ _salt;
		_check = value ^ _salt ^ 0x5A17C0DE9E3779B1L;
	}

	private long Decode()
	{
		if (_salt == 0L && _encoded == 0L && _check == 0L)
		{
			return 0L;
		}
		long num = _encoded ^ _salt;
		if ((num ^ _salt ^ 0x5A17C0DE9E3779B1L) != _check)
		{
			ProtectedValueGuard.ReportTamper((int)_check);
		}
		return num;
	}

	public static implicit operator long(ProtectedLong v)
	{
		return v.Decode();
	}

	public static implicit operator ProtectedLong(long v)
	{
		return new ProtectedLong(v);
	}

	public bool Equals(ProtectedLong other)
	{
		return Decode() == other.Decode();
	}

	public override bool Equals(object obj)
	{
		if (obj is ProtectedLong other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Decode().GetHashCode();
	}

	public int CompareTo(ProtectedLong other)
	{
		return Decode().CompareTo(other.Decode());
	}

	public override string ToString()
	{
		return Decode().ToString();
	}
}
