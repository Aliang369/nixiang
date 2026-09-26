using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarSettlePlayerInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSettlePlayerInfoAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkxlZ2lvbldhclNldHRsZVBsYXllckluZm9BY2sucHJvdG8SBXR5" + "cGVzGhplbnVtLkVMZWdpb25XYXJGb3JjZS5wcm90bxokdHlwZS5MZWdpb25X" + "YXJTZXR0bGVQbGF5ZXJJbmZvLnByb3RvIsABChxMZWdpb25XYXJTZXR0bGVQ" + "bGF5ZXJJbmZvQWNrEg8KB01heFpvbmUYASABKAUSEgoKU2V0dGxlWm9uZRgC" + "IAEoBRIQCghTZXR0bGVUbRgDIAEoAxI0CgpXaW5Gb3JjZUlkGAQgASgOMiAu" + "RUxlZ2lvbldhckZvcmNlLkVMZWdpb25XYXJGb3JjZRIzCglGb3JjZUxpc3QY" + "BSADKAsyIC50eXBlcy5MZWdpb25XYXJTZXR0bGVQbGF5ZXJJbmZvQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumELegionWarForceReflection.Descriptor,
			TypeLegionWarSettlePlayerInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSettlePlayerInfoAck), (MessageParser)(object)LegionWarSettlePlayerInfoAck.Parser, new string[5] { "MaxZone", "SettleZone", "SettleTm", "WinForceId", "ForceList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
