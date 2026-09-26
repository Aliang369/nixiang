using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatActionDataReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatActionDataReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkNoYXRBY3Rpb25EYXRhUmVxLnByb3RvEgV0eXBlcxoaZW51bS5F" + "Q2hhdEFjdGlvblR5cGUucHJvdG8iVwoRQ2hhdEFjdGlvbkRhdGFSZXESNAoK" + "QWN0aW9uVHlwZRgBIAEoDjIgLkVDaGF0QWN0aW9uVHlwZS5FQ2hhdEFjdGlv" + "blR5cGUSDAoEVVVJRBgCIAEoCUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChatActionTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatActionDataReq), (MessageParser)(object)ChatActionDataReq.Parser, new string[2] { "ActionType", "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
