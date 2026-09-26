using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonDataAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlNlYXNvbkRhdGFBY2sucHJvdG8SBXR5cGVzGhdlbnVtLkVTZWFz" + "b25Gb3JjZS5wcm90byKWAQoNU2Vhc29uRGF0YUFjaxIrCgdGb3JjZUlkGAEg" + "ASgOMhouRVNlYXNvbkZvcmNlLkVTZWFzb25Gb3JjZRINCgVNZXJpdBgCIAEo" + "BRISCgpEdXJhYmlsaXR5GAMgASgFEhUKDURlZkR1cmFiaWxpdHkYBCABKAUS" + "CwoDR2FzGAUgASgFEhEKCVRlY2hQb2ludBgGIAEoBUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonDataAck), (MessageParser)(object)SeasonDataAck.Parser, new string[6] { "ForceId", "Merit", "Durability", "DefDurability", "Gas", "TechPoint" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
