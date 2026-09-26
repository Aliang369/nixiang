using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePassportBuyLevelDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePassportBuyLevelDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlBhc3Nwb3J0QnV5TGV2ZWxEZXRhaWwucHJvdG8SBXR5cGVzIm4K" + "FlBhc3Nwb3J0QnV5TGV2ZWxEZXRhaWwSDQoFSW5kZXgYASABKAUSDwoHRGlh" + "bW9uZBgCIAEoBRIQCghBZGRMZXZlbBgDIAEoBRIQCghMaW1pdE51bRgEIAEo" + "BRIQCghCdXlDb3VudBgFIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PassportBuyLevelDetail), (MessageParser)(object)PassportBuyLevelDetail.Parser, new string[5] { "Index", "Diamond", "AddLevel", "LimitNum", "BuyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
