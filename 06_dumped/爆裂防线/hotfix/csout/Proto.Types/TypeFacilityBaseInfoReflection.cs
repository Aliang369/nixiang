using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeFacilityBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityBaseInfoReflection()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkZhY2lsaXR5QmFzZUluZm8ucHJvdG8SBXR5cGVzGhVlbnVtLkVC" + "b29sU3RhdGUucHJvdG8aE2VudW0uRVF1YWxpdHkucHJvdG8aF3R5cGUuUm9s" + "ZUJhc2VJbmZvLnByb3RvIpcCChBGYWNpbGl0eUJhc2VJbmZvEhIKCkZhY2ls" + "aXR5SWQYASABKAUSFAoMRmFjaWxpdHlUeXBlGAIgASgFEisKD0ZhY2lsaXR5" + "UXVhbGl0eRgDIAEoDjISLkVRdWFsaXR5LkVRdWFsaXR5EhMKC0ZhY2lsaXR5" + "TWFwGAQgASgFEiwKD0ZhY2lsaXR5Q3JlYXRvchgFIAEoCzITLnR5cGVzLlJv" + "bGVCYXNlSW5mbxIUCgxFeHBsb3JlU2hhcmUYBiABKAUSFAoMRXhwbG9yZVRv" + "dGFsGAcgASgFEhAKCEV4cGlyZVRtGAggASgDEisKC0RvdWJsZUF3YXJkGAkg" + "ASgOMhYuRUJvb2xTdGF0ZS5FQm9vbFN0YXRlQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEQualityReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityBaseInfo), (MessageParser)(object)FacilityBaseInfo.Parser, new string[9] { "FacilityId", "FacilityType", "FacilityQuality", "FacilityMap", "FacilityCreator", "ExploreShare", "ExploreTotal", "ExpireTm", "DoubleAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
