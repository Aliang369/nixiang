using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallDataAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLldhbGxEYXRhQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5EZWZlbnNl" + "SW5mby5wcm90bxoTdHlwZS5XYWxsSW5mby5wcm90byJZCgtXYWxsRGF0YUFj" + "axIhCghXYWxsSW5mbxgBIAEoCzIPLnR5cGVzLldhbGxJbmZvEicKC0RlZmVu" + "c2VJbmZvGAIgASgLMhIudHlwZXMuRGVmZW5zZUluZm9CLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallDataAck), (MessageParser)(object)WallDataAck.Parser, new string[2] { "WallInfo", "DefenseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
