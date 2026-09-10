using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankModule : UIDisplayModule
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameObject rankIcon;

	[SerializeField]
	private bool hideRankIcon;

	[SerializeField]
	private GameObject[] rankMark;

	[SerializeField]
	private Color maxVirutalQualityRankIconColor;

	[SerializeField]
	private Image skillRankBackground;

	public Func<QualityLevel> fQuality;

	public Func<QualityLevel> fMaxVirutalQuality;

	public PrefabPool<Transform> pool;

	public bool HideRankIcon
	{
		get
		{
			return hideRankIcon;
		}
		set
		{
			hideRankIcon = value;
		}
	}

	private int RealStarCount
	{
		get
		{
			QualityLevel? qualityLevel = fQuality?.Invoke();
			if (!qualityLevel.HasValue)
			{
				return 0;
			}
			return qualityLevel.Value.QualityToLevel();
		}
	}

	private int MaxStarCount
	{
		get
		{
			QualityLevel? qualityLevel = fMaxVirutalQuality?.Invoke();
			if (!qualityLevel.HasValue)
			{
				return 0;
			}
			return qualityLevel.Value.QualityToLevel();
		}
	}

	public override void Init()
	{
		if (rankIcon != null)
		{
			pool = new PrefabPool<Transform>(rankIcon.transform);
			rankIcon.gameObject.SetActive(value: false);
		}
	}

	public override void Setup()
	{
		QualityLevel? qualityLevel = fQuality?.Invoke();
		if (!qualityLevel.HasValue)
		{
			return;
		}
		QualityLevel value = qualityLevel.Value;
		if (text != null)
		{
			text.text = ScriptableObjectSingleton<TextDataPreset>.instance.GetQualityLevelText(value, withColor: true);
		}
		if (rankIcon != null)
		{
			int realStarCount = RealStarCount;
			int maxStarCount = MaxStarCount;
			pool.DespawnAll();
			if (!hideRankIcon)
			{
				for (int i = 0; i < realStarCount; i++)
				{
					pool.Spawn().GetComponent<Image>().color = Color.white;
				}
				if (maxStarCount > realStarCount)
				{
					for (int j = 0; j < maxStarCount - realStarCount; j++)
					{
						pool.Spawn().GetComponent<Image>().color = maxVirutalQualityRankIconColor;
					}
				}
			}
		}
		if (!rankMark.IsNullOrEmpty())
		{
			rankMark[0]?.gameObject.SetActive(value == QualityLevel.Base);
			rankMark[1]?.gameObject.SetActive(value == QualityLevel.Common);
			rankMark[2]?.gameObject.SetActive(value == QualityLevel.Uncommon);
			rankMark[3]?.gameObject.SetActive(value == QualityLevel.Rare);
			rankMark[4]?.gameObject.SetActive(value == QualityLevel.Epic);
		}
		if (skillRankBackground != null)
		{
			VFXDataPreset.QualityDisplayInfo qualityDisplayInfo = ScriptableObjectSingleton<VFXDataPreset>.instance.qualityDisplayInfos[value];
			skillRankBackground.color = qualityDisplayInfo.skillBackGroundColor;
			skillRankBackground.sprite = qualityDisplayInfo.skillBackGroundSprite;
		}
	}
}
