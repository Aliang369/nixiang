using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonLeaderStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonLeaderStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVTZWFzb25MZWFkZXJTdGF0ZS5wcm90bxISRVNlYXNvbkxlYWRl" + "clN0YXRlKk0KEkVTZWFzb25MZWFkZXJTdGF0ZRIOCgpFbXB0eVZhbHVlEAAS" + "CAoETm9uZRABEg0KCUluY3VtYmVudBACEg4KCkltcGVhY2hpbmcQA0IuWgku" + "Lztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonLeaderState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
