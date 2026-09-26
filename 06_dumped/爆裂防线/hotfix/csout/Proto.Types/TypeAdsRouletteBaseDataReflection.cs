using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsRouletteBaseDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsRouletteBaseDataReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkFkc1JvdWxldHRlQmFzZURhdGEucHJvdG8SBXR5cGVzIoUBChNB" + "ZHNSb3VsZXR0ZUJhc2VEYXRhEhEKCU9uY2VMdWNreRgBIAEoBRIQCghNYXhM" + "dWNreRgCIAEoBRINCgVMdWNreRgDIAEoBRISCgpEYWlseUxpbWl0GAQgASgF" + "EhIKCkRhaWx5Q291bnQYBSABKAUSEgoKVG90YWxDb3VudBgGIAEoBUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsRouletteBaseData), (MessageParser)(object)AdsRouletteBaseData.Parser, new string[6] { "OnceLucky", "MaxLucky", "Lucky", "DailyLimit", "DailyCount", "TotalCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
