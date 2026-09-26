using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeResourceCardDoubleInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeResourceCardDoubleInfoReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlJlc291cmNlQ2FyZERvdWJsZUluZm8ucHJvdG8SBXR5cGVzGhRl" + "bnVtLkVGdW5jVHlwZS5wcm90byI+ChZSZXNvdXJjZUNhcmREb3VibGVJbmZv" + "EiQKBkZ1bmNJZBgBIAEoDjIULkVGdW5jVHlwZS5FRnVuY1R5cGVCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEFuncTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ResourceCardDoubleInfo), (MessageParser)(object)ResourceCardDoubleInfo.Parser, new string[1] { "FuncId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
