using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandRoomEditDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandRoomEditDetailReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkhvbWVsYW5kUm9vbUVkaXREZXRhaWwucHJvdG8SBXR5cGVzIj4K" + "FkhvbWVsYW5kUm9vbUVkaXREZXRhaWwSDgoGUm9vbUlkGAEgASgFEgkKAVgY" + "AiABKAUSCQoBWRgDIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandRoomEditDetail), (MessageParser)(object)HomelandRoomEditDetail.Parser, new string[3] { "RoomId", "X", "Y" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
