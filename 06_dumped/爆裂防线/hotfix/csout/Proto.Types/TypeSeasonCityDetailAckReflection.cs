using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonCityDetailAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCityDetailAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlYXNvbkNpdHlEZXRhaWxBY2sucHJvdG8SBXR5cGVzGhtlbnVt" + "LkVTZWFzb25BcmVhU3RhdGUucHJvdG8aGmVudW0uRVNlYXNvbkFyZWFUeXBl" + "LnByb3RvGhdlbnVtLkVTZWFzb25Gb3JjZS5wcm90bxocdHlwZS5TZWFzb25S" + "ZXNOb2RlSW5mby5wcm90byL3AQoTU2Vhc29uQ2l0eURldGFpbEFjaxIOCgZD" + "aXR5SWQYASABKAUSMgoIQXJlYVR5cGUYAiABKA4yIC5FU2Vhc29uQXJlYVR5" + "cGUuRVNlYXNvbkFyZWFUeXBlEg0KBUxldmVsGAMgASgFEisKB0ZvcmNlSWQY" + "BCABKA4yGi5FU2Vhc29uRm9yY2UuRVNlYXNvbkZvcmNlEjEKBVN0YXRlGAUg" + "ASgOMiIuRVNlYXNvbkFyZWFTdGF0ZS5FU2Vhc29uQXJlYVN0YXRlEi0KC1Jl" + "c05vZGVMaXN0GAYgAygLMhgudHlwZXMuU2Vhc29uUmVzTm9kZUluZm9CLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumESeasonAreaStateReflection.Descriptor,
			EnumESeasonAreaTypeReflection.Descriptor,
			EnumESeasonForceReflection.Descriptor,
			TypeSeasonResNodeInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCityDetailAck), (MessageParser)(object)SeasonCityDetailAck.Parser, new string[6] { "CityId", "AreaType", "Level", "ForceId", "State", "ResNodeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
