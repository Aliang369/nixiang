using System;
using Common.UI;
using UnityEngine;

public class ItemInfoDisplay_Battle : ItemInfoDisplayBase
{
	[SerializeField]
	public MainItemInfoDisplay_Battle mainItemInfoDisplay;

	public EffectMarkReference effectMarkReferencePrefab;

	public RectTransform effectMarkReferenceParent;

	[NonSerialized]
	private CellUIPrefabPool<EffectMarkReference, EffectMarkAbility.EffectMarkInfo> effectMarkReferencePool;

	protected override void Init()
	{
		InitUI(mainItemInfoDisplay);
		InitEffectMarkReference();
		base.Init();
	}

	protected override void OnSetup(Item item)
	{
		base.gameObject.SetActive(value: true);
		mainItemInfoDisplay.Setup(item);
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
		GetModule<ItemInfoReferenceModule>().HideAll();
		HideAllEffectMarkReference();
	}

	public override bool IsReference(MainItemInfoDisplayBase display)
	{
		if (display != null)
		{
			return display != mainItemInfoDisplay;
		}
		return false;
	}

	public override Vector2 GetLastItemInfoDisplayPosition()
	{
		return mainItemInfoDisplay.RectTransform().anchoredPosition;
	}

	private void InitEffectMarkReference()
	{
		effectMarkReferencePool = GetPrefabPool<EffectMarkReference, EffectMarkAbility.EffectMarkInfo>(effectMarkReferencePrefab, effectMarkReferenceParent);
	}

	public void ShowAllEffectMarkReference()
	{
		effectMarkReferencePool.DespawnAll();
		if (CellUI<Item>.gameState == null)
		{
			return;
		}
		foreach (EffectMarkAbility.EffectMarkInfo effectMarkInfo in CellUI<Item>.gameState.gameEventManager.TriggerEffectMarkResolve(base.item).effectMarkInfos)
		{
			if (effectMarkInfo.showType.HasFlag(EffectMarkAbility.ShowType.Mark))
			{
				effectMarkReferencePool.Spawn(effectMarkInfo);
			}
		}
	}

	public void HideAllEffectMarkReference()
	{
		effectMarkReferencePool.DespawnAll();
	}
}
