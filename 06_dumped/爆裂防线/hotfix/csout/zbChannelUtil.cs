using Proto.Enums;

public class zbChannelUtil
{
	public static bool bFirstLoginGame = true;

	public static bool IsZBChannel(EChannelType channel)
	{
		return channel == EChannelType.Zb;
	}

	public static bool IsIosChannel()
	{
		return zbLoginUtil.appChannel == EChannelType.Ios;
	}

	public static bool IsVisitorAccount()
	{
		return zbLoginUtil.loginChannel == EChannelType.Visitor;
	}

	public static bool IsNeedUseZbLogin()
	{
		if (!IsZBChannel(zbLoginUtil.appChannel))
		{
			if (zbChannel_Login.IsOpenTSCheck())
			{
				return IsIosChannel();
			}
			return false;
		}
		return true;
	}

	public static bool IsNeedShowAcSdkWnd()
	{
		if (zbLoginUtil.appChannel != EChannelType.TapTap && zbLoginUtil.appChannel != EChannelType.Ios)
		{
			return zbLoginUtil.appChannel == EChannelType.Dyunion;
		}
		return true;
	}

	public static bool IsNeedIdentityCheck()
	{
		if (zbChannel_Login.IsOpenTSCheck())
		{
			return false;
		}
		if (zbLoginUtil.appChannel != EChannelType.TapTap && zbLoginUtil.appChannel != EChannelType.Ios && zbLoginUtil.appChannel != EChannelType.Dyunion)
		{
			return zbLoginUtil.appChannel == EChannelType.Hykb;
		}
		return true;
	}

	public static bool IsOpenShare()
	{
		bool result = false;
		if (zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.Hykb || zbLoginUtil.appChannel == EChannelType.Dyunion)
		{
			result = true;
		}
		if (zbChannel_Login.IsOpenTSCheck())
		{
			result = false;
		}
		return result;
	}
}
