using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonTechNodeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonTechNodeReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNlYXNvblRlY2hOb2RlLnByb3RvEgV0eXBlcyJmCg5TZWFzb25U" + "ZWNoTm9kZRIOCgZUZWNoSWQYASABKAUSEAoIQ2F0ZWdvcnkYAiABKAUSDQoF" + "TGV2ZWwYAyABKAUSEAoITWF4TGV2ZWwYBCABKAUSEQoJTmVlZFBvaW50GAUg" + "ASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonTechNode), (MessageParser)(object)SeasonTechNode.Parser, new string[5] { "TechId", "Category", "Level", "MaxLevel", "NeedPoint" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
