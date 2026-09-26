using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeModHandbookTypeTargetReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeModHandbookTypeTargetReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk1vZEhhbmRib29rVHlwZVRhcmdldC5wcm90bxIFdHlwZXMaF2Vu" + "dW0uRUFjdGl2ZVN0YXRlLnByb3RvGhVlbnVtLkVTa2lsbFR5cGUucHJvdG8i" + "fAoVTW9kSGFuZGJvb2tUeXBlVGFyZ2V0EikKCVNraWxsVHlwZRgBIAEoDjIW" + "LkVTa2lsbFR5cGUuRVNraWxsVHlwZRINCgVDb3VudBgCIAEoBRIpCgVTdGF0" + "ZRgDIAEoDjIaLkVBY3RpdmVTdGF0ZS5FQWN0aXZlU3RhdGVCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEActiveStateReflection.Descriptor,
			EnumESkillTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ModHandbookTypeTarget), (MessageParser)(object)ModHandbookTypeTarget.Parser, new string[3] { "SkillType", "Count", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
