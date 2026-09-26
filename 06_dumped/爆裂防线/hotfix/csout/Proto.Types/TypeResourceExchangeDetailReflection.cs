using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeResourceExchangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeResourceExchangeDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlJlc291cmNlRXhjaGFuZ2VEZXRhaWwucHJvdG8SBXR5cGVzGh10" + "eXBlLkV4Y2hhbmdlSXRlbURldGFpbC5wcm90bxoadHlwZS5PcGVyYXRlQmFz" + "ZUluZm8ucHJvdG8icwoWUmVzb3VyY2VFeGNoYW5nZURldGFpbBIoCghCYXNl" + "SW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNlSW5mbxIvCgxFeGNoYW5n" + "ZUxpc3QYAiADKAsyGS50eXBlcy5FeGNoYW5nZUl0ZW1EZXRhaWxCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeExchangeItemDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ResourceExchangeDetail), (MessageParser)(object)ResourceExchangeDetail.Parser, new string[2] { "BaseInfo", "ExchangeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
