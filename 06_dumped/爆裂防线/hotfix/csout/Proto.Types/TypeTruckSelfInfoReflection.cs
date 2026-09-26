using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTruckSelfInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckSelfInfoReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlRydWNrU2VsZkluZm8ucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29s" + "U3RhdGUucHJvdG8aFnR5cGUuVHJ1Y2tEZXRhaWwucHJvdG8i8wEKDVRydWNr" + "U2VsZkluZm8SFQoNVHJhbnNwb3J0VXNlZBgBIAEoBRIWCg5UcmFuc3BvcnRM" + "aW1pdBgCIAEoBRIXCg9UcmFuc3BvcnRCb3VnaHQYAyABKAUSDwoHUm9iVXNl" + "ZBgEIAEoBRIQCghSb2JMaW1pdBgFIAEoBRIRCglSb2JCb3VnaHQYBiABKAUS" + "JQoJVHJ1Y2tMaXN0GAcgAygLMhIudHlwZXMuVHJ1Y2tEZXRhaWwSEgoKV2Fu" + "dGVkTGlzdBgIIAMoBRIpCglQcml2aWxlZ2UYCSABKA4yFi5FQm9vbFN0YXRl" + "LkVCb29sU3RhdGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeTruckDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckSelfInfo), (MessageParser)(object)TruckSelfInfo.Parser, new string[9] { "TransportUsed", "TransportLimit", "TransportBought", "RobUsed", "RobLimit", "RobBought", "TruckList", "WantedList", "Privilege" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
