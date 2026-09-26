using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTruckMatchDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckMatchDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlRydWNrTWF0Y2hEZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVC" + "b29sU3RhdGUucHJvdG8aF3R5cGUuUm9sZUJhc2VJbmZvLnByb3RvGh90eXBl" + "LlRydWNrVHJhbnNwb3J0RGV0YWlsLnByb3RvIp4BChBUcnVja01hdGNoRGV0" + "YWlsEikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5m" + "bxIyCg1UcmFuc3BvcnRJbmZvGAIgASgLMhsudHlwZXMuVHJ1Y2tUcmFuc3Bv" + "cnREZXRhaWwSKwoLV2FudGVkU3RhdGUYAyABKA4yFi5FQm9vbFN0YXRlLkVC" + "b29sU3RhdGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor,
			TypeTruckTransportDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckMatchDetail), (MessageParser)(object)TruckMatchDetail.Parser, new string[3] { "RoleBaseInfo", "TransportInfo", "WantedState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
