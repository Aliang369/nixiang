using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossPeakBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzUGVha0J1eUNvbWJhdEFjay5wcm90bxIFdHlwZXMaHHR5" + "cGUuQ3Jvc3NQZWFrU2VsZkluZm8ucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90byJzChVDcm9zc1BlYWtCdXlDb21iYXRBY2sSMwoRQ3Jvc3NQZWFrU2Vs" + "ZkluZm8YASABKAsyGC50eXBlcy5Dcm9zc1BlYWtTZWxmSW5mbxIlCgpEYXRh" + "Q2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeCrossPeakSelfInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakBuyCombatAck), (MessageParser)(object)CrossPeakBuyCombatAck.Parser, new string[2] { "CrossPeakSelfInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
