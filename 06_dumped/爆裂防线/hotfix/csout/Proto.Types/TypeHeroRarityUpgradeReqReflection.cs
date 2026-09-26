using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroRarityUpgradeReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroRarityUpgradeReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkhlcm9SYXJpdHlVcGdyYWRlUmVxLnByb3RvEgV0eXBlcxoadHlw" + "ZS5CYWdTZWxlY3REZXRhaWwucHJvdG8iUwoUSGVyb1Jhcml0eVVwZ3JhZGVS" + "ZXESDwoHSGVyb1RpZBgBIAEoBRIqCgpTZWxlY3RMaXN0GAIgAygLMhYudHlw" + "ZXMuQmFnU2VsZWN0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBagSelectDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroRarityUpgradeReq), (MessageParser)(object)HeroRarityUpgradeReq.Parser, new string[2] { "HeroTid", "SelectList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
