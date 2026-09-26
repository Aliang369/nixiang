using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleMonsterDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMonsterDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkJhdHRsZU1vbnN0ZXJEZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVt" + "LkVCb29sU3RhdGUucHJvdG8aInR5cGUuQmF0dGxlTW9uc3RlckF0dHJEZXRh" + "aWwucHJvdG8iggEKE0JhdHRsZU1vbnN0ZXJEZXRhaWwSEQoJTW9uc3Rlcklk" + "GAEgASgFEjAKCEF0dHJMaXN0GAIgAygLMh4udHlwZXMuQmF0dGxlTW9uc3Rl" + "ckF0dHJEZXRhaWwSJgoGU3VtbW9uGAMgASgOMhYuRUJvb2xTdGF0ZS5FQm9v" + "bFN0YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeBattleMonsterAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMonsterDetail), (MessageParser)(object)BattleMonsterDetail.Parser, new string[3] { "MonsterId", "AttrList", "Summon" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
