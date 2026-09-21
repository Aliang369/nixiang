using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSpell : IChildren<IEffectNode>
{
	private const string ThrowItemFullId = "#IT#000477#";

	private const int ThrowItemSoIndex = 477;

	private const string ThrowItemName = "投掷";

	private const string ScreenFlyNfxName = "Fly_3";

	private const float ScreenFlyDuration = 180f;

	[SerializeField]
	public ItemRequirement _requirement = new ItemRequirement();

	[SerializeField]
	public ItemSpellCostMode costMode;

	[SerializeField]
	public bool burnCost;

	[SerializeField]
	public bool allowCastWithoutEnoughCost;

	[SerializeField]
	public int cooldown = 1;

	[SerializeField]
	public SpellNFX3DConfig spellEffect3DConfig;

	[SerializeField]
	public SpellNFXConfig spellEffectConfig;

	[SerializeField]
	public Effect spellNode;

	[SerializeField]
	public Effect costNode;

	public ItemRequirement requirement => _requirement;

	public bool ConsumesAllAvailableCost => costMode == ItemSpellCostMode.AllAvailable;

	public bool MovesHeadToCasterRightAsCost => costMode == ItemSpellCostMode.MoveHeadToCasterRight;

	public IntentType IntentType
	{
		get
		{
			List<DealDamage> source = spellNode.GetDescendantNodeOfType<DealDamage>(includeSelf: true).FindAll((DealDamage da) => da.targetBFT != GameEvent.EventBFTType.Owner);
			List<IAttackIntentNode> descendantNodeOfType = spellNode.GetDescendantNodeOfType<IAttackIntentNode>(includeSelf: true);
			List<SpawnItems> source2 = spellNode.GetDescendantNodeOfType<SpawnItems>(includeSelf: true).FindAll((SpawnItems ad) => ad.targetBFT == GameEvent.EventBFTType.Opponent && ad.targetContainerType == GameEvent.ContainerType.TableContainer);
			List<Heal> descendantNodeOfType2 = spellNode.GetDescendantNodeOfType<Heal>(includeSelf: true);
			if (!source.IsNullOrEmpty() || !descendantNodeOfType.IsNullOrEmpty())
			{
				return IntentType.Attack;
			}
			if (!descendantNodeOfType2.IsNullOrEmpty())
			{
				return IntentType.Heal;
			}
			if (!source2.IsNullOrEmpty())
			{
				return IntentType.AddItem;
			}
			return IntentType.Special;
		}
	}

	public virtual List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (requirement?.targetSelector != null)
		{
			list.Add(requirement.targetSelector);
		}
		if (requirement?.castCheckNode != null)
		{
			list.Add(requirement.castCheckNode);
		}
		if (requirement?.targetCheckNode != null)
		{
			list.Add(requirement.targetCheckNode);
		}
		if (requirement?.targetRequirement != null)
		{
			list.Add(requirement.targetRequirement);
		}
		if (spellNode != null)
		{
			list.Add(spellNode);
		}
		if (costNode != null)
		{
			list.Add(costNode);
		}
		return list;
	}

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public int GetCoolDown(Item mItem)
	{
		if (mItem == null)
		{
			return cooldown;
		}
		int num;
		if (mItem.upgraded)
		{
			ItemDefinition def = mItem.def;
			if ((object)def != null && def.SOindex == 1416)
			{
				num = -1;
				goto IL_0031;
			}
		}
		num = 0;
		goto IL_0031;
		IL_0031:
		int num2 = num;
		return Mathf.Max(cooldown + (int)mItem.deltaCD + num2, 0);
	}

	public NodeDisplayInfo GetSpellIntentEffectDisplayInfo(GameEvent eve)
	{
		List<IEffectNode> descendantNodes = spellNode.GetDescendantNodes(includeself: true);
		switch (IntentType)
		{
		case IntentType.None:
			return null;
		case IntentType.Attack:
		{
			object obj = descendantNodes.Find((IEffectNode t) => t is IAttackIntentNode)?.GetNodeDisplayInfo(eve);
			if (obj == null)
			{
				IEffectNode effectNode = descendantNodes.Find((IEffectNode t) => t.GetType() == typeof(DealDamage));
				if (effectNode == null)
				{
					return null;
				}
				obj = effectNode.GetNodeDisplayInfo(eve);
			}
			return (NodeDisplayInfo)obj;
		}
		case IntentType.Defend:
			return descendantNodes.Find((IEffectNode t) => t.GetType() == typeof(SpawnItems))?.GetNodeDisplayInfo(eve);
		case IntentType.Special:
			if (descendantNodes == null || descendantNodes.Count == 0)
			{
				return null;
			}
			return descendantNodes.First().GetNodeDisplayInfo(eve);
		case IntentType.Heal:
			return descendantNodes.Find((IEffectNode t) => t.GetType() == typeof(Heal))?.GetNodeDisplayInfo(eve);
		case IntentType.AddItem:
			return descendantNodes.Find((IEffectNode t) => t.GetType() == typeof(SpawnItems))?.GetNodeDisplayInfo(eve);
		default:
			return null;
		}
	}

	public void Cast(GameEvent eve)
	{
		spellEffectConfig?.Play(eve);
		spellEffect3DConfig?.Play(eve);
		TryEnqueueEnemyThrowScreenFlyVfx(eve);
		spellNode?.Cast(eve);
	}

	private static void TryEnqueueEnemyThrowScreenFlyVfx(GameEvent eve)
	{
		if (eve == null || eve.triggerType != GameEvent.TriggerType.Real)
		{
			return;
		}
		Item casterItem = eve.CasterItem;
		if (!IsEnemyThrow(casterItem))
		{
			return;
		}
		GameStateInfo gameState = eve.gameState;
		if (gameState != null)
		{
			PivotManager.IWithPivot playerHeartUIPivot = GetPlayerHeartUIPivot(gameState);
			NFXSetup nFXSetup = gameState.applicationState?.assetCatalog?.nfxCatalog?.ValidAssets?.FirstOrDefault((NFXSetup nfx) => nfx != null && nfx.name == "Fly_3");
			if (playerHeartUIPivot != null && !(nFXSetup == null))
			{
				gameState.actionManager.EnqueueAction(new DisplayPersistentNFXAction(nFXSetup, playerHeartUIPivot, 180f, casterItem.OwnerBft));
			}
		}
	}

	private static PivotManager.IWithPivot GetPlayerHeartUIPivot(GameStateInfo gameState)
	{
		return gameState?.UIManager?.playerBftUIDisplay?.heartPivot;
	}

	private static bool IsEnemyThrow(Item item)
	{
		ItemDefinition itemDefinition = item?.GetDef();
		if (!(itemDefinition == null))
		{
			BattleFieldTransform ownerBft = item.OwnerBft;
			if (ownerBft != null && ownerBft.isEnemy)
			{
				if (itemDefinition.SOindex != 477 && !(itemDefinition.FullID == "#IT#000477#"))
				{
					return item.ItemName == "投掷";
				}
				return true;
			}
		}
		return false;
	}
}
