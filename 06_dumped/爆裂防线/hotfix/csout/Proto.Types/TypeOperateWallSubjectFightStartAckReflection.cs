using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectFightStartAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLk9wZXJhdGVXYWxsU3ViamVjdEZpZ2h0U3RhcnRBY2sucHJvdG8S" + "BXR5cGVzGhV0eXBlLkJhdHRsZUluZm8ucHJvdG8iZwofT3BlcmF0ZVdhbGxT" + "dWJqZWN0RmlnaHRTdGFydEFjaxIMCgRVVUlEGAEgASgJEg8KB0xldmVsSWQY" + "AiABKAUSJQoKQmF0dGxlSW5mbxgDIAEoCzIRLnR5cGVzLkJhdHRsZUluZm9C" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectFightStartAck), (MessageParser)(object)OperateWallSubjectFightStartAck.Parser, new string[3] { "UUID", "LevelId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
