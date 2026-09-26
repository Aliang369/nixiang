using Proto.Types;
using zbType;

public class zbOperateData
{
	public EOperateType operateType;

	public int index;

	public OperateBaseInfo baseInfo;

	public zbOperateData(EOperateType newOperateType, int newIndex = 0, OperateBaseInfo newBaseInfo = null)
	{
		operateType = newOperateType;
		index = newIndex;
		baseInfo = newBaseInfo;
	}
}
