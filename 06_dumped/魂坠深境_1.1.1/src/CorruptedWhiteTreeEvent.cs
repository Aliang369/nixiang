using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using Sirenix.Serialization;
using UnityEngine;

public class CorruptedWhiteTreeEvent : WorldEvent
{
	public abstract class TreeEventOption : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		[LocalizeText("事件选项名称")]
		public string choiceName;

		[LocalizeText("事件选项描述")]
		public string choiceDescription;

		[LocalizeText("事件选项后描述")]
		[TextArea(2, 5)]
		public string chosenDescription;

		public CheckNode choosable;

		public CheckNode presenceCheck;

		public bool showTextReference = true;

		public bool hasNextStage;

		[OdinSerialize]
		public Stage nextStage = new Stage();

		public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public abstract void Execute(CorruptedWhiteTreeEvent owner, Action onExecute);

		public virtual List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			if (presenceCheck != null)
			{
				list.Add(presenceCheck);
			}
			if (choosable != null)
			{
				list.Add(choosable);
			}
			if (hasNextStage && nextStage != null)
			{
				list.Add(nextStage);
			}
			return list;
		}
	}

	public class TreeEffectOption : TreeEventOption
	{
		public Effect effect;

		public override void Execute(CorruptedWhiteTreeEvent owner, Action onExecute)
		{
			effect?.Cast(owner.tempGameEvent);
			onExecute?.Invoke();
		}

		public override List<IEffectNode> GetChildren()
		{
			List<IEffectNode> children = base.GetChildren();
			if (effect != null)
			{
				children.Add(effect);
			}
			return children;
		}
	}

	public class Stage : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		[LocalizeText("事件描述")]
		[TextArea(3, 10)]
		public string description;

		[OdinSerialize]
		public List<StageOption> options = new List<StageOption>();

		public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			if (options != null)
			{
				foreach (StageOption option in options)
				{
					if (option != null)
					{
						list.Add(option);
					}
				}
			}
			return list;
		}
	}

	public class StageOption : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		[LocalizeText("事件选项名称")]
		public string choiceName;

		[LocalizeText("事件选项描述")]
		public string choiceDescription;

		[LocalizeText("事件选项后描述")]
		[TextArea(2, 5)]
		public string chosenDescription;

		public CheckNode presenceCheck;

		public CheckNode choosable;

		public bool showTextReference = true;

		public Effect effect;

		public bool hasNextStage;

		[OdinSerialize]
		public Stage nextStage = new Stage();

		public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public void Execute(CorruptedWhiteTreeEvent owner, Action onExecute)
		{
			if (effect != null)
			{
				effect.Cast(owner.tempGameEvent);
			}
			if (hasNextStage && nextStage != null)
			{
				owner.ShowStage(nextStage);
			}
			else if (!string.IsNullOrEmpty(chosenDescription))
			{
				owner.EnqueueAction(new ActionAction(delegate
				{
					owner.RefreshAfterChoose(chosenDescription);
				}));
			}
			else
			{
				owner.FinishedWorldEvent();
			}
			onExecute?.Invoke();
		}

		public List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			if (presenceCheck != null)
			{
				list.Add(presenceCheck);
			}
			if (choosable != null)
			{
				list.Add(choosable);
			}
			if (effect != null)
			{
				list.Add(effect);
			}
			if (hasNextStage && nextStage != null)
			{
				list.Add(nextStage);
			}
			return list;
		}
	}

	[Serializable]
	public class TreeOfferingPool
	{
		public int satisfactionValue;

		[OdinSerialize]
		public List<TreeEventOption> options = new List<TreeEventOption>();
	}

	[Serializable]
	public class TreeRewardTier
	{
		public int requiredSatisfaction;

		[OdinSerialize]
		public List<TreeEventOption> rewardOptions = new List<TreeEventOption>();
	}

	public class TreeStage
	{
		[LocalizeText("事件描述")]
		public List<string> roundDescriptions = new List<string>();

		[LocalizeText("事件描述")]
		public string fallbackRoundDescription = "白树的阴影笼罩着你……";

		[LocalizeText("事件描述")]
		public string rewardStageDescription = "The tree seems satisfied.";

		[OdinSerialize]
		public List<TreeOfferingPool> offeringPools = new List<TreeOfferingPool>();

		[LocalizeText("选项名称")]
		public List<string> costOptionTexts = new List<string>();

		[OdinSerialize]
		public List<TreeEventOption> leaveOptions = new List<TreeEventOption>();

		[OdinSerialize]
		public List<TreeRewardTier> rewardTiers = new List<TreeRewardTier>();

		[NonSerialized]
		[HideInInspector]
		private int currentRoundIndex;

		[NonSerialized]
		[HideInInspector]
		private int currentSatisfaction;

		public void Init(CorruptedWhiteTreeEvent owner)
		{
			currentRoundIndex = 0;
			currentSatisfaction = 0;
			ShowRound(owner, 0);
		}

		private void ShowRound(CorruptedWhiteTreeEvent owner, int roundIndex)
		{
			int num = roundDescriptions?.Count ?? 0;
			if (roundIndex >= num)
			{
				ShowRewardStage(owner, currentSatisfaction);
				return;
			}
			currentRoundIndex = roundIndex;
			if (roundDescriptions != null && roundIndex < roundDescriptions.Count)
			{
				owner.currentDisplayInfo.description = roundDescriptions[roundIndex];
			}
			else
			{
				owner.currentDisplayInfo.description = fallbackRoundDescription;
			}
			owner.currentDisplayInfo.ClearChoice();
			if (offeringPools != null && offeringPools.Count > 0)
			{
				string prefix = ((costOptionTexts != null && roundIndex < costOptionTexts.Count) ? costOptionTexts[roundIndex] : "");
				foreach (TreeOfferingPool offeringPool in offeringPools)
				{
					AddRandomCostOption(owner, offeringPool, roundIndex, prefix);
				}
			}
			if (leaveOptions != null && roundIndex < leaveOptions.Count)
			{
				TreeEventOption leaveOpt = leaveOptions[roundIndex];
				AddEventOption(owner, leaveOpt, delegate
				{
					ResolveOption(owner, leaveOpt);
				});
			}
			owner.SetPanelDirty();
		}

		private void AddRandomCostOption(CorruptedWhiteTreeEvent owner, TreeOfferingPool offeringPool, int roundIndex, string prefix)
		{
			if (offeringPool?.options != null && offeringPool.options.Count != 0)
			{
				TreeEventOption treeEventOption = offeringPool.options.SelectRandom(owner.tempGameEvent.gameState.randomManager.NextDouble);
				AddEventOption(owner, treeEventOption, delegate
				{
					currentSatisfaction += offeringPool.satisfactionValue;
					ShowRound(owner, roundIndex + 1);
				}, prefix + treeEventOption.choiceName);
			}
		}

		private void ShowRewardStage(CorruptedWhiteTreeEvent owner, int satisfaction)
		{
			owner.currentDisplayInfo.description = rewardStageDescription;
			owner.currentDisplayInfo.ClearChoice();
			TreeRewardTier rewardTier = GetRewardTier(satisfaction);
			if (rewardTier?.rewardOptions != null)
			{
				foreach (TreeEventOption reward in rewardTier.rewardOptions)
				{
					AddEventOption(owner, reward, delegate
					{
						ResolveOption(owner, reward);
					});
				}
			}
			owner.SetPanelDirty();
		}

		private TreeRewardTier GetRewardTier(int satisfaction)
		{
			return rewardTiers?.Where((TreeRewardTier tier) => tier != null && tier.requiredSatisfaction <= satisfaction).OrderByDescending((TreeRewardTier tier) => tier.requiredSatisfaction).FirstOrDefault();
		}

		private static void ResolveOption(CorruptedWhiteTreeEvent owner, TreeEventOption option)
		{
			if (option.hasNextStage && option.nextStage != null)
			{
				owner.ShowStage(option.nextStage);
			}
			else if (!string.IsNullOrEmpty(option.chosenDescription))
			{
				owner.EnqueueAction(new ActionAction(delegate
				{
					owner.RefreshAfterChoose(option.chosenDescription);
				}));
			}
			else
			{
				owner.FinishedWorldEvent();
			}
		}

		private void AddEventOption(CorruptedWhiteTreeEvent owner, TreeEventOption opt, Action onExecute, string overrideName = null)
		{
			if (opt == null || (opt.presenceCheck != null && opt.presenceCheck.Check(owner.tempGameEvent) == false))
			{
				return;
			}
			string text = overrideName ?? opt.choiceName;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.buttonText = text;
			string rawDescription = opt.choiceDescription ?? "";
			worldEventChoice.descriptionText = TextDataPreset.FormatEffectObjectDescription(owner, rawDescription, owner.tempGameEvent, out var generator);
			worldEventChoice.executeAbleChecker = () => opt.choosable == null || opt.choosable.Check(owner.tempGameEvent) == true;
			if (opt.showTextReference && generator != null)
			{
				foreach (Item item in generator.refLinkObjects.OfType<Item>())
				{
					worldEventChoice.AddDisplayItem(item);
				}
				foreach (GameSkill item2 in generator.refLinkObjects.OfType<GameSkill>())
				{
					worldEventChoice.AddDisplaySkill(item2);
				}
				foreach (Rune item3 in generator.refLinkObjects.OfType<Rune>())
				{
					worldEventChoice.AddDisplayRune(item3);
				}
				foreach (Enchant item4 in generator.refLinkObjects.OfType<Enchant>())
				{
					worldEventChoice.AddDisplayEnchant(item4);
				}
				foreach (Influence item5 in generator.refLinkObjects.OfType<Influence>())
				{
					worldEventChoice.AddDisplayInfluence(item5);
				}
			}
			worldEventChoice.executeAction = delegate
			{
				opt.Execute(owner, onExecute);
			};
			owner.currentDisplayInfo.AddChoice(worldEventChoice);
		}

		public List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			if (offeringPools != null)
			{
				foreach (TreeOfferingPool offeringPool in offeringPools)
				{
					if (offeringPool?.options == null)
					{
						continue;
					}
					foreach (TreeEventOption option in offeringPool.options)
					{
						if (option != null)
						{
							list.Add(option);
						}
					}
				}
			}
			if (leaveOptions != null)
			{
				foreach (TreeEventOption leaveOption in leaveOptions)
				{
					if (leaveOption != null)
					{
						list.Add(leaveOption);
					}
				}
			}
			if (rewardTiers != null)
			{
				foreach (TreeRewardTier rewardTier in rewardTiers)
				{
					if (rewardTier?.rewardOptions == null)
					{
						continue;
					}
					foreach (TreeEventOption rewardOption in rewardTier.rewardOptions)
					{
						if (rewardOption != null)
						{
							list.Add(rewardOption);
						}
					}
				}
			}
			return list;
		}
	}

	[OdinSerialize]
	public TreeStage treeStage = new TreeStage();

	[NonSerialized]
	[HideInInspector]
	public GameEvent tempGameEvent;

	public void ShowStage(Stage stage)
	{
		if (stage == null)
		{
			FinishedWorldEvent();
			return;
		}
		currentDisplayInfo.description = stage.description;
		currentDisplayInfo.ClearChoice();
		if (stage.options != null)
		{
			foreach (StageOption option in stage.options)
			{
				AddEventOption(option);
			}
		}
		SetPanelDirty();
	}

	private void AddEventOption(StageOption opt)
	{
		if (opt == null || (opt.presenceCheck != null && opt.presenceCheck.Check(tempGameEvent) == false))
		{
			return;
		}
		string choiceName = opt.choiceName;
		if (string.IsNullOrEmpty(choiceName))
		{
			return;
		}
		WorldEventChoice worldEventChoice = new WorldEventChoice();
		worldEventChoice.buttonText = choiceName;
		string rawDescription = opt.choiceDescription ?? "";
		worldEventChoice.descriptionText = TextDataPreset.FormatEffectObjectDescription(this, rawDescription, tempGameEvent, out var generator);
		worldEventChoice.executeAbleChecker = () => opt.choosable == null || opt.choosable.Check(tempGameEvent) == true;
		if (opt.showTextReference && generator != null)
		{
			foreach (Item item in generator.refLinkObjects.OfType<Item>())
			{
				worldEventChoice.AddDisplayItem(item);
			}
			foreach (GameSkill item2 in generator.refLinkObjects.OfType<GameSkill>())
			{
				worldEventChoice.AddDisplaySkill(item2);
			}
			foreach (Rune item3 in generator.refLinkObjects.OfType<Rune>())
			{
				worldEventChoice.AddDisplayRune(item3);
			}
			foreach (Enchant item4 in generator.refLinkObjects.OfType<Enchant>())
			{
				worldEventChoice.AddDisplayEnchant(item4);
			}
			foreach (Influence item5 in generator.refLinkObjects.OfType<Influence>())
			{
				worldEventChoice.AddDisplayInfluence(item5);
			}
		}
		worldEventChoice.executeAction = delegate
		{
			opt.Execute(this, null);
		};
		currentDisplayInfo.AddChoice(worldEventChoice);
	}

	protected override void Init()
	{
		base.Init();
		tempGameEvent = new GameEvent(GameEvent.Type.WorldEventChoose, GameEvent.TriggerType.Real);
		tempGameEvent.gameState = gameState;
		tempGameEvent.SetCaster(this);
		tempGameEvent.TargetWorldEvent = this;
		treeStage.Init(this);
	}

	public override List<IEffectNode> GetChildren()
	{
		return treeStage.GetChildren();
	}
}
