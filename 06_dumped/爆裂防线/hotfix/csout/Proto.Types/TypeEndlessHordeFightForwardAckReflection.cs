using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEndlessHordeFightForwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeFightForwardAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkVuZGxlc3NIb3JkZUZpZ2h0Rm9yd2FyZEFjay5wcm90bxIFdHlw" + "ZXMaFWVudW0uRUJvb2xTdGF0ZS5wcm90bxoVdHlwZS5CYXR0bGVJbmZvLnBy" + "b3RvGht0eXBlLkVuZGxlc3NIb3JkZUluZm8ucHJvdG8ivgEKG0VuZGxlc3NI" + "b3JkZUZpZ2h0Rm9yd2FyZEFjaxIsCgxCZXN0RGlzdGFuY2UYASABKA4yFi5F" + "Qm9vbFN0YXRlLkVCb29sU3RhdGUSMQoQRW5kbGVzc0hvcmRlSW5mbxgCIAEo" + "CzIXLnR5cGVzLkVuZGxlc3NIb3JkZUluZm8SEwoLTmV4dExldmVsSWQYAyAB" + "KAUSKQoOTmV4dEJhdHRsZUluZm8YBCABKAsyES50eXBlcy5CYXR0bGVJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeBattleInfoReflection.Descriptor,
			TypeEndlessHordeInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeFightForwardAck), (MessageParser)(object)EndlessHordeFightForwardAck.Parser, new string[4] { "BestDistance", "EndlessHordeInfo", "NextLevelId", "NextBattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
