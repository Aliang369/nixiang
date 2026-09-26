using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEWorkerRarityTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEWorkerRarityTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVXb3JrZXJSYXJpdHlUeXBlLnByb3RvEhFFV29ya2VyUmFyaXR5" + "VHlwZSpAChFFV29ya2VyUmFyaXR5VHlwZRIOCgpFbXB0eVZhbHVlEAASBQoB" + "QhABEgUKAUEQAhIFCgFTEAMSBgoCU1MQBEIuWgkuLztwcm90b2OqAgtQcm90" + "by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EWorkerRarityType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
