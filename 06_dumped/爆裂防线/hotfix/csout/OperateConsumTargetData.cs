using Proto.Types;
using zbType;

public class OperateConsumTargetData
{
	public ConsumeTargetDetail TargetDetail;

	public int Count;

	public string UUID;

	public EOperateType OperateType;

	public OperateConsumTargetData(ConsumeTargetDetail targetDetail, int count, string uuid, EOperateType operateType)
	{
		TargetDetail = targetDetail;
		Count = count;
		UUID = uuid;
		OperateType = operateType;
	}
}
