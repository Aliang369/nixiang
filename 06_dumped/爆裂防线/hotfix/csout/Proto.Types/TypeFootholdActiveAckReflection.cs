using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFootholdActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFootholdActiveAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkZvb3Rob2xkQWN0aXZlQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5E" + "YXRhQ2hhbmdlLnByb3RvGhd0eXBlLkZvb3Rob2xkSW5mby5wcm90byJ5ChFG" + "b290aG9sZEFjdGl2ZUFjaxISCgpGb290aG9sZElkGAEgASgFEikKDEZvb3Ro" + "b2xkSW5mbxgCIAEoCzITLnR5cGVzLkZvb3Rob2xkSW5mbxIlCgpEYXRhQ2hh" + "bmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeFootholdInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FootholdActiveAck), (MessageParser)(object)FootholdActiveAck.Parser, new string[3] { "FootholdId", "FootholdInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
