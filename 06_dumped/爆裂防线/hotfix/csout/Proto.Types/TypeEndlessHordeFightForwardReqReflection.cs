using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessHordeFightForwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeFightForwardReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkVuZGxlc3NIb3JkZUZpZ2h0Rm9yd2FyZFJlcS5wcm90bxIFdHlw" + "ZXMaG3R5cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJzChtFbmRsZXNzSG9y" + "ZGVGaWdodEZvcndhcmRSZXESDwoHTGV2ZWxJZBgBIAEoBRItCgxCYXR0bGVS" + "ZXN1bHQYAiABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvEhQKDFBhc3NE" + "aXN0YW5jZRgDIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeFightForwardReq), (MessageParser)(object)EndlessHordeFightForwardReq.Parser, new string[3] { "LevelId", "BattleResult", "PassDistance" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
