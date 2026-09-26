using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitVipHeroBaseDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitVipHeroBaseDataReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkxpbWl0VmlwSGVyb0Jhc2VEYXRhLnByb3RvEgV0eXBlcyKfAQoU" + "TGltaXRWaXBIZXJvQmFzZURhdGESDwoHSGVyb1RpZBgBIAEoBRIQCghMaW1p" + "dFZpcBgCIAEoBRIQCghPbmNlQ29zdBgDIAEoBRIPCgdUZW5Db3N0GAQgASgF" + "EhEKCU9uY2VMdWNreRgFIAEoBRIQCghNYXhMdWNreRgGIAEoBRINCgVDb3Vu" + "dBgHIAEoBRINCgVMdWNreRgIIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitVipHeroBaseData), (MessageParser)(object)LimitVipHeroBaseData.Parser, new string[8] { "HeroTid", "LimitVip", "OnceCost", "TenCost", "OnceLucky", "MaxLucky", "Count", "Lucky" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
