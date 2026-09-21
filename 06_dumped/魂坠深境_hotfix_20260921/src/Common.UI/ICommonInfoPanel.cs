namespace Common.UI;

public interface ICommonInfoPanel
{
	void ShowCommonInfo(CommonInfoData info);

	void HideCommonInfo(object obj, CommonInfoData.ControlType controlType);
}
