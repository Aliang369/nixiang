using System.Collections.Generic;

public class ContractRuntimeStateInfo : InGameManager
{
	[SaveField(null, null)]
	public Dictionary<string, int> intValues = new Dictionary<string, int>();

	public override void Init(GameStateInfo gameState)
	{
		if (intValues == null)
		{
			intValues = new Dictionary<string, int>();
		}
	}

	public void Reset()
	{
		intValues = new Dictionary<string, int>();
	}

	public int GetInt(string key)
	{
		if (string.IsNullOrEmpty(key) || intValues == null)
		{
			return 0;
		}
		if (!intValues.TryGetValue(key, out var value))
		{
			return 0;
		}
		return value;
	}

	public void SetInt(string key, int value)
	{
		if (!string.IsNullOrEmpty(key))
		{
			if (intValues == null)
			{
				intValues = new Dictionary<string, int>();
			}
			intValues[key] = value;
		}
	}

	public int ChangeInt(string key, int delta)
	{
		int num = GetInt(key) + delta;
		SetInt(key, num);
		return num;
	}
}
