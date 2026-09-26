using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityRollingAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityRollingAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkZhY2lsaXR5Um9sbGluZ0Fjay5wcm90bxIFdHlwZXMaFXR5cGUu" + "RGF0YUNoYW5nZS5wcm90bxogdHlwZS5GYWNpbGl0eVJvbGxpbmdEZXRhaWwu" + "cHJvdG8iagoSRmFjaWxpdHlSb2xsaW5nQWNrEi0KB1JvbGxpbmcYASABKAsy" + "HC50eXBlcy5GYWNpbGl0eVJvbGxpbmdEZXRhaWwSJQoKRGF0YUNoYW5nZRgC" + "IAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeFacilityRollingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityRollingAck), (MessageParser)(object)FacilityRollingAck.Parser, new string[2] { "Rolling", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
