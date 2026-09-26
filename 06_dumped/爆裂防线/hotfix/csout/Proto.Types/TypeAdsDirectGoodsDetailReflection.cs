using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsDirectGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsDirectGoodsDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkFkc0RpcmVjdEdvb2RzRGV0YWlsLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90byJwChRBZHNEaXJlY3RHb29kc0RldGFpbBIN" + "CgVJbmRleBgBIAEoBRIQCghMaW1pdE51bRgCIAEoBRIQCghCdXlDb3VudBgD" + "IAEoBRIlCglBd2FyZExpc3QYBCADKAsyEi50eXBlcy5Bd2FyZERldGFpbEIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsDirectGoodsDetail), (MessageParser)(object)AdsDirectGoodsDetail.Parser, new string[4] { "Index", "LimitNum", "BuyCount", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
