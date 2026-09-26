using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventModRandAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventModRandAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLldvcmxkRHVuZ2VvbkV2ZW50TW9kUmFuZEFjay5wcm90bxIFdHlw" + "ZXMaGnR5cGUuRHVuZ2VvbkJhc2VJbmZvLnByb3RvGhl0eXBlLkR1bmdlb25N" + "YXBJbmZvLnByb3RvIn0KG1dvcmxkRHVuZ2VvbkV2ZW50TW9kUmFuZEFjaxIv" + "Cg9EdW5nZW9uQmFzZUluZm8YASABKAsyFi50eXBlcy5EdW5nZW9uQmFzZUlu" + "Zm8SLQoORHVuZ2Vvbk1hcEluZm8YAiABKAsyFS50eXBlcy5EdW5nZW9uTWFw" + "SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventModRandAck), (MessageParser)(object)WorldDungeonEventModRandAck.Parser, new string[2] { "DungeonBaseInfo", "DungeonMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
