using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkNoYXREZXRhaWwucHJvdG8SBXR5cGVzGhRlbnVtLkVDaGF0VHlw" + "ZS5wcm90bxobdHlwZS5DaGF0QWN0aW9uRGV0YWlsLnByb3RvGhd0eXBlLlJv" + "bGVCYXNlSW5mby5wcm90byKYAQoKQ2hhdERldGFpbBIpCgxSb2xlQmFzZUlu" + "Zm8YASABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8SJgoIQ2hhdFR5cGUYAiAB" + "KA4yFC5FQ2hhdFR5cGUuRUNoYXRUeXBlEg4KBkNoYXRUbRgDIAEoAxInCgZB" + "Y3Rpb24YBCABKAsyFy50eXBlcy5DaGF0QWN0aW9uRGV0YWlsQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEChatTypeReflection.Descriptor,
			TypeChatActionDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatDetail), (MessageParser)(object)ChatDetail.Parser, new string[4] { "RoleBaseInfo", "ChatType", "ChatTm", "Action" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
