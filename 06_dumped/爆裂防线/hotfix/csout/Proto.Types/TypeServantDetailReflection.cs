using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeServantDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantDetailReflection()
	{
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlNlcnZhbnREZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29s" + "U3RhdGUucHJvdG8aE2VudW0uRVF1YWxpdHkucHJvdG8aHHR5cGUuU2VydmFu" + "dEF0dHJEZXRhaWwucHJvdG8aHXR5cGUuU2VydmFudFNraWxsRGV0YWlsLnBy" + "b3RvIpgDCg1TZXJ2YW50RGV0YWlsEhEKCVNlcnZhbnRJZBgBIAEoCRISCgpT" + "ZXJ2YW50VGlkGAIgASgFEiMKB1F1YWxpdHkYAyABKA4yEi5FUXVhbGl0eS5F" + "UXVhbGl0eRIUCgxRdWFsaXR5Q291bnQYBCABKAUSDQoFTGV2ZWwYBSABKAUS" + "LAoJTWFpblNraWxsGAYgASgLMhkudHlwZXMuU2VydmFudFNraWxsRGV0YWls" + "Ei8KDFN1YlNraWxsTGlzdBgHIAMoCzIZLnR5cGVzLlNlcnZhbnRTa2lsbERl" + "dGFpbBItCgtTdWJBdHRyTGlzdBgIIAMoCzIYLnR5cGVzLlNlcnZhbnRBdHRy" + "RGV0YWlsEhsKE1N1YkF0dHJVcGdyYWRlQ291bnQYCSABKAUSHQoVU3ViQXR0" + "clVwZ3JhZGVDb3VudGVkGAogASgFEiQKBExvY2sYCyABKA4yFi5FQm9vbFN0" + "YXRlLkVCb29sU3RhdGUSJgoGRGVwbG95GAwgASgOMhYuRUJvb2xTdGF0ZS5F" + "Qm9vbFN0YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEQualityReflection.Descriptor,
			TypeServantAttrDetailReflection.Descriptor,
			TypeServantSkillDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantDetail), (MessageParser)(object)ServantDetail.Parser, new string[12]
			{
				"ServantId", "ServantTid", "Quality", "QualityCount", "Level", "MainSkill", "SubSkillList", "SubAttrList", "SubAttrUpgradeCount", "SubAttrUpgradeCounted",
				"Lock", "Deploy"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
