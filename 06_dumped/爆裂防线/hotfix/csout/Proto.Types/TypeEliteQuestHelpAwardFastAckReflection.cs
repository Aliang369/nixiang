using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestHelpAwardFastAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestHelpAwardFastAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkVsaXRlUXVlc3RIZWxwQXdhcmRGYXN0QWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvGhN0eXBlLkhlbHBJbmZvLnByb3RvGht0eXBlLkhlbHBSZWNvcmREZXRh" + "aWwucHJvdG8iugEKGkVsaXRlUXVlc3RIZWxwQXdhcmRGYXN0QWNrEisKClJl" + "Y29yZExpc3QYASADKAsyFy50eXBlcy5IZWxwUmVjb3JkRGV0YWlsEiEKCEhl" + "bHBJbmZvGAIgASgLMg8udHlwZXMuSGVscEluZm8SJQoJQXdhcmRMaXN0GAMg" + "AygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIR" + "LnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeHelpInfoReflection.Descriptor,
			TypeHelpRecordDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestHelpAwardFastAck), (MessageParser)(object)EliteQuestHelpAwardFastAck.Parser, new string[4] { "RecordList", "HelpInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
