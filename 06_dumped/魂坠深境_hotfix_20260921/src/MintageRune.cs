using System;
using System.Collections.Generic;

public class MintageRune : Effect
{
	public class MintageRuneInfo
	{
		public RuneDef runeDef;

		public ValueGenerator deltaValueGenerator;

		public Rune GenerateRune(GameEvent eve)
		{
			Rune rune = new Rune(runeDef);
			int value = deltaValueGenerator?.GenerateValue(eve) ?? rune.value;
			rune.value = value;
			return rune;
		}
	}

	public RuneGenerator runeGenerator;

	public Effect afterMintage;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return NodeCastResult.Failure;
		}
		MintageRunePanel_Select.SetupInfo setupInfo = new MintageRunePanel_Select.SetupInfo(runeGenerator?.GenerateRunes(eve));
		setupInfo.canBack = (Func<bool>)Delegate.Combine(setupInfo.canBack, (Func<bool>)(() => eve.gameState.UIManager.mintageRunePanel_Select.GetRunes().Count == 0));
		setupInfo.onFinish = delegate
		{
			afterMintage?.Cast(eve);
		};
		eve.gameState.UIManager.mintageRunePanel_Select.Setup(setupInfo);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { runeGenerator, afterMintage };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		List<Rune> value = runeGenerator?.GenerateRunes(eve);
		nodeDisplayInfo.refRunes = new NodeDisplayInfo.Element<List<Rune>>(value);
		return nodeDisplayInfo;
	}
}
