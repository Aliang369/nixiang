public struct zbSafeInt(int value, string fieldName = "unknown")
{
	private int _obfValue = value ^ _key;

	private int _key = Rand.NextInt();

	private int _shadowObf = value ^ _shadowKey;

	private int _shadowKey = Rand.NextInt();

	private readonly string _fieldName = fieldName;

	public int Value
	{
		get
		{
			int num = _obfValue ^ _key;
			int num2 = _shadowObf ^ _shadowKey;
			if (num != num2)
			{
				zbCheatContext.ReportCheat(num2, num, _fieldName);
				return num2;
			}
			return num;
		}
		set
		{
			_key = Rand.NextInt();
			_shadowKey = Rand.NextInt();
			_obfValue = value ^ _key;
			_shadowObf = value ^ _shadowKey;
		}
	}

	public static implicit operator int(zbSafeInt v)
	{
		return v.Value;
	}

	public static zbSafeInt operator ++(zbSafeInt a)
	{
		a.Value++;
		return a;
	}

	public static zbSafeInt operator --(zbSafeInt a)
	{
		a.Value--;
		return a;
	}

	public static bool operator >(zbSafeInt a, zbSafeInt b)
	{
		return a.Value > b.Value;
	}

	public static bool operator <(zbSafeInt a, zbSafeInt b)
	{
		return a.Value < b.Value;
	}

	public static bool operator >=(zbSafeInt a, zbSafeInt b)
	{
		return a.Value >= b.Value;
	}

	public static bool operator <=(zbSafeInt a, zbSafeInt b)
	{
		return a.Value <= b.Value;
	}

	public static bool operator ==(zbSafeInt a, zbSafeInt b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(zbSafeInt a, zbSafeInt b)
	{
		return a.Value != b.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is zbSafeInt zbSafeInt2)
		{
			return this == zbSafeInt2;
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
}
