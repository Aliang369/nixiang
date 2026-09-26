using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaFastUpgradeLevelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaFastUpgradeLevelAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLk1lY2hhRmFzdFVwZ3JhZGVMZXZlbEFjay5wcm90bxIFdHlwZXMa" + "FXR5cGUuRGF0YUNoYW5nZS5wcm90bxoUdHlwZS5NZWNoYUluZm8ucHJvdG8i" + "ZgoYTWVjaGFGYXN0VXBncmFkZUxldmVsQWNrEiMKCU1lY2hhSW5mbxgBIAEo" + "CzIQLnR5cGVzLk1lY2hhSW5mbxIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlw" + "ZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeMechaInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaFastUpgradeLevelAck), (MessageParser)(object)MechaFastUpgradeLevelAck.Parser, new string[2] { "MechaInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
