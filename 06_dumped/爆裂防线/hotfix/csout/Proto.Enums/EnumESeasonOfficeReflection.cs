using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonOfficeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonOfficeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChhlbnVtLkVTZWFzb25PZmZpY2UucHJvdG8SDUVTZWFzb25PZmZpY2UqRwoN" + "RVNlYXNvbk9mZmljZRIOCgpFbXB0eVZhbHVlEAASBgoCUjEQARIGCgJSMhAC" + "EgYKAlIzEAMSBgoCUjQQBBIGCgJSNRAFQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonOffice) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
