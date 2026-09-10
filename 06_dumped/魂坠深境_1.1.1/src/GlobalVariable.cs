using System;

[Serializable]
public class GlobalVariable
{
	[SaveField(null, null)]
	public string id;

	[SaveField(null, null)]
	public int value;
}
