using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeActiveInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeActiveInfoReflection()
	{
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlJlY2hhcmdlQWN0aXZlSW5mby5wcm90bxIFdHlwZXMaGXR5cGUu" + "Q2hhcmdlYmFja0luZm8ucHJvdG8aGHR5cGUuRGFpbHlQYWNrSW5mby5wcm90" + "bxoZdHlwZS5Ob3ZpY2VQYWNrSW5mby5wcm90bxoXdHlwZS5QYXNzcG9ydElu" + "Zm8ucHJvdG8aIXR5cGUuUmVzb3VyY2VDYXJkRG91YmxlSW5mby5wcm90bxog" + "dHlwZS5UcmVhc3VyZVByaXZpbGVnZUluZm8ucHJvdG8aHXR5cGUuWmhhbkxp" + "bmdDb21tb25JbmZvLnByb3RvGht0eXBlLlpoYW5MaW5nTWFpbkluZm8ucHJv" + "dG8iigUKElJlY2hhcmdlQWN0aXZlSW5mbxItCg5Ob3ZpY2VQYWNrSW5mbxgB" + "IAEoCzIVLnR5cGVzLk5vdmljZVBhY2tJbmZvEisKDURhaWx5UGFja0luZm8Y" + "AiABKAsyFC50eXBlcy5EYWlseVBhY2tJbmZvEi0KDkNoYXJnZWJhY2tJbmZv" + "GAMgASgLMhUudHlwZXMuQ2hhcmdlYmFja0luZm8SMwoQTGV2ZWxEaWFtb25k" + "SW5mbxgEIAEoCzIZLnR5cGVzLlpoYW5MaW5nQ29tbW9uSW5mbxIpCgxQYXNz" + "cG9ydEluZm8YBSABKAsyEy50eXBlcy5QYXNzcG9ydEluZm8SMgoTTm92aWNl" + "UGFja0V4dHJhTGlzdBgGIAMoCzIVLnR5cGVzLk5vdmljZVBhY2tJbmZvEjEK" + "EFpoYW5MaW5nTWFpbkluZm8YByABKAsyFy50eXBlcy5aaGFuTGluZ01haW5J" + "bmZvEjYKE1poYW5MaW5nRWxlbWVudEluZm8YCCABKAsyGS50eXBlcy5aaGFu" + "TGluZ0NvbW1vbkluZm8SNgoTWmhhbkxpbmdFbmRsZXNzSW5mbxgJIAEoCzIZ" + "LnR5cGVzLlpoYW5MaW5nQ29tbW9uSW5mbxI2ChVaaGFuTGluZ0xlZ2VuZGFy" + "eUluZm8YCiABKAsyFy50eXBlcy5aaGFuTGluZ01haW5JbmZvEjsKFVRyZWFz" + "dXJlUHJpdmlsZWdlSW5mbxgLIAEoCzIcLnR5cGVzLlRyZWFzdXJlUHJpdmls" + "ZWdlSW5mbxI9ChZSZXNvdXJjZUNhcmREb3VibGVJbmZvGAwgASgLMh0udHlw" + "ZXMuUmVzb3VyY2VDYXJkRG91YmxlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[8]
		{
			TypeChargebackInfoReflection.Descriptor,
			TypeDailyPackInfoReflection.Descriptor,
			TypeNovicePackInfoReflection.Descriptor,
			TypePassportInfoReflection.Descriptor,
			TypeResourceCardDoubleInfoReflection.Descriptor,
			TypeTreasurePrivilegeInfoReflection.Descriptor,
			TypeZhanLingCommonInfoReflection.Descriptor,
			TypeZhanLingMainInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeActiveInfo), (MessageParser)(object)RechargeActiveInfo.Parser, new string[12]
			{
				"NovicePackInfo", "DailyPackInfo", "ChargebackInfo", "LevelDiamondInfo", "PassportInfo", "NovicePackExtraList", "ZhanLingMainInfo", "ZhanLingElementInfo", "ZhanLingEndlessInfo", "ZhanLingLegendaryInfo",
				"TreasurePrivilegeInfo", "ResourceCardDoubleInfo"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
