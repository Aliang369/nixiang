using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEChatTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEChatTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVDaGF0VHlwZS5wcm90bxIJRUNoYXRUeXBlKlwKCUVDaGF0VHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASCgoGRVdvcmxkEAESCwoHRVNlcnZlchACEgsK" + "B0VMZWdpb24QAxIMCghFUHJpdmF0ZRAEEgsKB0VTeXN0ZW0QBUIuWgkuLztw" + "cm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EChatType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
