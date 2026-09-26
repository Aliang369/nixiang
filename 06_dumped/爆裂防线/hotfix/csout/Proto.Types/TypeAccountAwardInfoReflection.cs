using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAccountAwardInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAccountAwardInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkFjY291bnRBd2FyZEluZm8ucHJvdG8SBXR5cGVzGhVlbnVtLkVC" + "b29sU3RhdGUucHJvdG8aGXR5cGUuUmVjaGFyZ2VSZWJhdGUucHJvdG8ibAoQ" + "QWNjb3VudEF3YXJkSW5mbxIpCglCaW5kU3RhdGUYASABKA4yFi5FQm9vbFN0" + "YXRlLkVCb29sU3RhdGUSLQoOUmVjaGFyZ2VSZWJhdGUYAiABKAsyFS50eXBl" + "cy5SZWNoYXJnZVJlYmF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeRechargeRebateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AccountAwardInfo), (MessageParser)(object)AccountAwardInfo.Parser, new string[2] { "BindState", "RechargeRebate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
