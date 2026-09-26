using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaResetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaResetAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLk1lY2hhUmVzZXRBY2sucHJvdG8SBXR5cGVzGhZ0eXBlLkF3YXJk" + "RGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFHR5cGUuTWVj" + "aGFJbmZvLnByb3RvIoIBCg1NZWNoYVJlc2V0QWNrEiMKCU1lY2hhSW5mbxgB" + "IAEoCzIQLnR5cGVzLk1lY2hhSW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50" + "eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMu" + "RGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMechaInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaResetAck), (MessageParser)(object)MechaResetAck.Parser, new string[3] { "MechaInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
