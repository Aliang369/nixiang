public abstract class ActivityDef : CTScriptableObject
{
	public UnityDateTime startTime;

	public UnityDateTime endTime;

	public override string CTAssetIDPrepend => "ACT";

	public string activityId => base.FullID;
}
