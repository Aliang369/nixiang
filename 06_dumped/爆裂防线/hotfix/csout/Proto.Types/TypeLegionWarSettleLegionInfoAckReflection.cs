using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarSettleLegionInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSettleLegionInfoAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkxlZ2lvbldhclNldHRsZUxlZ2lvbkluZm9BY2sucHJvdG8SBXR5" + "cGVzGhplbnVtLkVMZWdpb25XYXJGb3JjZS5wcm90bxokdHlwZS5MZWdpb25X" + "YXJTZXR0bGVMZWdpb25JbmZvLnByb3RvIsABChxMZWdpb25XYXJTZXR0bGVM" + "ZWdpb25JbmZvQWNrEg8KB01heFpvbmUYASABKAUSEgoKU2V0dGxlWm9uZRgC" + "IAEoBRIQCghTZXR0bGVUbRgDIAEoAxI0CgpXaW5Gb3JjZUlkGAQgASgOMiAu" + "RUxlZ2lvbldhckZvcmNlLkVMZWdpb25XYXJGb3JjZRIzCglGb3JjZUxpc3QY" + "BSADKAsyIC50eXBlcy5MZWdpb25XYXJTZXR0bGVMZWdpb25JbmZvQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumELegionWarForceReflection.Descriptor,
			TypeLegionWarSettleLegionInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSettleLegionInfoAck), (MessageParser)(object)LegionWarSettleLegionInfoAck.Parser, new string[5] { "MaxZone", "SettleZone", "SettleTm", "WinForceId", "ForceList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
