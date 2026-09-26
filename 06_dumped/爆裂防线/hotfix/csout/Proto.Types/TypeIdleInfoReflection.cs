using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeIdleInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeIdleInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChN0eXBlLklkbGVJbmZvLnByb3RvEgV0eXBlcxoWdHlwZS5Bd2FyZERldGFp" + "bC5wcm90byJYCghJZGxlSW5mbxIOCgZHYWluVG0YASABKAMSFQoNRmFzdEdh" + "aW5Db3VudBgCIAEoBRIlCglBd2FyZExpc3QYAyADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(IdleInfo), (MessageParser)(object)IdleInfo.Parser, new string[3] { "GainTm", "FastGainCount", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
