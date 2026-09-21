using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public class InfluenceDisplayArea : UIBase
{
	public static List<InfluenceDisplayArea> allAreas = new List<InfluenceDisplayArea>();

	[SerializeField]
	private Transform influenceCellPivot;

	[SerializeField]
	private InfluenceDisplay influenceCellPrefab;

	[SerializeField]
	private LayoutSpacingAdjustor spacingAdjustor;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceCellPool;

	public static List<InfluenceDisplayArea> current => allAreas.FindAll((InfluenceDisplayArea a) => a != null && a.gameObject != null && a.gameObject.activeInHierarchy);

	protected virtual BattleFieldTransform BftOwner => UIBase.gameState.battleState?.player;

	protected virtual IList<Influence> DisplayInfluences => BftOwner?.influences;

	protected override void Init()
	{
		allAreas.Add(this);
		influenceCellPrefab.gameObject.SetActive(value: false);
		influenceCellPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceCellPrefab, influenceCellPivot);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddInfluence, _InfluenceChange);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.RemoveInfluence, _InfluenceChange);
		influenceCellPool.ConfigInfoPanel(UIBase.applicationState.runningGameState.UIManager.influenceInfoPanel);
		SetDirty();
	}

	protected override void Dispose()
	{
		allAreas.Remove(this);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddInfluence, _InfluenceChange);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.RemoveInfluence, _InfluenceChange);
	}

	protected override void UpdateDisplay()
	{
		influenceCellPool?.Sync(DisplayInfluences);
		spacingAdjustor.Adjust();
	}

	public InfluenceDisplay GetInfluenceDisplay(Influence influence)
	{
		return influenceCellPool.GetActiveElements().FirstOrDefault((InfluenceDisplay t) => t.influence == influence);
	}

	public void TriggerAbilityAnim(Influence influence, Ability.AbilityAnimPriority priority)
	{
		UIBase.gameState.StartCoroutine(Coroutine);
		IEnumerator Coroutine()
		{
			InfluenceDisplay influenceDisplay = GetInfluenceDisplay(influence);
			if (!(influenceDisplay == null))
			{
				UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Spell);
				yield return influenceDisplay.GetModule<InfluenceStateModule_InfoPanel>().TriggerAbilityAnim(priority);
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Spell);
			}
		}
	}

	private void _InfluenceChange(GameEvent.Type type, GameEvent eve)
	{
		SetDirty();
	}
}
