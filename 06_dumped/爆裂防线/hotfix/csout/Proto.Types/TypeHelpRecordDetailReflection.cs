using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHelpRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHelpRecordDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkhlbHBSZWNvcmREZXRhaWwucHJvdG8SBXR5cGVzGhd0eXBlLlJv" + "bGVCYXNlSW5mby5wcm90byJ1ChBIZWxwUmVjb3JkRGV0YWlsEhAKCFJlY29y" + "ZElkGAEgASgFEhIKCkVsaXRlTGV2ZWwYAiABKAUSEAoIUmVjb3JkVG0YAyAB" + "KAMSKQoMUm9sZUJhc2VJbmZvGAQgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HelpRecordDetail), (MessageParser)(object)HelpRecordDetail.Parser, new string[4] { "RecordId", "EliteLevel", "RecordTm", "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
