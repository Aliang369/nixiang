using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlJvbGVGaWdodFJlc3VsdFJlcS5wcm90bxIFdHlwZXMaG3R5cGUu" + "QmF0dGxlUmVzdWx0SW5mby5wcm90byJVChJSb2xlRmlnaHRSZXN1bHRSZXES" + "EAoIUGxheWVySWQYASABKAUSLQoMQmF0dGxlUmVzdWx0GAIgASgLMhcudHlw" + "ZXMuQmF0dGxlUmVzdWx0SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleFightResultReq), (MessageParser)(object)RoleFightResultReq.Parser, new string[2] { "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
