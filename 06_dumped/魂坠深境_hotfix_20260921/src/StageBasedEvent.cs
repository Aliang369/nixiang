using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEngine;

public class StageBasedEvent : WorldEvent
{
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

		public void Execute(StageBasedEvent owner, Action onExecute)
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

	[OdinSerialize]
	public Stage rootStage = new Stage();

	[NonSerialized]
	[HideInInspector]
	public GameEvent tempGameEvent;

	protected override void Init()
	{
		base.Init();
		tempGameEvent = new GameEvent(GameEvent.Type.WorldEventChoose, GameEvent.TriggerType.Real);
		tempGameEvent.gameState = gameState;
		tempGameEvent.SetCaster(this);
		tempGameEvent.TargetWorldEvent = this;
		ShowStage(rootStage);
	}

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

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (rootStage != null)
		{
			list.Add(rootStage);
		}
		return list;
	}
}
