using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJiZiDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJiZiDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkppWmlEZXRhaWwucHJvdG8SBXR5cGVzGhp0eXBlLk9wZXJhdGVC" + "YXNlSW5mby5wcm90bxokdHlwZS5PcGVyYXRlSmlaaUV4Y2hhbmdlRGV0YWls" + "LnByb3RvGiB0eXBlLk9wZXJhdGVKaVppUHJvcERldGFpbC5wcm90byKeAQoK" + "SmlaaURldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVC" + "YXNlSW5mbxIuCghQcm9wTGlzdBgCIAMoCzIcLnR5cGVzLk9wZXJhdGVKaVpp" + "UHJvcERldGFpbBI2CgxFeGNoYW5nZUxpc3QYAyADKAsyIC50eXBlcy5PcGVy" + "YXRlSmlaaUV4Y2hhbmdlRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateJiZiExchangeDetailReflection.Descriptor,
			TypeOperateJiZiPropDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JiZiDetail), (MessageParser)(object)JiZiDetail.Parser, new string[3] { "BaseInfo", "PropList", "ExchangeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
