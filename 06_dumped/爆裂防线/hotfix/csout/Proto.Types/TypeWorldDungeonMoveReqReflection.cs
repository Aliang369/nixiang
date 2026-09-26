using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonMoveReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonMoveReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldvcmxkRHVuZ2Vvbk1vdmVSZXEucHJvdG8SBXR5cGVzGh50eXBl" + "LkR1bmdlb25Sb2xlU2l0ZUluZm8ucHJvdG8iRwoTV29ybGREdW5nZW9uTW92" + "ZVJlcRIwCgxSb2xlU2l0ZUluZm8YASABKAsyGi50eXBlcy5EdW5nZW9uUm9s" + "ZVNpdGVJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeDungeonRoleSiteInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonMoveReq), (MessageParser)(object)WorldDungeonMoveReq.Parser, new string[1] { "RoleSiteInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
