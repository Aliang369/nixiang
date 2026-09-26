using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSnowVeilDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSnowVeilDetailReflection()
	{
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNub3dWZWlsRGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5FQm9v" + "bFN0YXRlLnByb3RvGhp0eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90bxomdHlw" + "ZS5PcGVyYXRlU25vd1ZlaWxUYXJnZXREZXRhaWwucHJvdG8aJ3R5cGUuT3Bl" + "cmF0ZVRhcmdldE5leHRHcmFuZERldGFpbC5wcm90bxoddHlwZS5Tbm93VmVp" + "bExhbmREZXRhaWwucHJvdG8aHXR5cGUuU25vd1ZlaWxQcm9wRGV0YWlsLnBy" + "b3RvGhx0eXBlLlNub3dWZWlsUm9sZVN0YXRlLnByb3RvGiB0eXBlLlNub3dW" + "ZWlsUm9sbGluZ0RldGFpbC5wcm90bxoidHlwZS5Tbm93VmVpbFR1cm50YWJs" + "ZURldGFpbC5wcm90byLaAwoOU25vd1ZlaWxEZXRhaWwSKAoIQmFzZUluZm8Y" + "ASABKAsyFi50eXBlcy5PcGVyYXRlQmFzZUluZm8SLQoNQWN0aXZhdGVBd2Fy" + "ZBgCIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZRISCgpUb3RhbFNjb3Jl" + "GAMgASgFEisKCExhbmRMaXN0GAQgAygLMhkudHlwZXMuU25vd1ZlaWxMYW5k" + "RGV0YWlsEjEKCVR1cm50YWJsZRgFIAEoCzIeLnR5cGVzLlNub3dWZWlsVHVy" + "bnRhYmxlRGV0YWlsEi0KB1JvbGxpbmcYBiABKAsyHC50eXBlcy5Tbm93VmVp" + "bFJvbGxpbmdEZXRhaWwSKwoJUm9sZVN0YXRlGAcgASgLMhgudHlwZXMuU25v" + "d1ZlaWxSb2xlU3RhdGUSJwoEUHJvcBgIIAEoCzIZLnR5cGVzLlNub3dWZWls" + "UHJvcERldGFpbBI4CgxUYXJnZXREZXRhaWwYCSABKAsyIi50eXBlcy5PcGVy" + "YXRlU25vd1ZlaWxUYXJnZXREZXRhaWwSPAoPVGFyZ2V0TmV4dEdyYW5kGAog" + "ASgLMiMudHlwZXMuT3BlcmF0ZVRhcmdldE5leHRHcmFuZERldGFpbEIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[9]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateSnowVeilTargetDetailReflection.Descriptor,
			TypeOperateTargetNextGrandDetailReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilPropDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor,
			TypeSnowVeilRollingDetailReflection.Descriptor,
			TypeSnowVeilTurntableDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SnowVeilDetail), (MessageParser)(object)SnowVeilDetail.Parser, new string[10] { "BaseInfo", "ActivateAward", "TotalScore", "LandList", "Turntable", "Rolling", "RoleState", "Prop", "TargetDetail", "TargetNextGrand" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
