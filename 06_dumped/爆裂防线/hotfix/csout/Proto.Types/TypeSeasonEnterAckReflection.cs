using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonEnterAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonEnterAckReflection()
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNlYXNvbkVudGVyQWNrLnByb3RvEgV0eXBlcxoXZW51bS5FU2Vh" + "c29uRm9yY2UucHJvdG8aF2VudW0uRVNlYXNvblBoYXNlLnByb3RvIsUCCg5T" + "ZWFzb25FbnRlckFjaxIQCghTZWFzb25JZBgBIAEoBRIpCgVQaGFzZRgCIAEo" + "DjIaLkVTZWFzb25QaGFzZS5FU2Vhc29uUGhhc2USDwoHU3RhcnRUbRgDIAEo" + "AxINCgVFbmRUbRgEIAEoAxIrCgdGb3JjZUlkGAUgASgOMhouRVNlYXNvbkZv" + "cmNlLkVTZWFzb25Gb3JjZRIRCglGb3JjZU5hbWUYBiABKAkSEQoJRm9yY2VG" + "bGFnGAcgASgJEhMKC0ZvcmNlTm90aWNlGAggASgJEg0KBU1lcml0GAkgASgF" + "EhIKCkR1cmFiaWxpdHkYCiABKAUSFQoNRGVmRHVyYWJpbGl0eRgLIAEoBRIL" + "CgNHYXMYDCABKAUSEQoJVGVjaFBvaW50GA0gASgFEhQKDE5leHRTZWFzb25U" + "bRgOIAEoA0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			EnumESeasonPhaseReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonEnterAck), (MessageParser)(object)SeasonEnterAck.Parser, new string[14]
			{
				"SeasonId", "Phase", "StartTm", "EndTm", "ForceId", "ForceName", "ForceFlag", "ForceNotice", "Merit", "Durability",
				"DefDurability", "Gas", "TechPoint", "NextSeasonTm"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
