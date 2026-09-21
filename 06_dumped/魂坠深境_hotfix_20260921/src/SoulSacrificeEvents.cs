using System;
using System.Collections.Generic;

public class SoulSacrificeEvents
{
	public static class RedDotStateChangeEvent
	{
		public class Data
		{
			public string redDotId;

			public bool show;
		}

		public static EventInfo<Data> eventInfo = new EventInfo<Data>();

		public static void Trigger(string redDotId, bool show)
		{
			eventInfo.TriggerEvent(new Data
			{
				redDotId = redDotId,
				show = show
			});
		}

		public static void AddListener(Action<Data> listener)
		{
			eventInfo.AddListener(listener);
		}

		public static void RemoveListener(Action<Data> listener)
		{
			eventInfo.RemoveListener(listener);
		}
	}

	public static class ForgetConfigApplyEvent
	{
		public class Data
		{
			public CharacterClassType classType;

			public List<ItemDefinition> forgottenCardDefs;
		}

		public static EventInfo<Data> eventInfo = new EventInfo<Data>();

		public static void Trigger(CharacterClassType classType, List<ItemDefinition> forgottenCardDefs)
		{
			eventInfo.TriggerEvent(new Data
			{
				classType = classType,
				forgottenCardDefs = forgottenCardDefs
			});
		}

		public static void AddListener(Action<Data> listener)
		{
			eventInfo.AddListener(listener);
		}

		public static void RemoveListener(Action<Data> listener)
		{
			eventInfo.RemoveListener(listener);
		}
	}

	public static class SeasonSwitchEvent
	{
		public class Data
		{
			public string seasonId;

			public string seasonName;
		}

		public static EventInfo<Data> eventInfo = new EventInfo<Data>();

		public static void Trigger(string seasonId, string seasonName)
		{
			eventInfo.TriggerEvent(new Data
			{
				seasonId = seasonId,
				seasonName = seasonName
			});
		}

		public static void AddListener(Action<Data> listener)
		{
			eventInfo.AddListener(listener);
		}

		public static void RemoveListener(Action<Data> listener)
		{
			eventInfo.RemoveListener(listener);
		}
	}
}
