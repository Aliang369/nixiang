using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLkFyZW5hSW5mby5wcm90bxIFdHlwZXMaGnR5cGUuQXJlbmFHaWZ0" + "RGV0YWlsLnByb3RvIpABCglBcmVuYUluZm8SDAoEUmFuaxgBIAEoBRIQCghC" + "ZXN0UmFuaxgCIAEoBRITCgtDb21iYXRDb3VudBgDIAEoBRISCgpEYWlseUNv" + "dW50GAQgASgFEhAKCEJ1eUNvdW50GAUgASgFEigKCEdpZnRMaXN0GAYgAygL" + "MhYudHlwZXMuQXJlbmFHaWZ0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeArenaGiftDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaInfo), (MessageParser)(object)ArenaInfo.Parser, new string[6] { "Rank", "BestRank", "CombatCount", "DailyCount", "BuyCount", "GiftList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
