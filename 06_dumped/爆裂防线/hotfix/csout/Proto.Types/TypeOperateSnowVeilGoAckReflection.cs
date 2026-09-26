using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilGoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilGoAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk9wZXJhdGVTbm93VmVpbEdvQWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5EYXRhQ2hhbmdlLnByb3RvGh10eXBlLlNub3dWZWlsTGFuZERldGFpbC5w" + "cm90bxocdHlwZS5Tbm93VmVpbFJvbGVTdGF0ZS5wcm90byKlAQoUT3BlcmF0" + "ZVNub3dWZWlsR29BY2sSDAoEVVVJRBgBIAEoCRIrCghMYW5kTGlzdBgCIAMo" + "CzIZLnR5cGVzLlNub3dWZWlsTGFuZERldGFpbBIrCglSb2xlU3RhdGUYAyAB" + "KAsyGC50eXBlcy5Tbm93VmVpbFJvbGVTdGF0ZRIlCgpEYXRhQ2hhbmdlGAQg" + "ASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilGoAck), (MessageParser)(object)OperateSnowVeilGoAck.Parser, new string[4] { "UUID", "LandList", "RoleState", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
