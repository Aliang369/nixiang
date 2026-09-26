using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainRobFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainRobFightStartAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlRyYWluUm9iRmlnaHRTdGFydEFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuQmF0dGxlSW5mby5wcm90byJTChVUcmFpblJvYkZpZ2h0U3RhcnRBY2sS" + "EwoLVHJhbnNwb3J0SWQYASABKAUSJQoKQmF0dGxlSW5mbxgCIAEoCzIRLnR5" + "cGVzLkJhdHRsZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainRobFightStartAck), (MessageParser)(object)TrainRobFightStartAck.Parser, new string[2] { "TransportId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
