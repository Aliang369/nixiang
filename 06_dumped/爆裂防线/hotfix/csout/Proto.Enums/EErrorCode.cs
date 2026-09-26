using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EErrorCode
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Unknown")]
	Unknown = 30000,
	[OriginalName("ServiceException")]
	ServiceException = 30001,
	[OriginalName("ServiceBusy")]
	ServiceBusy = 30003,
	[OriginalName("ServiceClose")]
	ServiceClose = 30005,
	[OriginalName("ActionOverload")]
	ActionOverload = 30007,
	[OriginalName("RouterNotFound")]
	RouterNotFound = 30009,
	[OriginalName("NetworkBusy")]
	NetworkBusy = 30011,
	[OriginalName("NewVersionFound")]
	NewVersionFound = 30013,
	[OriginalName("NotSupport")]
	NotSupport = 30021,
	[OriginalName("ServerCloseRegister")]
	ServerCloseRegister = 30023,
	[OriginalName("IllegalRequest")]
	IllegalRequest = 30031,
	[OriginalName("IllegalData")]
	IllegalData = 30033,
	[OriginalName("InvalidRequest")]
	InvalidRequest = 30035,
	[OriginalName("ParamsError")]
	ParamsError = 30037,
	[OriginalName("SignError")]
	SignError = 30039,
	[OriginalName("DataNotExists")]
	DataNotExists = 30041,
	[OriginalName("DataAbort")]
	DataAbort = 30043,
	[OriginalName("DataExpired")]
	DataExpired = 30045,
	[OriginalName("AccountAbnormal")]
	AccountAbnormal = 30051,
	[OriginalName("AccountExists")]
	AccountExists = 30053,
	[OriginalName("AccountBindFail")]
	AccountBindFail = 30055,
	[OriginalName("AccountBindNotMet")]
	AccountBindNotMet = 30057,
	[OriginalName("AccountBindTimeLimit")]
	AccountBindTimeLimit = 30059,
	[OriginalName("SDKServerAbnormal")]
	SdkserverAbnormal = 30061,
	[OriginalName("SDKTokenError")]
	SdktokenError = 30063,
	[OriginalName("RepeatLoginGame")]
	RepeatLoginGame = 30071,
	[OriginalName("AlreadyLoginGame")]
	AlreadyLoginGame = 30073,
	[OriginalName("NotLoginGame")]
	NotLoginGame = 30075,
	[OriginalName("UserTicketError")]
	UserTicketError = 30077,
	[OriginalName("OrderAbnormal")]
	OrderAbnormal = 30081,
	[OriginalName("OrderCreateFail")]
	OrderCreateFail = 30083,
	[OriginalName("GameConfigLost")]
	GameConfigLost = 30091,
	[OriginalName("NotAllowedAction")]
	NotAllowedAction = 30201,
	[OriginalName("RepeatAction")]
	RepeatAction = 30203,
	[OriginalName("RepeatGain")]
	RepeatGain = 30205,
	[OriginalName("RepeatBattleResult")]
	RepeatBattleResult = 30207,
	[OriginalName("UnableMatch")]
	UnableMatch = 30211,
	[OriginalName("NotExists")]
	NotExists = 30213,
	[OriginalName("AlreadyExists")]
	AlreadyExists = 30215,
	[OriginalName("NoChange")]
	NoChange = 30217,
	[OriginalName("NotOpen")]
	NotOpen = 30221,
	[OriginalName("NotInSignUpTime")]
	NotInSignUpTime = 30222,
	[OriginalName("NotInOpenTime")]
	NotInOpenTime = 30223,
	[OriginalName("NotSignUp")]
	NotSignUp = 30224,
	[OriginalName("NotActive")]
	NotActive = 30225,
	[OriginalName("NotStart")]
	NotStart = 30227,
	[OriginalName("NoSweep")]
	NoSweep = 30229,
	[OriginalName("TimeRangeError")]
	TimeRangeError = 30231,
	[OriginalName("InCD")]
	InCd = 30233,
	[OriginalName("StateAbnormal")]
	StateAbnormal = 30235,
	[OriginalName("AlreadyFinished")]
	AlreadyFinished = 30237,
	[OriginalName("NotEmpty")]
	NotEmpty = 30241,
	[OriginalName("NameExists")]
	NameExists = 30243,
	[OriginalName("NameLengthLimit")]
	NameLengthLimit = 30245,
	[OriginalName("LengthLimit")]
	LengthLimit = 30247,
	[OriginalName("CodeInvalidId")]
	CodeInvalidId = 30261,
	[OriginalName("CodeUsed")]
	CodeUsed = 30263,
	[OriginalName("CodeLimitCount")]
	CodeLimitCount = 30265,
	[OriginalName("ActivityExpired")]
	ActivityExpired = 30301,
	[OriginalName("NotReach")]
	NotReach = 30303,
	[OriginalName("ReachLimit")]
	ReachLimit = 30305,
	[OriginalName("ReachDailyLimit")]
	ReachDailyLimit = 30307,
	[OriginalName("ReachTypeLimit")]
	ReachTypeLimit = 30309,
	[OriginalName("NotUnlock")]
	NotUnlock = 30321,
	[OriginalName("RoleLevelLimit")]
	RoleLevelLimit = 30323,
	[OriginalName("LevelLimit")]
	LevelLimit = 30325,
	[OriginalName("LevelReachLimit")]
	LevelReachLimit = 30327,
	[OriginalName("MoreThanLimit")]
	MoreThanLimit = 30329,
	[OriginalName("PreLevelPassLimit")]
	PreLevelPassLimit = 30331,
	[OriginalName("PreDiffPassLimit")]
	PreDiffPassLimit = 30333,
	[OriginalName("QualityLimit")]
	QualityLimit = 30341,
	[OriginalName("QualityReachLimit")]
	QualityReachLimit = 30343,
	[OriginalName("TypeLimit")]
	TypeLimit = 30351,
	[OriginalName("CombatNumLimit")]
	CombatNumLimit = 30353,
	[OriginalName("VipLimit")]
	VipLimit = 30355,
	[OriginalName("TwoCardLimit")]
	TwoCardLimit = 30357,
	[OriginalName("VipCardFuncNotActive")]
	VipCardFuncNotActive = 30359,
	[OriginalName("TreasurePrivilegeNotActive")]
	TreasurePrivilegeNotActive = 30361,
	[OriginalName("AwardConditionLimit")]
	AwardConditionLimit = 30371,
	[OriginalName("CollectAward")]
	CollectAward = 30373,
	[OriginalName("CollectNumLimit")]
	CollectNumLimit = 30375,
	[OriginalName("BattleRecordInvalid")]
	BattleRecordInvalid = 30377,
	[OriginalName("MailNotExists")]
	MailNotExists = 30381,
	[OriginalName("MailUUIDExists")]
	MailUuidexists = 30383,
	[OriginalName("MoveUnableReach")]
	MoveUnableReach = 30391,
	[OriginalName("FriendsLimit")]
	FriendsLimit = 30501,
	[OriginalName("HisFriendsLimit")]
	HisFriendsLimit = 30503,
	[OriginalName("FriendsRelationNotExists")]
	FriendsRelationNotExists = 30505,
	[OriginalName("LegionHaveJoined")]
	LegionHaveJoined = 30521,
	[OriginalName("LegionNotJoin")]
	LegionNotJoin = 30523,
	[OriginalName("LegionNameExists")]
	LegionNameExists = 30525,
	[OriginalName("LegionAccessLimit")]
	LegionAccessLimit = 30527,
	[OriginalName("LegionDissolve")]
	LegionDissolve = 30529,
	[OriginalName("LegionMembersLimit")]
	LegionMembersLimit = 30531,
	[OriginalName("LegionJoinQuickNotMeet")]
	LegionJoinQuickNotMeet = 30533,
	[OriginalName("LegionLeaderQuitLimit")]
	LegionLeaderQuitLimit = 30535,
	[OriginalName("ResourceLimit")]
	ResourceLimit = 31001,
	[OriginalName("ResourceTypeError")]
	ResourceTypeError = 31003,
	[OriginalName("ResourceCostNumError")]
	ResourceCostNumError = 31005,
	[OriginalName("ResourceCostConfigError")]
	ResourceCostConfigError = 31007,
	[OriginalName("ResourceOutputConfigError")]
	ResourceOutputConfigError = 31011,
	[OriginalName("ResourceNotEnough")]
	ResourceNotEnough = 31031,
	[OriginalName("GoldNotEnough")]
	GoldNotEnough = 31033,
	[OriginalName("DiamondNotEnough")]
	DiamondNotEnough = 31035,
	[OriginalName("VoucherNotEnough")]
	VoucherNotEnough = 31037,
	[OriginalName("TiliNotEnough")]
	TiliNotEnough = 31041,
	[OriginalName("PropNotEnough")]
	PropNotEnough = 31043,
	[OriginalName("ItemNotEnough")]
	ItemNotEnough = 31045,
	[OriginalName("FragNotEnough")]
	FragNotEnough = 31051,
	[OriginalName("GemNotEnough")]
	GemNotEnough = 31053,
	[OriginalName("BoxNotEnough")]
	BoxNotEnough = 31055,
	[OriginalName("GeneNotEnough")]
	GeneNotEnough = 31057,
	[OriginalName("BagEquipNotExists")]
	BagEquipNotExists = 31071,
	[OriginalName("BagLimit")]
	BagLimit = 31073,
	[OriginalName("BagEquipLimit")]
	BagEquipLimit = 31075,
	[OriginalName("TiliMaxLimit")]
	TiliMaxLimit = 31077,
	[OriginalName("HeroNotExists")]
	HeroNotExists = 31101,
	[OriginalName("HeroInitFailed")]
	HeroInitFailed = 31103,
	[OriginalName("EquipInitFailed")]
	EquipInitFailed = 31105,
	[OriginalName("GemLocked")]
	GemLocked = 31107,
	[OriginalName("GeneFiveHoleLocked")]
	GeneFiveHoleLocked = 31111,
	[OriginalName("HomeWaterNotEnough")]
	HomeWaterNotEnough = 31501,
	[OriginalName("HomeFoodNotEnough")]
	HomeFoodNotEnough = 31503,
	[OriginalName("HomePowerNotEnough")]
	HomePowerNotEnough = 31505,
	[OriginalName("HomeBedsNotEnough")]
	HomeBedsNotEnough = 31507,
	[OriginalName("HomeControlCenterLevelLimit")]
	HomeControlCenterLevelLimit = 31511,
	[OriginalName("HomeWorkerNotExists")]
	HomeWorkerNotExists = 31513,
	[OriginalName("WarRoomLevelLimit")]
	WarRoomLevelLimit = 31515,
	[OriginalName("WorldReconStateAbnormal")]
	WorldReconStateAbnormal = 31551,
	[OriginalName("PeerageLimit")]
	PeerageLimit = 31601,
	[OriginalName("MainQuestLimit")]
	MainQuestLimit = 32001,
	[OriginalName("HeroTrialExpired")]
	HeroTrialExpired = 32003,
	[OriginalName("WorldLairPassed")]
	WorldLairPassed = 32005,
	[OriginalName("CrossLadderSeasonInit")]
	CrossLadderSeasonInit = 32007,
	[OriginalName("CrossLadderStateError")]
	CrossLadderStateError = 32009,
	[OriginalName("CrossLadderMatchFail")]
	CrossLadderMatchFail = 32011,
	[OriginalName("MineWarArenaRankLimit")]
	MineWarArenaRankLimit = 32021,
	[OriginalName("MineWarInCrossTime")]
	MineWarInCrossTime = 32023,
	[OriginalName("OccupyMinePoint")]
	OccupyMinePoint = 32025,
	[OriginalName("MineMapHasChanged")]
	MineMapHasChanged = 32027,
	[OriginalName("OccupyOuterMinePoint")]
	OccupyOuterMinePoint = 32029,
	[OriginalName("LegionWarInFight")]
	LegionWarInFight = 32041,
	[OriginalName("LegionWarConnectionLimit")]
	LegionWarConnectionLimit = 32043,
	[OriginalName("LegionWarPointForceLimit")]
	LegionWarPointForceLimit = 32045,
	[OriginalName("LegionWarPointMatchEmpty")]
	LegionWarPointMatchEmpty = 32047,
	[OriginalName("LegionWarPointMatchProtect")]
	LegionWarPointMatchProtect = 32048,
	[OriginalName("LegionWarActionPointsLimit")]
	LegionWarActionPointsLimit = 32049,
	[OriginalName("LegionWarLegionQuitLimit")]
	LegionWarLegionQuitLimit = 32053,
	[OriginalName("SeasonNoForce")]
	SeasonNoForce = 32054,
	[OriginalName("SeasonOwnCity")]
	SeasonOwnCity = 32055,
	[OriginalName("SeasonNotAdjacent")]
	SeasonNotAdjacent = 32056,
	[OriginalName("SeasonDeclareMax")]
	SeasonDeclareMax = 32057,
	[OriginalName("SeasonAlreadyDeclared")]
	SeasonAlreadyDeclared = 32058,
	[OriginalName("SeasonCityWarClosed")]
	SeasonCityWarClosed = 32059,
	[OriginalName("SeasonNoDeclare")]
	SeasonNoDeclare = 32060,
	[OriginalName("SeasonNotAtkForce")]
	SeasonNotAtkForce = 32061,
	[OriginalName("SeasonAlreadyFighting")]
	SeasonAlreadyFighting = 32062,
	[OriginalName("SeasonAirdropNotFound")]
	SeasonAirdropNotFound = 32063,
	[OriginalName("SeasonAirdropFull")]
	SeasonAirdropFull = 32064,
	[OriginalName("SeasonAirdropNoGas")]
	SeasonAirdropNoGas = 32065,
	[OriginalName("SeasonAirdropAlreadyJoin")]
	SeasonAirdropAlreadyJoin = 32066,
	[OriginalName("SeasonAirdropNotJoin")]
	SeasonAirdropNotJoin = 32067,
	[OriginalName("SeasonAirdropTargetInvalid")]
	SeasonAirdropTargetInvalid = 32068,
	[OriginalName("SeasonAirdropDailyLimit")]
	SeasonAirdropDailyLimit = 32069,
	[OriginalName("SeasonAirdropBattleSelf")]
	SeasonAirdropBattleSelf = 32070,
	[OriginalName("SeasonNotLeader")]
	SeasonNotLeader = 32071,
	[OriginalName("SeasonNotOwnForce")]
	SeasonNotOwnForce = 32072,
	[OriginalName("SeasonOfficeInvalid")]
	SeasonOfficeInvalid = 32073,
	[OriginalName("SeasonImpeachSelf")]
	SeasonImpeachSelf = 32075,
	[OriginalName("SeasonImpeachNotLeader")]
	SeasonImpeachNotLeader = 32076,
	[OriginalName("SeasonElectNotReady")]
	SeasonElectNotReady = 32077,
	[OriginalName("SeasonAlreadyImpeaching")]
	SeasonAlreadyImpeaching = 32078,
	[OriginalName("SeasonCityFallen")]
	SeasonCityFallen = 32079,
	[OriginalName("SeasonRecruitGuardFull")]
	SeasonRecruitGuardFull = 32080,
	[OriginalName("SeasonRecruitDailyLimit")]
	SeasonRecruitDailyLimit = 32081,
	[OriginalName("SeasonRecruitCostFail")]
	SeasonRecruitCostFail = 32082,
	[OriginalName("SeasonBirthProtect")]
	SeasonBirthProtect = 32083,
	[OriginalName("SeasonImpeachPowerLow")]
	SeasonImpeachPowerLow = 32084,
	[OriginalName("SeasonMarchPosInvalid")]
	SeasonMarchPosInvalid = 32085,
	[OriginalName("SeasonTeamNotFound")]
	SeasonTeamNotFound = 32086,
	[OriginalName("SeasonTeamBusy")]
	SeasonTeamBusy = 32087,
	[OriginalName("SeasonNotOwnCity")]
	SeasonNotOwnCity = 32088,
	[OriginalName("SeasonTeamTypeMismatch")]
	SeasonTeamTypeMismatch = 32089,
	[OriginalName("SeasonNoGas")]
	SeasonNoGas = 32090,
	[OriginalName("SeasonWaitOpen")]
	SeasonWaitOpen = 32091,
	[OriginalName("SeasonClosed")]
	SeasonClosed = 32092,
	[OriginalName("SeasonShopMoneyNotOpen")]
	SeasonShopMoneyNotOpen = 32093,
	[OriginalName("SeasonShopCostFail")]
	SeasonShopCostFail = 32094,
	[OriginalName("SeasonFuncLocked")]
	SeasonFuncLocked = 32095,
	[OriginalName("SeasonMarchSpeedUpNotMarching")]
	SeasonMarchSpeedUpNotMarching = 32096,
	[OriginalName("SeasonMarchSpeedUpTooShort")]
	SeasonMarchSpeedUpTooShort = 32097,
	[OriginalName("SeasonMarchSpeedUpCostFail")]
	SeasonMarchSpeedUpCostFail = 32098,
	[OriginalName("SeasonMarchSpeedUpBusy")]
	SeasonMarchSpeedUpBusy = 32099,
	[OriginalName("ErrorCodeMax")]
	ErrorCodeMax = 49999
}
