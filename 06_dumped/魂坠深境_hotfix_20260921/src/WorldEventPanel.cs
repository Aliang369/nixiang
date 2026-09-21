using System;
using System.Collections;
using Common.UI;
using UnityEngine;

public abstract class WorldEventPanel : UIBase
{
	[NonSerialized]
	public WorldEvent worldEvent;

	[NonSerialized]
	public WorldEventItemChoiceDisplay selected;

	public virtual void Setup(WorldEvent worldEvent)
	{
		this.worldEvent = worldEvent;
		base.gameObject.SetActive(value: true);
		SetDirty();
	}

	public virtual void Unset()
	{
		worldEvent = null;
	}

	protected override void Init()
	{
	}

	protected override void Dispose()
	{
		worldEvent = null;
	}

	public IEnumerator ShowPanel()
	{
		base.gameObject.SetActive(value: true);
		UIBase.gameState.applicationState.eventManager.enterWorldEventEvent.TriggerEvent();
		TriggerEvent(UIEvent.OnPanelShow);
		yield break;
	}

	public IEnumerator HidePanel()
	{
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		yield return new WaitForSeconds(dungeonMapDisplayConfig.fogFadeTime / base.uiTimeScale);
		base.gameObject.SetActive(value: false);
	}

	public abstract bool HasAnim();

	public void SelectChoice(WorldEventItemChoiceDisplay display)
	{
		selected = display;
		SetDirty();
	}

	public void UnselectChoices()
	{
		selected = null;
		SetDirty();
	}

	public virtual void HideDescription()
	{
	}

	public virtual void AfterChoiceConfirm(WorldEventChoice choice)
	{
	}
}
