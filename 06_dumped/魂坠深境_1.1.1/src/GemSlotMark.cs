using UnityEngine;
using UnityEngine.UI;

public class GemSlotMark : CellUI<HexShape>
{
	[SerializeField]
	private Image gemSlotMark;

	protected override void OnSetup(HexShape setupData)
	{
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		gemSlotMark.gameObject.SetActive(value: true);
		if (base.setupData.points.Count >= 15)
		{
			gemSlotMark.sprite = ScriptableObjectSingleton<GameDataManager>.instance.gemMasks[4];
		}
		else if (base.setupData.points.Count >= 13)
		{
			gemSlotMark.sprite = ScriptableObjectSingleton<GameDataManager>.instance.gemMasks[3];
		}
		else if (base.setupData.points.Count >= 10)
		{
			gemSlotMark.sprite = ScriptableObjectSingleton<GameDataManager>.instance.gemMasks[2];
		}
		else if (base.setupData.points.Count >= 7)
		{
			gemSlotMark.sprite = ScriptableObjectSingleton<GameDataManager>.instance.gemMasks[1];
		}
		else if (base.setupData.points.Count >= 1)
		{
			gemSlotMark.sprite = ScriptableObjectSingleton<GameDataManager>.instance.gemMasks[0];
		}
		else
		{
			gemSlotMark.gameObject.SetActive(value: false);
		}
	}
}
