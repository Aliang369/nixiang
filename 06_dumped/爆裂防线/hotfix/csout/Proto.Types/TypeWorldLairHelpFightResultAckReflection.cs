using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairHelpFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairHelpFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLldvcmxkTGFpckhlbHBGaWdodFJlc3VsdEFjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aG3R5cGUuQmF0dGxlUmVzdWx0" + "SW5mby5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhx0eXBlLldvcmxk" + "TGFpckhlbHBJbmZvLnByb3RvGhh0eXBlLldvcmxkTGFpckluZm8ucHJvdG8i" + "8wEKG1dvcmxkTGFpckhlbHBGaWdodFJlc3VsdEFjaxIrCg1Xb3JsZExhaXJJ" + "bmZvGAEgASgLMhQudHlwZXMuV29ybGRMYWlySW5mbxIqCghIZWxwSW5mbxgC" + "IAEoCzIYLnR5cGVzLldvcmxkTGFpckhlbHBJbmZvEi0KDEJhdHRsZVNldHRs" + "ZRgDIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm8SJQoJQXdhcmRMaXN0" + "GAQgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgFIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWorldLairHelpInfoReflection.Descriptor,
			TypeWorldLairInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairHelpFightResultAck), (MessageParser)(object)WorldLairHelpFightResultAck.Parser, new string[5] { "WorldLairInfo", "HelpInfo", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
