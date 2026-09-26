using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonAreaStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonAreaStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVTZWFzb25BcmVhU3RhdGUucHJvdG8SEEVTZWFzb25BcmVhU3Rh" + "dGUqWAoQRVNlYXNvbkFyZWFTdGF0ZRIOCgpFbXB0eVZhbHVlEAASCwoHTmV1" + "dHJhbBABEg0KCUNvbnRlc3RlZBACEgwKCE9jY3VwaWVkEAMSCgoGRmFsbGVu" + "EARCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9c" + "RW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonAreaState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
