using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarOpenInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarOpenInfoReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkxlZ2lvbldhck9wZW5JbmZvLnByb3RvEgV0eXBlcyLLAQoRTGVn" + "aW9uV2FyT3BlbkluZm8SFgoOUHJldmlld1N0YXJ0VG0YASABKAMSFAoMUHJl" + "dmlld0VuZFRtGAIgASgDEhUKDVNpZ25VcFN0YXJ0VG0YAyABKAMSEwoLU2ln" + "blVwRW5kVG0YBCABKAMSFgoOUHJlcGFyZVN0YXJ0VG0YBSABKAMSFAoMUHJl" + "cGFyZUVuZFRtGAYgASgDEhcKD0ZpZ2h0aW5nU3RhcnRUbRgHIAEoAxIVCg1G" + "aWdodGluZ0VuZFRtGAggASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarOpenInfo), (MessageParser)(object)LegionWarOpenInfo.Parser, new string[8] { "PreviewStartTm", "PreviewEndTm", "SignUpStartTm", "SignUpEndTm", "PrepareStartTm", "PrepareEndTm", "FightingStartTm", "FightingEndTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
