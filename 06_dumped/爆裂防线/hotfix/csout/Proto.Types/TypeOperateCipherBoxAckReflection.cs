using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateCipherBoxAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateCipherBoxAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk9wZXJhdGVDaXBoZXJCb3hBY2sucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhp0eXBlLkNpcGhlckJveERldGFpbC5wcm90" + "bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvIpIBChNPcGVyYXRlQ2lwaGVyQm94" + "QWNrEi0KDUNpcGhlckJveExpc3QYASADKAsyFi50eXBlcy5DaXBoZXJCb3hE" + "ZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwS" + "JQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeCipherBoxDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateCipherBoxAck), (MessageParser)(object)OperateCipherBoxAck.Parser, new string[3] { "CipherBoxList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
