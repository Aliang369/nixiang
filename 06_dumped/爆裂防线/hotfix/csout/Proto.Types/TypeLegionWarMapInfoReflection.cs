using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarMapInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarMapInfoReflection()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkxlZ2lvbldhck1hcEluZm8ucHJvdG8SBXR5cGVzGhplbnVtLkVM" + "ZWdpb25XYXJGb3JjZS5wcm90bxoaZW51bS5FTGVnaW9uV2FyU3RhdGUucHJv" + "dG8aJHR5cGUuTGVnaW9uV2FyRm9yY2VTdGF0ZURldGFpbC5wcm90bxokdHlw" + "ZS5MZWdpb25XYXJQb2ludFN0YXRlRGV0YWlsLnByb3RvIoICChBMZWdpb25X" + "YXJNYXBJbmZvEjIKCFdhclN0YXRlGAEgASgOMiAuRUxlZ2lvbldhclN0YXRl" + "LkVMZWdpb25XYXJTdGF0ZRIQCghTZXR0bGVUbRgCIAEoAxI0CgpXaW5Gb3Jj" + "ZUlkGAMgASgOMiAuRUxlZ2lvbldhckZvcmNlLkVMZWdpb25XYXJGb3JjZRI4" + "Cg5Gb3JjZVN0YXRlTGlzdBgEIAMoCzIgLnR5cGVzLkxlZ2lvbldhckZvcmNl" + "U3RhdGVEZXRhaWwSOAoOUG9pbnRTdGF0ZUxpc3QYBSADKAsyIC50eXBlcy5M" + "ZWdpb25XYXJQb2ludFN0YXRlRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumELegionWarForceReflection.Descriptor,
			EnumELegionWarStateReflection.Descriptor,
			TypeLegionWarForceStateDetailReflection.Descriptor,
			TypeLegionWarPointStateDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarMapInfo), (MessageParser)(object)LegionWarMapInfo.Parser, new string[5] { "WarState", "SettleTm", "WinForceId", "ForceStateList", "PointStateList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
