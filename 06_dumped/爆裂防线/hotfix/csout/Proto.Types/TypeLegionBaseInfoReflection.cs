using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBaseInfoReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkxlZ2lvbkJhc2VJbmZvLnByb3RvEgV0eXBlcxoVZW51bS5FQm9v" + "bFN0YXRlLnByb3RvIsUBCg5MZWdpb25CYXNlSW5mbxIQCghMZWdpb25JZBgB" + "IAEoBRISCgpMZWdpb25OYW1lGAIgASgJEgwKBEljb24YAyABKAUSDQoFTGV2" + "ZWwYBCABKAUSEwoLTWVtYmVyQ291bnQYBSABKAUSEQoJTWVtYmVyTWF4GAYg" + "ASgFEg8KB0pvaW5MdmwYByABKAUSJwoHQ29uZmlybRgIIAEoDjIWLkVCb29s" + "U3RhdGUuRUJvb2xTdGF0ZRIOCgZBY3RpdmUYCSABKAVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBaseInfo), (MessageParser)(object)LegionBaseInfo.Parser, new string[9] { "LegionId", "LegionName", "Icon", "Level", "MemberCount", "MemberMax", "JoinLvl", "Confirm", "Active" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
