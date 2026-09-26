using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckRobFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckRobFightStartAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlRydWNrUm9iRmlnaHRTdGFydEFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuQmF0dGxlSW5mby5wcm90byJlChVUcnVja1JvYkZpZ2h0U3RhcnRBY2sS" + "EAoIUGxheWVySWQYASABKAUSEwoLVHJhbnNwb3J0SWQYAiABKAUSJQoKQmF0" + "dGxlSW5mbxgDIAEoCzIRLnR5cGVzLkJhdHRsZUluZm9CLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckRobFightStartAck), (MessageParser)(object)TruckRobFightStartAck.Parser, new string[3] { "PlayerId", "TransportId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
