using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaFastUpgradeLevelCalcAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaFastUpgradeLevelCalcAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk1lY2hhRmFzdFVwZ3JhZGVMZXZlbENhbGNBY2sucHJvdG8SBXR5" + "cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvImcKHE1lY2hhRmFzdFVwZ3Jh" + "ZGVMZXZlbENhbGNBY2sSDwoHTWVjaGFJZBgBIAEoBRIPCgdUb0xldmVsGAIg" + "ASgFEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaFastUpgradeLevelCalcAck), (MessageParser)(object)MechaFastUpgradeLevelCalcAck.Parser, new string[3] { "MechaId", "ToLevel", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
