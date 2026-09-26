using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatActionParamsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatActionParamsReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkNoYXRBY3Rpb25QYXJhbXMucHJvdG8SBXR5cGVzGhVlbnVtLkVF" + "cXVpcFR5cGUucHJvdG8aGnR5cGUuQ29tbW9uQXdhcmRJbmZvLnByb3RvIpEB" + "ChBDaGF0QWN0aW9uUGFyYW1zEigKCEJhclNoYXJlGAEgASgLMhYudHlwZXMu" + "Q29tbW9uQXdhcmRJbmZvEhEKCUhlcm9TaGFyZRgCIAEoBRIqCgpFcXVpcFNo" + "YXJlGAMgASgOMhYuRUVxdWlwVHlwZS5FRXF1aXBUeXBlEhQKDEJhdHRsZVJl" + "c3VsdBgEIAEoCUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEEquipTypeReflection.Descriptor,
			TypeCommonAwardInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatActionParams), (MessageParser)(object)ChatActionParams.Parser, new string[4] { "BarShare", "HeroShare", "EquipShare", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
