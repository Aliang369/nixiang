using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESkillAttackTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESkillAttackTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVTa2lsbEF0dGFja1R5cGUucHJvdG8SEEVTa2lsbEF0dGFja1R5" + "cGUqXgoQRVNraWxsQXR0YWNrVHlwZRIOCgpFbXB0eVZhbHVlEAASCAoERWxl" + "YxABEggKBEZpcmUQAhIHCgNJY2UQAxIHCgNQaHkQBBIKCgZFbmVyZ3kQBRII" + "CgRXaW5kEAZCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVc" + "UHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESkillAttackType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
