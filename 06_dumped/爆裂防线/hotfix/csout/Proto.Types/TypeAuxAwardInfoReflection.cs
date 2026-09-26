using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAuxAwardInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxAwardInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkF1eEF3YXJkSW5mby5wcm90bxIFdHlwZXMaG3R5cGUuQWNjb3Vu" + "dEF3YXJkSW5mby5wcm90bxokdHlwZS5SYW5rRmlyc3RSZWFjaEF3YXJkRGV0" + "YWlsLnByb3RvIocBCgxBdXhBd2FyZEluZm8SMQoQQWNjb3VudEF3YXJkSW5m" + "bxgBIAEoCzIXLnR5cGVzLkFjY291bnRBd2FyZEluZm8SEAoIRnVuY0xpc3QY" + "AiADKAUSMgoIUmFua0xpc3QYAyADKAsyIC50eXBlcy5SYW5rRmlyc3RSZWFj" + "aEF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAccountAwardInfoReflection.Descriptor,
			TypeRankFirstReachAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxAwardInfo), (MessageParser)(object)AuxAwardInfo.Parser, new string[3] { "AccountAwardInfo", "FuncList", "RankList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
