using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderFightResultAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkNyb3NzTGFkZGVyRmlnaHRSZXN1bHRBY2sucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGht0eXBlLkJhdHRsZVJlc3VsdElu" + "Zm8ucHJvdG8aGnR5cGUuQ3Jvc3NMYWRkZXJJbmZvLnByb3RvGhV0eXBlLkRh" + "dGFDaGFuZ2UucHJvdG8iyQEKGUNyb3NzTGFkZGVyRmlnaHRSZXN1bHRBY2sS" + "LwoPQ3Jvc3NMYWRkZXJJbmZvGAEgASgLMhYudHlwZXMuQ3Jvc3NMYWRkZXJJ" + "bmZvEi0KDEJhdHRsZVNldHRsZRgCIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3Vs" + "dEluZm8SJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwS" + "JQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeCrossLadderInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderFightResultAck), (MessageParser)(object)CrossLadderFightResultAck.Parser, new string[4] { "CrossLadderInfo", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
