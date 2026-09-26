using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELegionOfficialReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELegionOfficialReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVMZWdpb25PZmZpY2lhbC5wcm90bxIPRUxlZ2lvbk9mZmljaWFs" + "KkYKD0VMZWdpb25PZmZpY2lhbBIOCgpFbXB0eVZhbHVlEAASCgoGTWVtYmVy" + "EAESCwoHQWR2aXNlchACEgoKBkxlYWRlchADQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELegionOfficial) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
