using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleFieldTransformDisplay : MonoBehaviour
{
	[SerializeField]
	public List<Transform> displayPivots;

	[SerializeField]
	public Transform lookPivot;

	[SerializeField]
	public Transform enemyTablePivot;

	[SerializeField]
	private Transform hitPivot;

	[SerializeField]
	private Transform _effectUpperPivot;

	[SerializeField]
	private Transform _effectLowerPivot;

	public BattleFieldTransform attachBft;

	[NonSerialized]
	public ConvertablePivot effectLowerPivot;

	[NonSerialized]
	public ConvertablePivot effectUpperPivot;

	[NonSerialized]
	public ConvertablePivot effectHitPivot;

	[NonSerialized]
	public List<Item> ringItem;

	[NonSerialized]
	public bool skipNextTurn;

	[SerializeField]
	public List<DisplayUnit> displayUnits;

	[SerializeField]
	private GameObject ring;

	[NonSerialized]
	public bool animFrozen;

	[NonSerialized]
	public Dictionary<DisplayUnit, Transform> usedTransform;

	public GameStateInfo GameState => attachBft?.gameState;

	public bool isEnemy => GetComponent<CharacterAIComponent>() != null;

	private void OnDestroy()
	{
		DisposeAnim();
	}

	public void Init(BattleFieldTransform bft)
	{
		attachBft = bft;
		bft.EnsureRuntimeContainers(GameState);
		effectUpperPivot = new ConvertablePivot((_effectUpperPivot != null) ? _effectUpperPivot : base.transform, null);
		effectLowerPivot = new ConvertablePivot((_effectLowerPivot != null) ? _effectLowerPivot : base.transform, null);
		effectHitPivot = new ConvertablePivot((hitPivot != null) ? hitPivot : base.transform, GameState.UIManager.ui3DWorldCanvas);
		bft.attachDisplay = this;
		InitAnim();
		if (isEnemy)
		{
			InitRendering();
			ringItem = new List<Item>();
			ring.SetActive(value: false);
		}
	}

	private void InitRendering()
	{
	}

	public void TriggerEnemyRing(Item item, bool show)
	{
		if (show)
		{
			ringItem.Add(item);
		}
		else
		{
			ringItem.Remove(item);
		}
		if (ringItem.Count > 0)
		{
			ring.GetComponent<MeshRenderer>().material.mainTexture = item.GetDef().ringSprite.texture;
			ring.SetActive(value: true);
		}
		else
		{
			ring.SetActive(value: false);
		}
	}

	public DisplayUnit GetItemAttachDisplayUnit(ItemDefinition def)
	{
		return displayUnits.FirstOrDefault();
	}

	public void InitAnim()
	{
		if (!isEnemy)
		{
			return;
		}
		GameState.gameEventManager.onTriggerGameEvent += HandleGameEvent;
		foreach (DisplayUnit displayUnit in displayUnits)
		{
			displayUnit.Init();
		}
		usedTransform = new Dictionary<DisplayUnit, Transform>();
		animFrozen = false;
	}

	private void DisposeAnim()
	{
		if (isEnemy)
		{
			GameState.gameEventManager.onTriggerGameEvent -= HandleGameEvent;
			usedTransform = null;
		}
	}

	public void PlayPerformance(DisplayUnit.EnemyPerformanceState performanceState)
	{
		if (performanceState.animationType == EnemyAnimationType.death)
		{
			foreach (DisplayUnit displayUnit2 in displayUnits)
			{
				if (displayUnit2.gameObject.activeSelf)
				{
					displayUnit2.PlayPerformance(performanceState);
				}
			}
			performanceState.isFinished = true;
			return;
		}
		if (animFrozen)
		{
			performanceState.isFinished = true;
		}
		DisplayUnit displayUnit = GetDisplayUnit(performanceState.sourceItem);
		if (displayUnit == null)
		{
			displayUnit = displayUnits.FirstOrDefault();
		}
		if (displayUnit != null)
		{
			displayUnit.PlayPerformance(performanceState);
			return;
		}
		performanceState.TriggerEffectAction();
		performanceState.isFinished = true;
	}

	public void FreezeAnim()
	{
		if (animFrozen)
		{
			return;
		}
		animFrozen = true;
		foreach (DisplayUnit displayUnit in displayUnits)
		{
			displayUnit.FreezeAnim();
		}
	}

	public void UnfreezeAnim()
	{
		if (!animFrozen)
		{
			return;
		}
		animFrozen = false;
		foreach (DisplayUnit displayUnit in displayUnits)
		{
			displayUnit.UnfreezeAnim();
		}
	}

	public bool IsPushingForward()
	{
		for (int i = 0; i < displayUnits.Count; i++)
		{
			DisplayUnit displayUnit = displayUnits[i];
			if ((object)displayUnit != null && displayUnit.IsPushingForward())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnimationPlaying()
	{
		for (int i = 0; i < displayUnits.Count; i++)
		{
			DisplayUnit displayUnit = displayUnits[i];
			if ((object)displayUnit != null && displayUnit.HasAnimationPlaying())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasIdleDialogTrigger()
	{
		for (int i = 0; i < displayUnits.Count; i++)
		{
			DisplayUnit displayUnit = displayUnits[i];
			if ((object)displayUnit != null && displayUnit.hasIdleDialogTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHitDialogTrigger()
	{
		for (int i = 0; i < displayUnits.Count; i++)
		{
			DisplayUnit displayUnit = displayUnits[i];
			if ((object)displayUnit != null && displayUnit.hasHitDialogTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasDefeatDialogTrigger()
	{
		for (int i = 0; i < displayUnits.Count; i++)
		{
			DisplayUnit displayUnit = displayUnits[i];
			if ((object)displayUnit != null && displayUnit.hasDefeatDialogTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public void SetOutlineAlpha(float value, float time)
	{
		foreach (DisplayUnit displayUnit in displayUnits)
		{
			displayUnit.SetOutlineAlpha(value, time);
		}
	}

	private void ShowUnit(DisplayUnit displayUnit)
	{
		displayUnit.gameObject.SetActive(value: true);
		DisplayAnimator animator = displayUnit.animator;
		if (!(animator == null))
		{
			Transform transform = displayPivots.Find((Transform t) => !usedTransform.ContainsValue(t));
			usedTransform.Add(displayUnit, transform);
			displayUnit.transform.position = transform.position;
			if (animator.PlaySpineAnimation(EnemyAnimationType.debut) == null)
			{
				animator.PlaySpineAnimation(EnemyAnimationType.idle);
			}
		}
	}

	private void HideUnit(DisplayUnit displayUnit, ConsumeItemAction.ConsumeType type)
	{
		DisplayAnimator animator = displayUnit.animator;
		if (!(animator == null))
		{
			if (type != ConsumeItemAction.ConsumeType.Damage)
			{
				animator.PlaySpineAnimation(EnemyAnimationType.leave);
			}
			else
			{
				animator.PlaySpineAnimation(EnemyAnimationType.death);
			}
			usedTransform.Remove(displayUnit);
			GameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				animator.gameObject.SetActive(value: false);
			}));
		}
	}

	private DisplayUnit GetDisplayUnit(Item item)
	{
		if (item == null)
		{
			return null;
		}
		int num = item.blackboard.GetValue(ItemBlackBoard.ValueType.DisplayUnitIndex) ?? (-1);
		if (num >= 0 && num < displayUnits.Count)
		{
			return displayUnits[num];
		}
		return null;
	}

	private void HandleGameEvent(GameEvent eve)
	{
		switch (eve.type)
		{
		case GameEvent.Type.AddItem:
			_OnAddItem(eve.TargetItem);
			break;
		case GameEvent.Type.AfterConsume:
			_OnAfterConsumeItem(eve.TargetItem, eve.consumeType);
			break;
		case GameEvent.Type.AfterDamage:
			_OnAfterDamage(eve.TargetItem);
			break;
		}
	}

	private void _OnAddItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		ItemContainer owner = item.owner;
		if (owner == null || owner.type != ItemContainer.ContainerType.Table)
		{
			return;
		}
		BattleFieldTransform ownerBft = item.OwnerBft;
		if (ownerBft == null || ownerBft.isEnemy)
		{
			DisplayUnit displayUnit = GetDisplayUnit(item);
			if (!(displayUnit == null))
			{
				ShowUnit(displayUnit);
			}
		}
	}

	private void _OnAfterConsumeItem(Item item, ConsumeItemAction.ConsumeType type)
	{
		if (item == null)
		{
			return;
		}
		ItemContainer owner = item.owner;
		if (owner == null || owner.type != ItemContainer.ContainerType.WastePile)
		{
			return;
		}
		BattleFieldTransform ownerBft = item.OwnerBft;
		if (ownerBft == null || ownerBft.isEnemy)
		{
			DisplayUnit displayUnit = GetDisplayUnit(item);
			if (!(displayUnit == null))
			{
				HideUnit(displayUnit, type);
			}
		}
	}

	private void _OnAfterDamage(Item item)
	{
		if (item == null)
		{
			return;
		}
		ItemContainer owner = item.owner;
		if (owner == null || owner.type != ItemContainer.ContainerType.Table)
		{
			return;
		}
		BattleFieldTransform ownerBft = item.OwnerBft;
		if (ownerBft != null && !ownerBft.isEnemy)
		{
			return;
		}
		DisplayUnit displayUnit = GetDisplayUnit(item);
		if (!(displayUnit == null))
		{
			DisplayAnimator animator = displayUnit.animator;
			if (!(animator == null))
			{
				animator.PlaySpineAnimation(EnemyAnimationType.hit);
			}
		}
	}
}
