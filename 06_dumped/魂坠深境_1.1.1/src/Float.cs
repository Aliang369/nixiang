public class Float
{
	public float Value { get; set; }

	public Float(float f)
	{
		Value = f;
	}

	public static implicit operator float(Float f)
	{
		return f.Value;
	}

	public static implicit operator Float(float f)
	{
		return new Float(f);
	}
}
