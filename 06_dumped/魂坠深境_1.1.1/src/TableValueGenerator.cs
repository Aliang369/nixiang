public abstract class TableValueGenerator : ValueGenerator
{
	protected abstract object GetRowKey(GameEvent eve);

	protected abstract object GetColKey(GameEvent eve);

	public override int? GenerateValue(GameEvent eve)
	{
		ScriptableObjectSingleton<GameStatisticData>.instance.nightmareTable.TryGet(GetRowKey(eve), GetColKey(eve), out var value);
		return value;
	}
}
