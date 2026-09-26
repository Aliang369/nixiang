using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilEventRollingAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilEventRollingAckReflection()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVTbm93VmVpbEV2ZW50Um9sbGluZ0Fjay5wcm90bxIF" + "dHlwZXMaFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoddHlwZS5Tbm93VmVpbExh" + "bmREZXRhaWwucHJvdG8aHHR5cGUuU25vd1ZlaWxSb2xlU3RhdGUucHJvdG8a" + "IHR5cGUuU25vd1ZlaWxSb2xsaW5nRGV0YWlsLnByb3RvIt4BCh5PcGVyYXRl" + "U25vd1ZlaWxFdmVudFJvbGxpbmdBY2sSDAoEVVVJRBgBIAEoCRIrCghMYW5k" + "TGlzdBgCIAMoCzIZLnR5cGVzLlNub3dWZWlsTGFuZERldGFpbBItCgdSb2xs" + "aW5nGAMgASgLMhwudHlwZXMuU25vd1ZlaWxSb2xsaW5nRGV0YWlsEisKCVJv" + "bGVTdGF0ZRgEIAEoCzIYLnR5cGVzLlNub3dWZWlsUm9sZVN0YXRlEiUKCkRh" + "dGFDaGFuZ2UYBSABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor,
			TypeSnowVeilRollingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilEventRollingAck), (MessageParser)(object)OperateSnowVeilEventRollingAck.Parser, new string[5] { "UUID", "LandList", "Rolling", "RoleState", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
