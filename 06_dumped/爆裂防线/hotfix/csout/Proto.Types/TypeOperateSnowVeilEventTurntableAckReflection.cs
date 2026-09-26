using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilEventTurntableAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilEventTurntableAckReflection()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLk9wZXJhdGVTbm93VmVpbEV2ZW50VHVybnRhYmxlQWNrLnByb3Rv" + "EgV0eXBlcxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGh10eXBlLlNub3dWZWls" + "TGFuZERldGFpbC5wcm90bxocdHlwZS5Tbm93VmVpbFJvbGVTdGF0ZS5wcm90" + "bxoidHlwZS5Tbm93VmVpbFR1cm50YWJsZURldGFpbC5wcm90byLkAQogT3Bl" + "cmF0ZVNub3dWZWlsRXZlbnRUdXJudGFibGVBY2sSDAoEVVVJRBgBIAEoCRIr" + "CghMYW5kTGlzdBgCIAMoCzIZLnR5cGVzLlNub3dWZWlsTGFuZERldGFpbBIx" + "CglUdXJudGFibGUYAyABKAsyHi50eXBlcy5Tbm93VmVpbFR1cm50YWJsZURl" + "dGFpbBIrCglSb2xlU3RhdGUYBCABKAsyGC50eXBlcy5Tbm93VmVpbFJvbGVT" + "dGF0ZRIlCgpEYXRhQ2hhbmdlGAUgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor,
			TypeSnowVeilTurntableDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilEventTurntableAck), (MessageParser)(object)OperateSnowVeilEventTurntableAck.Parser, new string[5] { "UUID", "LandList", "Turntable", "RoleState", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
