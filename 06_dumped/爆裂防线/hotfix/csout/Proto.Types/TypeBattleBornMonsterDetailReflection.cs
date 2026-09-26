using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleBornMonsterDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleBornMonsterDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkJhdHRsZUJvcm5Nb25zdGVyRGV0YWlsLnByb3RvEgV0eXBlcxoV" + "ZW51bS5FQm9vbFN0YXRlLnByb3RvImQKF0JhdHRsZUJvcm5Nb25zdGVyRGV0" + "YWlsEhEKCU1vbnN0ZXJJZBgBIAEoBRILCgNOdW0YAiABKAUSKQoJVHVybnRh" + "YmxlGAMgASgOMhYuRUJvb2xTdGF0ZS5FQm9vbFN0YXRlQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleBornMonsterDetail), (MessageParser)(object)BattleBornMonsterDetail.Parser, new string[3] { "MonsterId", "Num", "Turntable" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
