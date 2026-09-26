using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementGiftAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementGiftAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkFkdmVydGlzZW1lbnRHaWZ0QXdhcmRBY2sucHJvdG8SBXR5cGVz" + "Ghx0eXBlLkFkdmVydGlzZW1lbnRJbmZvLnByb3RvGhZ0eXBlLkF3YXJkRGV0" + "YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ingEKGUFkdmVydGlz" + "ZW1lbnRHaWZ0QXdhcmRBY2sSMwoRQWR2ZXJ0aXNlbWVudEluZm8YASABKAsy" + "GC50eXBlcy5BZHZlcnRpc2VtZW50SW5mbxIlCglBd2FyZExpc3QYAiADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlw" + "ZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementGiftAwardAck), (MessageParser)(object)AdvertisementGiftAwardAck.Parser, new string[3] { "AdvertisementInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
