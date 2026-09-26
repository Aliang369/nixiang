using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAuxResourceQuickBuyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxResourceQuickBuyAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkF1eFJlc291cmNlUXVpY2tCdXlBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8i" + "ZgoWQXV4UmVzb3VyY2VRdWlja0J1eUFjaxIlCglBd2FyZExpc3QYASADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlw" + "ZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxResourceQuickBuyAck), (MessageParser)(object)AuxResourceQuickBuyAck.Parser, new string[2] { "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
