using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderSelfInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderSelfInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkNyb3NzTGFkZGVyU2VsZkluZm8ucHJvdG8SBXR5cGVzGiF0eXBl" + "LkNyb3NzTGFkZGVyU2Vhc29uQXdhcmQucHJvdG8imgEKE0Nyb3NzTGFkZGVy" + "U2VsZkluZm8SEwoLQ29tYmF0Q291bnQYASABKAUSEQoJQnV5Q29tYmF0GAIg" + "ASgFEhIKCkRhaWx5Q291bnQYAyABKAUSEwoLVGFyZ2V0QXdhcmQYBCABKAUS" + "MgoLU2Vhc29uQXdhcmQYBSABKAsyHS50eXBlcy5Dcm9zc0xhZGRlclNlYXNv" + "bkF3YXJkQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeCrossLadderSeasonAwardReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderSelfInfo), (MessageParser)(object)CrossLadderSelfInfo.Parser, new string[5] { "CombatCount", "BuyCombat", "DailyCount", "TargetAward", "SeasonAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
