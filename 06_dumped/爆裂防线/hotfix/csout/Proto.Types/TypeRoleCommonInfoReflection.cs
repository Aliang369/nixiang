using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleCommonInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleCommonInfoReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlJvbGVDb21tb25JbmZvLnByb3RvEgV0eXBlcyK5AQoOUm9sZUNv" + "bW1vbkluZm8SEAoIQ3JlYXRlVG0YASABKAMSEwoLTGFzdFJlZnJlc2gYAiAB" + "KAMSEwoLTmV4dFJlZnJlc2gYAyABKAMSEgoKU2VydmVyRGF5cxgEIAEoBRIP" + "CgdSb2xlRXhwGAUgASgFEhYKDlJvbGVOYW1lQ2hhbmdlGAYgASgFEhYKDlJl" + "Y2hhcmdlQW1vdW50GAcgASgCEhYKDlNlcnZlck9wZW5UaW1lGAggASgDQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleCommonInfo), (MessageParser)(object)RoleCommonInfo.Parser, new string[8] { "CreateTm", "LastRefresh", "NextRefresh", "ServerDays", "RoleExp", "RoleNameChange", "RechargeAmount", "ServerOpenTime" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
