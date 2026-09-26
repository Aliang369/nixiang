using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperatePropExchangeTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperatePropExchangeTargetAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVQcm9wRXhjaGFuZ2VUYXJnZXRBY2sucHJvdG8SBXR5" + "cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2Uu" + "cHJvdG8aHXR5cGUuUHJvcEV4Y2hhbmdlRGV0YWlsLnByb3RvIqEBChxPcGVy" + "YXRlUHJvcEV4Y2hhbmdlVGFyZ2V0QWNrEjMKEFByb3BFeGNoYW5nZUxpc3QY" + "ASADKAsyGS50eXBlcy5Qcm9wRXhjaGFuZ2VEZXRhaWwSJQoJQXdhcmRMaXN0" + "GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypePropExchangeDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperatePropExchangeTargetAck), (MessageParser)(object)OperatePropExchangeTargetAck.Parser, new string[3] { "PropExchangeList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
