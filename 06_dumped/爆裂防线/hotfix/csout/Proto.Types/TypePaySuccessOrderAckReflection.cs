using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePaySuccessOrderAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePaySuccessOrderAckReflection()
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlBheVN1Y2Nlc3NPcmRlckFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoWdHlw" + "ZS5PcmRlckRldGFpbC5wcm90bxoddHlwZS5SZWNoYXJnZUFjdGl2ZUluZm8u" + "cHJvdG8aG3R5cGUuUmVjaGFyZ2VTaG9wSW5mby5wcm90bxoYdHlwZS5TdWJz" + "Y3JpYmVJbmZvLnByb3RvGhJ0eXBlLlZpcEluZm8ucHJvdG8i1QIKElBheVN1" + "Y2Nlc3NPcmRlckFjaxISCgpUb3RhbFByaWNlGAEgASgCEjEKEFJlY2hhcmdl" + "U2hvcEluZm8YAiABKAsyFy50eXBlcy5SZWNoYXJnZVNob3BJbmZvEjUKElJl" + "Y2hhcmdlQWN0aXZlSW5mbxgDIAEoCzIZLnR5cGVzLlJlY2hhcmdlQWN0aXZl" + "SW5mbxIfCgdWaXBJbmZvGAQgASgLMg4udHlwZXMuVmlwSW5mbxIrCg1TdWJz" + "Y3JpYmVJbmZvGAUgASgLMhQudHlwZXMuU3Vic2NyaWJlSW5mbxIlCglBd2Fy" + "ZExpc3QYBiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdl" + "GAcgASgLMhEudHlwZXMuRGF0YUNoYW5nZRIlCglPcmRlckxpc3QYCCADKAsy" + "Ei50eXBlcy5PcmRlckRldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[7]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeOrderDetailReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor,
			TypeRechargeShopInfoReflection.Descriptor,
			TypeSubscribeInfoReflection.Descriptor,
			TypeVipInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PaySuccessOrderAck), (MessageParser)(object)PaySuccessOrderAck.Parser, new string[8] { "TotalPrice", "RechargeShopInfo", "RechargeActiveInfo", "VipInfo", "SubscribeInfo", "AwardList", "DataChange", "OrderList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
