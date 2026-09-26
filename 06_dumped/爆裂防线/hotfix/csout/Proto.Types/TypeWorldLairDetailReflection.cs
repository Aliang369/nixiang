using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldLairDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLldvcmxkTGFpckRldGFpbC5wcm90bxIFdHlwZXMaFWVudW0uRUJv" + "b2xTdGF0ZS5wcm90byKiAQoPV29ybGRMYWlyRGV0YWlsEgwKBFVVSUQYASAB" + "KAkSDgoGTGFpcklkGAIgASgFEg0KBUxldmVsGAMgASgFEhEKCUtpbGxDb3Vu" + "dBgEIAEoBRIQCghCZXN0S2lsbBgFIAEoBRISCgpQYXNzVGFyZ2V0GAYgASgF" + "EikKCVBhc3NTdGF0ZRgHIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairDetail), (MessageParser)(object)WorldLairDetail.Parser, new string[7] { "UUID", "LairId", "Level", "KillCount", "BestKill", "PassTarget", "PassState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
