using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantUpgradeSubSkillAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantUpgradeSubSkillAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLlNlcnZhbnRVcGdyYWRlU3ViU2tpbGxBY2sucHJvdG8SBXR5cGVz" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFnR5cGUuU2VydmFudEluZm8ucHJv" + "dG8iawoZU2VydmFudFVwZ3JhZGVTdWJTa2lsbEFjaxInCgtTZXJ2YW50SW5m" + "bxgBIAEoCzISLnR5cGVzLlNlcnZhbnRJbmZvEiUKCkRhdGFDaGFuZ2UYAiAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantUpgradeSubSkillAck), (MessageParser)(object)ServantUpgradeSubSkillAck.Parser, new string[2] { "ServantInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
