using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarDataAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkxlZ2lvbldhckRhdGFBY2sucHJvdG8SBXR5cGVzGhplbnVtLkVM" + "ZWdpb25XYXJTdGF0ZS5wcm90bxocdHlwZS5MZWdpb25XYXJCYXNlSW5mby5w" + "cm90bxoedHlwZS5MZWdpb25XYXJMZWdpb25JbmZvLnByb3RvIrQBChBMZWdp" + "b25XYXJEYXRhQWNrEjIKCFdhclN0YXRlGAEgASgOMiAuRUxlZ2lvbldhclN0" + "YXRlLkVMZWdpb25XYXJTdGF0ZRIzChFMZWdpb25XYXJCYXNlSW5mbxgCIAEo" + "CzIYLnR5cGVzLkxlZ2lvbldhckJhc2VJbmZvEjcKE0xlZ2lvbldhckxlZ2lv" + "bkluZm8YAyABKAsyGi50eXBlcy5MZWdpb25XYXJMZWdpb25JbmZvQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumELegionWarStateReflection.Descriptor,
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarLegionInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarDataAck), (MessageParser)(object)LegionWarDataAck.Parser, new string[3] { "WarState", "LegionWarBaseInfo", "LegionWarLegionInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
