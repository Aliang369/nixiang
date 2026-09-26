using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaActiveAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLk1lY2hhQWN0aXZlQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5EYXRh" + "Q2hhbmdlLnByb3RvGhR0eXBlLk1lY2hhSW5mby5wcm90byJtCg5NZWNoYUFj" + "dGl2ZUFjaxIPCgdNZWNoYUlkGAEgASgFEiMKCU1lY2hhSW5mbxgCIAEoCzIQ" + "LnR5cGVzLk1lY2hhSW5mbxIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMu" + "RGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeMechaInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaActiveAck), (MessageParser)(object)MechaActiveAck.Parser, new string[3] { "MechaId", "MechaInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
