using System.Collections.Generic;
using System.Linq;

public class NodeBasedWorldEvent : WorldEvent
{
	public class WorldEventChoiceSetup : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		public WorldEventChoiceText choiceText;

		public CheckNode choosable;

		public Effect chooseEffect;

		public CheckNode presenceCheck;

		public bool showTextReference = true;

		public bool disableAutoAfterChoose;

		public bool finishAfterChoose;

		public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { presenceCheck, choosable, chooseEffect };
		}
	}

	public List<WorldEventChoiceSetup> setups;

	public bool stayRoomAfterFinish;

	protected override void Init()
	{
		base.Init();
		GameEvent eve = GenerateGameEvent();
		foreach (WorldEventChoiceSetup setup in setups.OrEmptyIfNull())
		{
			if (setup.presenceCheck != null && setup.presenceCheck.Check(eve) == false)
			{
				continue;
			}
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.buttonText = setup.choiceText.choiceName;
			worldEventChoice.descriptionText = TextDataPreset.FormatEffectObjectDescription(this, setup.choiceText.choiceDescription, eve, out var generator);
			worldEventChoice.executeAbleChecker = () => setup.choosable == null || setup.choosable.Check(eve) == true;
			worldEventChoice.executeAction = delegate
			{
				eve.worldEventChoiceSetup = setup;
				setup.chooseEffect?.Cast(eve);
				if (!setup.disableAutoAfterChoose && !setup.finishAfterChoose)
				{
					RefreshAfterChoose(setup.choiceText.chosenDescription);
				}
				if (setup.finishAfterChoose)
				{
					FinishedWorldEvent(!stayRoomAfterFinish);
				}
			};
			if (setup.showTextReference)
			{
				foreach (Item item in generator.refLinkObjects.OfType<Item>())
				{
					worldEventChoice.AddDisplayItem(item);
				}
				foreach (ItemDefinition item2 in generator.refLinkObjects.OfType<ItemDefinition>())
				{
					worldEventChoice.AddDisplayItem(new Item(item2));
				}
				foreach (GameSkill item3 in generator.refLinkObjects.OfType<GameSkill>())
				{
					worldEventChoice.AddDisplaySkill(item3);
				}
				foreach (Rune item4 in generator.refLinkObjects.OfType<Rune>())
				{
					worldEventChoice.AddDisplayRune(item4);
				}
				foreach (Enchant item5 in generator.refLinkObjects.OfType<Enchant>())
				{
					worldEventChoice.AddDisplayEnchant(item5);
				}
				foreach (Influence item6 in generator.refLinkObjects.OfType<Influence>())
				{
					worldEventChoice.AddDisplayInfluence(item6);
				}
			}
			currentDisplayInfo.AddChoice(worldEventChoice);
		}
	}

	private GameEvent GenerateGameEvent()
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.WorldEventChoose, GameEvent.TriggerType.Real);
		gameEvent.gameState = gameState;
		gameEvent.SetCaster(this);
		gameEvent.TargetWorldEvent = this;
		return gameEvent;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		foreach (WorldEventChoiceSetup item in setups.OrEmptyIfNull())
		{
			list.Add(item);
		}
		return list;
	}
}
