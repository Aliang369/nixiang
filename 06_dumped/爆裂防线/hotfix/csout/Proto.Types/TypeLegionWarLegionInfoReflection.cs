using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarLegionInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarLegionInfoReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxlZ2lvbldhckxlZ2lvbkluZm8ucHJvdG8SBXR5cGVzGhVlbnVt" + "LkVCb29sU3RhdGUucHJvdG8aGmVudW0uRUxlZ2lvbldhckZvcmNlLnByb3Rv" + "IrMBChNMZWdpb25XYXJMZWdpb25JbmZvEhIKCkxlZ2lvblpvbmUYASABKAUS" + "NwoNTGVnaW9uRm9yY2VJZBgCIAEoDjIgLkVMZWdpb25XYXJGb3JjZS5FTGVn" + "aW9uV2FyRm9yY2USEwoLTWFya1BvaW50SWQYAyABKAUSEgoKU2NvcmVDb3Vu" + "dBgEIAEoBRImCgZTaWdudXAYBSABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3Rh" + "dGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumELegionWarForceReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarLegionInfo), (MessageParser)(object)LegionWarLegionInfo.Parser, new string[5] { "LegionZone", "LegionForceId", "MarkPointId", "ScoreCount", "Signup" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
