using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonCityWarStateNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCityWarStateNtfReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlYXNvbkNpdHlXYXJTdGF0ZU50Zi5wcm90bxIFdHlwZXMaHmVu" + "dW0uRVNlYXNvbkNpdHlXYXJTdGF0ZS5wcm90byJfChVTZWFzb25DaXR5V2Fy" + "U3RhdGVOdGYSNwoFU3RhdGUYASABKA4yKC5FU2Vhc29uQ2l0eVdhclN0YXRl" + "LkVTZWFzb25DaXR5V2FyU3RhdGUSDQoFRW5kVG0YAiABKANCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonCityWarStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCityWarStateNtf), (MessageParser)(object)SeasonCityWarStateNtf.Parser, new string[2] { "State", "EndTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
