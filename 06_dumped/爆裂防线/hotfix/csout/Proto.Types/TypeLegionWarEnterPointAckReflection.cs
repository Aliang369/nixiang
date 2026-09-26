using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarEnterPointAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarEnterPointAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbldhckVudGVyUG9pbnRBY2sucHJvdG8SBXR5cGVzGh50" + "eXBlLkxlZ2lvbldhckxlZ2lvbkluZm8ucHJvdG8aHXR5cGUuTGVnaW9uV2Fy" + "UG9pbnRJbmZvLnByb3RvGhx0eXBlLkxlZ2lvbldhclNlbGZJbmZvLnByb3Rv" + "Ir0BChZMZWdpb25XYXJFbnRlclBvaW50QWNrEjcKE0xlZ2lvbldhckxlZ2lv" + "bkluZm8YASABKAsyGi50eXBlcy5MZWdpb25XYXJMZWdpb25JbmZvEjMKEUxl" + "Z2lvbldhclNlbGZJbmZvGAIgASgLMhgudHlwZXMuTGVnaW9uV2FyU2VsZklu" + "Zm8SNQoSTGVnaW9uV2FyUG9pbnRJbmZvGAMgASgLMhkudHlwZXMuTGVnaW9u" + "V2FyUG9pbnRJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarPointInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarEnterPointAck), (MessageParser)(object)LegionWarEnterPointAck.Parser, new string[3] { "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarPointInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
