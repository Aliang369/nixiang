using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEScrollTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEScrollTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZlbnVtLkVTY3JvbGxUeXBlLnByb3RvEgtFU2Nyb2xsVHlwZSpDCgtFU2Ny" + "b2xsVHlwZRIOCgpFbXB0eVZhbHVlEAASCwoHRVNjcm9sbBABEgkKBUVUdXJu" + "EAISDAoIRUJhcnJhZ2UQA0IuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oC" + "ElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EScrollType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
