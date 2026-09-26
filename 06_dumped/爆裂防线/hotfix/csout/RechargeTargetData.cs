using Proto.Types;
using zbType;

public class RechargeTargetData
{
	public RechargeTargetDetail TargetDetail;

	public int Count;

	public string UUID;

	public EOperateType OperateType;

	public RechargeTargetData(RechargeTargetDetail targetDetail, int count, string uuid, EOperateType operateType)
	{
		TargetDetail = targetDetail;
		Count = count;
		UUID = uuid;
		OperateType = operateType;
	}
}
