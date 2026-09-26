using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonForceReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonForceReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVTZWFzb25Gb3JjZS5wcm90bxIMRVNlYXNvbkZvcmNlKlAKDEVT" + "ZWFzb25Gb3JjZRIQCgxGb3JjZU5ldXRyYWwQABIKCgZGb3JjZUEQARIKCgZG" + "b3JjZUIQAhIKCgZGb3JjZUMQAxIKCgZGb3JjZUQQBEIuWgkuLztwcm90b2Oq" + "AgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonForce) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
