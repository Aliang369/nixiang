using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonAirdropQualityReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonAirdropQualityReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiBlbnVtLkVTZWFzb25BaXJkcm9wUXVhbGl0eS5wcm90bxIVRVNlYXNvbkFp" + "cmRyb3BRdWFsaXR5KkcKFUVTZWFzb25BaXJkcm9wUXVhbGl0eRIOCgpFbXB0" + "eVZhbHVlEAASCAoER29sZBABEgoKBlB1cnBsZRACEggKBEJsdWUQA0IuWgku" + "Lztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonAirdropQuality) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
