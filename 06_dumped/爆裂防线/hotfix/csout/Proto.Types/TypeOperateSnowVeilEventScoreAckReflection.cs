using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilEventScoreAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilEventScoreAckReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVTbm93VmVpbEV2ZW50U2NvcmVBY2sucHJvdG8SBXR5" + "cGVzGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aJnR5cGUuT3BlcmF0ZVNub3dW" + "ZWlsVGFyZ2V0RGV0YWlsLnByb3RvGid0eXBlLk9wZXJhdGVUYXJnZXROZXh0" + "R3JhbmREZXRhaWwucHJvdG8aHXR5cGUuU25vd1ZlaWxMYW5kRGV0YWlsLnBy" + "b3RvGhx0eXBlLlNub3dWZWlsUm9sZVN0YXRlLnByb3RvIrkCChxPcGVyYXRl" + "U25vd1ZlaWxFdmVudFNjb3JlQWNrEgwKBFVVSUQYASABKAkSEgoKVG90YWxT" + "Y29yZRgCIAEoBRI4CgxUYXJnZXREZXRhaWwYAyABKAsyIi50eXBlcy5PcGVy" + "YXRlU25vd1ZlaWxUYXJnZXREZXRhaWwSPAoPVGFyZ2V0TmV4dEdyYW5kGAQg" + "ASgLMiMudHlwZXMuT3BlcmF0ZVRhcmdldE5leHRHcmFuZERldGFpbBIrCghM" + "YW5kTGlzdBgFIAMoCzIZLnR5cGVzLlNub3dWZWlsTGFuZERldGFpbBIrCglS" + "b2xlU3RhdGUYBiABKAsyGC50eXBlcy5Tbm93VmVpbFJvbGVTdGF0ZRIlCgpE" + "YXRhQ2hhbmdlGAcgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeDataChangeReflection.Descriptor,
			TypeOperateSnowVeilTargetDetailReflection.Descriptor,
			TypeOperateTargetNextGrandDetailReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilEventScoreAck), (MessageParser)(object)OperateSnowVeilEventScoreAck.Parser, new string[7] { "UUID", "TotalScore", "TargetDetail", "TargetNextGrand", "LandList", "RoleState", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
