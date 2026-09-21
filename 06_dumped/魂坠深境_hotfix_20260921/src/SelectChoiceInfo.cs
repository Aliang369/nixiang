using System;
using System.Collections.Generic;

public class SelectChoiceInfo
{
	public class SelectChoice
	{
		[LocalizeText("多选项框描述文本")]
		public string choiceText;

		public TextDataGenerator generator;

		public Action onSelect;

		public Func<bool> selectable;
	}

	public List<SelectChoice> choices;

	public TextDataGenerator generator;

	public Action onConfirm;

	[LocalizeText("多选项框确认文本")]
	public string panelHint;

	public BattleFieldTransform selectorBft;
}
