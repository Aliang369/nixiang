using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperatePropExchangeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperatePropExchangeAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLk9wZXJhdGVQcm9wRXhjaGFuZ2VBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8a" + "HXR5cGUuUHJvcEV4Y2hhbmdlRGV0YWlsLnByb3RvIpsBChZPcGVyYXRlUHJv" + "cEV4Y2hhbmdlQWNrEjMKEFByb3BFeGNoYW5nZUxpc3QYASADKAsyGS50eXBl" + "cy5Qcm9wRXhjaGFuZ2VEZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIudHlw" + "ZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRh" + "dGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypePropExchangeDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperatePropExchangeAck), (MessageParser)(object)OperatePropExchangeAck.Parser, new string[3] { "PropExchangeList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
