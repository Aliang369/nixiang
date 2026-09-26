using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBarrageCmdDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBarrageCmdDetailReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkJhcnJhZ2VDbWREZXRhaWwucHJvdG8SBXR5cGVzGhZ0eXBlLkF3" + "YXJkRGV0YWlsLnByb3RvGh90eXBlLkJhcnJhZ2VDbWRMYW5nRGV0YWlsLnBy" + "b3RvGhp0eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90byKzAQoQQmFycmFnZUNt" + "ZERldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNl" + "SW5mbxIQCghMaW1pdE51bRgCIAEoBRINCgVDb3VudBgDIAEoBRItCghMYW5n" + "TGlzdBgEIAMoCzIbLnR5cGVzLkJhcnJhZ2VDbWRMYW5nRGV0YWlsEiUKCUF3" + "YXJkTGlzdBgFIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBarrageCmdLangDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BarrageCmdDetail), (MessageParser)(object)BarrageCmdDetail.Parser, new string[5] { "BaseInfo", "LimitNum", "Count", "LangList", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
