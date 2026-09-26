using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePayVoucherConsumeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayVoucherConsumeAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlBheVZvdWNoZXJDb25zdW1lQWNrLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGh10" + "eXBlLlJlY2hhcmdlQWN0aXZlSW5mby5wcm90bxobdHlwZS5SZWNoYXJnZVNo" + "b3BJbmZvLnByb3RvGhh0eXBlLlN1YnNjcmliZUluZm8ucHJvdG8i+wEKFFBh" + "eVZvdWNoZXJDb25zdW1lQWNrEjEKEFJlY2hhcmdlU2hvcEluZm8YASABKAsy" + "Fy50eXBlcy5SZWNoYXJnZVNob3BJbmZvEjUKElJlY2hhcmdlQWN0aXZlSW5m" + "bxgCIAEoCzIZLnR5cGVzLlJlY2hhcmdlQWN0aXZlSW5mbxIrCg1TdWJzY3Jp" + "YmVJbmZvGAMgASgLMhQudHlwZXMuU3Vic2NyaWJlSW5mbxIlCglBd2FyZExp" + "c3QYBCADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAUg" + "ASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor,
			TypeRechargeShopInfoReflection.Descriptor,
			TypeSubscribeInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayVoucherConsumeAck), (MessageParser)(object)PayVoucherConsumeAck.Parser, new string[5] { "RechargeShopInfo", "RechargeActiveInfo", "SubscribeInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
