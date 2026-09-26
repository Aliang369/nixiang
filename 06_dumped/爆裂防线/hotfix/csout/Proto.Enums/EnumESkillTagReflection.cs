using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESkillTagReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESkillTagReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVTa2lsbFRhZy5wcm90bxIJRVNraWxsVGFnKk4KCUVTa2lsbFRh" + "ZxIOCgpFbXB0eVZhbHVlEAASDQoJQmFsbGlzdGljEAESCQoFTGFzZXIQAhIK" + "CgZHcm91bmQQAxILCgdFeHBsb2RlEARCLloJLi87cHJvdG9jqgILUHJvdG8u" + "RW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESkillTag) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
