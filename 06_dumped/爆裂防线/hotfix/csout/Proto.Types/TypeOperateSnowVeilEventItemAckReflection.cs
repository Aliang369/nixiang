using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilEventItemAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilEventItemAckReflection()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVTbm93VmVpbEV2ZW50SXRlbUFjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90bxoddHlwZS5Tbm93VmVpbExhbmREZXRhaWwucHJvdG8aHHR5cGUuU25v" + "d1ZlaWxSb2xlU3RhdGUucHJvdG8i0wEKG09wZXJhdGVTbm93VmVpbEV2ZW50" + "SXRlbUFjaxIMCgRVVUlEGAEgASgJEisKCExhbmRMaXN0GAIgAygLMhkudHlw" + "ZXMuU25vd1ZlaWxMYW5kRGV0YWlsEisKCVJvbGVTdGF0ZRgDIAEoCzIYLnR5" + "cGVzLlNub3dWZWlsUm9sZVN0YXRlEiUKCUF3YXJkTGlzdBgEIAMoCzISLnR5" + "cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBSABKAsyES50eXBlcy5E" + "YXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilEventItemAck), (MessageParser)(object)OperateSnowVeilEventItemAck.Parser, new string[5] { "UUID", "LandList", "RoleState", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
