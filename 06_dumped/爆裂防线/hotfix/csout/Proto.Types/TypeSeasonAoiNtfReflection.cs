using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonAoiNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAoiNtfReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlNlYXNvbkFvaU50Zi5wcm90bxIFdHlwZXMaGnR5cGUuU2Vhc29u" + "UGxheWVyUG9zLnByb3RvIlEKDFNlYXNvbkFvaU50ZhIqCgpBcHBlYXJMaXN0" + "GAEgAygLMhYudHlwZXMuU2Vhc29uUGxheWVyUG9zEhUKDURpc2FwcGVhckxp" + "c3QYAiADKANCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonPlayerPosReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAoiNtf), (MessageParser)(object)SeasonAoiNtf.Parser, new string[2] { "AppearList", "DisappearList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
