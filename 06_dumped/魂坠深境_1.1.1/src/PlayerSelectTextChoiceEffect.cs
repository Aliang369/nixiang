using System.Collections.Generic;

public class PlayerSelectTextChoiceEffect : Effect
{
	public class PlayerSelectTextChoiceSetup
	{
		public Effect effect;

		public CheckNode selectCheck;

		[LocalizeText("多选一的选择选项描述")]
		public string text;
	}

	public GameEvent.EventBFTType bftType;

	public List<PlayerSelectTextChoiceSetup> setups;

	[LocalizeText("选择选项标题描述")]
	public string titleText;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		SelectChoiceInfo info = GenerateSelectChoiceInfo(eve);
		Effect.EnqueueAction(eve, new SelectChoiceAction(info));
		return NodeCastResult.Success;
	}

	private SelectChoiceInfo GenerateSelectChoiceInfo(GameEvent eve)
	{
		GameEvent seve = eve.ShallowCopy();
		SelectChoiceInfo selectChoiceInfo = new SelectChoiceInfo();
		IEffectObjectDisplay effectObjectDisplay = eve.GetEveObj(GameEvent.EveObjType.Caster) as IEffectObjectDisplay;
		selectChoiceInfo.panelHint = ((effectObjectDisplay != null) ? TextDataPreset.FormatEffectObjectDescription(effectObjectDisplay, titleText, eve, out selectChoiceInfo.generator) : titleText);
		selectChoiceInfo.choices = new List<SelectChoiceInfo.SelectChoice>();
		foreach (PlayerSelectTextChoiceSetup setup in setups.OrEmptyIfNull())
		{
			SelectChoiceInfo.SelectChoice selectChoice = new SelectChoiceInfo.SelectChoice();
			selectChoice.choiceText = ((effectObjectDisplay != null) ? TextDataPreset.FormatEffectObjectDescription(effectObjectDisplay, setup.text, eve, out selectChoice.generator) : setup.text);
			selectChoice.selectable = () => setup.selectCheck == null || setup.selectCheck.Check(eve) == true;
			selectChoice.onSelect = delegate
			{
				setup.effect.Cast(seve);
			};
			selectChoiceInfo.choices.Add(selectChoice);
		}
		selectChoiceInfo.selectorBft = eve.GetEventBFT(bftType);
		return selectChoiceInfo;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		foreach (PlayerSelectTextChoiceSetup item in setups.OrEmptyIfNull())
		{
			list.Add(item.effect);
			list.Add(item.selectCheck);
		}
		return list;
	}
}
