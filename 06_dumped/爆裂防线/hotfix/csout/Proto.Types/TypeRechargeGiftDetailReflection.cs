using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeGiftDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeGiftDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlJlY2hhcmdlR2lmdERldGFpbC5wcm90bxIFdHlwZXMaGnR5cGUu" + "T3BlcmF0ZUJhc2VJbmZvLnByb3RvGh90eXBlLlJlY2hhcmdlRGlyZWN0RGV0" + "YWlsLnByb3RvGh50eXBlLlJlY2hhcmdlR29vZHNEZXRhaWwucHJvdG8ingEK" + "ElJlY2hhcmdlR2lmdERldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVz" + "Lk9wZXJhdGVCYXNlSW5mbxItCglHb29kc0xpc3QYAiADKAsyGi50eXBlcy5S" + "ZWNoYXJnZUdvb2RzRGV0YWlsEi8KCkRpcmVjdExpc3QYAyADKAsyGy50eXBl" + "cy5SZWNoYXJnZURpcmVjdERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeRechargeDirectDetailReflection.Descriptor,
			TypeRechargeGoodsDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeGiftDetail), (MessageParser)(object)RechargeGiftDetail.Parser, new string[3] { "BaseInfo", "GoodsList", "DirectList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
