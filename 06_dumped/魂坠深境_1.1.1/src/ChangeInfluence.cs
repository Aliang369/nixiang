using System.Collections.Generic;
using System.Linq;

public class ChangeInfluence : Effect
{
	public enum ChangeType
	{
		Add,
		Remove
	}

	public InfluenceDefGenerator influenceDefGenerator;

	public bool showMultiPanel;

	public bool preserveWillPowerCostOnRemove;

	public ChangeType type;

	public GameEvent.EventBFTType bftType = GameEvent.EventBFTType.Player;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<InfluenceDef> list = influenceDefGenerator?.GenerateInfluenceDefs(eve);
		BattleFieldTransform eventBFT = eve.GetEventBFT(bftType);
		if (list == null || eventBFT == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (InfluenceDef def in list)
		{
			switch (type)
			{
			case ChangeType.Add:
				if (eventBFT.isPlayer && showMultiPanel)
				{
					eve.gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
					{
						influences = new List<Influence>
						{
							new Influence(def)
						}
					}));
				}
				else
				{
					Effect.EnqueueAction(eve, new AddInfluenceAction(new Influence(def), eventBFT));
				}
				break;
			case ChangeType.Remove:
			{
				Influence influence = eventBFT.influences.Find((Influence i) => i.def == def);
				if (influence != null)
				{
					RemoveInfluenceAction action = new RemoveInfluenceAction(influence, preserveWillPowerCostOnRemove);
					Effect.EnqueueAction(eve, action);
				}
				break;
			}
			}
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		List<Influence> value = influenceDefGenerator?.GetNodeDisplayInfo(eve).refInfluences?.value.ToList();
		nodeDisplayInfo.refInfluences = new NodeDisplayInfo.Element<List<Influence>>(value);
		return nodeDisplayInfo;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (influenceDefGenerator != null)
		{
			list.Add(influenceDefGenerator);
		}
		return list;
	}
}
