using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SegmentedProgressBar : UIBase
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Image segmentDividerPrefab;

	[SerializeField]
	private RectTransform progressBarBackground;

	[NonSerialized]
	private int currentProgress;

	[NonSerialized]
	private int totalProgress = 100;

	private List<RectTransform> dividers = new List<RectTransform>();

	public void Set(int currentProgress, int totalProgress)
	{
		this.totalProgress = totalProgress;
		this.currentProgress = Mathf.Clamp(currentProgress, 0, totalProgress);
		ClearDividers();
		CreateDividers();
		UpdateProgressBar();
	}

	private void UpdateProgressBar()
	{
		if (totalProgress > 0)
		{
			slider.value = (float)currentProgress / (float)totalProgress;
		}
	}

	private void CreateDividers()
	{
		if (totalProgress <= 1 || segmentDividerPrefab == null)
		{
			return;
		}
		float width = progressBarBackground.rect.width;
		for (int i = 1; i < totalProgress; i++)
		{
			RectTransform component = UnityEngine.Object.Instantiate(segmentDividerPrefab, progressBarBackground).GetComponent<RectTransform>();
			if (component != null)
			{
				float x = width * (float)i / (float)totalProgress - width / 2f;
				component.anchoredPosition = new Vector2(x, 0f);
				dividers.Add(component);
			}
		}
	}

	private void ClearDividers()
	{
		foreach (RectTransform divider in dividers)
		{
			if (divider != null)
			{
				UnityEngine.Object.Destroy(divider.gameObject);
			}
		}
		dividers.Clear();
	}
}
