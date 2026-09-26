using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilitySelfInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilitySelfInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkZhY2lsaXR5U2VsZkluZm8ucHJvdG8SBXR5cGVzGht0eXBlLkZh" + "Y2lsaXR5QmFzZUluZm8ucHJvdG8aIHR5cGUuRmFjaWxpdHlSb2xsaW5nRGV0" + "YWlsLnByb3RvIpEBChBGYWNpbGl0eVNlbGZJbmZvEicKBkNyZWF0ZRgBIAEo" + "CzIXLnR5cGVzLkZhY2lsaXR5QmFzZUluZm8SJQoESm9pbhgCIAEoCzIXLnR5" + "cGVzLkZhY2lsaXR5QmFzZUluZm8SLQoHUm9sbGluZxgDIAEoCzIcLnR5cGVz" + "LkZhY2lsaXR5Um9sbGluZ0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeFacilityBaseInfoReflection.Descriptor,
			TypeFacilityRollingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilitySelfInfo), (MessageParser)(object)FacilitySelfInfo.Parser, new string[3] { "Create", "Join", "Rolling" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
