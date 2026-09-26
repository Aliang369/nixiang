using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckTransportRefreshAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckTransportRefreshAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlRydWNrVHJhbnNwb3J0UmVmcmVzaEFjay5wcm90bxIFdHlwZXMa" + "FXR5cGUuRGF0YUNoYW5nZS5wcm90bxoYdHlwZS5UcnVja1NlbGZJbmZvLnBy" + "b3RvIm4KGFRydWNrVHJhbnNwb3J0UmVmcmVzaEFjaxIrCg1UcnVja1NlbGZJ" + "bmZvGAEgASgLMhQudHlwZXMuVHJ1Y2tTZWxmSW5mbxIlCgpEYXRhQ2hhbmdl" + "GAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTruckSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckTransportRefreshAck), (MessageParser)(object)TruckTransportRefreshAck.Parser, new string[2] { "TruckSelfInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
