using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkFyZW5hQnV5Q29tYmF0QWNrLnByb3RvEgV0eXBlcxoUdHlwZS5B" + "cmVuYUluZm8ucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byJfChFBcmVu" + "YUJ1eUNvbWJhdEFjaxIjCglBcmVuYUluZm8YASABKAsyEC50eXBlcy5BcmVu" + "YUluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeArenaInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaBuyCombatAck), (MessageParser)(object)ArenaBuyCombatAck.Parser, new string[2] { "ArenaInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
