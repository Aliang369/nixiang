using UnityEngine;

public class DebugLogNode : Effect
{
	public bool EditorPause;

	public string log;

	public ValueGenerator logValue;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int? num = logValue?.GenerateValue(eve);
		Debug.Log(log + (num.HasValue ? ((object)num.Value) : ""));
		return NodeCastResult.Success;
	}
}
