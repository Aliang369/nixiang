using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckTransportCompleteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckTransportCompleteAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLlRydWNrVHJhbnNwb3J0Q29tcGxldGVBY2sucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJv" + "dG8aGHR5cGUuVHJ1Y2tTZWxmSW5mby5wcm90bxoddHlwZS5UcnVja1RyYW5z" + "cG9ydEluZm8ucHJvdG8izQEKGVRydWNrVHJhbnNwb3J0Q29tcGxldGVBY2sS" + "KwoNVHJ1Y2tTZWxmSW5mbxgBIAEoCzIULnR5cGVzLlRydWNrU2VsZkluZm8S" + "NQoSVHJ1Y2tUcmFuc3BvcnRJbmZvGAIgASgLMhkudHlwZXMuVHJ1Y2tUcmFu" + "c3BvcnRJbmZvEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0" + "YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeTruckSelfInfoReflection.Descriptor,
			TypeTruckTransportInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckTransportCompleteAck), (MessageParser)(object)TruckTransportCompleteAck.Parser, new string[4] { "TruckSelfInfo", "TruckTransportInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
