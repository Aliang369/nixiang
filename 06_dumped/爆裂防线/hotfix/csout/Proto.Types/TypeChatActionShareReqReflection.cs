using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatActionShareReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatActionShareReqReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkNoYXRBY3Rpb25TaGFyZVJlcS5wcm90bxIFdHlwZXMaGmVudW0u" + "RUNoYXRBY3Rpb25UeXBlLnByb3RvGhRlbnVtLkVDaGF0VHlwZS5wcm90bxob" + "dHlwZS5DaGF0QWN0aW9uUGFyYW1zLnByb3RvIqEBChJDaGF0QWN0aW9uU2hh" + "cmVSZXESJgoIQ2hhdFR5cGUYASABKA4yFC5FQ2hhdFR5cGUuRUNoYXRUeXBl" + "EjQKCkFjdGlvblR5cGUYAiABKA4yIC5FQ2hhdEFjdGlvblR5cGUuRUNoYXRB" + "Y3Rpb25UeXBlEi0KDEFjdGlvblBhcmFtcxgDIAEoCzIXLnR5cGVzLkNoYXRB" + "Y3Rpb25QYXJhbXNCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEChatActionTypeReflection.Descriptor,
			EnumEChatTypeReflection.Descriptor,
			TypeChatActionParamsReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatActionShareReq), (MessageParser)(object)ChatActionShareReq.Parser, new string[3] { "ChatType", "ActionType", "ActionParams" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
