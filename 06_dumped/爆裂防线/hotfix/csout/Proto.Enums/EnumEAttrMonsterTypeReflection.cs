using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEAttrMonsterTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEAttrMonsterTypeReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVBdHRyTW9uc3RlclR5cGUucHJvdG8SEEVBdHRyTW9uc3RlclR5" + "cGUqnwEKEEVBdHRyTW9uc3RlclR5cGUSDgoKRW1wdHlWYWx1ZRAAEgYKAkhw" + "EAESCgoGRGFtYWdlEAISDwoLQXR0YWNrU3BlZWQQAxIPCgtBdHRhY2tSYW5n" + "ZRAEEg0KCU1vdmVTcGVlZBAFEgwKCE1vdmVUeXBlEAYSDQoJQm9keVNjYWxl" + "EAcSCAoETWFzcxAIEg8KC0RhbWFnZVRha2VuEAlCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EAttrMonsterType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
