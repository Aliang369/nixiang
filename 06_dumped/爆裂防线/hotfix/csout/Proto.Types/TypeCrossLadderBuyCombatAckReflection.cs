using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkNyb3NzTGFkZGVyQnV5Q29tYmF0QWNrLnByb3RvEgV0eXBlcxoa" + "dHlwZS5Dcm9zc0xhZGRlckluZm8ucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90byJxChdDcm9zc0xhZGRlckJ1eUNvbWJhdEFjaxIvCg9Dcm9zc0xhZGRl" + "ckluZm8YASABKAsyFi50eXBlcy5Dcm9zc0xhZGRlckluZm8SJQoKRGF0YUNo" + "YW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeCrossLadderInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderBuyCombatAck), (MessageParser)(object)CrossLadderBuyCombatAck.Parser, new string[2] { "CrossLadderInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
