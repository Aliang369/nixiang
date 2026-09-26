using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonStageTaskNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonStageTaskNtfReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlNlYXNvblN0YWdlVGFza050Zi5wcm90bxIFdHlwZXMiVQoSU2Vh" + "c29uU3RhZ2VUYXNrTnRmEgsKA1RpZBgBIAEoBRIQCghUYXNrVHlwZRgCIAEo" + "BRIQCghQcm9ncmVzcxgDIAEoBRIOCgZUYXJnZXQYBCABKAVCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonStageTaskNtf), (MessageParser)(object)SeasonStageTaskNtf.Parser, new string[4] { "Tid", "TaskType", "Progress", "Target" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
