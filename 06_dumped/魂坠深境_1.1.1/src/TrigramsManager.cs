using System;

public class TrigramsManager : InGameManager
{
	public enum DurationType
	{
		Temp = 1,
		Constant,
		Turn
	}

	[Flags]
	public enum YinYangStateType
	{
		None = 0,
		Yin = 1,
		Yang = 2,
		ExtremeYin = 4,
		ExtremeYang = 8,
		Balance = 0x10,
		FullFill = 0x20,
		FullFillYang = 0x40,
		FullFillYin = 0x80,
		Empty = 0x100
	}

	public enum YinYangType
	{
		None,
		Yin,
		Yang
	}

	public class ConvertYinYangPowerAction : ExecuteAction
	{
		private readonly int amount;

		private readonly YinYangType type;

		private readonly object source;

		public ConvertYinYangPowerAction(int amount, YinYangType type, object source)
		{
			this.amount = amount;
			this.type = type;
			this.source = source;
		}

		protected override void Update()
		{
			ProtectedInt yangPower = gameState.trigramsManager.yangPower;
			ProtectedInt yinPower = gameState.trigramsManager.yinPower;
			switch (type)
			{
			case YinYangType.Yin:
				if ((int)yinPower < amount)
				{
					return;
				}
				yinPower = (int)yinPower - amount;
				yangPower = (int)yangPower + amount;
				break;
			case YinYangType.Yang:
				if ((int)yangPower < amount)
				{
					return;
				}
				yinPower = (int)yinPower + amount;
				yangPower = (int)yangPower - amount;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			gameState.trigramsManager.yangPower = yangPower;
			gameState.trigramsManager.yinPower = yinPower;
			gameState.gameEventManager.TriggerConvertYinYangPower(type, amount, source);
			gameState.UIManager.trigramsDisplay.OnConvertYinYang(amount, type);
		}
	}

	public class ChangYinYangPowerAction : ExecuteAction
	{
		private readonly DurationType durationType;

		private readonly int yang;

		private readonly int yin;

		private readonly object source;

		public ChangYinYangPowerAction(int yin, int yang, DurationType durationType, object source)
		{
			this.yin = yin;
			this.yang = yang;
			this.durationType = durationType;
			this.source = source;
		}

		protected override void Update()
		{
			if (yin == 0 && yang == 0)
			{
				return;
			}
			TrigramsManager trigramsManager = gameState.trigramsManager;
			if (yin < 0)
			{
				for (int i = 0; i < -yin; i++)
				{
					trigramsManager.ConsumePower(YinYangType.Yin);
				}
			}
			else
			{
				trigramsManager.yinPower = (int)trigramsManager.yinPower + yin;
				switch (durationType)
				{
				case DurationType.Temp:
					if (yin < 0)
					{
						trigramsManager.turnMaxYinPower += yin;
						if (trigramsManager.turnMaxYinPower < 0)
						{
							trigramsManager.turnMaxYinPower = 0;
						}
					}
					break;
				case DurationType.Constant:
					trigramsManager.constantMaxYinPower += yin;
					break;
				case DurationType.Turn:
					trigramsManager.turnMaxYinPower += yin;
					if (trigramsManager.turnMaxYinPower < 0)
					{
						trigramsManager.turnMaxYinPower = 0;
					}
					break;
				default:
					TraceLog.Error($"侠客阴阳DurationType错误{durationType}");
					break;
				}
			}
			if (yang < 0)
			{
				for (int j = 0; j < -yang; j++)
				{
					trigramsManager.ConsumePower(YinYangType.Yang);
				}
			}
			else
			{
				trigramsManager.yangPower = (int)trigramsManager.yangPower + yang;
				switch (durationType)
				{
				case DurationType.Temp:
					if (yang < 0)
					{
						trigramsManager.turnMaxYangPower += yang;
						if (trigramsManager.turnMaxYangPower < 0)
						{
							trigramsManager.turnMaxYangPower = 0;
						}
					}
					break;
				case DurationType.Constant:
					trigramsManager.constantMaxYangPower += yang;
					break;
				case DurationType.Turn:
					trigramsManager.turnMaxYangPower += yang;
					if (trigramsManager.turnMaxYangPower < 0)
					{
						trigramsManager.turnMaxYangPower = 0;
					}
					break;
				default:
					TraceLog.Error($"侠客阴阳DurationType错误{durationType}");
					break;
				}
			}
			if ((int)trigramsManager.yangPower < 0)
			{
				trigramsManager.yinPower = (int)trigramsManager.yinPower + (int)trigramsManager.yangPower;
				trigramsManager.yangPower = 0;
			}
			if ((int)trigramsManager.yinPower < 0)
			{
				trigramsManager.yangPower = (int)trigramsManager.yangPower + (int)trigramsManager.yinPower;
				trigramsManager.yinPower = 0;
			}
			gameState.UIManager.trigramsDisplay.OnChangYinYangPower(yin, yang);
			gameState.gameEventManager.TriggerChangeYinYangPower(source, yin, yang, durationType != DurationType.Constant);
		}
	}

