using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleServantDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleServantDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkJhdHRsZVNlcnZhbnREZXRhaWwucHJvdG8SBXR5cGVzGiN0eXBl" + "LkJhdHRsZVNlcnZhbnRTa2lsbERldGFpbC5wcm90byKnAQoTQmF0dGxlU2Vy" + "dmFudERldGFpbBIRCglTa2lsbFR5cGUYASABKAUSEgoKU2VydmFudFRpZBgC" + "IAEoBRIyCglNYWluU2tpbGwYAyABKAsyHy50eXBlcy5CYXR0bGVTZXJ2YW50" + "U2tpbGxEZXRhaWwSNQoMU3ViU2tpbGxMaXN0GAQgAygLMh8udHlwZXMuQmF0" + "dGxlU2VydmFudFNraWxsRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleServantSkillDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleServantDetail), (MessageParser)(object)BattleServantDetail.Parser, new string[4] { "SkillType", "ServantTid", "MainSkill", "SubSkillList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
