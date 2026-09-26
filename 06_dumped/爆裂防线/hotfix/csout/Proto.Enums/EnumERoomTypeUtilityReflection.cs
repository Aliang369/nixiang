using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumERoomTypeUtilityReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumERoomTypeUtilityReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVSb29tVHlwZVV0aWxpdHkucHJvdG8SEEVSb29tVHlwZVV0aWxp" + "dHkqSQoQRVJvb21UeXBlVXRpbGl0eRIOCgpFbXB0eVZhbHVlEAASCAoESGFs" + "bBABEg0KCUNvbW1SYWRhchACEgwKCFNwZWNpbWVuEANCLloJLi87cHJvdG9j" + "qgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ERoomTypeUtility) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
