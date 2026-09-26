using Proto.Types;

public class OperateTargetData
{
	public OperateTargetDetail TargetDetail;

	public int Count;

	public string UUID;

	public OperateTargetData(OperateTargetDetail targetDetail, int count, string uuid)
	{
		TargetDetail = targetDetail;
		Count = count;
		UUID = uuid;
	}
}
