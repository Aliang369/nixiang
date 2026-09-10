using System;
using TMPro;
using UnityEngine;

public class DebugValueCell : MonoBehaviour
{
	[SerializeField]
	public TextMeshProUGUI text;

	[SerializeField]
	public TextMeshProUGUI value;

	[SerializeField]
	public GameButton minusButton;

	[SerializeField]
	public GameButton plusButton;

	[NonSerialized]
	public Func<float> updateValue;

	private void Update()
	{
		if (updateValue != null)
		{
			value.text = updateValue().ToString();
		}
	}
}
