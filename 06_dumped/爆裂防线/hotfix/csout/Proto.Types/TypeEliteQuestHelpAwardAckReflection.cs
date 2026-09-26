using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestHelpAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestHelpAwardAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkVsaXRlUXVlc3RIZWxwQXdhcmRBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8a" + "E3R5cGUuSGVscEluZm8ucHJvdG8aG3R5cGUuSGVscFJlY29yZERldGFpbC5w" + "cm90byK2AQoWRWxpdGVRdWVzdEhlbHBBd2FyZEFjaxIrCgpSZWNvcmRMaXN0" + "GAEgAygLMhcudHlwZXMuSGVscFJlY29yZERldGFpbBIhCghIZWxwSW5mbxgC" + "IAEoCzIPLnR5cGVzLkhlbHBJbmZvEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5" + "cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5E" + "YXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeHelpInfoReflection.Descriptor,
			TypeHelpRecordDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestHelpAwardAck), (MessageParser)(object)EliteQuestHelpAwardAck.Parser, new string[4] { "RecordList", "HelpInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
