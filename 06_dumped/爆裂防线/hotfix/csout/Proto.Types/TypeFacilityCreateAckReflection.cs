using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityCreateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityCreateAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkZhY2lsaXR5Q3JlYXRlQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5E" + "YXRhQ2hhbmdlLnByb3RvGhp0eXBlLkZhY2lsaXR5TWFwSW5mby5wcm90bxob" + "dHlwZS5GYWNpbGl0eVNlbGZJbmZvLnByb3RvIp4BChFGYWNpbGl0eUNyZWF0" + "ZUFjaxIxChBGYWNpbGl0eVNlbGZJbmZvGAEgASgLMhcudHlwZXMuRmFjaWxp" + "dHlTZWxmSW5mbxIvCg9GYWNpbGl0eU1hcEluZm8YAiABKAsyFi50eXBlcy5G" + "YWNpbGl0eU1hcEluZm8SJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRh" + "dGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilitySelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityCreateAck), (MessageParser)(object)FacilityCreateAck.Parser, new string[3] { "FacilitySelfInfo", "FacilityMapInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
