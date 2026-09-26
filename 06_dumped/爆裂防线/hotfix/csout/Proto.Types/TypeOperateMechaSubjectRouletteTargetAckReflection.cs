using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateMechaSubjectRouletteTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateMechaSubjectRouletteTargetAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci90eXBlLk9wZXJhdGVNZWNoYVN1YmplY3RSb3VsZXR0ZVRhcmdldEFjay5w" + "cm90bxIFdHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0" + "YUNoYW5nZS5wcm90bxoddHlwZS5NZWNoYVN1YmplY3REZXRhaWwucHJvdG8i" + "qQEKJE9wZXJhdGVNZWNoYVN1YmplY3RSb3VsZXR0ZVRhcmdldEFjaxIzChBN" + "ZWNoYVN1YmplY3RMaXN0GAEgAygLMhkudHlwZXMuTWVjaGFTdWJqZWN0RGV0" + "YWlsEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUK" + "CkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMechaSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateMechaSubjectRouletteTargetAck), (MessageParser)(object)OperateMechaSubjectRouletteTargetAck.Parser, new string[3] { "MechaSubjectList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
