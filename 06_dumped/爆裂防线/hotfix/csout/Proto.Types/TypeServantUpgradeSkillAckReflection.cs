using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantUpgradeSkillAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantUpgradeSkillAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlNlcnZhbnRVcGdyYWRlU2tpbGxBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkRhdGFDaGFuZ2UucHJvdG8aFnR5cGUuU2VydmFudEluZm8ucHJvdG8i" + "aAoWU2VydmFudFVwZ3JhZGVTa2lsbEFjaxInCgtTZXJ2YW50SW5mbxgBIAEo" + "CzISLnR5cGVzLlNlcnZhbnRJbmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50" + "eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantUpgradeSkillAck), (MessageParser)(object)ServantUpgradeSkillAck.Parser, new string[2] { "ServantInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
