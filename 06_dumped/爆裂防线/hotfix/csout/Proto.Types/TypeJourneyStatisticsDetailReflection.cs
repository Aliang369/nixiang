using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyStatisticsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyStatisticsDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkpvdXJuZXlTdGF0aXN0aWNzRGV0YWlsLnByb3RvEgV0eXBlcxok" + "dHlwZS5Kb3VybmV5VGFyZ2V0UmVjb3JkRGV0YWlsLnByb3RvIl4KF0pvdXJu" + "ZXlTdGF0aXN0aWNzRGV0YWlsEg0KBVJlYWNoGAEgASgFEjQKClRhcmdldExp" + "c3QYAiADKAsyIC50eXBlcy5Kb3VybmV5VGFyZ2V0UmVjb3JkRGV0YWlsQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeJourneyTargetRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyStatisticsDetail), (MessageParser)(object)JourneyStatisticsDetail.Parser, new string[2] { "Reach", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
