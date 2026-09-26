using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSuitActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSuitActiveAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlN1aXRBY3RpdmVBY2sucHJvdG8SBXR5cGVzGhV0eXBlLkRhdGFD" + "aGFuZ2UucHJvdG8aGXR5cGUuUGxheWVyU3VpdEluZm8ucHJvdG8igQEKDVN1" + "aXRBY3RpdmVBY2sSDgoGU3VpdElkGAEgASgFEicKCFN1aXRJbmZvGAIgASgL" + "MhUudHlwZXMuUGxheWVyU3VpdEluZm8SEAoISGVhZExpc3QYAyADKAUSJQoK" + "RGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SuitActiveAck), (MessageParser)(object)SuitActiveAck.Parser, new string[4] { "SuitId", "SuitInfo", "HeadList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
