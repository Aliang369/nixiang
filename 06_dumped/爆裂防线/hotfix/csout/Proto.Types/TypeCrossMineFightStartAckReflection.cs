using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossMineFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineFightStartAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkNyb3NzTWluZUZpZ2h0U3RhcnRBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkJhdHRsZUluZm8ucHJvdG8aGnR5cGUuTWluZVdhckJhc2VJbmZvLnBy" + "b3RvIqUBChZDcm9zc01pbmVGaWdodFN0YXJ0QWNrEhAKCFNlcnZlcklkGAEg" + "ASgFEg8KB1BvaW50SWQYAiABKAUSEAoIUGxheWVySWQYAyABKAUSJQoKQmF0" + "dGxlSW5mbxgEIAEoCzIRLnR5cGVzLkJhdHRsZUluZm8SLwoPTWluZVdhckJh" + "c2VJbmZvGAUgASgLMhYudHlwZXMuTWluZVdhckJhc2VJbmZvQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineFightStartAck), (MessageParser)(object)CrossMineFightStartAck.Parser, new string[5] { "ServerId", "PointId", "PlayerId", "BattleInfo", "MineWarBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
