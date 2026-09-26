using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonFuncReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonFuncReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZlbnVtLkVTZWFzb25GdW5jLnByb3RvEgtFU2Vhc29uRnVuYypzCgtFU2Vh" + "c29uRnVuYxIOCgpFbXB0eVZhbHVlEAASDwoLTGVhZGVyRWxlY3QQZBILCgdD" + "aXR5V2FyEGUSDwoLRGVjbGFyZUhpZ2gQZhILCgdBaXJkcm9wEGcSDgoKU2Vh" + "c29uU2hvcBBoEggKBFRlY2gQaUIuWgkuLztwcm90b2OqAgtQcm90by5FbnVt" + "c8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonFunc) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
