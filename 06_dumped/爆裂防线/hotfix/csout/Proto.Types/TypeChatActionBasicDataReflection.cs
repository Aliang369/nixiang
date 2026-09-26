using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeChatActionBasicDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatActionBasicDataReflection()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkNoYXRBY3Rpb25CYXNpY0RhdGEucHJvdG8SBXR5cGVzGht0eXBl" + "LkJhdHRsZVJlc3VsdEluZm8ucHJvdG8aGnR5cGUuQ29tbW9uQXdhcmRJbmZv" + "LnByb3RvGhh0eXBlLkNvbnRlbnREZXRhaWwucHJvdG8aFnR5cGUuRXF1aXBE" + "ZXRhaWwucHJvdG8aFXR5cGUuSGVyb0RldGFpbC5wcm90bxoZdHlwZS5MZWdp" + "b25CYXNlSW5mby5wcm90bxoadHlwZS5Xb3JsZExhaXJEZXRhaWwucHJvdG8i" + "uAIKE0NoYXRBY3Rpb25CYXNpY0RhdGESIgoEVGV4dBgBIAEoCzIULnR5cGVz" + "LkNvbnRlbnREZXRhaWwSKwoMTGVnaW9uSW52aXRlGAIgASgLMhUudHlwZXMu" + "TGVnaW9uQmFzZUluZm8SKAoIQmFyU2hhcmUYAyABKAsyFi50eXBlcy5Db21t" + "b25Bd2FyZEluZm8SJAoJSGVyb1NoYXJlGAQgASgLMhEudHlwZXMuSGVyb0Rl" + "dGFpbBImCgpFcXVpcFNoYXJlGAUgASgLMhIudHlwZXMuRXF1aXBEZXRhaWwS" + "LQoMQmF0dGxlUmVzdWx0GAYgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5m" + "bxIpCglXb3JsZExhaXIYByABKAsyFi50eXBlcy5Xb3JsZExhaXJEZXRhaWxC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[7]
		{
			TypeBattleResultInfoReflection.Descriptor,
			TypeCommonAwardInfoReflection.Descriptor,
			TypeContentDetailReflection.Descriptor,
			TypeEquipDetailReflection.Descriptor,
			TypeHeroDetailReflection.Descriptor,
			TypeLegionBaseInfoReflection.Descriptor,
			TypeWorldLairDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatActionBasicData), (MessageParser)(object)ChatActionBasicData.Parser, new string[7] { "Text", "LegionInvite", "BarShare", "HeroShare", "EquipShare", "BattleResult", "WorldLair" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
