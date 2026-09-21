using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	public TextMeshProUGUI text;

	public void Set(float currentProgress, float totalProgress, Func<string> textGetter = null)
	{
		if (textGetter == null)
		{
			text.text = $"{currentProgress}/{totalProgress}";
		}
		else
		{
			text.text = textGetter();
		}
		slider.value = Mathf.Clamp(currentProgress, 0f, totalProgress) / totalProgress;
	}
}
