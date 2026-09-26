using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkNyb3NzTGFkZGVyRGF0YUFjay5wcm90bxIFdHlwZXMaGnR5cGUu" + "Q3Jvc3NMYWRkZXJJbmZvLnByb3RvIkUKEkNyb3NzTGFkZGVyRGF0YUFjaxIv" + "Cg9Dcm9zc0xhZGRlckluZm8YASABKAsyFi50eXBlcy5Dcm9zc0xhZGRlcklu" + "Zm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeCrossLadderInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderDataAck), (MessageParser)(object)CrossLadderDataAck.Parser, new string[1] { "CrossLadderInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