	public class ConvertAllPowerToTurnAndAmplifyAction : ExecuteAction
	{
		private readonly int divisor;

		private readonly object source;

		public ConvertAllPowerToTurnAndAmplifyAction(int divisor, object source)
		{
			this.divisor = divisor;
			this.source = source;
		}

		protected override void Update()
		{
			TrigramsManager trigramsManager = gameState?.trigramsManager;
			if (trigramsManager != null && divisor > 0)
			{
				int num = (int)trigramsManager.yinPower / divisor;
				int num2 = (int)trigramsManager.yangPower / divisor;
				trigramsManager.yinPower = (int)trigramsManager.yinPower + num;
				trigramsManager.yangPower = (int)trigramsManager.yangPower + num2;
				trigramsManager.turnMaxYinPower = trigramsManager.yinPower;
				trigramsManager.turnMaxYangPower = trigramsManager.yangPower;
				gameState.UIManager?.trigramsDisplay?.OnConvertAllPowerToTurn();
				if (num != 0 || num2 != 0)
				{
					gameState.gameEventManager?.TriggerChangeYinYangPower(source, num, num2, temp: true);
				}
			}
		}
	}

	[SaveField(null, null)]
	public bool active;

	[SaveField(null, null)]
	public int battleConsumeYang;

	[SaveField(null, null)]
	public int battleConsumeYin;

	[SaveField(null, null)]
	public int constantMaxYangPower;

	[SaveField(null, null)]
	public int constantMaxYinPower;

	[SaveField(null, null)]
	public new GameStateInfo gameState;

	[SaveField(null, null)]
	public int turnConvertYang;

	[SaveField(null, null)]
	public int turnConvertYin;

	[SaveField(null, null)]
	public int turnMaxYangPower;

	[SaveField(null, null)]
	public int turnMaxYinPower;

	[SaveField(null, null)]
	public ProtectedInt yangPower;

	[SaveField(null, null)]
	public ProtectedInt yinPower;

	public int maxPower => (int)yangPower + (int)yinPower;

	public override void Init(GameStateInfo gameState)
	{
		this.gameState = gameState;
		this.gameState.UIManager.trigramsDisplay.gameObject.SetActive(value: false);
		gameState.applicationState.eventManager.gameEvent.AddGlobalListener(_OnGameEventTrigger);
	}

	public override void Dispose()
	{
		if (active)
		{
			active = false;
			gameState.applicationState.eventManager.gameEvent.RemoveGlobalListener(_OnGameEventTrigger);
		}
	}

	public void SetupCharacter(CharacterGameInitConfig characterGameConfig, bool newGame)
	{
		constantMaxYangPower = 1;
		constantMaxYinPower = 1;
		if ((characterGameConfig.characterDef.attachClass.mClass == CharacterClassType.XiaKe) & newGame)
		{
			constantMaxYangPower = characterGameConfig.overrideYan;
			constantMaxYinPower = characterGameConfig.overrideYin;
		}
		RefreshActiveDisplay(characterGameConfig.characterDef.attachClass.mClass == CharacterClassType.XiaKe);
	}

