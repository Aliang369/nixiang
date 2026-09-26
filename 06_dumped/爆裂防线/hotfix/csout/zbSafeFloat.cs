using System;

public struct zbSafeFloat
{
	private int _obfValue;

	private int _key;

	private int _shadowObf;

	private int _shadowKey;

	private readonly string _fieldName;

	public float Value
	{
		get
		{
			float num = BitConverter.Int32BitsToSingle(_obfValue ^ _key);
			float num2 = BitConverter.Int32BitsToSingle(_shadowObf ^ _shadowKey);
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
			int num = BitConverter.SingleToInt32Bits(value);
			_obfValue = num ^ _key;
			_shadowObf = num ^ _shadowKey;
		}
	}

	public zbSafeFloat(float value, string fieldName = "unknown")
	{
		_fieldName = fieldName ?? "unknown";
		_key = Rand.NextInt();
		_shadowKey = Rand.NextInt();
		int num = BitConverter.SingleToInt32Bits(value);
		_obfValue = num ^ _key;
		_shadowObf = num ^ _shadowKey;
	}

	public static implicit operator float(zbSafeFloat v)
	{
		return v.Value;
	}

	public static bool operator >(zbSafeFloat a, zbSafeFloat b)
	{
		return a.Value > b.Value;
	}

	public static bool operator <(zbSafeFloat a, zbSafeFloat b)
	{
		return a.Value < b.Value;
	}

	public static bool operator >=(zbSafeFloat a, zbSafeFloat b)
	{
		return a.Value >= b.Value;
	}

	public static bool operator <=(zbSafeFloat a, zbSafeFloat b)
	{
		return a.Value <= b.Value;
	}

	public static bool operator ==(zbSafeFloat a, zbSafeFloat b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(zbSafeFloat a, zbSafeFloat b)
	{
		return a.Value != b.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is zbSafeFloat zbSafeFloat2)
		{
			return this == zbSafeFloat2;
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

	internal void AddFast(float delta)
	{
		float value = BitConverter.Int32BitsToSingle(_obfValue ^ _key) + delta;
		_key = Rand.NextInt();
		int num = BitConverter.SingleToInt32Bits(value);
		_obfValue = num ^ _key;
		_shadowObf = num ^ _shadowKey;
	}
}
