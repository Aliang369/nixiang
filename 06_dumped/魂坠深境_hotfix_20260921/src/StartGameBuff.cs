using System;
using System.Collections.Generic;
using System.Linq;

public class StartGameBuff : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber
{
	[SaveField(null, null)]
	public StartGameBuffDef def;

	[SaveField(null, null)]
	public int expireDayPeriod;

	private const int DefaultExpireDays = 7;

	private StartGameBuff()
	{
	}

	public static StartGameBuff CreateBuffOfLifeTime(StartGameBuffDef startGameBuffDef, int lifeTimeDays = 7)
	{
		return new StartGameBuff(startGameBuffDef)
		{
			expireDayPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod + lifeTimeDays
		};
	}

	public static StartGameBuff CreateBuffWithExpireDayPeriod(StartGameBuffDef startGameBuffDef, int expireDayPeriod)
	{
		return new StartGameBuff(startGameBuffDef)
		{
			expireDayPeriod = expireDayPeriod
		};
	}

	public StartGameBuff(StartGameBuffDef startGameBuffDef)
	{
		def = startGameBuffDef;
		expireDayPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod + 7;
	}

	public TimeSpan GetTimeToExpire(ApplicationStateInfo asi)
	{
		if (!IsActivatable(asi))
		{
			return TimeSpan.Zero;
		}
		int num = expireDayPeriod - ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod;
		return ScriptableObjectSingleton<CalendarDataManager>.instance.NextDailyResetTime - DateTime.UtcNow + TimeSpan.FromDays(num);
	}

	public string GetTimeExpireDescription(ApplicationStateInfo asi)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_start_game_buff_expire_in_time_duration.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(GetTimeToExpire(asi)));
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return def.GetNodeDisplayInfo(eve);
	}

	public List<IEffectNode> GetChildren()
	{
		return def.GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		return def.abilities.SelectMany((Ability x) => x.GetAllSubscribeGameEventType()).Distinct();
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return def.abilities.Any((Ability x) => x.CheckTriggerGameEventAble(eve));
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability ability in def.abilities)
		{
			ability.TriggerGameEvent(eve);
		}
	}

	public bool IsActivatable(ApplicationStateInfo asi)
	{
		return ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod <= expireDayPeriod;
	}

	public void Activate(GameStateInfo gs)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.ActivateStartGameBuff, GameEvent.TriggerType.Real);
		gameEvent.gameState = gs;
		gameEvent.SetCaster(this);
		def.activateEffect.Cast(gameEvent);
		gs.applicationState.startGameBuffManager.currentStartGameBuffs.Remove(this);
	}
}
