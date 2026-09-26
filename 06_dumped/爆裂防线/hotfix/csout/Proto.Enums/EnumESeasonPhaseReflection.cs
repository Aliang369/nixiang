using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonPhaseReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonPhaseReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVTZWFzb25QaGFzZS5wcm90bxIMRVNlYXNvblBoYXNlKmUKDEVT" + "ZWFzb25QaGFzZRIOCgpFbXB0eVZhbHVlEAASCwoHUHJlcGFyZRABEgwKCEZp" + "Z2h0aW5nEAISDQoJTWlkU2V0dGxlEAMSDwoLRmluYWxTZXR0bGUQBBIKCgZD" + "bG9zZWQQBUIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQ" + "cm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonPhase) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
