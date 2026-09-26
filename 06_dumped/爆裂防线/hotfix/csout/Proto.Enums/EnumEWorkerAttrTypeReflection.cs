using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEWorkerAttrTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEWorkerAttrTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVXb3JrZXJBdHRyVHlwZS5wcm90bxIPRVdvcmtlckF0dHJUeXBl" + "KkEKD0VXb3JrZXJBdHRyVHlwZRIOCgpFbXB0eVZhbHVlEAASCQoFU3RyZW4Q" + "ARIICgRBZ2lsEAISCQoFSW50ZWwQA0IuWgkuLztwcm90b2OqAgtQcm90by5F" + "bnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EWorkerAttrType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
