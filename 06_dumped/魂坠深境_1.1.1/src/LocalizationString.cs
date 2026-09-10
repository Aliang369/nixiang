using UnityEngine;

public class LocalizationString
{
	[SerializeField]
	private string key;

	public string str => LocalizationManager.GetString(key);
}
