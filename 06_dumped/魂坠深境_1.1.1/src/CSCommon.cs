public static class CSCommon
{
	public static TPackage packageType => (TPackage)3;

	public static NetWorkEndPointType netWorkEndPointType => (NetWorkEndPointType)3;

	public static ChannelType channelType
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return CommonConfig.Instance.channelType;
		}
	}

	public static string version => CommonConfig.Instance.version;

	public static int versionCode => CommonConfig.Instance.versionCode;

	public static bool developingAssetFilter
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			TPackage val = packageType;
			return (int)val == 3 || (int)val == 4;
		}
	}

	public static bool removeInvalidAssetsFromBuild
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			TPackage val = packageType;
			return (int)val == 3 || (int)val == 4;
		}
	}

	public static bool enableNetwork
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			TPackage val = packageType;
			if ((int)val != 1)
			{
				return (int)val != 4;
			}
			return false;
		}
	}

	public static bool cheatJumpMap => isDebug;

	public static bool showDebugPanel => isDebug;

	public static bool enableUrpDebugRuntimeUI => isDebug;

	public static BootstrapType bootStrapType
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			TPackage val = packageType;
			if ((int)val == 1 || (int)val == 4)
			{
				return (BootstrapType)0;
			}
			return (BootstrapType)2;
		}
	}

	public static bool isDebug => false;
}
