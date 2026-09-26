using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumETruckPrivilegeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumETruckPrivilegeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVUcnVja1ByaXZpbGVnZS5wcm90bxIPRVRydWNrUHJpdmlsZWdl" + "KlsKD0VUcnVja1ByaXZpbGVnZRIOCgpFbXB0eVZhbHVlEAASDgoKVW5sb2Nr" + "U2xvdBABEhQKEFRyYW5zcG9ydFJlZnJlc2gQAhISCg5UcmFuc3BvcnRDb3Vu" + "dBADQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3Rv" + "XEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ETruckPrivilege) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
