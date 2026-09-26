public struct zbSafeLong(long value, string fieldName = "unknown")
{
	private long _obfValue = value ^ _key;

	private long _key = Rand.NextLong();

	private long _shadowObf = value ^ _shadowKey;

	private long _shadowKey = Rand.NextLong();

	private readonly string _fieldName = fieldName;

	public long Value
	{
		get
		{
			long num = _obfValue ^ _key;
			long num2 = _shadowObf ^ _shadowKey;
			if (num != num2)
			{
				zbCheatContext.ReportCheat(num2, num, _fieldName);
				return num2;
			}
			return num;
		}
		set
		{
			_key = Rand.NextLong();
			_shadowKey = Rand.NextLong();
			_obfValue = value ^ _key;
			_shadowObf = value ^ _shadowKey;
		}
	}

	public static implicit operator long(zbSafeLong v)
	{
		return v.Value;
	}

	public static zbSafeLong operator ++(zbSafeLong a)
	{
		a.Value++;
		return a;
	}

	public static zbSafeLong operator --(zbSafeLong a)
	{
		a.Value--;
		return a;
	}

	public static bool operator >(zbSafeLong a, zbSafeLong b)
	{
		return a.Value > b.Value;
	}

	public static bool operator <(zbSafeLong a, zbSafeLong b)
	{
		return a.Value < b.Value;
	}

	public static bool operator >=(zbSafeLong a, zbSafeLong b)
	{
		return a.Value >= b.Value;
	}

	public static bool operator <=(zbSafeLong a, zbSafeLong b)
	{
		return a.Value <= b.Value;
	}

	public static bool operator ==(zbSafeLong a, zbSafeLong b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(zbSafeLong a, zbSafeLong b)
	{
		return a.Value != b.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is zbSafeLong zbSafeLong2)
		{
			return this == zbSafeLong2;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	internal void AddFast(long delta)
	{
		long num = (_obfValue ^ _key) + delta;
		_obfValue = num ^ _key;
		_shadowObf = num ^ _shadowKey;
	}
}
