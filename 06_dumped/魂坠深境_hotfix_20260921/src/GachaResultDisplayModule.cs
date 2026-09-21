using System;
using Common.UI;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultDisplayModule : GlobalItemDisplay.DisplayModule
{
	[SerializeField]
	private GameObject newMask;

	[SerializeField]
	private GameObject[] rank1Mask;

	[SerializeField]
	private GameObject[] rank2Mask;

	[SerializeField]
	private GameObject[] rank3Mask;

	[SerializeField]
	private GameObject[] rank4Mask;

	[SerializeField]
	private GameObject[] rank5Mask;

	[SerializeField]
	private TextMeshProUGUI typeText;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image effect;

	[NonSerialized]
	private PrefabPool<Image> pool;

	public override void Init()
	{
		pool = new PrefabPool<Image>(effect);
	}

	public override void Setup()
	{
		pool.DespawnAll();
		pool.DestroyInActive();
		((Behaviour)(object)animator).enabled = false;
		GetComponent<CanvasGroup>().alpha = 0f;
		newMask.gameObject.SetActive(base.globalItem.isNew);
		rank1Mask.ForEach(delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		rank2Mask.ForEach(delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		rank3Mask.ForEach(delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		rank4Mask.ForEach(delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		rank5Mask.ForEach(delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		if (base.globalItem.GlobalItemQuality == QualityLevel.Base)
		{
			rank1Mask.ForEach(delegate(GameObject o)
			{
				o.SetActive(value: true);
			});
		}
		else if (base.globalItem.GlobalItemQuality == QualityLevel.Common)
		{
			rank2Mask.ForEach(delegate(GameObject o)
			{
				o.SetActive(value: true);
			});
		}
		else if (base.globalItem.GlobalItemQuality == QualityLevel.Uncommon)
		{
			rank3Mask.ForEach(delegate(GameObject o)
			{
				o.SetActive(value: true);
			});
		}
		else if (base.globalItem.GlobalItemQuality == QualityLevel.Rare)
		{
			rank4Mask.ForEach(delegate(GameObject o)
			{
				o.SetActive(value: true);
			});
		}
		else if (base.globalItem.GlobalItemQuality == QualityLevel.Epic)
		{
			rank5Mask.ForEach(delegate(GameObject o)
			{
				o.SetActive(value: true);
			});
		}
		typeText.text = GetBottomDisplayText();
	}

	public override void Unset()
	{
		((Behaviour)(object)animator).enabled = false;
		pool.DespawnAll();
	}

	public void SpawnEffect()
	{
		pool.Spawn().gameObject.SetActive(value: true);
	}

	public void PlayShowAnim()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
	}

	private string GetBottomDisplayText()
	{
		string globalItemCellName = base.globalItem.GlobalItemCellName;
		if (globalItemCellName.IsNullOrEmpty() && base.globalItem is GlobalRewardInfo globalRewardInfo)
		{
			return globalRewardInfo.gachaTypeString;
		}
		int globalItemCount = base.globalItem.GlobalItemCount;
		return TextDisplayHelper.FormatNameWithCount(globalItemCellName, globalItemCount);
	}
}
