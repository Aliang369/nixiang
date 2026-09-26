using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeDirectDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeDirectDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlJlY2hhcmdlRGlyZWN0RGV0YWlsLnByb3RvEgV0eXBlcyJhChRS" + "ZWNoYXJnZURpcmVjdERldGFpbBIQCghEaXJlY3RJZBgBIAEoBRIQCghMaW1p" + "dE51bRgCIAEoBRIQCghMaW1pdFZpcBgDIAEoBRITCgtCb3VnaHRDb3VudBgE" + "IAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeDirectDetail), (MessageParser)(object)RechargeDirectDetail.Parser, new string[4] { "DirectId", "LimitNum", "LimitVip", "BoughtCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
