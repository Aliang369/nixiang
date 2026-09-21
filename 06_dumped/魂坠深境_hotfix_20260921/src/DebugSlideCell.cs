using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugSlideCell : MonoBehaviour
{
	[SerializeField]
	public Slider slider;

	[SerializeField]
	public TextMeshProUGUI text;

	[SerializeField]
	public TextMeshProUGUI value;

	[NonSerialized]
	public float max;

	[NonSerialized]
	public float min;
}
