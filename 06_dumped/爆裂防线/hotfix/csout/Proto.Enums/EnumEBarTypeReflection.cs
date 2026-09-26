using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEBarTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEBarTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNlbnVtLkVCYXJUeXBlLnByb3RvEghFQmFyVHlwZSpfCghFQmFyVHlwZRIO" + "CgpFbXB0eVZhbHVlEAASCgoGTm9ybWFsEAESCwoHU3BlY2lhbBACEgwKCFRy" + "ZWFzdXJlEAMSEAoMVHJlYXN1cmVQbHVzEAQSCgoGVGFjdGljEAVCLloJLi87" + "cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EBarType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
