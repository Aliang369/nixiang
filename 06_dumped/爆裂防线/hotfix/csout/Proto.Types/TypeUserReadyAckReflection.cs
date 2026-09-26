using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeUserReadyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUserReadyAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlVzZXJSZWFkeUFjay5wcm90bxIFdHlwZXMaEnR5cGUuR2FtZUVu" + "di5wcm90bxoPdHlwZS5HYXRlLnByb3RvIlwKDFVzZXJSZWFkeUFjaxIfCgdH" + "YW1lRW52GAEgASgLMg4udHlwZXMuR2FtZUVudhIZCgRHYXRlGAIgASgLMgsu" + "dHlwZXMuR2F0ZRIQCghTZXJ2ZXJJZBgDIAEoBUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeGameEnvReflection.Descriptor,
			TypeGateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UserReadyAck), (MessageParser)(object)UserReadyAck.Parser, new string[3] { "GameEnv", "Gate", "ServerId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
