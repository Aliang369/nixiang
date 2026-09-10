using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber
{
	public enum AbilityAnimPriority
	{
		None,
		Weak,
		Medium,
		Strong
	}

	public enum AbilityAnimType
	{
		None,
		Positive,
		Negative
	}

	[Flags]
	public enum TriggerOccasion
	{
		None = 0,
		InTableItem = 1,
		InDrawPileItem = 2,
		InWastePileItem = 4,
		InDeckItem = 8,
		OutContainerItem = 0x10,
		VisibleNotItem = 0x20,
		InCompanion = 0x40,
		InActivatedSkill = 0x80,
		GlobalObserve = 0x100,
		Always = 0x40000000,
		InVisibleBattle = InTableItem | VisibleNotItem | InCompanion | InActivatedSkill | GlobalObserve,
		InRealBattle = InVisibleBattle | InDrawPileItem | InWastePileItem | OutContainerItem,
		All = -1
	}

	public AbilityAnimType abilityAnimType;

	public AbilityAnimPriority animPriority = AbilityAnimPriority.Weak;

	public TriggerOccasion occasion = TriggerOccasion.InVisibleBattle;

	[SerializeField]
	protected SpellNFX3DConfig spellEffect3DConfig;

	[SerializeField]
	protected SpellNFXConfig spellEffectConfig;

	public CheckNode triggerCheckNode;

	[NonSerialized]
	public bool isDebug;

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode };
	}

	public abstract IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType();

	public abstract void TriggerGameEvent(GameEvent eve);

	public virtual bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (GetAllSubscribeGameEventType().Contains(eve.type) && OccasionCheck(eve))
		{
			if (triggerCheckNode != null)
			{
				return triggerCheckNode.Check(eve) == true;
			}
			return true;
		}
		return false;
	}

	protected bool IsPlayerAnimAble(GameEvent eve)
	{
		if (animPriority != AbilityAnimPriority.None)
		{
			if (eve.IsResolveGameEvent())
			{
				if (eve.IsResolveGameEvent())
				{
					return eve.triggerType == GameEvent.TriggerType.Real;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected void TriggerEffect(GameEvent eve, Effect effect)
	{
		bool flag = eve.type != GameEvent.Type.AfterTriggerAbility;
		if (IsPlayerAnimAble(eve))
		{
			if (eve.CasterEnchant != null)
			{
				TriggerAnim(eve.gameState, eve.CasterEnchant, eve.CasterEnchant.attachItem);
				if (flag)
				{
					eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
					{
						eve.gameState.gameEventManager.TriggerAfterTriggerAbility(eve.CasterAttachObject as Item);
					}));
				}
			}
			else if (eve.CasterRune != null)
			{
				TriggerAnim(eve.gameState, eve.CasterRune, eve.CasterRune.attachItem);
			}
			else if (eve.CasterInfluence != null)
			{
				TriggerAnim(eve.gameState, eve.CasterInfluence);
			}
			else if (eve.CasterSkill != null)
			{
				TriggerAnim(eve.gameState, eve.CasterSkill, animPriority);
			}
			else if (eve.CasterSkillWord != null)
			{
				TriggerAnim(eve.gameState, eve.CasterSkillWord, animPriority);
			}
			else if (eve.CasterItem != null)
			{
				if (eve.type == GameEvent.Type.BeforeChangeStance)
				{
					TriggerStanceAnim(eve);
				}
				TriggerAnim(eve.gameState, eve.CasterItem);
				if (flag)
				{
					eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
					{
						eve.gameState.gameEventManager.TriggerAfterTriggerAbility(eve.CasterItem);
					}));
				}
			}
			else if (eve.CasterStance != null)
			{
				TriggerAnim(eve);
			}
		}
		spellEffectConfig?.Play(eve);
		spellEffect3DConfig?.Play(eve);
		effect?.Cast(eve);
		if (IsPlayerAnimAble(eve) && eve.CasterItem != null)
		{
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				TriggerAnimEnd(eve.gameState, eve.CasterItem);
			}));
		}
	}

	private bool OccasionCheck(GameEvent eve)
	{
		bool flag = false;
		IGameEventSubscriber gameEventSubscriber = eve.Caster as IGameEventSubscriber;
		Item item = eve.CasterItem ?? (eve.CasterAttachObject as Item);
		GameSkill gameSkill = eve.CasterSkill ?? (eve.CasterAttachObject as GameSkill);
		if (occasion.HasFlag(TriggerOccasion.Always))
		{
			return true;
		}
		if (occasion.HasFlag(TriggerOccasion.InTableItem))
		{
			flag |= item?.owner != null && item.owner.type == ItemContainer.ContainerType.Table;
		}
		if (occasion.HasFlag(TriggerOccasion.InDrawPileItem))
		{
			flag |= item?.owner != null && item.owner.type == ItemContainer.ContainerType.DrawPile;
		}
		if (occasion.HasFlag(TriggerOccasion.InWastePileItem))
		{
			flag |= item?.owner != null && item.owner.type == ItemContainer.ContainerType.WastePile;
		}
		if (occasion.HasFlag(TriggerOccasion.InDeckItem))
		{
			flag |= item?.owner != null && item.owner.type == ItemContainer.ContainerType.Deck;
		}
		if (occasion.HasFlag(TriggerOccasion.InCompanion))
		{
			flag |= item?.owner != null && item.owner.type == ItemContainer.ContainerType.Companion;
		}
		if (occasion.HasFlag(TriggerOccasion.OutContainerItem))
		{
			flag |= item != null && (item.owner == null || item.owner.type == ItemContainer.ContainerType.ExilePile);
		}
		if (occasion.HasFlag(TriggerOccasion.InActivatedSkill))
		{
			flag |= gameSkill != null && eve.gameState?.playerState?.ActivatedSkills != null && eve.gameState.playerState.ActivatedSkills.Contains(gameSkill);
		}
		if (occasion.HasFlag(TriggerOccasion.GlobalObserve))
		{
			flag |= gameEventSubscriber is Achievement || gameEventSubscriber is DifficultyInfo || gameEventSubscriber is Contract || gameEventSubscriber is EnvironmentInfluence || gameEventSubscriber is StartGameBuffDef || gameEventSubscriber is CharacterMastery;
		}
		if (occasion.HasFlag(TriggerOccasion.VisibleNotItem))
		{
			bool num = flag;
			int num2;
			if (!(gameEventSubscriber is Influence { OwnerBft: not null }) && (!(gameEventSubscriber is Stance stance) || stance != eve.gameState?.stanceManager.currentStance))
			{
				CharacterAI characterAI = gameEventSubscriber as CharacterAI;
				num2 = ((characterAI != null) ? 1 : 0);
			}
			else
			{
				num2 = 1;
			}
			flag = (byte)((num ? 1u : 0u) | (uint)num2) != 0;
		}
		return flag;
	}

	protected void TriggerAnim(GameStateInfo gameState, Item item)
	{
		if (item == null || animPriority == AbilityAnimPriority.None)
		{
			return;
		}
		ItemContainer.ContainerType? containerType = item.owner?.type;
		BattleFieldTransform ownerBft = item.OwnerBft;
		if (ownerBft != null && ownerBft.isPlayer)
		{
			if (containerType == ItemContainer.ContainerType.Table)
			{
				gameState.UIManager.playerItemArea.OnTriggerAbilityAnim(item, animPriority, abilityAnimType);
			}
			else if (animPriority == AbilityAnimPriority.Strong && (containerType == ItemContainer.ContainerType.DrawPile || containerType == ItemContainer.ContainerType.WastePile))
			{
				BattleItemInfoPanel.TriggerPlayerItemAbilityAnim(gameState, item);
			}
			else if (animPriority == AbilityAnimPriority.Strong && containerType == ItemContainer.ContainerType.Companion)
			{
				gameState.UIManager.playerBftUIDisplay.companionItemArea.OnItemSpell(item);
				gameState.UIManager.companionActionPanel.Setup(item.def);
			}
			return;
		}
		BattleFieldTransform ownerBft2 = item.OwnerBft;
		if (ownerBft2 != null && ownerBft2.isEnemy)
		{
			if (containerType == ItemContainer.ContainerType.Table)
			{
				gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnItemAbilityTrigger(item, animPriority);
			}
			else if (animPriority == AbilityAnimPriority.Strong && containerType == ItemContainer.ContainerType.Companion)
			{
				gameState.UIManager.enemyBftUIDisplay.companionItemArea.OnItemSpell(item);
				gameState.UIManager.companionActionPanel.Setup(item.def);
			}
		}
	}

	protected void TriggerStanceAnim(GameEvent eve)
	{
		GameStateInfo gameState = eve.gameState;
		Item eveObj = eve.GetEveObj<Item>(GameEvent.EveObjType.Caster);
		if (eveObj != null && animPriority != AbilityAnimPriority.None)
		{
			BattleFieldTransform ownerBft = eveObj.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer && eveObj.owner?.type == ItemContainer.ContainerType.Table)
			{
				gameState.UIManager.playerItemArea.TriggerStanceItemAnim(eveObj, animPriority, eve);
			}
		}
	}

	protected void TriggerAnim(GameStateInfo gameState, Influence influence)
	{
		List<InfluenceDisplayArea> current = InfluenceDisplayArea.current;
		if (current.Count == 0)
		{
			gameState.actionManager.actionQueueCleanEvent += DelayPlay;
			return;
		}
		for (int i = 0; i < current.Count; i++)
		{
			current[i].TriggerAbilityAnim(influence, animPriority);
		}
		void DelayPlay()
		{
			gameState.actionManager.actionQueueCleanEvent -= DelayPlay;
			TriggerAnim(gameState, influence);
		}
	}

	protected void TriggerAnim(GameStateInfo gameState, GameSkill skill, AbilityAnimPriority priority)
	{
		if (skill != null && priority != AbilityAnimPriority.None)
		{
			BattleFieldTransform ownerBft = skill.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.UIManager.playerBftUIDisplay?.skillPanel?.TriggerAbilityInfo(skill, priority);
			}
		}
	}

	protected void TriggerAnim(GameStateInfo gameState, SkillWord skillWord, AbilityAnimPriority priority)
	{
		if (skillWord != null && priority != AbilityAnimPriority.None)
		{
			BattleFieldTransform ownerBft = skillWord.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.UIManager.playerBftUIDisplay?.skillPanel?.TriggerAbilityInfo(skillWord, priority);
			}
		}
	}

	protected void TriggerAnim(GameEvent eve)
	{
		GameStateInfo gameState = eve.gameState;
		Stance eveObj = eve.GetEveObj<Stance>(GameEvent.EveObjType.Origin);
		Stance eveObj2 = eve.GetEveObj<Stance>(GameEvent.EveObjType.Destination);
		gameState.UIManager.stanceDisplay.ApplyStance(eveObj2);
		if ((eveObj != null || eveObj2 != null) && animPriority != AbilityAnimPriority.None)
		{
			if (gameState.UIManager.stanceDisplay != null)
			{
				gameState.UIManager.stanceDisplay.TriggerAbilityAnim(eveObj2, animPriority);
			}
			else
			{
				Debug.LogWarning("StanceDisplay is not initialized, cannot trigger stance ability animation.");
			}
		}
	}

	protected void TriggerAnim(GameStateInfo gameState, Enchant enchant, Item item)
	{
		if (enchant != null && item != null && animPriority != AbilityAnimPriority.None)
		{
			BattleFieldTransform ownerBft = item.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer && item.owner?.type == ItemContainer.ContainerType.Table)
			{
				gameState.UIManager.playerItemArea.TriggerEnchantAbilityAnim(item, animPriority, enchant);
			}
		}
	}

	protected void TriggerAnim(GameStateInfo gameState, Rune rune, Item item)
	{
		if (rune != null && item != null && animPriority != AbilityAnimPriority.None)
		{
			BattleFieldTransform ownerBft = item.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer && item.owner?.type == ItemContainer.ContainerType.Table)
			{
				gameState.UIManager.playerItemArea.TriggerRuneAbilityAnim(item, animPriority, rune);
			}
		}
	}

	protected void TriggerAnimEnd(GameStateInfo gameState, Item item)
	{
		if (item == null || animPriority == AbilityAnimPriority.None)
		{
			return;
		}
		ItemContainer.ContainerType? containerType = item.owner?.type;
		BattleFieldTransform ownerBft = item.OwnerBft;
		if (ownerBft != null && ownerBft.isPlayer)
		{
			gameState.UIManager.playerItemArea.OnTriggerAbilityEndAnim(item);
			return;
		}
		BattleFieldTransform ownerBft2 = item.OwnerBft;
		if (ownerBft2 != null && ownerBft2.isEnemy && containerType == ItemContainer.ContainerType.Table && animPriority == AbilityAnimPriority.Strong)
		{
			gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnItemAbilityTriggerEnd(item);
		}
	}

	protected void EnqueueAction(GameEvent eve, GameAction action)
	{
		eve.gameState.actionManager.EnqueueAction(action);
	}
}
