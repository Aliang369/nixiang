using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePassportLevelInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePassportLevelInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlBhc3Nwb3J0TGV2ZWxJbmZvLnByb3RvEgV0eXBlcxohdHlwZS5Q" + "YXNzcG9ydEJ1eUxldmVsRGV0YWlsLnByb3RvIokBChFQYXNzcG9ydExldmVs" + "SW5mbxINCgVMZXZlbBgBIAEoBRILCgNFeHAYAiABKAUSEAoITG93QXdhcmQY" + "AyABKAUSEQoJSGlnaEF3YXJkGAQgASgFEjMKDEJ1eUxldmVsTGlzdBgFIAMo" + "CzIdLnR5cGVzLlBhc3Nwb3J0QnV5TGV2ZWxEZXRhaWxCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypePassportBuyLevelDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PassportLevelInfo), (MessageParser)(object)PassportLevelInfo.Parser, new string[5] { "Level", "Exp", "LowAward", "HighAward", "BuyLevelList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
