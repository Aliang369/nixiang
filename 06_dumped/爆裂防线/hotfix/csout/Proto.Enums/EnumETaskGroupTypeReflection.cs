using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumETaskGroupTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumETaskGroupTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChllbnVtLkVUYXNrR3JvdXBUeXBlLnByb3RvEg5FVGFza0dyb3VwVHlwZSo6" + "Cg5FVGFza0dyb3VwVHlwZRIOCgpFbXB0eVZhbHVlEAASDAoIUGFyYWxsZWwQ" + "ARIKCgZTZXJpYWwQAkIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpi" + "Z2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ETaskGroupType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
