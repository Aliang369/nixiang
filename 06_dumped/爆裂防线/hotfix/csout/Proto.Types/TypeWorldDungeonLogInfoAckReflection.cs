using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonLogInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonLogInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLldvcmxkRHVuZ2VvbkxvZ0luZm9BY2sucHJvdG8SBXR5cGVzGhl0" + "eXBlLkR1bmdlb25Mb2dJbmZvLnByb3RvIkcKFldvcmxkRHVuZ2VvbkxvZ0lu" + "Zm9BY2sSLQoORHVuZ2VvbkxvZ0luZm8YASABKAsyFS50eXBlcy5EdW5nZW9u" + "TG9nSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeDungeonLogInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonLogInfoAck), (MessageParser)(object)WorldDungeonLogInfoAck.Parser, new string[1] { "DungeonLogInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
