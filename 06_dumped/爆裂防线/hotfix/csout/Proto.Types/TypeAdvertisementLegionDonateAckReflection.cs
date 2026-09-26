using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementLegionDonateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementLegionDonateAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkFkdmVydGlzZW1lbnRMZWdpb25Eb25hdGVBY2sucHJvdG8SBXR5" + "cGVzGhx0eXBlLkFkdmVydGlzZW1lbnRJbmZvLnByb3RvGhZ0eXBlLkF3YXJk" + "RGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFXR5cGUuTGVn" + "aW9uSW5mby5wcm90bxoZdHlwZS5MZWdpb25TZWxmSW5mby5wcm90byL3AQoc" + "QWR2ZXJ0aXNlbWVudExlZ2lvbkRvbmF0ZUFjaxIzChFBZHZlcnRpc2VtZW50" + "SW5mbxgBIAEoCzIYLnR5cGVzLkFkdmVydGlzZW1lbnRJbmZvEi0KDkxlZ2lv" + "blNlbGZJbmZvGAIgASgLMhUudHlwZXMuTGVnaW9uU2VsZkluZm8SJQoKTGVn" + "aW9uSW5mbxgDIAEoCzIRLnR5cGVzLkxlZ2lvbkluZm8SJQoJQXdhcmRMaXN0" + "GAQgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgFIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementLegionDonateAck), (MessageParser)(object)AdvertisementLegionDonateAck.Parser, new string[5] { "AdvertisementInfo", "LegionSelfInfo", "LegionInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
