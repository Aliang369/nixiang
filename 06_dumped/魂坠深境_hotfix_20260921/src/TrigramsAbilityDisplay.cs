using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class TrigramsAbilityDisplay : UIBase
{
	[SerializeField]
	private TextMeshProUGUI textPrefab;

	[SerializeField]
	private RectTransform textParent;

	[NonSerialized]
	private PrefabPool<TextMeshProUGUI> textPool;

	protected override void Init()
	{
		textPrefab.gameObject.SetActive(value: false);
		textPool = new PrefabPool<TextMeshProUGUI>(textPrefab, textParent);
	}

	protected override void UpdateDisplay()
	{
		textPool.DespawnAll();
		TrigramsManager.YinYangStateType currentState = UIBase.gameState.trigramsManager.GetCurrentState();
		if (currentState.HasFlag(TrigramsManager.YinYangStateType.ExtremeYang))
		{
			textPool.Spawn().text = ScriptableObjectSingleton<TextDataPreset>.instance.term_trigrams_extreme_yang_description;
		}
		if (currentState.HasFlag(TrigramsManager.YinYangStateType.ExtremeYin))
		{
			textPool.Spawn().text = ScriptableObjectSingleton<TextDataPreset>.instance.term_trigrams_extreme_yin_description;
		}
	}
}
