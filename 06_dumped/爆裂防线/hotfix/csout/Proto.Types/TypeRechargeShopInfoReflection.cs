using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeShopInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeShopInfoReflection()
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlJlY2hhcmdlU2hvcEluZm8ucHJvdG8SBXR5cGVzGhx0eXBlLkRp" + "YW1vbmRTaG9wRGV0YWlsLnByb3RvGhl0eXBlLkZ1bmNTaG9wRGV0YWlsLnBy" + "b3RvGh90eXBlLkdyb3d0aEdpZnRTaG9wRGV0YWlsLnByb3RvGiB0eXBlLlJl" + "Y2hhcmdlR29vZHNCYXNlSW5mby5wcm90bxocdHlwZS5WaXBDYXJkU2hvcERl" + "dGFpbC5wcm90bxocdHlwZS5Wb3VjaGVyU2hvcERldGFpbC5wcm90byLlAgoQ" + "UmVjaGFyZ2VTaG9wSW5mbxItCgtEaWFtb25kU2hvcBgBIAMoCzIYLnR5cGVz" + "LkRpYW1vbmRTaG9wRGV0YWlsEi0KC1ZpcENhcmRTaG9wGAIgAygLMhgudHlw" + "ZXMuVmlwQ2FyZFNob3BEZXRhaWwSLQoLVm91Y2hlclNob3AYAyADKAsyGC50" + "eXBlcy5Wb3VjaGVyU2hvcERldGFpbBInCghGdW5jU2hvcBgEIAMoCzIVLnR5" + "cGVzLkZ1bmNTaG9wRGV0YWlsEjMKDkdyb3d0aEdpZnRTaG9wGAUgAygLMhsu" + "dHlwZXMuR3Jvd3RoR2lmdFNob3BEZXRhaWwSMQoLUGF0aHdheVNob3AYBiAD" + "KAsyHC50eXBlcy5SZWNoYXJnZUdvb2RzQmFzZUluZm8SMwoNUmVwYXltZW50" + "U2hvcBgHIAMoCzIcLnR5cGVzLlJlY2hhcmdlR29vZHNCYXNlSW5mb0IuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeDiamondShopDetailReflection.Descriptor,
			TypeFuncShopDetailReflection.Descriptor,
			TypeGrowthGiftShopDetailReflection.Descriptor,
			TypeRechargeGoodsBaseInfoReflection.Descriptor,
			TypeVipCardShopDetailReflection.Descriptor,
			TypeVoucherShopDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeShopInfo), (MessageParser)(object)RechargeShopInfo.Parser, new string[7] { "DiamondShop", "VipCardShop", "VoucherShop", "FuncShop", "GrowthGiftShop", "PathwayShop", "RepaymentShop" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