	public YinYangStateType GetCurrentState()
	{
		YinYangStateType yinYangStateType = YinYangStateType.None;
		if ((int)yinPower > (int)yangPower)
		{
			yinYangStateType |= YinYangStateType.Yin;
		}
		if ((int)yangPower > (int)yinPower)
		{
			yinYangStateType |= YinYangStateType.Yang;
		}
		if ((int)yinPower == maxPower)
		{
			yinYangStateType |= YinYangStateType.ExtremeYin;
		}
		if ((int)yangPower == maxPower)
		{
			yinYangStateType |= YinYangStateType.ExtremeYang;
		}
		if ((int)yangPower == (int)yinPower)
		{
			yinYangStateType |= YinYangStateType.Balance;
		}
		if ((int)yinPower == maxPower)
		{
			yinYangStateType |= YinYangStateType.FullFillYin;
		}
		if ((int)yangPower == maxPower)
		{
			yinYangStateType |= YinYangStateType.FullFillYang;
		}
		if ((int)yinPower == maxPower && (int)yangPower == maxPower)
		{
			yinYangStateType |= YinYangStateType.FullFill;
		}
		if ((int)yangPower + (int)yinPower == 0)
		{
			yinYangStateType |= YinYangStateType.Empty;
		}
		return yinYangStateType;
	}

	public (YinYangType type, DurationType durationType) GetPower(int index)
	{
		if (index > maxPower)
		{
			return (type: YinYangType.None, durationType: DurationType.Constant);
		}
		int item = ((index >= (int)yangPower) ? 1 : 2);
		DurationType item2 = ((index >= turnMaxYangPower && index < maxPower - turnMaxYinPower) ? DurationType.Temp : DurationType.Turn);
		return (type: (YinYangType)item, durationType: item2);
	}

	private void ConsumePower(YinYangType type)
	{
		(YinYangType type, DurationType durationType) power = GetPower((type != YinYangType.Yang) ? (maxPower - 1) : 0);
		if (power.type == YinYangType.Yang)
		{
			yangPower = (int)yangPower - 1;
		}
		else
		{
			yinPower = (int)yinPower - 1;
		}
		if (power.durationType != DurationType.Turn)
		{
			return;
		}
		if (type == YinYangType.Yang)
		{
			if (turnMaxYangPower > 0)
			{
				turnMaxYangPower--;
			}
			else
			{
				turnMaxYinPower--;
			}
		}
		else if (turnMaxYinPower > 0)
		{
			turnMaxYinPower--;
		}
		else
		{
			turnMaxYangPower--;
		}
	}

	private void _OnGameEventTrigger(GameEvent.Type type, GameEvent eve)
	{
		if (type == GameEvent.Type.AfterLoadGame)
		{
			RefreshActiveDisplay(gameState.playerState.character.def.attachClass.mClass == CharacterClassType.XiaKe);
		}
		if (!active)
		{
			return;
		}
		switch (type)
		{
		case GameEvent.Type.BattleStart:
			battleConsumeYang = 0;
			battleConsumeYin = 0;
			yangPower = constantMaxYangPower;
			yinPower = constantMaxYinPower;
			turnMaxYangPower = 0;
			turnMaxYinPower = 0;
			eve.gameState.UIManager.trigramsDisplay.OnBattleStartInit();
			break;
		case GameEvent.Type.BeforeBattleEnd:
			yangPower = constantMaxYangPower;
			yinPower = constantMaxYinPower;
			turnMaxYangPower = 0;
			turnMaxYinPower = 0;
			eve.gameState.UIManager.trigramsDisplay.OnBattleEnd();
			break;
		case GameEvent.Type.PlayerTurnStart:
			turnConvertYang = 0;
			turnConvertYin = 0;
			break;
		case GameEvent.Type.EnemyTurnEnd:
			gameState.actionManager.EnqueueAction(new ChangYinYangPowerAction(-turnMaxYinPower, -turnMaxYangPower, DurationType.Turn, eve.Caster));
			gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
			break;
		case GameEvent.Type.ConvertYinYangPower:
			if (eve.boolValue)
			{
				turnConvertYang += eve.amount;
			}
			else
			{
				turnConvertYin += eve.amount;
			}
			break;
		}
	}

	private void RefreshActiveDisplay(bool active)
	{
		this.active = active;
		if (active)
		{
			gameState.UIManager.trigramsDisplay.gameObject.SetActive(value: true);
			gameState.UIManager.trigramsDisplay.Setup();
		}
		else
		{
			gameState.UIManager.trigramsDisplay.gameObject.SetActive(value: false);
		}
	}
}
