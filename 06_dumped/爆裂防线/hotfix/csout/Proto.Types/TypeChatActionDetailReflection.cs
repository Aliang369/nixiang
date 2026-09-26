using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatActionDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatActionDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkNoYXRBY3Rpb25EZXRhaWwucHJvdG8SBXR5cGVzGhplbnVtLkVD" + "aGF0QWN0aW9uVHlwZS5wcm90bxoedHlwZS5DaGF0QWN0aW9uQmFzaWNEYXRh" + "LnByb3RvIoUBChBDaGF0QWN0aW9uRGV0YWlsEjQKCkFjdGlvblR5cGUYASAB" + "KA4yIC5FQ2hhdEFjdGlvblR5cGUuRUNoYXRBY3Rpb25UeXBlEgwKBFVVSUQY" + "AiABKAkSLQoJQmFzaWNEYXRhGAMgASgLMhoudHlwZXMuQ2hhdEFjdGlvbkJh" + "c2ljRGF0YUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChatActionTypeReflection.Descriptor,
			TypeChatActionBasicDataReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatActionDetail), (MessageParser)(object)ChatActionDetail.Parser, new string[3] { "ActionType", "UUID", "BasicData" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
