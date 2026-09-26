using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeModHandbookDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeModHandbookDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLk1vZEhhbmRib29rRGV0YWlsLnByb3RvEgV0eXBlcxoXZW51bS5F" + "QWN0aXZlU3RhdGUucHJvdG8iUgoRTW9kSGFuZGJvb2tEZXRhaWwSEgoKSGFu" + "ZGJvb2tJZBgBIAEoBRIpCgVTdGF0ZRgCIAEoDjIaLkVBY3RpdmVTdGF0ZS5F" + "QWN0aXZlU3RhdGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEActiveStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ModHandbookDetail), (MessageParser)(object)ModHandbookDetail.Parser, new string[2] { "HandbookId", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
