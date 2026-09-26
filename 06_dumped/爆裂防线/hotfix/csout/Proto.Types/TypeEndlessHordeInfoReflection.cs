using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessHordeInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkVuZGxlc3NIb3JkZUluZm8ucHJvdG8SBXR5cGVzInMKEEVuZGxl" + "c3NIb3JkZUluZm8SDQoFTGV2ZWwYASABKAUSFAoMUGFzc0Rpc3RhbmNlGAIg" + "ASgFEhMKC0NvbWJhdENvdW50GAMgASgFEhEKCUJ1eUNvbWJhdBgEIAEoBRIS" + "CgpEYWlseUNvdW50GAUgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeInfo), (MessageParser)(object)EndlessHordeInfo.Parser, new string[5] { "Level", "PassDistance", "CombatCount", "BuyCombat", "DailyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
