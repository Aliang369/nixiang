using Google.Protobuf.Reflection;

namespace Proto.Messages;

public enum Message
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Error_Ack")]
	ErrorAck = 10000,
	[OriginalName("HeartBeat_Req")]
	HeartBeatReq = 10001,
	[OriginalName("HeartBeat_Ack")]
	HeartBeatAck = 10002,
	[OriginalName("MessageSync_Ntf")]
	SyncNtf = 10004,
	[OriginalName("User_Login_Req")]
	UserLoginReq = 11001,
	[OriginalName("User_Login_Ack")]
	UserLoginAck = 11002,
	[OriginalName("User_Ready_Req")]
	UserReadyReq = 11003,
	[OriginalName("User_Ready_Ack")]
	UserReadyAck = 11004,
	[OriginalName("User_SetLang_Req")]
	UserSetLangReq = 11005,
	[OriginalName("User_SetLang_Ack")]
	UserSetLangAck = 11006,
	[OriginalName("User_VisitorInit_Req")]
	UserVisitorInitReq = 11011,
	[OriginalName("User_VisitorInit_Ack")]
	UserVisitorInitAck = 11012,
	[OriginalName("User_VisitorBind_Req")]
	UserVisitorBindReq = 11013,
	[OriginalName("User_VisitorBind_Ack")]
	UserVisitorBindAck = 11014,
	[OriginalName("Server_List_Req")]
	ServerListReq = 11031,
	[OriginalName("Server_List_Ack")]
	ServerListAck = 11032,
	[OriginalName("Notice_List_Req")]
	NoticeListReq = 11041,
	[OriginalName("Notice_List_Ack")]
	NoticeListAck = 11042,
	[OriginalName("Translate_Baidu_Req")]
	TranslateBaiduReq = 11051,
	[OriginalName("Translate_Baidu_Ack")]
	TranslateBaiduAck = 11052,
	[OriginalName("Audit_Check_Req")]
	AuditCheckReq = 11061,
	[OriginalName("Audit_Check_Ack")]
	AuditCheckAck = 11062,
	[OriginalName("Push_TokenReport_Req")]
	PushTokenReportReq = 11071,
	[OriginalName("Push_TokenReport_Ack")]
	PushTokenReportAck = 11072,
	[OriginalName("Pay_CreateOrder_Req")]
	PayCreateOrderReq = 11101,
	[OriginalName("Pay_CreateOrder_Ack")]
	PayCreateOrderAck = 11102,
	[OriginalName("Pay_VerifyOrder_Req")]
	PayVerifyOrderReq = 11103,
	[OriginalName("Pay_VerifyOrder_Ack")]
	PayVerifyOrderAck = 11104,
	[OriginalName("Pay_VerifyOrderV2_Req")]
	PayVerifyOrderV2Req = 11105,
	[OriginalName("Pay_VerifyOrderV2_Ack")]
	PayVerifyOrderV2Ack = 11106,
	[OriginalName("Pay_CreateOrderSign_Req")]
	PayCreateOrderSignReq = 11107,
	[OriginalName("Pay_CreateOrderSign_Ack")]
	PayCreateOrderSignAck = 11108,
	[OriginalName("Client_Dump_Req")]
	ClientDumpReq = 11201,
	[OriginalName("Client_Dump_Ack")]
	ClientDumpAck = 11202,
	[OriginalName("Chat_Send_Req")]
	ChatSendReq = 13001,
	[OriginalName("Chat_Send_Ack")]
	ChatSendAck = 13002,
	[OriginalName("Chat_Data_Req")]
	ChatDataReq = 13003,
	[OriginalName("Chat_Data_Ack")]
	ChatDataAck = 13004,
	[OriginalName("Chat_ActionShare_Req")]
	ChatActionShareReq = 13005,
	[OriginalName("Chat_ActionShare_Ack")]
	ChatActionShareAck = 13006,
	[OriginalName("Chat_ActionData_Req")]
	ChatActionDataReq = 13007,
	[OriginalName("Chat_ActionData_Ack")]
	ChatActionDataAck = 13008,
	[OriginalName("Chat_Receive_Ntf")]
	ChatReceiveNtf = 13009,
	[OriginalName("Chat_Scroll_Ntf")]
	ChatScrollNtf = 13010,
	[OriginalName("Game_Login_Req")]
	GameLoginReq = 15001,
	[OriginalName("Game_Login_Ack")]
	GameLoginAck = 15002,
	[OriginalName("Game_LoginOut_Req")]
	GameLoginOutReq = 15003,
	[OriginalName("Game_LoginOut_Ack")]
	GameLoginOutAck = 15004,
	[OriginalName("Game_Kick_Req")]
	GameKickReq = 15011,
	[OriginalName("Game_Kick_Ack")]
	GameKickAck = 15012,
	[OriginalName("Game_Kick_Ntf")]
	GameKickNtf = 15013,
	[OriginalName("Role_Create_Req")]
	RoleCreateReq = 15051,
	[OriginalName("Role_Create_Ack")]
	RoleCreateAck = 15052,
	[OriginalName("Role_Ready_Req")]
	RoleReadyReq = 15053,
	[OriginalName("Role_Ready_Ack")]
	RoleReadyAck = 15054,
	[OriginalName("Role_ReadyEnd_Req")]
	RoleReadyEndReq = 15055,
	[OriginalName("Role_Data_Req")]
	RoleDataReq = 15057,
	[OriginalName("Role_Data_Ack")]
	RoleDataAck = 15058,
	[OriginalName("Role_MiniTipsInfo_Req")]
	RoleMiniTipsInfoReq = 15061,
	[OriginalName("Role_MiniTipsInfo_Ack")]
	RoleMiniTipsInfoAck = 15062,
	[OriginalName("Role_TipsInfo_Req")]
	RoleTipsInfoReq = 15063,
	[OriginalName("Role_TipsInfo_Ack")]
	RoleTipsInfoAck = 15064,
	[OriginalName("Role_TipsDetail_Req")]
	RoleTipsDetailReq = 15065,
	[OriginalName("Role_TipsDetail_Ack")]
	RoleTipsDetailAck = 15066,
	[OriginalName("Role_PvpTipsInfo_Req")]
	RolePvpTipsInfoReq = 15067,
	[OriginalName("Role_PvpTipsInfo_Ack")]
	RolePvpTipsInfoAck = 15068,
	[OriginalName("Role_FightStart_Req")]
	RoleFightStartReq = 15071,
	[OriginalName("Role_FightStart_Ack")]
	RoleFightStartAck = 15072,
	[OriginalName("Role_FightResult_Req")]
	RoleFightResultReq = 15073,
	[OriginalName("Role_FightResult_Ack")]
	RoleFightResultAck = 15074,
	[OriginalName("Reset_RoleName_Req")]
	ResetRoleNameReq = 15101,
	[OriginalName("Reset_RoleName_Ack")]
	ResetRoleNameAck = 15102,
	[OriginalName("Reset_HeadId_Req")]
	ResetHeadIdReq = 15103,
	[OriginalName("Reset_HeadId_Ack")]
	ResetHeadIdAck = 15104,
	[OriginalName("Reset_HeadFrameId_Req")]
	ResetHeadFrameIdReq = 15105,
	[OriginalName("Reset_HeadFrameId_Ack")]
	ResetHeadFrameIdAck = 15106,
	[OriginalName("Reset_SloganId_Req")]
	ResetSloganIdReq = 15111,
	[OriginalName("Reset_SloganId_Ack")]
	ResetSloganIdAck = 15112,
	[OriginalName("Rewards_RedeemCode_Req")]
	RewardsRedeemCodeReq = 15121,
	[OriginalName("Rewards_RedeemCode_Ack")]
	RewardsRedeemCodeAck = 15122,
	[OriginalName("Account_BindAward_Req")]
	AccountBindAwardReq = 15131,
	[OriginalName("Account_BindAward_Ack")]
	AccountBindAwardAck = 15132,
	[OriginalName("Account_RebateAward_Req")]
	AccountRebateAwardReq = 15133,
	[OriginalName("Account_RebateAward_Ack")]
	AccountRebateAwardAck = 15134,
	[OriginalName("Abnormal_ClientReport_Req")]
	AbnormalClientReportReq = 15137,
	[OriginalName("Abnormal_ClientReport_Ack")]
	AbnormalClientReportAck = 15138,
	[OriginalName("PushSet_Data_Req")]
	PushSetDataReq = 15141,
	[OriginalName("PushSet_Data_Ack")]
	PushSetDataAck = 15142,
	[OriginalName("PushSet_Close_Req")]
	PushSetCloseReq = 15143,
	[OriginalName("PushSet_Close_Ack")]
	PushSetCloseAck = 15144,
	[OriginalName("Aux_FuncUnlockAward_Req")]
	AuxFuncUnlockAwardReq = 15151,
	[OriginalName("Aux_FuncUnlockAward_Ack")]
	AuxFuncUnlockAwardAck = 15152,
	[OriginalName("Aux_ExchangeMoney_Req")]
	AuxExchangeMoneyReq = 15153,
	[OriginalName("Aux_ExchangeMoney_Ack")]
	AuxExchangeMoneyAck = 15154,
	[OriginalName("Aux_ExchangeHalf_Req")]
	AuxExchangeHalfReq = 15155,
	[OriginalName("Aux_ExchangeHalf_Ack")]
	AuxExchangeHalfAck = 15156,
	[OriginalName("Aux_ResourceQuickBuy_Req")]
	AuxResourceQuickBuyReq = 15157,
	[OriginalName("Aux_ResourceQuickBuy_Ack")]
	AuxResourceQuickBuyAck = 15158,
	[OriginalName("Aux_FreeGiftAward_Req")]
	AuxFreeGiftAwardReq = 15161,
	[OriginalName("Aux_FreeGiftAward_Ack")]
	AuxFreeGiftAwardAck = 15162,
	[OriginalName("Aux_PlayerProcess_Req")]
	AuxPlayerProcessReq = 15171,
	[OriginalName("Aux_PlayerProcess_Ack")]
	AuxPlayerProcessAck = 15172,
	[OriginalName("Aux_PlayerSuggest_Req")]
	AuxPlayerSuggestReq = 15173,
	[OriginalName("Aux_PlayerSuggest_Ack")]
	AuxPlayerSuggestAck = 15174,
	[OriginalName("Aux_ProgressTrack_Req")]
	AuxProgressTrackReq = 15175,
	[OriginalName("Aux_ProgressTrack_Ack")]
	AuxProgressTrackAck = 15176,
	[OriginalName("Aux_ReminderList_Req")]
	AuxReminderListReq = 15181,
	[OriginalName("Aux_ReminderList_Ack")]
	AuxReminderListAck = 15182,
	[OriginalName("Aux_ResourceConvert_Req")]
	AuxResourceConvertReq = 15183,
	[OriginalName("Aux_ResourceConvert_Ack")]
	AuxResourceConvertAck = 15184,
	[OriginalName("Mail_GetList_Req")]
	MailGetListReq = 15201,
	[OriginalName("Mail_GetList_Ack")]
	MailGetListAck = 15202,
	[OriginalName("Mail_Read_Req")]
	MailReadReq = 15203,
	[OriginalName("Mail_Read_Ack")]
	MailReadAck = 15204,
	[OriginalName("Mail_GetAward_Req")]
	MailGetAwardReq = 15205,
	[OriginalName("Mail_GetAward_Ack")]
	MailGetAwardAck = 15206,
	[OriginalName("Mail_GetAllAward_Req")]
	MailGetAllAwardReq = 15207,
	[OriginalName("Mail_GetAllAward_Ack")]
	MailGetAllAwardAck = 15208,
	[OriginalName("Mail_Delete_Req")]
	MailDeleteReq = 15211,
	[OriginalName("Mail_Delete_Ack")]
	MailDeleteAck = 15212,
	[OriginalName("Rank_Top_Req")]
	RankTopReq = 15231,
	[OriginalName("Rank_Top_Ack")]
	RankTopAck = 15232,
	[OriginalName("Rank_List_Req")]
	RankListReq = 15233,
	[OriginalName("Rank_List_Ack")]
	RankListAck = 15234,
	[OriginalName("Rank_FirstReach_Req")]
	RankFirstReachReq = 15235,
	[OriginalName("Rank_FirstReach_Ack")]
	RankFirstReachAck = 15236,
	[OriginalName("Rank_FirstReachAward_Req")]
	RankFirstReachAwardReq = 15237,
	[OriginalName("Rank_FirstReachAward_Ack")]
	RankFirstReachAwardAck = 15238,
	[OriginalName("OpenServer_RankTop_Req")]
	OpenServerRankTopReq = 15251,
	[OriginalName("OpenServer_RankTop_Ack")]
	OpenServerRankTopAck = 15252,
	[OriginalName("OpenServer_RankList_Req")]
	OpenServerRankListReq = 15253,
	[OriginalName("OpenServer_RankList_Ack")]
	OpenServerRankListAck = 15254,
	[OriginalName("Pay_SuccessOrder_Req")]
	PaySuccessOrderReq = 15341,
	[OriginalName("Pay_SuccessOrder_Ack")]
	PaySuccessOrderAck = 15342,
	[OriginalName("Pay_VoucherConsume_Req")]
	PayVoucherConsumeReq = 15343,
	[OriginalName("Pay_VoucherConsume_Ack")]
	PayVoucherConsumeAck = 15344,
	[OriginalName("RechargeShop_GrowthGift_Req")]
	RechargeShopGrowthGiftReq = 15353,
	[OriginalName("RechargeShop_GrowthGift_Ack")]
	RechargeShopGrowthGiftAck = 15354,
	[OriginalName("RechargeShop_VipCardAward_Req")]
	RechargeShopVipCardAwardReq = 15355,
	[OriginalName("RechargeShop_VipCardAward_Ack")]
	RechargeShopVipCardAwardAck = 15356,
	[OriginalName("RechargeActive_Data_Req")]
	RechargeActiveDataReq = 15401,
	[OriginalName("RechargeActive_Data_Ack")]
	RechargeActiveDataAck = 15402,
	[OriginalName("RechargeActive_NovicePack_Req")]
	RechargeActiveNovicePackReq = 15403,
	[OriginalName("RechargeActive_NovicePack_Ack")]
	RechargeActiveNovicePackAck = 15404,
	[OriginalName("RechargeActive_NovicePackExtra_Req")]
	RechargeActiveNovicePackExtraReq = 15405,
	[OriginalName("RechargeActive_NovicePackExtra_Ack")]
	RechargeActiveNovicePackExtraAck = 15406,
	[OriginalName("RechargeActive_TreasurePrivilege_Req")]
	RechargeActiveTreasurePrivilegeReq = 15407,
	[OriginalName("RechargeActive_TreasurePrivilege_Ack")]
	RechargeActiveTreasurePrivilegeAck = 15408,
	[OriginalName("RechargeActive_DailyPack_Req")]
	RechargeActiveDailyPackReq = 15411,
	[OriginalName("RechargeActive_DailyPack_Ack")]
	RechargeActiveDailyPackAck = 15412,
	[OriginalName("RechargeActive_DailyPackGetAll_Req")]
	RechargeActiveDailyPackGetAllReq = 15415,
	[OriginalName("RechargeActive_DailyPackGetAll_Ack")]
	RechargeActiveDailyPackGetAllAck = 15416,
	[OriginalName("RechargeActive_LevelDiamondAward_Req")]
	RechargeActiveLevelDiamondAwardReq = 15421,
	[OriginalName("RechargeActive_LevelDiamondAward_Ack")]
	RechargeActiveLevelDiamondAwardAck = 15422,
	[OriginalName("RechargeActive_ZhanLingMainAward_Req")]
	RechargeActiveZhanLingMainAwardReq = 15423,
	[OriginalName("RechargeActive_ZhanLingMainAward_Ack")]
	RechargeActiveZhanLingMainAwardAck = 15424,
	[OriginalName("RechargeActive_ZhanLingElementAward_Req")]
	RechargeActiveZhanLingElementAwardReq = 15425,
	[OriginalName("RechargeActive_ZhanLingElementAward_Ack")]
	RechargeActiveZhanLingElementAwardAck = 15426,
	[OriginalName("RechargeActive_ZhanLingEndlessAward_Req")]
	RechargeActiveZhanLingEndlessAwardReq = 15427,
	[OriginalName("RechargeActive_ZhanLingEndlessAward_Ack")]
	RechargeActiveZhanLingEndlessAwardAck = 15428,
	[OriginalName("RechargeActive_ZhanLingLegendary_Req")]
	RechargeActiveZhanLingLegendaryReq = 15429,
	[OriginalName("RechargeActive_ZhanLingLegendary_Ack")]
	RechargeActiveZhanLingLegendaryAck = 15430,
	[OriginalName("Subscribe_Data_Req")]
	SubscribeDataReq = 15431,
	[OriginalName("Subscribe_Data_Ack")]
	SubscribeDataAck = 15432,
	[OriginalName("Subscribe_BuyGoods_Req")]
	SubscribeBuyGoodsReq = 15433,
	[OriginalName("Subscribe_BuyGoods_Ack")]
	SubscribeBuyGoodsAck = 15434,
	[OriginalName("Passport_BuyLevel_Req")]
	PassportBuyLevelReq = 15451,
	[OriginalName("Passport_BuyLevel_Ack")]
	PassportBuyLevelAck = 15452,
	[OriginalName("Passport_LevelAward_Req")]
	PassportLevelAwardReq = 15453,
	[OriginalName("Passport_LevelAward_Ack")]
	PassportLevelAwardAck = 15454,
	[OriginalName("VoucherShop_Data_Req")]
	VoucherShopDataReq = 15501,
	[OriginalName("VoucherShop_Data_Ack")]
	VoucherShopDataAck = 15502,
	[OriginalName("VoucherShop_CartAdd_Req")]
	VoucherShopCartAddReq = 15503,
	[OriginalName("VoucherShop_CartAdd_Ack")]
	VoucherShopCartAddAck = 15504,
	[OriginalName("VoucherShop_CartRemove_Req")]
	VoucherShopCartRemoveReq = 15505,
	[OriginalName("VoucherShop_CartRemove_Ack")]
	VoucherShopCartRemoveAck = 15506,
	[OriginalName("VoucherShop_CartRemoveAll_Req")]
	VoucherShopCartRemoveAllReq = 15507,
	[OriginalName("VoucherShop_CartRemoveAll_Ack")]
	VoucherShopCartRemoveAllAck = 15508,
	[OriginalName("VoucherShop_CartCheckOut_Req")]
	VoucherShopCartCheckOutReq = 15511,
	[OriginalName("VoucherShop_CartCheckOut_Ack")]
	VoucherShopCartCheckOutAck = 15512,
	[OriginalName("VoucherShop_CartPay_Req")]
	VoucherShopCartPayReq = 15513,
	[OriginalName("VoucherShop_CartPay_Ack")]
	VoucherShopCartPayAck = 15514,
	[OriginalName("Activity_Data_Req")]
	ActivityDataReq = 15551,
	[OriginalName("Activity_Data_Ack")]
	ActivityDataAck = 15552,
	[OriginalName("Activity_NoviceLogin_Req")]
	ActivityNoviceLoginReq = 15553,
	[OriginalName("Activity_NoviceLogin_Ack")]
	ActivityNoviceLoginAck = 15554,
	[OriginalName("Activity_Sign_Req")]
	ActivitySignReq = 15555,
	[OriginalName("Activity_Sign_Ack")]
	ActivitySignAck = 15556,
	[OriginalName("Activity_SignTargetAward_Req")]
	ActivitySignTargetAwardReq = 15557,
	[OriginalName("Activity_SignTargetAward_Ack")]
	ActivitySignTargetAwardAck = 15558,
	[OriginalName("Activity_TiliBankAward_Req")]
	ActivityTiliBankAwardReq = 15561,
	[OriginalName("Activity_TiliBankAward_Ack")]
	ActivityTiliBankAwardAck = 15562,
	[OriginalName("Activity_TiliBankAwardAll_Req")]
	ActivityTiliBankAwardAllReq = 15563,
	[OriginalName("Activity_TiliBankAwardAll_Ack")]
	ActivityTiliBankAwardAllAck = 15564,
	[OriginalName("Activity_BubbleGiftRand_Req")]
	ActivityBubbleGiftRandReq = 15565,
	[OriginalName("Activity_BubbleGiftRand_Ack")]
	ActivityBubbleGiftRandAck = 15566,
	[OriginalName("Activity_BubbleGiftBuy_Req")]
	ActivityBubbleGiftBuyReq = 15567,
	[OriginalName("Activity_BubbleGiftBuy_Ack")]
	ActivityBubbleGiftBuyAck = 15568,
	[OriginalName("Vip_Data_Req")]
	VipDataReq = 15701,
	[OriginalName("Vip_Data_Ack")]
	VipDataAck = 15702,
	[OriginalName("Vip_FreeExp_Req")]
	VipFreeExpReq = 15703,
	[OriginalName("Vip_FreeExp_Ack")]
	VipFreeExpAck = 15704,
	[OriginalName("Vip_DailyFree_Req")]
	VipDailyFreeReq = 15707,
	[OriginalName("Vip_DailyFree_Ack")]
	VipDailyFreeAck = 15708,
	[OriginalName("Operate_Data_Req")]
	OperateDataReq = 16001,
	[OriginalName("Operate_Data_Ack")]
	OperateDataAck = 16002,
	[OriginalName("Operate_RechargeDaily_Req")]
	OperateRechargeDailyReq = 16011,
	[OriginalName("Operate_RechargeDaily_Ack")]
	OperateRechargeDailyAck = 16012,
	[OriginalName("Operate_RechargeDailySum_Req")]
	OperateRechargeDailySumReq = 16013,
	[OriginalName("Operate_RechargeDailySum_Ack")]
	OperateRechargeDailySumAck = 16014,
	[OriginalName("Operate_RechargeSum_Req")]
	OperateRechargeSumReq = 16015,
	[OriginalName("Operate_RechargeSum_Ack")]
	OperateRechargeSumAck = 16016,
	[OriginalName("Operate_RechargeDirect_Req")]
	OperateRechargeDirectReq = 16023,
	[OriginalName("Operate_RechargeDirect_Ack")]
	OperateRechargeDirectAck = 16024,
	[OriginalName("Operate_ConsumeDailySum_Req")]
	OperateConsumeDailySumReq = 16031,
	[OriginalName("Operate_ConsumeDailySum_Ack")]
	OperateConsumeDailySumAck = 16032,
	[OriginalName("Operate_ConsumeSum_Req")]
	OperateConsumeSumReq = 16033,
	[OriginalName("Operate_ConsumeSum_Ack")]
	OperateConsumeSumAck = 16034,
	[OriginalName("Operate_LimitTimeHeroDraw_Req")]
	OperateLimitTimeHeroDrawReq = 16041,
	[OriginalName("Operate_LimitTimeHeroDraw_Ack")]
	OperateLimitTimeHeroDrawAck = 16042,
	[OriginalName("Operate_LimitTimeHeroScore_Req")]
	OperateLimitTimeHeroScoreReq = 16043,
	[OriginalName("Operate_LimitTimeHeroScore_Ack")]
	OperateLimitTimeHeroScoreAck = 16044,
	[OriginalName("Operate_LimitTimeShop_Req")]
	OperateLimitTimeShopReq = 16045,
	[OriginalName("Operate_LimitTimeShop_Ack")]
	OperateLimitTimeShopAck = 16046,
	[OriginalName("Operate_LoginSum_Req")]
	OperateLoginSumReq = 16061,
	[OriginalName("Operate_LoginSum_Ack")]
	OperateLoginSumAck = 16062,
	[OriginalName("Operate_DailyActive_Req")]
	OperateDailyActiveReq = 16063,
	[OriginalName("Operate_DailyActive_Ack")]
	OperateDailyActiveAck = 16064,
	[OriginalName("Operate_BarDraw_Req")]
	OperateBarDrawReq = 16065,
	[OriginalName("Operate_BarDraw_Ack")]
	OperateBarDrawAck = 16066,
	[OriginalName("Operate_PropExchange_Req")]
	OperatePropExchangeReq = 16071,
	[OriginalName("Operate_PropExchange_Ack")]
	OperatePropExchangeAck = 16072,
	[OriginalName("Operate_PropExchangeTarget_Req")]
	OperatePropExchangeTargetReq = 16073,
	[OriginalName("Operate_PropExchangeTarget_Ack")]
	OperatePropExchangeTargetAck = 16074,
	[OriginalName("Operate_ScratchCard_Req")]
	OperateScratchCardReq = 16081,
	[OriginalName("Operate_ScratchCard_Ack")]
	OperateScratchCardAck = 16082,
	[OriginalName("Operate_ScratchCardTarget_Req")]
	OperateScratchCardTargetReq = 16083,
	[OriginalName("Operate_ScratchCardTarget_Ack")]
	OperateScratchCardTargetAck = 16084,
	[OriginalName("Operate_ScratchCardReset_Req")]
	OperateScratchCardResetReq = 16085,
	[OriginalName("Operate_ScratchCardReset_Ack")]
	OperateScratchCardResetAck = 16086,
	[OriginalName("Operate_JiZi_Req")]
	OperateJiZiReq = 16091,
	[OriginalName("Operate_JiZi_Ack")]
	OperateJiZiAck = 16092,
	[OriginalName("Operate_CipherBox_Req")]
	OperateCipherBoxReq = 16101,
	[OriginalName("Operate_CipherBox_Ack")]
	OperateCipherBoxAck = 16102,
	[OriginalName("Operate_BarrageCmd_Req")]
	OperateBarrageCmdReq = 16103,
	[OriginalName("Operate_BarrageCmd_Ack")]
	OperateBarrageCmdAck = 16104,
	[OriginalName("Operate_LimitTimeCatcherCoin_Req")]
	OperateLimitTimeCatcherCoinReq = 16111,
	[OriginalName("Operate_LimitTimeCatcherCoin_Ack")]
	OperateLimitTimeCatcherCoinAck = 16112,
	[OriginalName("Operate_LimitTimeCatcherGrab_Req")]
	OperateLimitTimeCatcherGrabReq = 16113,
	[OriginalName("Operate_LimitTimeCatcherGrab_Ack")]
	OperateLimitTimeCatcherGrabAck = 16114,
	[OriginalName("Operate_LimitTimeCatcherAward_Req")]
	OperateLimitTimeCatcherAwardReq = 16115,
	[OriginalName("Operate_LimitTimeCatcherAward_Ack")]
	OperateLimitTimeCatcherAwardAck = 16116,
	[OriginalName("Operate_HolidayTaskData_Req")]
	OperateHolidayTaskDataReq = 16121,
	[OriginalName("Operate_HolidayTaskData_Ack")]
	OperateHolidayTaskDataAck = 16122,
	[OriginalName("Operate_HolidayTaskTaskAward_Req")]
	OperateHolidayTaskTaskAwardReq = 16123,
	[OriginalName("Operate_HolidayTaskTaskAward_Ack")]
	OperateHolidayTaskTaskAwardAck = 16124,
	[OriginalName("Operate_HolidayTaskLevelAward_Req")]
	OperateHolidayTaskLevelAwardReq = 16125,
	[OriginalName("Operate_HolidayTaskLevelAward_Ack")]
	OperateHolidayTaskLevelAwardAck = 16126,
	[OriginalName("Operate_HolidayTaskScoreAward_Req")]
	OperateHolidayTaskScoreAwardReq = 16127,
	[OriginalName("Operate_HolidayTaskScoreAward_Ack")]
	OperateHolidayTaskScoreAwardAck = 16128,
	[OriginalName("Operate_HolidayTaskBuyLevel_Req")]
	OperateHolidayTaskBuyLevelReq = 16131,
	[OriginalName("Operate_HolidayTaskBuyLevel_Ack")]
	OperateHolidayTaskBuyLevelAck = 16132,
	[OriginalName("Operate_PrayLanternDraw_Req")]
	OperatePrayLanternDrawReq = 16141,
	[OriginalName("Operate_PrayLanternDraw_Ack")]
	OperatePrayLanternDrawAck = 16142,
	[OriginalName("Operate_PrayLanternTarget_Req")]
	OperatePrayLanternTargetReq = 16143,
	[OriginalName("Operate_PrayLanternTarget_Ack")]
	OperatePrayLanternTargetAck = 16144,
	[OriginalName("Operate_PrayLanternSelect_Req")]
	OperatePrayLanternSelectReq = 16145,
	[OriginalName("Operate_PrayLanternSelect_Ack")]
	OperatePrayLanternSelectAck = 16146,
	[OriginalName("Operate_PrayLanternUltimate_Req")]
	OperatePrayLanternUltimateReq = 16147,
	[OriginalName("Operate_PrayLanternUltimate_Ack")]
	OperatePrayLanternUltimateAck = 16148,
	[OriginalName("Operate_ResourceExchange_Req")]
	OperateResourceExchangeReq = 16151,
	[OriginalName("Operate_ResourceExchange_Ack")]
	OperateResourceExchangeAck = 16152,
	[OriginalName("Operate_FusionData_Req")]
	OperateFusionDataReq = 16161,
	[OriginalName("Operate_FusionData_Ack")]
	OperateFusionDataAck = 16162,
	[OriginalName("Operate_FusionUpdate_Req")]
	OperateFusionUpdateReq = 16163,
	[OriginalName("Operate_FusionUpdate_Ack")]
	OperateFusionUpdateAck = 16164,
	[OriginalName("Operate_FusionDailyAward_Req")]
	OperateFusionDailyAwardReq = 16165,
	[OriginalName("Operate_FusionDailyAward_Ack")]
	OperateFusionDailyAwardAck = 16166,
	[OriginalName("Operate_FusionTargetAward_Req")]
	OperateFusionTargetAwardReq = 16167,
	[OriginalName("Operate_FusionTargetAward_Ack")]
	OperateFusionTargetAwardAck = 16168,
	[OriginalName("Operate_FusionFriendsRank_Req")]
	OperateFusionFriendsRankReq = 16169,
	[OriginalName("Operate_FusionFriendsRank_Ack")]
	OperateFusionFriendsRankAck = 16170,
	[OriginalName("Operate_InfiniteGiftAward_Req")]
	OperateInfiniteGiftAwardReq = 16171,
	[OriginalName("Operate_InfiniteGiftAward_Ack")]
	OperateInfiniteGiftAwardAck = 16172,
	[OriginalName("Operate_InfiniteGiftTarget_Req")]
	OperateInfiniteGiftTargetReq = 16173,
	[OriginalName("Operate_InfiniteGiftTarget_Ack")]
	OperateInfiniteGiftTargetAck = 16174,
	[OriginalName("Operate_LimitTimeRouletteDraw_Req")]
	OperateLimitTimeRouletteDrawReq = 16181,
	[OriginalName("Operate_LimitTimeRouletteDraw_Ack")]
	OperateLimitTimeRouletteDrawAck = 16182,
	[OriginalName("Operate_LimitTimeRouletteTarget_Req")]
	OperateLimitTimeRouletteTargetReq = 16183,
	[OriginalName("Operate_LimitTimeRouletteTarget_Ack")]
	OperateLimitTimeRouletteTargetAck = 16184,
	[OriginalName("Operate_InfiniteGiftFree_Req")]
	OperateInfiniteGiftFreeReq = 16185,
	[OriginalName("Operate_InfiniteGiftFree_Ack")]
	OperateInfiniteGiftFreeAck = 16186,
	[OriginalName("Operate_LimitTimeTreasureDraw_Req")]
	OperateLimitTimeTreasureDrawReq = 16191,
	[OriginalName("Operate_LimitTimeTreasureDraw_Ack")]
	OperateLimitTimeTreasureDrawAck = 16192,
	[OriginalName("Operate_LimitTimeTreasureTarget_Req")]
	OperateLimitTimeTreasureTargetReq = 16193,
	[OriginalName("Operate_LimitTimeTreasureTarget_Ack")]
	OperateLimitTimeTreasureTargetAck = 16194,
	[OriginalName("Operate_LimitTimeVendorDraw_Req")]
	OperateLimitTimeVendorDrawReq = 16195,
	[OriginalName("Operate_LimitTimeVendorDraw_Ack")]
	OperateLimitTimeVendorDrawAck = 16196,
	[OriginalName("Operate_LimitTimeVendorTarget_Req")]
	OperateLimitTimeVendorTargetReq = 16197,
	[OriginalName("Operate_LimitTimeVendorTarget_Ack")]
	OperateLimitTimeVendorTargetAck = 16198,
	[OriginalName("Operate_TacticRoulette_Req")]
	OperateTacticRouletteReq = 16201,
	[OriginalName("Operate_TacticRoulette_Ack")]
	OperateTacticRouletteAck = 16202,
	[OriginalName("Operate_TacticRouletteTarget_Req")]
	OperateTacticRouletteTargetReq = 16203,
	[OriginalName("Operate_TacticRouletteTarget_Ack")]
	OperateTacticRouletteTargetAck = 16204,
	[OriginalName("Operate_TacticRouletteSelect_Req")]
	OperateTacticRouletteSelectReq = 16205,
	[OriginalName("Operate_TacticRouletteSelect_Ack")]
	OperateTacticRouletteSelectAck = 16206,
	[OriginalName("Operate_RestaurantActivateAward_Req")]
	OperateRestaurantActivateAwardReq = 16301,
	[OriginalName("Operate_RestaurantActivateAward_Ack")]
	OperateRestaurantActivateAwardAck = 16302,
	[OriginalName("Operate_RestaurantDailyAward_Req")]
	OperateRestaurantDailyAwardReq = 16303,
	[OriginalName("Operate_RestaurantDailyAward_Ack")]
	OperateRestaurantDailyAwardAck = 16304,
	[OriginalName("Operate_RestaurantBuyPiece_Req")]
	OperateRestaurantBuyPieceReq = 16305,
	[OriginalName("Operate_RestaurantBuyPiece_Ack")]
	OperateRestaurantBuyPieceAck = 16306,
	[OriginalName("Operate_RestaurantDelPiece_Req")]
	OperateRestaurantDelPieceReq = 16307,
	[OriginalName("Operate_RestaurantDelPiece_Ack")]
	OperateRestaurantDelPieceAck = 16308,
	[OriginalName("Operate_RestaurantSlotSync_Req")]
	OperateRestaurantSlotSyncReq = 16309,
	[OriginalName("Operate_RestaurantSlotSync_Ack")]
	OperateRestaurantSlotSyncAck = 16310,
	[OriginalName("Operate_RestaurantTarget_Req")]
	OperateRestaurantTargetReq = 16311,
	[OriginalName("Operate_RestaurantTarget_Ack")]
	OperateRestaurantTargetAck = 16312,
	[OriginalName("Operate_RestaurantOfferPiece_Req")]
	OperateRestaurantOfferPieceReq = 16313,
	[OriginalName("Operate_RestaurantOfferPiece_Ack")]
	OperateRestaurantOfferPieceAck = 16314,
	[OriginalName("Operate_ThemeTaskData_Req")]
	OperateThemeTaskDataReq = 16321,
	[OriginalName("Operate_ThemeTaskData_Ack")]
	OperateThemeTaskDataAck = 16322,
	[OriginalName("Operate_ThemeTaskTaskAward_Req")]
	OperateThemeTaskTaskAwardReq = 16323,
	[OriginalName("Operate_ThemeTaskTaskAward_Ack")]
	OperateThemeTaskTaskAwardAck = 16324,
	[OriginalName("Operate_ThemeTaskTargetAward_Req")]
	OperateThemeTaskTargetAwardReq = 16325,
	[OriginalName("Operate_ThemeTaskTargetAward_Ack")]
	OperateThemeTaskTargetAwardAck = 16326,
	[OriginalName("Operate_SnowVeilActivate_Req")]
	OperateSnowVeilActivateReq = 16341,
	[OriginalName("Operate_SnowVeilActivate_Ack")]
	OperateSnowVeilActivateAck = 16342,
	[OriginalName("Operate_SnowVeilTarget_Req")]
	OperateSnowVeilTargetReq = 16343,
	[OriginalName("Operate_SnowVeilTarget_Ack")]
	OperateSnowVeilTargetAck = 16344,
	[OriginalName("Operate_SnowVeilGo_Req")]
	OperateSnowVeilGoReq = 16351,
	[OriginalName("Operate_SnowVeilGo_Ack")]
	OperateSnowVeilGoAck = 16352,
	[OriginalName("Operate_SnowVeilEventItem_Ack")]
	OperateSnowVeilEventItemAck = 16353,
	[OriginalName("Operate_SnowVeilEventTurntable_Ack")]
	OperateSnowVeilEventTurntableAck = 16354,
	[OriginalName("Operate_SnowVeilEventRolling_Ack")]
	OperateSnowVeilEventRollingAck = 16355,
	[OriginalName("Operate_SnowVeilEventScore_Ack")]
	OperateSnowVeilEventScoreAck = 16356,
	[OriginalName("Operate_SnowVeilEventHighScore_Ack")]
	OperateSnowVeilEventHighScoreAck = 16357,
	[OriginalName("Operate_SnowVeilRolling_Req")]
	OperateSnowVeilRollingReq = 16361,
	[OriginalName("Operate_SnowVeilRolling_Ack")]
	OperateSnowVeilRollingAck = 16362,
	[OriginalName("Operate_SnowVeilRollingEnd_Req")]
	OperateSnowVeilRollingEndReq = 16363,
	[OriginalName("Operate_SnowVeilRollingEnd_Ack")]
	OperateSnowVeilRollingEndAck = 16364,
	[OriginalName("Operate_SnowVeilTurntable_Req")]
	OperateSnowVeilTurntableReq = 16365,
	[OriginalName("Operate_SnowVeilTurntable_Ack")]
	OperateSnowVeilTurntableAck = 16366,
	[OriginalName("Operate_SnowVeilPropBuy_Req")]
	OperateSnowVeilPropBuyReq = 16367,
	[OriginalName("Operate_SnowVeilPropBuy_Ack")]
	OperateSnowVeilPropBuyAck = 16368,
	[OriginalName("EndlessMine_Data_Req")]
	EndlessMineDataReq = 16381,
	[OriginalName("EndlessMine_Data_Ack")]
	EndlessMineDataAck = 16382,
	[OriginalName("EndlessMine_ActivateAward_Req")]
	EndlessMineActivateAwardReq = 16383,
	[OriginalName("EndlessMine_ActivateAward_Ack")]
	EndlessMineActivateAwardAck = 16384,
	[OriginalName("EndlessMine_Dig_Req")]
	EndlessMineDigReq = 16385,
	[OriginalName("EndlessMine_Dig_Ack")]
	EndlessMineDigAck = 16386,
	[OriginalName("EndlessMine_Drill_Req")]
	EndlessMineDrillReq = 16387,
	[OriginalName("EndlessMine_Drill_Ack")]
	EndlessMineDrillAck = 16388,
	[OriginalName("EndlessMine_Bomb_Req")]
	EndlessMineBombReq = 16391,
	[OriginalName("EndlessMine_Bomb_Ack")]
	EndlessMineBombAck = 16392,
	[OriginalName("EndlessMine_PropBuy_Req")]
	EndlessMinePropBuyReq = 16393,
	[OriginalName("EndlessMine_PropBuy_Ack")]
	EndlessMinePropBuyAck = 16394,
	[OriginalName("EndlessMine_ExchangeAward_Req")]
	EndlessMineExchangeAwardReq = 16395,
	[OriginalName("EndlessMine_ExchangeAward_Ack")]
	EndlessMineExchangeAwardAck = 16396,
	[OriginalName("EndlessMine_TargetAward_Req")]
	EndlessMineTargetAwardReq = 16397,
	[OriginalName("EndlessMine_TargetAward_Ack")]
	EndlessMineTargetAwardAck = 16398,
	[OriginalName("Operate_ZhanLingData_Req")]
	OperateZhanLingDataReq = 16401,
	[OriginalName("Operate_ZhanLingData_Ack")]
	OperateZhanLingDataAck = 16402,
	[OriginalName("Operate_ZhanLingKillAward_Req")]
	OperateZhanLingKillAwardReq = 16403,
	[OriginalName("Operate_ZhanLingKillAward_Ack")]
	OperateZhanLingKillAwardAck = 16404,
	[OriginalName("Operate_BossSubData_Req")]
	OperateBossSubDataReq = 16421,
	[OriginalName("Operate_BossSubData_Ack")]
	OperateBossSubDataAck = 16422,
	[OriginalName("Operate_BossSubFightStart_Req")]
	OperateBossSubFightStartReq = 16423,
	[OriginalName("Operate_BossSubFightStart_Ack")]
	OperateBossSubFightStartAck = 16424,
	[OriginalName("Operate_BossSubFightResult_Req")]
	OperateBossSubFightResultReq = 16425,
	[OriginalName("Operate_BossSubFightResult_Ack")]
	OperateBossSubFightResultAck = 16426,
	[OriginalName("Operate_BossSubZhanLingAward_Req")]
	OperateBossSubZhanLingAwardReq = 16427,
	[OriginalName("Operate_BossSubZhanLingAward_Ack")]
	OperateBossSubZhanLingAwardAck = 16428,
	[OriginalName("Operate_BossSubRoulette_Req")]
	OperateBossSubRouletteReq = 16431,
	[OriginalName("Operate_BossSubRoulette_Ack")]
	OperateBossSubRouletteAck = 16432,
	[OriginalName("Operate_BossSubRouletteTarget_Req")]
	OperateBossSubRouletteTargetReq = 16433,
	[OriginalName("Operate_BossSubRouletteTarget_Ack")]
	OperateBossSubRouletteTargetAck = 16434,
	[OriginalName("Operate_BossSubTaskAward_Req")]
	OperateBossSubTaskAwardReq = 16435,
	[OriginalName("Operate_BossSubTaskAward_Ack")]
	OperateBossSubTaskAwardAck = 16436,
	[OriginalName("Operate_BossSubSaoDang_Req")]
	OperateBossSubSaoDangReq = 16437,
	[OriginalName("Operate_BossSubSaoDang_Ack")]
	OperateBossSubSaoDangAck = 16438,
	[OriginalName("Operate_AdsDirectAward_Req")]
	OperateAdsDirectAwardReq = 16451,
	[OriginalName("Operate_AdsDirectAward_Ack")]
	OperateAdsDirectAwardAck = 16452,
	[OriginalName("Operate_AdsGiftFreeAward_Req")]
	OperateAdsGiftFreeAwardReq = 16453,
	[OriginalName("Operate_AdsGiftFreeAward_Ack")]
	OperateAdsGiftFreeAwardAck = 16454,
	[OriginalName("Operate_AdsGiftAdAward_Req")]
	OperateAdsGiftAdAwardReq = 16455,
	[OriginalName("Operate_AdsGiftAdAward_Ack")]
	OperateAdsGiftAdAwardAck = 16456,
	[OriginalName("Operate_AdsRouletteDraw_Req")]
	OperateAdsRouletteDrawReq = 16457,
	[OriginalName("Operate_AdsRouletteDraw_Ack")]
	OperateAdsRouletteDrawAck = 16458,
	[OriginalName("Operate_AdsRouletteTarget_Req")]
	OperateAdsRouletteTargetReq = 16459,
	[OriginalName("Operate_AdsRouletteTarget_Ack")]
	OperateAdsRouletteTargetAck = 16460,
	[OriginalName("Operate_AdsSignFreeAward_Req")]
	OperateAdsSignFreeAwardReq = 16461,
	[OriginalName("Operate_AdsSignFreeAward_Ack")]
	OperateAdsSignFreeAwardAck = 16462,
	[OriginalName("Operate_AdsSignAdAward_Req")]
	OperateAdsSignAdAwardReq = 16463,
	[OriginalName("Operate_AdsSignAdAward_Ack")]
	OperateAdsSignAdAwardAck = 16464,
	[OriginalName("Operate_AdsZhanLingData_Req")]
	OperateAdsZhanLingDataReq = 16465,
	[OriginalName("Operate_AdsZhanLingData_Ack")]
	OperateAdsZhanLingDataAck = 16466,
	[OriginalName("Operate_AdsZhanLingFreeAward_Req")]
	OperateAdsZhanLingFreeAwardReq = 16467,
	[OriginalName("Operate_AdsZhanLingFreeAward_Ack")]
	OperateAdsZhanLingFreeAwardAck = 16468,
	[OriginalName("Operate_AdsZhanLingAdAward_Req")]
	OperateAdsZhanLingAdAwardReq = 16469,
	[OriginalName("Operate_AdsZhanLingAdAward_Ack")]
	OperateAdsZhanLingAdAwardAck = 16470,
	[OriginalName("Operate_WallSubjectData_Req")]
	OperateWallSubjectDataReq = 16481,
	[OriginalName("Operate_WallSubjectData_Ack")]
	OperateWallSubjectDataAck = 16482,
	[OriginalName("Operate_WallSubjectFightStart_Req")]
	OperateWallSubjectFightStartReq = 16483,
	[OriginalName("Operate_WallSubjectFightStart_Ack")]
	OperateWallSubjectFightStartAck = 16484,
	[OriginalName("Operate_WallSubjectFightResult_Req")]
	OperateWallSubjectFightResultReq = 16485,
	[OriginalName("Operate_WallSubjectFightResult_Ack")]
	OperateWallSubjectFightResultAck = 16486,
	[OriginalName("Operate_WallSubjectChapterAward_Req")]
	OperateWallSubjectChapterAwardReq = 16487,
	[OriginalName("Operate_WallSubjectChapterAward_Ack")]
	OperateWallSubjectChapterAwardAck = 16488,
	[OriginalName("Operate_WallSubjectBarDraw_Req")]
	OperateWallSubjectBarDrawReq = 16489,
	[OriginalName("Operate_WallSubjectBarDraw_Ack")]
	OperateWallSubjectBarDrawAck = 16490,
	[OriginalName("Operate_WallSubjectZhanLingAward_Req")]
	OperateWallSubjectZhanLingAwardReq = 16491,
	[OriginalName("Operate_WallSubjectZhanLingAward_Ack")]
	OperateWallSubjectZhanLingAwardAck = 16492,
	[OriginalName("Operate_WallSubjectRoulette_Req")]
	OperateWallSubjectRouletteReq = 16493,
	[OriginalName("Operate_WallSubjectRoulette_Ack")]
	OperateWallSubjectRouletteAck = 16494,
	[OriginalName("Operate_WallSubjectRouletteTarget_Req")]
	OperateWallSubjectRouletteTargetReq = 16495,
	[OriginalName("Operate_WallSubjectRouletteTarget_Ack")]
	OperateWallSubjectRouletteTargetAck = 16496,
	[OriginalName("Operate_MechaSubjectData_Req")]
	OperateMechaSubjectDataReq = 16501,
	[OriginalName("Operate_MechaSubjectData_Ack")]
	OperateMechaSubjectDataAck = 16502,
	[OriginalName("Operate_MechaSubjectFightStart_Req")]
	OperateMechaSubjectFightStartReq = 16503,
	[OriginalName("Operate_MechaSubjectFightStart_Ack")]
	OperateMechaSubjectFightStartAck = 16504,
	[OriginalName("Operate_MechaSubjectFightResult_Req")]
	OperateMechaSubjectFightResultReq = 16505,
	[OriginalName("Operate_MechaSubjectFightResult_Ack")]
	OperateMechaSubjectFightResultAck = 16506,
	[OriginalName("Operate_MechaSubjectChapterAward_Req")]
	OperateMechaSubjectChapterAwardReq = 16507,
	[OriginalName("Operate_MechaSubjectChapterAward_Ack")]
	OperateMechaSubjectChapterAwardAck = 16508,
	[OriginalName("Operate_MechaSubjectBarDraw_Req")]
	OperateMechaSubjectBarDrawReq = 16509,
	[OriginalName("Operate_MechaSubjectBarDraw_Ack")]
	OperateMechaSubjectBarDrawAck = 16510,
	[OriginalName("Operate_MechaSubjectZhanLingAward_Req")]
	OperateMechaSubjectZhanLingAwardReq = 16511,
	[OriginalName("Operate_MechaSubjectZhanLingAward_Ack")]
	OperateMechaSubjectZhanLingAwardAck = 16512,
	[OriginalName("Operate_MechaSubjectRoulette_Req")]
	OperateMechaSubjectRouletteReq = 16513,
	[OriginalName("Operate_MechaSubjectRoulette_Ack")]
	OperateMechaSubjectRouletteAck = 16514,
	[OriginalName("Operate_MechaSubjectRouletteTarget_Req")]
	OperateMechaSubjectRouletteTargetReq = 16515,
	[OriginalName("Operate_MechaSubjectRouletteTarget_Ack")]
	OperateMechaSubjectRouletteTargetAck = 16516,
	[OriginalName("Journey_Data_Req")]
	JourneyDataReq = 16601,
	[OriginalName("Journey_Data_Ack")]
	JourneyDataAck = 16602,
	[OriginalName("Journey_ChapterOneBarDraw_Req")]
	JourneyChapterOneBarDrawReq = 16603,
	[OriginalName("Journey_ChapterOneBarDraw_Ack")]
	JourneyChapterOneBarDrawAck = 16604,
	[OriginalName("Journey_ChapterOneBarTarget_Req")]
	JourneyChapterOneBarTargetReq = 16605,
	[OriginalName("Journey_ChapterOneBarTarget_Ack")]
	JourneyChapterOneBarTargetAck = 16606,
	[OriginalName("Journey_ChapterOneSign_Req")]
	JourneyChapterOneSignReq = 16607,
	[OriginalName("Journey_ChapterOneSign_Ack")]
	JourneyChapterOneSignAck = 16608,
	[OriginalName("Journey_ChapterOneRechargeSum_Req")]
	JourneyChapterOneRechargeSumReq = 16609,
	[OriginalName("Journey_ChapterOneRechargeSum_Ack")]
	JourneyChapterOneRechargeSumAck = 16610,
	[OriginalName("Journey_ChapterOneExchange_Req")]
	JourneyChapterOneExchangeReq = 16611,
	[OriginalName("Journey_ChapterOneExchange_Ack")]
	JourneyChapterOneExchangeAck = 16612,
	[OriginalName("Journey_ChapterOneZhanLingAward_Req")]
	JourneyChapterOneZhanLingAwardReq = 16613,
	[OriginalName("Journey_ChapterOneZhanLingAward_Ack")]
	JourneyChapterOneZhanLingAwardAck = 16614,
	[OriginalName("Journey_ChapterTwoBarDraw_Req")]
	JourneyChapterTwoBarDrawReq = 16621,
	[OriginalName("Journey_ChapterTwoBarDraw_Ack")]
	JourneyChapterTwoBarDrawAck = 16622,
	[OriginalName("Journey_ChapterTwoBarTarget_Req")]
	JourneyChapterTwoBarTargetReq = 16623,
	[OriginalName("Journey_ChapterTwoBarTarget_Ack")]
	JourneyChapterTwoBarTargetAck = 16624,
	[OriginalName("Journey_ChapterTwoActive_Req")]
	JourneyChapterTwoActiveReq = 16625,
	[OriginalName("Journey_ChapterTwoActive_Ack")]
	JourneyChapterTwoActiveAck = 16626,
	[OriginalName("Journey_ChapterTwoDiamondCost_Req")]
	JourneyChapterTwoDiamondCostReq = 16627,
	[OriginalName("Journey_ChapterTwoDiamondCost_Ack")]
	JourneyChapterTwoDiamondCostAck = 16628,
	[OriginalName("Journey_ChapterTwoExchange_Req")]
	JourneyChapterTwoExchangeReq = 16629,
	[OriginalName("Journey_ChapterTwoExchange_Ack")]
	JourneyChapterTwoExchangeAck = 16630,
	[OriginalName("Journey_ChapterTwoZhanLingAward_Req")]
	JourneyChapterTwoZhanLingAwardReq = 16631,
	[OriginalName("Journey_ChapterTwoZhanLingAward_Ack")]
	JourneyChapterTwoZhanLingAwardAck = 16632,
	[OriginalName("Journey_ChapterThreeBarDraw_Req")]
	JourneyChapterThreeBarDrawReq = 16641,
	[OriginalName("Journey_ChapterThreeBarDraw_Ack")]
	JourneyChapterThreeBarDrawAck = 16642,
	[OriginalName("Journey_ChapterThreeBarTarget_Req")]
	JourneyChapterThreeBarTargetReq = 16643,
	[OriginalName("Journey_ChapterThreeBarTarget_Ack")]
	JourneyChapterThreeBarTargetAck = 16644,
	[OriginalName("Journey_ChapterThreeSign_Req")]
	JourneyChapterThreeSignReq = 16645,
	[OriginalName("Journey_ChapterThreeSign_Ack")]
	JourneyChapterThreeSignAck = 16646,
	[OriginalName("Journey_ChapterThreeExchange_Req")]
	JourneyChapterThreeExchangeReq = 16647,
	[OriginalName("Journey_ChapterThreeExchange_Ack")]
	JourneyChapterThreeExchangeAck = 16648,
	[OriginalName("Journey_ChapterThreeDrawCount_Req")]
	JourneyChapterThreeDrawCountReq = 16649,
	[OriginalName("Journey_ChapterThreeDrawCount_Ack")]
	JourneyChapterThreeDrawCountAck = 16650,
	[OriginalName("Journey_ChapterThreeZhanLingAward_Req")]
	JourneyChapterThreeZhanLingAwardReq = 16651,
	[OriginalName("Journey_ChapterThreeZhanLingAward_Ack")]
	JourneyChapterThreeZhanLingAwardAck = 16652,
	[OriginalName("Friends_List_Req")]
	FriendsListReq = 17001,
	[OriginalName("Friends_List_Ack")]
	FriendsListAck = 17002,
	[OriginalName("Friends_Recommend_Req")]
	FriendsRecommendReq = 17003,
	[OriginalName("Friends_Recommend_Ack")]
	FriendsRecommendAck = 17004,
	[OriginalName("Friends_Search_Req")]
	FriendsSearchReq = 17005,
	[OriginalName("Friends_Search_Ack")]
	FriendsSearchAck = 17006,
	[OriginalName("Friends_Delete_Req")]
	FriendsDeleteReq = 17007,
	[OriginalName("Friends_Delete_Ack")]
	FriendsDeleteAck = 17008,
	[OriginalName("Friends_Apply_Req")]
	FriendsApplyReq = 17011,
	[OriginalName("Friends_Apply_Ack")]
	FriendsApplyAck = 17012,
	[OriginalName("Friends_ApplyBatch_Req")]
	FriendsApplyBatchReq = 17013,
	[OriginalName("Friends_ApplyBatch_Ack")]
	FriendsApplyBatchAck = 17014,
	[OriginalName("Friends_ApplyList_Req")]
	FriendsApplyListReq = 17015,
	[OriginalName("Friends_ApplyList_Ack")]
	FriendsApplyListAck = 17016,
	[OriginalName("Friends_ApplyPass_Req")]
	FriendsApplyPassReq = 17017,
	[OriginalName("Friends_ApplyPass_Ack")]
	FriendsApplyPassAck = 17018,
	[OriginalName("Friends_ApplyPassAll_Req")]
	FriendsApplyPassAllReq = 17021,
	[OriginalName("Friends_ApplyPassAll_Ack")]
	FriendsApplyPassAllAck = 17022,
	[OriginalName("Friends_ApplyRefuse_Req")]
	FriendsApplyRefuseReq = 17023,
	[OriginalName("Friends_ApplyRefuse_Ack")]
	FriendsApplyRefuseAck = 17024,
	[OriginalName("Friends_ApplyRefuseAll_Req")]
	FriendsApplyRefuseAllReq = 17025,
	[OriginalName("Friends_ApplyRefuseAll_Ack")]
	FriendsApplyRefuseAllAck = 17026,
	[OriginalName("Friends_ActionGive_Req")]
	FriendsActionGiveReq = 17031,
	[OriginalName("Friends_ActionGive_Ack")]
	FriendsActionGiveAck = 17032,
	[OriginalName("Friends_ActionCollect_Req")]
	FriendsActionCollectReq = 17033,
	[OriginalName("Friends_ActionCollect_Ack")]
	FriendsActionCollectAck = 17034,
	[OriginalName("Friends_ActionGiveAll_Req")]
	FriendsActionGiveAllReq = 17035,
	[OriginalName("Friends_ActionGiveAll_Ack")]
	FriendsActionGiveAllAck = 17036,
	[OriginalName("Friends_ActionCollectAll_Req")]
	FriendsActionCollectAllReq = 17037,
	[OriginalName("Friends_ActionCollectAll_Ack")]
	FriendsActionCollectAllAck = 17038,
	[OriginalName("Legion_Recommend_Req")]
	LegionRecommendReq = 17201,
	[OriginalName("Legion_Recommend_Ack")]
	LegionRecommendAck = 17202,
	[OriginalName("Legion_Search_Req")]
	LegionSearchReq = 17203,
	[OriginalName("Legion_Search_Ack")]
	LegionSearchAck = 17204,
	[OriginalName("Legion_TipsInfo_Req")]
	LegionTipsInfoReq = 17205,
	[OriginalName("Legion_TipsInfo_Ack")]
	LegionTipsInfoAck = 17206,
	[OriginalName("Legion_Create_Req")]
	LegionCreateReq = 17207,
	[OriginalName("Legion_Create_Ack")]
	LegionCreateAck = 17208,
	[OriginalName("Legion_Apply_Req")]
	LegionApplyReq = 17211,
	[OriginalName("Legion_Apply_Ack")]
	LegionApplyAck = 17212,
	[OriginalName("Legion_JoinQuick_Req")]
	LegionJoinQuickReq = 17213,
	[OriginalName("Legion_JoinQuick_Ack")]
	LegionJoinQuickAck = 17214,
	[OriginalName("Legion_ClearJoinCD_Req")]
	LegionClearJoinCdReq = 17215,
	[OriginalName("Legion_ClearJoinCD_Ack")]
	LegionClearJoinCdAck = 17216,
	[OriginalName("Legion_Quit_Req")]
	LegionQuitReq = 17217,
	[OriginalName("Legion_Quit_Ack")]
	LegionQuitAck = 17218,
	[OriginalName("Legion_Data_Req")]
	LegionDataReq = 17221,
	[OriginalName("Legion_Data_Ack")]
	LegionDataAck = 17222,
	[OriginalName("Legion_Members_Req")]
	LegionMembersReq = 17223,
	[OriginalName("Legion_Members_Ack")]
	LegionMembersAck = 17224,
	[OriginalName("Legion_ApplyList_Req")]
	LegionApplyListReq = 17225,
	[OriginalName("Legion_ApplyList_Ack")]
	LegionApplyListAck = 17226,
	[OriginalName("Legion_ApplyPass_Req")]
	LegionApplyPassReq = 17227,
	[OriginalName("Legion_ApplyPass_Ack")]
	LegionApplyPassAck = 17228,
	[OriginalName("Legion_ApplyPassAll_Req")]
	LegionApplyPassAllReq = 17231,
	[OriginalName("Legion_ApplyPassAll_Ack")]
	LegionApplyPassAllAck = 17232,
	[OriginalName("Legion_ApplyRefuse_Req")]
	LegionApplyRefuseReq = 17233,
	[OriginalName("Legion_ApplyRefuse_Ack")]
	LegionApplyRefuseAck = 17234,
	[OriginalName("Legion_ApplyRefuseAll_Req")]
	LegionApplyRefuseAllReq = 17235,
	[OriginalName("Legion_ApplyRefuseAll_Ack")]
	LegionApplyRefuseAllAck = 17236,
	[OriginalName("Legion_Kick_Req")]
	LegionKickReq = 17237,
	[OriginalName("Legion_Kick_Ack")]
	LegionKickAck = 17238,
	[OriginalName("Legion_NameChange_Req")]
	LegionNameChangeReq = 17241,
	[OriginalName("Legion_NameChange_Ack")]
	LegionNameChangeAck = 17242,
	[OriginalName("Legion_IconChange_Req")]
	LegionIconChangeReq = 17243,
	[OriginalName("Legion_IconChange_Ack")]
	LegionIconChangeAck = 17244,
	[OriginalName("Legion_SignChange_Req")]
	LegionSignChangeReq = 17245,
	[OriginalName("Legion_SignChange_Ack")]
	LegionSignChangeAck = 17246,
	[OriginalName("Legion_JoinSet_Req")]
	LegionJoinSetReq = 17247,
	[OriginalName("Legion_JoinSet_Ack")]
	LegionJoinSetAck = 17248,
	[OriginalName("Legion_OfficialSet_Req")]
	LegionOfficialSetReq = 17251,
	[OriginalName("Legion_OfficialSet_Ack")]
	LegionOfficialSetAck = 17252,
	[OriginalName("Legion_LogList_Req")]
	LegionLogListReq = 17253,
	[OriginalName("Legion_LogList_Ack")]
	LegionLogListAck = 17254,
	[OriginalName("LegionAux_Donate_Req")]
	LegionAuxDonateReq = 17261,
	[OriginalName("LegionAux_Donate_Ack")]
	LegionAuxDonateAck = 17262,
	[OriginalName("LegionAux_DonateTarget_Req")]
	LegionAuxDonateTargetReq = 17263,
	[OriginalName("LegionAux_DonateTarget_Ack")]
	LegionAuxDonateTargetAck = 17264,
	[OriginalName("LegionAux_Dinner_Req")]
	LegionAuxDinnerReq = 17265,
	[OriginalName("LegionAux_Dinner_Ack")]
	LegionAuxDinnerAck = 17266,
	[OriginalName("LegionAux_Reward_Req")]
	LegionAuxRewardReq = 17267,
	[OriginalName("LegionAux_Reward_Ack")]
	LegionAuxRewardAck = 17268,
	[OriginalName("LegionBoss_FightStart_Req")]
	LegionBossFightStartReq = 17271,
	[OriginalName("LegionBoss_FightStart_Ack")]
	LegionBossFightStartAck = 17272,
	[OriginalName("LegionBoss_FightResult_Req")]
	LegionBossFightResultReq = 17273,
	[OriginalName("LegionBoss_FightResult_Ack")]
	LegionBossFightResultAck = 17274,
	[OriginalName("LegionBoss_SaoDang_Req")]
	LegionBossSaoDangReq = 17275,
	[OriginalName("LegionBoss_SaoDang_Ack")]
	LegionBossSaoDangAck = 17276,
	[OriginalName("LegionBoss_PassAward_Req")]
	LegionBossPassAwardReq = 17277,
	[OriginalName("LegionBoss_PassAward_Ack")]
	LegionBossPassAwardAck = 17278,
	[OriginalName("LegionBoss_BuyCombat_Req")]
	LegionBossBuyCombatReq = 17279,
	[OriginalName("LegionBoss_BuyCombat_Ack")]
	LegionBossBuyCombatAck = 17280,
	[OriginalName("LegionBoss_RankList_Req")]
	LegionBossRankListReq = 17281,
	[OriginalName("LegionBoss_RankList_Ack")]
	LegionBossRankListAck = 17282,
	[OriginalName("LegionBoss_RecordList_Req")]
	LegionBossRecordListReq = 17283,
	[OriginalName("LegionBoss_RecordList_Ack")]
	LegionBossRecordListAck = 17284,
	[OriginalName("Advertisement_Data_Req")]
	AdvertisementDataReq = 17501,
	[OriginalName("Advertisement_Data_Ack")]
	AdvertisementDataAck = 17502,
	[OriginalName("Advertisement_MainAwardDouble_Req")]
	AdvertisementMainAwardDoubleReq = 17503,
	[OriginalName("Advertisement_MainAwardDouble_Ack")]
	AdvertisementMainAwardDoubleAck = 17504,
	[OriginalName("Advertisement_GiftAward_Req")]
	AdvertisementGiftAwardReq = 17505,
	[OriginalName("Advertisement_GiftAward_Ack")]
	AdvertisementGiftAwardAck = 17506,
	[OriginalName("Advertisement_LegionDonate_Req")]
	AdvertisementLegionDonateReq = 17507,
	[OriginalName("Advertisement_LegionDonate_Ack")]
	AdvertisementLegionDonateAck = 17508,
	[OriginalName("Advertisement_BarDraw_Req")]
	AdvertisementBarDrawReq = 17511,
	[OriginalName("Advertisement_BarDraw_Ack")]
	AdvertisementBarDrawAck = 17512,
	[OriginalName("Advertisement_LimitTimeRoulette_Req")]
	AdvertisementLimitTimeRouletteReq = 17515,
	[OriginalName("Advertisement_LimitTimeRoulette_Ack")]
	AdvertisementLimitTimeRouletteAck = 17516,
	[OriginalName("Advertisement_LimitTimeTreasure_Req")]
	AdvertisementLimitTimeTreasureReq = 17517,
	[OriginalName("Advertisement_LimitTimeTreasure_Ack")]
	AdvertisementLimitTimeTreasureAck = 17518,
	[OriginalName("Advertisement_LimitTimeVendor_Req")]
	AdvertisementLimitTimeVendorReq = 17521,
	[OriginalName("Advertisement_LimitTimeVendor_Ack")]
	AdvertisementLimitTimeVendorAck = 17522,
	[OriginalName("Advertisement_ChapterOneBar_Req")]
	AdvertisementChapterOneBarReq = 17523,
	[OriginalName("Advertisement_ChapterOneBar_Ack")]
	AdvertisementChapterOneBarAck = 17524,
	[OriginalName("Advertisement_ChapterTwoBar_Req")]
	AdvertisementChapterTwoBarReq = 17525,
	[OriginalName("Advertisement_ChapterTwoBar_Ack")]
	AdvertisementChapterTwoBarAck = 17526,
	[OriginalName("Advertisement_ChapterThreeBar_Req")]
	AdvertisementChapterThreeBarReq = 17527,
	[OriginalName("Advertisement_ChapterThreeBar_Ack")]
	AdvertisementChapterThreeBarAck = 17528,
	[OriginalName("Advertisement_BossSubRoulette_Req")]
	AdvertisementBossSubRouletteReq = 17531,
	[OriginalName("Advertisement_BossSubRoulette_Ack")]
	AdvertisementBossSubRouletteAck = 17532,
	[OriginalName("Advertisement_WallSubjectRoulette_Req")]
	AdvertisementWallSubjectRouletteReq = 17533,
	[OriginalName("Advertisement_WallSubjectRoulette_Ack")]
	AdvertisementWallSubjectRouletteAck = 17534,
	[OriginalName("Advertisement_MechaSubjectRoulette_Req")]
	AdvertisementMechaSubjectRouletteReq = 17535,
	[OriginalName("Advertisement_MechaSubjectRoulette_Ack")]
	AdvertisementMechaSubjectRouletteAck = 17536,
	[OriginalName("Bar_Data_Req")]
	BarDataReq = 18001,
	[OriginalName("Bar_Data_Ack")]
	BarDataAck = 18002,
	[OriginalName("Bar_Draw_Req")]
	BarDrawReq = 18003,
	[OriginalName("Bar_Draw_Ack")]
	BarDrawAck = 18004,
	[OriginalName("Bar_TargetAward_Req")]
	BarTargetAwardReq = 18005,
	[OriginalName("Bar_TargetAward_Ack")]
	BarTargetAwardAck = 18006,
	[OriginalName("Shop_Data_Req")]
	ShopDataReq = 18011,
	[OriginalName("Shop_Data_Ack")]
	ShopDataAck = 18012,
	[OriginalName("Shop_Buy_Req")]
	ShopBuyReq = 18013,
	[OriginalName("Shop_Buy_Ack")]
	ShopBuyAck = 18014,
	[OriginalName("Peerage_Data_Req")]
	PeerageDataReq = 18201,
	[OriginalName("Peerage_Data_Ack")]
	PeerageDataAck = 18202,
	[OriginalName("Peerage_Upgrade_Req")]
	PeerageUpgradeReq = 18203,
	[OriginalName("Peerage_Upgrade_Ack")]
	PeerageUpgradeAck = 18204,
	[OriginalName("Peerage_UpgradeTask_Req")]
	PeerageUpgradeTaskReq = 18205,
	[OriginalName("Peerage_UpgradeTask_Ack")]
	PeerageUpgradeTaskAck = 18206,
	[OriginalName("DailyTask_Data_Req")]
	DailyTaskDataReq = 18221,
	[OriginalName("DailyTask_Data_Ack")]
	DailyTaskDataAck = 18222,
	[OriginalName("DailyTask_Award_Req")]
	DailyTaskAwardReq = 18223,
	[OriginalName("DailyTask_Award_Ack")]
	DailyTaskAwardAck = 18224,
	[OriginalName("DailyTask_TargetAward_Req")]
	DailyTaskTargetAwardReq = 18225,
	[OriginalName("DailyTask_TargetAward_Ack")]
	DailyTaskTargetAwardAck = 18226,
	[OriginalName("Bag_ItemSale_Req")]
	BagItemSaleReq = 18501,
	[OriginalName("Bag_ItemSale_Ack")]
	BagItemSaleAck = 18502,
	[OriginalName("Bag_LimitItemSale_Req")]
	BagLimitItemSaleReq = 18503,
	[OriginalName("Bag_LimitItemSale_Ack")]
	BagLimitItemSaleAck = 18504,
	[OriginalName("Bag_ItemCompose_Req")]
	BagItemComposeReq = 18505,
	[OriginalName("Bag_ItemCompose_Ack")]
	BagItemComposeAck = 18506,
	[OriginalName("Bag_RandBoxOpen_Req")]
	BagRandBoxOpenReq = 18507,
	[OriginalName("Bag_RandBoxOpen_Ack")]
	BagRandBoxOpenAck = 18508,
	[OriginalName("Bag_SelectBoxView_Req")]
	BagSelectBoxViewReq = 18511,
	[OriginalName("Bag_SelectBoxView_Ack")]
	BagSelectBoxViewAck = 18512,
	[OriginalName("Bag_SelectBoxOpen_Req")]
	BagSelectBoxOpenReq = 18513,
	[OriginalName("Bag_SelectBoxOpen_Ack")]
	BagSelectBoxOpenAck = 18514,
	[OriginalName("Bag_ComposeBoxOpen_Req")]
	BagComposeBoxOpenReq = 18515,
	[OriginalName("Bag_ComposeBoxOpen_Ack")]
	BagComposeBoxOpenAck = 18516,
	[OriginalName("Bag_HeadActive_Req")]
	BagHeadActiveReq = 18521,
	[OriginalName("Bag_HeadActive_Ack")]
	BagHeadActiveAck = 18522,
	[OriginalName("Bag_HeadFrameActive_Req")]
	BagHeadFrameActiveReq = 18523,
	[OriginalName("Bag_HeadFrameActive_Ack")]
	BagHeadFrameActiveAck = 18524,
	[OriginalName("Bag_VoucherCouponActive_Req")]
	BagVoucherCouponActiveReq = 18527,
	[OriginalName("Bag_VoucherCouponActive_Ack")]
	BagVoucherCouponActiveAck = 18528,
	[OriginalName("Bag_GeneComposeCalc_Req")]
	BagGeneComposeCalcReq = 18531,
	[OriginalName("Bag_GeneComposeCalc_Ack")]
	BagGeneComposeCalcAck = 18532,
	[OriginalName("Bag_GeneCompose_Req")]
	BagGeneComposeReq = 18533,
	[OriginalName("Bag_GeneCompose_Ack")]
	BagGeneComposeAck = 18534,
	[OriginalName("Hero_Compose_Req")]
	HeroComposeReq = 19001,
	[OriginalName("Hero_Compose_Ack")]
	HeroComposeAck = 19002,
	[OriginalName("Hero_QualityUpgrade_Req")]
	HeroQualityUpgradeReq = 19003,
	[OriginalName("Hero_QualityUpgrade_Ack")]
	HeroQualityUpgradeAck = 19004,
	[OriginalName("Hero_FragDecompose_Req")]
	HeroFragDecomposeReq = 19011,
	[OriginalName("Hero_FragDecompose_Ack")]
	HeroFragDecomposeAck = 19012,
	[OriginalName("Hero_RarityUpgrade_Req")]
	HeroRarityUpgradeReq = 19013,
	[OriginalName("Hero_RarityUpgrade_Ack")]
	HeroRarityUpgradeAck = 19014,
	[OriginalName("EquipSlot_LevelUpgrade_Req")]
	EquipSlotLevelUpgradeReq = 19051,
	[OriginalName("EquipSlot_LevelUpgrade_Ack")]
	EquipSlotLevelUpgradeAck = 19052,
	[OriginalName("EquipSlot_LevelUpgradeAll_Req")]
	EquipSlotLevelUpgradeAllReq = 19053,
	[OriginalName("EquipSlot_LevelUpgradeAll_Ack")]
	EquipSlotLevelUpgradeAllAck = 19054,
	[OriginalName("EquipSlot_EquipSet_Req")]
	EquipSlotEquipSetReq = 19055,
	[OriginalName("EquipSlot_EquipSet_Ack")]
	EquipSlotEquipSetAck = 19056,
	[OriginalName("EquipSlot_EquipUpgrade_Req")]
	EquipSlotEquipUpgradeReq = 19057,
	[OriginalName("EquipSlot_EquipUpgrade_Ack")]
	EquipSlotEquipUpgradeAck = 19058,
	[OriginalName("EquipSlot_EquipLock_Req")]
	EquipSlotEquipLockReq = 19061,
	[OriginalName("EquipSlot_EquipLock_Ack")]
	EquipSlotEquipLockAck = 19062,
	[OriginalName("EquipSlot_GemSet_Req")]
	EquipSlotGemSetReq = 19081,
	[OriginalName("EquipSlot_GemSet_Ack")]
	EquipSlotGemSetAck = 19082,
	[OriginalName("EquipSlot_GemReplace_Req")]
	EquipSlotGemReplaceReq = 19083,
	[OriginalName("EquipSlot_GemReplace_Ack")]
	EquipSlotGemReplaceAck = 19084,
	[OriginalName("EquipSlot_GemRemove_Req")]
	EquipSlotGemRemoveReq = 19085,
	[OriginalName("EquipSlot_GemRemove_Ack")]
	EquipSlotGemRemoveAck = 19086,
	[OriginalName("EquipSlot_GemPlan_Req")]
	EquipSlotGemPlanReq = 19087,
	[OriginalName("EquipSlot_GemPlan_Ack")]
	EquipSlotGemPlanAck = 19088,
	[OriginalName("Equip_Decompose_Req")]
	EquipDecomposeReq = 19111,
	[OriginalName("Equip_Decompose_Ack")]
	EquipDecomposeAck = 19112,
	[OriginalName("Equip_Lock_Req")]
	EquipLockReq = 19113,
	[OriginalName("Equip_Lock_Ack")]
	EquipLockAck = 19114,
	[OriginalName("Gem_ComposePreview_Req")]
	GemComposePreviewReq = 19121,
	[OriginalName("Gem_ComposePreview_Ack")]
	GemComposePreviewAck = 19122,
	[OriginalName("Gem_Compose_Req")]
	GemComposeReq = 19123,
	[OriginalName("Gem_Compose_Ack")]
	GemComposeAck = 19124,
	[OriginalName("Gem_Lock_Req")]
	GemLockReq = 19125,
	[OriginalName("Gem_Lock_Ack")]
	GemLockAck = 19126,
	[OriginalName("Gem_Decompose_Req")]
	GemDecomposeReq = 19127,
	[OriginalName("Gem_Decompose_Ack")]
	GemDecomposeAck = 19128,
	[OriginalName("Gem_Cast_Req")]
	GemCastReq = 19129,
	[OriginalName("Gem_Cast_Ack")]
	GemCastAck = 19130,
	[OriginalName("HeroAwaken_Upgrade_Req")]
	HeroAwakenUpgradeReq = 19141,
	[OriginalName("HeroAwaken_Upgrade_Ack")]
	HeroAwakenUpgradeAck = 19142,
	[OriginalName("HeroAwaken_Advance_Req")]
	HeroAwakenAdvanceReq = 19143,
	[OriginalName("HeroAwaken_Advance_Ack")]
	HeroAwakenAdvanceAck = 19144,
	[OriginalName("HeroAwaken_GeneOpenHole_Req")]
	HeroAwakenGeneOpenHoleReq = 19151,
	[OriginalName("HeroAwaken_GeneOpenHole_Ack")]
	HeroAwakenGeneOpenHoleAck = 19152,
	[OriginalName("HeroAwaken_GeneRefreshHole_Req")]
	HeroAwakenGeneRefreshHoleReq = 19153,
	[OriginalName("HeroAwaken_GeneRefreshHole_Ack")]
	HeroAwakenGeneRefreshHoleAck = 19154,
	[OriginalName("HeroAwaken_GeneSet_Req")]
	HeroAwakenGeneSetReq = 19155,
	[OriginalName("HeroAwaken_GeneSet_Ack")]
	HeroAwakenGeneSetAck = 19156,
	[OriginalName("HeroAwaken_GeneRemove_Req")]
	HeroAwakenGeneRemoveReq = 19157,
	[OriginalName("HeroAwaken_GeneRemove_Ack")]
	HeroAwakenGeneRemoveAck = 19158,
	[OriginalName("HeroAwaken_GeneCompose_Req")]
	HeroAwakenGeneComposeReq = 19159,
	[OriginalName("HeroAwaken_GeneCompose_Ack")]
	HeroAwakenGeneComposeAck = 19160,
	[OriginalName("HeroFollow_Activate_Req")]
	HeroFollowActivateReq = 19171,
	[OriginalName("HeroFollow_Activate_Ack")]
	HeroFollowActivateAck = 19172,
	[OriginalName("HeroFollow_Upgrade_Req")]
	HeroFollowUpgradeReq = 19173,
	[OriginalName("HeroFollow_Upgrade_Ack")]
	HeroFollowUpgradeAck = 19174,
	[OriginalName("Suit_Active_Req")]
	SuitActiveReq = 19201,
	[OriginalName("Suit_Active_Ack")]
	SuitActiveAck = 19202,
	[OriginalName("Suit_Set_Req")]
	SuitSetReq = 19203,
	[OriginalName("Suit_Set_Ack")]
	SuitSetAck = 19204,
	[OriginalName("Suit_Remove_Req")]
	SuitRemoveReq = 19205,
	[OriginalName("Suit_Remove_Ack")]
	SuitRemoveAck = 19206,
	[OriginalName("Suit_UpgradeStar_Req")]
	SuitUpgradeStarReq = 19207,
	[OriginalName("Suit_UpgradeStar_Ack")]
	SuitUpgradeStarAck = 19208,
	[OriginalName("Suit_Convert_Req")]
	SuitConvertReq = 19211,
	[OriginalName("Suit_Convert_Ack")]
	SuitConvertAck = 19212,
	[OriginalName("Suit_SkillSet_Req")]
	SuitSkillSetReq = 19221,
	[OriginalName("Suit_SkillSet_Ack")]
	SuitSkillSetAck = 19222,
	[OriginalName("Suit_SkillRemove_Req")]
	SuitSkillRemoveReq = 19223,
	[OriginalName("Suit_SkillRemove_Ack")]
	SuitSkillRemoveAck = 19224,
	[OriginalName("Suit_SuperSkillSet_Req")]
	SuitSuperSkillSetReq = 19225,
	[OriginalName("Suit_SuperSkillSet_Ack")]
	SuitSuperSkillSetAck = 19226,
	[OriginalName("Suit_SuperSkillRemove_Req")]
	SuitSuperSkillRemoveReq = 19227,
	[OriginalName("Suit_SuperSkillRemove_Ack")]
	SuitSuperSkillRemoveAck = 19228,
	[OriginalName("Gun_Active_Req")]
	GunActiveReq = 19251,
	[OriginalName("Gun_Active_Ack")]
	GunActiveAck = 19252,
	[OriginalName("Gun_Set_Req")]
	GunSetReq = 19253,
	[OriginalName("Gun_Set_Ack")]
	GunSetAck = 19254,
	[OriginalName("Gun_Remove_Req")]
	GunRemoveReq = 19255,
	[OriginalName("Gun_Remove_Ack")]
	GunRemoveAck = 19256,
	[OriginalName("Gun_UpgradeStar_Req")]
	GunUpgradeStarReq = 19257,
	[OriginalName("Gun_UpgradeStar_Ack")]
	GunUpgradeStarAck = 19258,
	[OriginalName("Gun_Convert_Req")]
	GunConvertReq = 19261,
	[OriginalName("Gun_Convert_Ack")]
	GunConvertAck = 19262,
	[OriginalName("Gun_SetBullet_Req")]
	GunSetBulletReq = 19263,
	[OriginalName("Gun_SetBullet_Ack")]
	GunSetBulletAck = 19264,
	[OriginalName("Mecha_Data_Req")]
	MechaDataReq = 19271,
	[OriginalName("Mecha_Data_Ack")]
	MechaDataAck = 19272,
	[OriginalName("Mecha_Active_Req")]
	MechaActiveReq = 19273,
	[OriginalName("Mecha_Active_Ack")]
	MechaActiveAck = 19274,
	[OriginalName("Mecha_Set_Req")]
	MechaSetReq = 19275,
	[OriginalName("Mecha_Set_Ack")]
	MechaSetAck = 19276,
	[OriginalName("Mecha_UpgradeLevel_Req")]
	MechaUpgradeLevelReq = 19277,
	[OriginalName("Mecha_UpgradeLevel_Ack")]
	MechaUpgradeLevelAck = 19278,
	[OriginalName("Mecha_UpgradeStar_Req")]
	MechaUpgradeStarReq = 19281,
	[OriginalName("Mecha_UpgradeStar_Ack")]
	MechaUpgradeStarAck = 19282,
	[OriginalName("Mecha_UpgradeSkill_Req")]
	MechaUpgradeSkillReq = 19283,
	[OriginalName("Mecha_UpgradeSkill_Ack")]
	MechaUpgradeSkillAck = 19284,
	[OriginalName("Mecha_Convert_Req")]
	MechaConvertReq = 19285,
	[OriginalName("Mecha_Convert_Ack")]
	MechaConvertAck = 19286,
	[OriginalName("Mecha_ResetCalc_Req")]
	MechaResetCalcReq = 19287,
	[OriginalName("Mecha_ResetCalc_Ack")]
	MechaResetCalcAck = 19288,
	[OriginalName("Mecha_Reset_Req")]
	MechaResetReq = 19291,
	[OriginalName("Mecha_Reset_Ack")]
	MechaResetAck = 19292,
	[OriginalName("Mecha_FastUpgradeLevelCalc_Req")]
	MechaFastUpgradeLevelCalcReq = 19293,
	[OriginalName("Mecha_FastUpgradeLevelCalc_Ack")]
	MechaFastUpgradeLevelCalcAck = 19294,
	[OriginalName("Mecha_FastUpgradeLevel_Req")]
	MechaFastUpgradeLevelReq = 19295,
	[OriginalName("Mecha_FastUpgradeLevel_Ack")]
	MechaFastUpgradeLevelAck = 19296,
	[OriginalName("Wall_Data_Req")]
	WallDataReq = 19301,
	[OriginalName("Wall_Data_Ack")]
	WallDataAck = 19302,
	[OriginalName("Wall_Active_Req")]
	WallActiveReq = 19303,
	[OriginalName("Wall_Active_Ack")]
	WallActiveAck = 19304,
	[OriginalName("Wall_Set_Req")]
	WallSetReq = 19305,
	[OriginalName("Wall_Set_Ack")]
	WallSetAck = 19306,
	[OriginalName("Wall_UpgradeLevel_Req")]
	WallUpgradeLevelReq = 19307,
	[OriginalName("Wall_UpgradeLevel_Ack")]
	WallUpgradeLevelAck = 19308,
	[OriginalName("Wall_UpgradeStar_Req")]
	WallUpgradeStarReq = 19311,
	[OriginalName("Wall_UpgradeStar_Ack")]
	WallUpgradeStarAck = 19312,
	[OriginalName("Wall_Convert_Req")]
	WallConvertReq = 19313,
	[OriginalName("Wall_Convert_Ack")]
	WallConvertAck = 19314,
	[OriginalName("Wall_SkillSet_Req")]
	WallSkillSetReq = 19315,
	[OriginalName("Wall_SkillSet_Ack")]
	WallSkillSetAck = 19316,
	[OriginalName("Wall_SkillRemove_Req")]
	WallSkillRemoveReq = 19317,
	[OriginalName("Wall_SkillRemove_Ack")]
	WallSkillRemoveAck = 19318,
	[OriginalName("Foothold_Data_Req")]
	FootholdDataReq = 19331,
	[OriginalName("Foothold_Data_Ack")]
	FootholdDataAck = 19332,
	[OriginalName("Foothold_Active_Req")]
	FootholdActiveReq = 19333,
	[OriginalName("Foothold_Active_Ack")]
	FootholdActiveAck = 19334,
	[OriginalName("Foothold_Set_Req")]
	FootholdSetReq = 19335,
	[OriginalName("Foothold_Set_Ack")]
	FootholdSetAck = 19336,
	[OriginalName("Foothold_UpgradeStar_Req")]
	FootholdUpgradeStarReq = 19337,
	[OriginalName("Foothold_UpgradeStar_Ack")]
	FootholdUpgradeStarAck = 19338,
	[OriginalName("Foothold_Convert_Req")]
	FootholdConvertReq = 19341,
	[OriginalName("Foothold_Convert_Ack")]
	FootholdConvertAck = 19342,
	[OriginalName("Arena_Data_Req")]
	ArenaDataReq = 19381,
	[OriginalName("Arena_Data_Ack")]
	ArenaDataAck = 19382,
	[OriginalName("Arena_CombatList_Req")]
	ArenaCombatListReq = 19383,
	[OriginalName("Arena_CombatList_Ack")]
	ArenaCombatListAck = 19384,
	[OriginalName("Arena_FightStart_Req")]
	ArenaFightStartReq = 19385,
	[OriginalName("Arena_FightStart_Ack")]
	ArenaFightStartAck = 19386,
	[OriginalName("Arena_FightResult_Req")]
	ArenaFightResultReq = 19387,
	[OriginalName("Arena_FightResult_Ack")]
	ArenaFightResultAck = 19388,
	[OriginalName("Arena_BuyCombat_Req")]
	ArenaBuyCombatReq = 19391,
	[OriginalName("Arena_BuyCombat_Ack")]
	ArenaBuyCombatAck = 19392,
	[OriginalName("Arena_SaoDang_Req")]
	ArenaSaoDangReq = 19393,
	[OriginalName("Arena_SaoDang_Ack")]
	ArenaSaoDangAck = 19394,
	[OriginalName("Arena_RecordList_Req")]
	ArenaRecordListReq = 19395,
	[OriginalName("Arena_RecordList_Ack")]
	ArenaRecordListAck = 19396,
	[OriginalName("Arena_BattleResult_Req")]
	ArenaBattleResultReq = 19397,
	[OriginalName("Arena_BattleResult_Ack")]
	ArenaBattleResultAck = 19398,
	[OriginalName("Arena_RankList_Req")]
	ArenaRankListReq = 19401,
	[OriginalName("Arena_RankList_Ack")]
	ArenaRankListAck = 19402,
	[OriginalName("Arena_GiftBuy_Req")]
	ArenaGiftBuyReq = 19403,
	[OriginalName("Arena_GiftBuy_Ack")]
	ArenaGiftBuyAck = 19404,
	[OriginalName("PvpSet_Data_Req")]
	PvpSetDataReq = 19431,
	[OriginalName("PvpSet_Data_Ack")]
	PvpSetDataAck = 19432,
	[OriginalName("PvpSet_ModStart_Req")]
	PvpSetModStartReq = 19433,
	[OriginalName("PvpSet_ModStart_Ack")]
	PvpSetModStartAck = 19434,
	[OriginalName("PvpSet_ModRefresh_Req")]
	PvpSetModRefreshReq = 19435,
	[OriginalName("PvpSet_ModRefresh_Ack")]
	PvpSetModRefreshAck = 19436,
	[OriginalName("PvpSet_ModSelect_Req")]
	PvpSetModSelectReq = 19437,
	[OriginalName("PvpSet_ModSelect_Ack")]
	PvpSetModSelectAck = 19438,
	[OriginalName("PvpSet_ModSubmit_Req")]
	PvpSetModSubmitReq = 19441,
	[OriginalName("PvpSet_ModSubmit_Ack")]
	PvpSetModSubmitAck = 19442,
	[OriginalName("PvpSet_ModCancel_Req")]
	PvpSetModCancelReq = 19443,
	[OriginalName("PvpSet_ModCancel_Ack")]
	PvpSetModCancelAck = 19444,
	[OriginalName("Handbook_Data_Req")]
	HandbookDataReq = 19501,
	[OriginalName("Handbook_Data_Ack")]
	HandbookDataAck = 19502,
	[OriginalName("Handbook_MonsterActive_Req")]
	HandbookMonsterActiveReq = 19503,
	[OriginalName("Handbook_MonsterActive_Ack")]
	HandbookMonsterActiveAck = 19504,
	[OriginalName("Handbook_MonsterUpgrade_Req")]
	HandbookMonsterUpgradeReq = 19505,
	[OriginalName("Handbook_MonsterUpgrade_Ack")]
	HandbookMonsterUpgradeAck = 19506,
	[OriginalName("Handbook_ModActiveAward_Req")]
	HandbookModActiveAwardReq = 19511,
	[OriginalName("Handbook_ModActiveAward_Ack")]
	HandbookModActiveAwardAck = 19512,
	[OriginalName("Handbook_ModTypeTargetAward_Req")]
	HandbookModTypeTargetAwardReq = 19513,
	[OriginalName("Handbook_ModTypeTargetAward_Ack")]
	HandbookModTypeTargetAwardAck = 19514,
	[OriginalName("Handbook_ModTargetAward_Req")]
	HandbookModTargetAwardReq = 19515,
	[OriginalName("Handbook_ModTargetAward_Ack")]
	HandbookModTargetAwardAck = 19516,
	[OriginalName("Tactic_Compose_Req")]
	TacticComposeReq = 19551,
	[OriginalName("Tactic_Compose_Ack")]
	TacticComposeAck = 19552,
	[OriginalName("Tactic_StarUpgrade_Req")]
	TacticStarUpgradeReq = 19553,
	[OriginalName("Tactic_StarUpgrade_Ack")]
	TacticStarUpgradeAck = 19554,
	[OriginalName("Tactic_Convert_Req")]
	TacticConvertReq = 19555,
	[OriginalName("Tactic_Convert_Ack")]
	TacticConvertAck = 19556,
	[OriginalName("Treasure_Data_Req")]
	TreasureDataReq = 19601,
	[OriginalName("Treasure_Data_Ack")]
	TreasureDataAck = 19602,
	[OriginalName("Treasure_Active_Req")]
	TreasureActiveReq = 19603,
	[OriginalName("Treasure_Active_Ack")]
	TreasureActiveAck = 19604,
	[OriginalName("Treasure_UpgradeLevel_Req")]
	TreasureUpgradeLevelReq = 19605,
	[OriginalName("Treasure_UpgradeLevel_Ack")]
	TreasureUpgradeLevelAck = 19606,
	[OriginalName("Treasure_ActiveAll_Req")]
	TreasureActiveAllReq = 19607,
	[OriginalName("Treasure_ActiveAll_Ack")]
	TreasureActiveAllAck = 19608,
	[OriginalName("Treasure_UpgradeLevelAll_Req")]
	TreasureUpgradeLevelAllReq = 19609,
	[OriginalName("Treasure_UpgradeLevelAll_Ack")]
	TreasureUpgradeLevelAllAck = 19610,
	[OriginalName("Treasure_Convert_Req")]
	TreasureConvertReq = 19611,
	[OriginalName("Treasure_Convert_Ack")]
	TreasureConvertAck = 19612,
	[OriginalName("Servant_Data_Req")]
	ServantDataReq = 19651,
	[OriginalName("Servant_Data_Ack")]
	ServantDataAck = 19652,
	[OriginalName("Servant_UpgradeLevel_Req")]
	ServantUpgradeLevelReq = 19653,
	[OriginalName("Servant_UpgradeLevel_Ack")]
	ServantUpgradeLevelAck = 19654,
	[OriginalName("Servant_UpgradeQuality_Req")]
	ServantUpgradeQualityReq = 19655,
	[OriginalName("Servant_UpgradeQuality_Ack")]
	ServantUpgradeQualityAck = 19656,
	[OriginalName("Servant_UpgradeQualityProp_Req")]
	ServantUpgradeQualityPropReq = 19657,
	[OriginalName("Servant_UpgradeQualityProp_Ack")]
	ServantUpgradeQualityPropAck = 19658,
	[OriginalName("Servant_UpgradeSkill_Req")]
	ServantUpgradeSkillReq = 19661,
	[OriginalName("Servant_UpgradeSkill_Ack")]
	ServantUpgradeSkillAck = 19662,
	[OriginalName("Servant_UpgradeSubSkill_Req")]
	ServantUpgradeSubSkillReq = 19663,
	[OriginalName("Servant_UpgradeSubSkill_Ack")]
	ServantUpgradeSubSkillAck = 19664,
	[OriginalName("Servant_SubSkillFlush_Req")]
	ServantSubSkillFlushReq = 19665,
	[OriginalName("Servant_SubSkillFlush_Ack")]
	ServantSubSkillFlushAck = 19666,
	[OriginalName("Servant_UpgradeAttr_Req")]
	ServantUpgradeAttrReq = 19671,
	[OriginalName("Servant_UpgradeAttr_Ack")]
	ServantUpgradeAttrAck = 19672,
	[OriginalName("Servant_Lock_Req")]
	ServantLockReq = 19673,
	[OriginalName("Servant_Lock_Ack")]
	ServantLockAck = 19674,
	[OriginalName("Servant_Deploy_Req")]
	ServantDeployReq = 19675,
	[OriginalName("Servant_Deploy_Ack")]
	ServantDeployAck = 19676,
	[OriginalName("Servant_DecomposeCalc_Req")]
	ServantDecomposeCalcReq = 19677,
	[OriginalName("Servant_DecomposeCalc_Ack")]
	ServantDecomposeCalcAck = 19678,
	[OriginalName("Servant_Decompose_Req")]
	ServantDecomposeReq = 19681,
	[OriginalName("Servant_Decompose_Ack")]
	ServantDecomposeAck = 19682,
	[OriginalName("Incubate_Data_Req")]
	IncubateDataReq = 19701,
	[OriginalName("Incubate_Data_Ack")]
	IncubateDataAck = 19702,
	[OriginalName("Incubate_SlotUnlock_Req")]
	IncubateSlotUnlockReq = 19703,
	[OriginalName("Incubate_SlotUnlock_Ack")]
	IncubateSlotUnlockAck = 19704,
	[OriginalName("Incubate_Start_Req")]
	IncubateStartReq = 19705,
	[OriginalName("Incubate_Start_Ack")]
	IncubateStartAck = 19706,
	[OriginalName("Incubate_Speed_Req")]
	IncubateSpeedReq = 19707,
	[OriginalName("Incubate_Speed_Ack")]
	IncubateSpeedAck = 19708,
	[OriginalName("Incubate_Complete_Req")]
	IncubateCompleteReq = 19711,
	[OriginalName("Incubate_Complete_Ack")]
	IncubateCompleteAck = 19712,
	[OriginalName("MainQuest_Data_Req")]
	MainQuestDataReq = 20001,
	[OriginalName("MainQuest_Data_Ack")]
	MainQuestDataAck = 20002,
	[OriginalName("MainQuest_FightStart_Req")]
	MainQuestFightStartReq = 20003,
	[OriginalName("MainQuest_FightStart_Ack")]
	MainQuestFightStartAck = 20004,
	[OriginalName("MainQuest_FightResult_Req")]
	MainQuestFightResultReq = 20005,
	[OriginalName("MainQuest_FightResult_Ack")]
	MainQuestFightResultAck = 20006,
	[OriginalName("MainQuest_ScoreAward_Req")]
	MainQuestScoreAwardReq = 20011,
	[OriginalName("MainQuest_ScoreAward_Ack")]
	MainQuestScoreAwardAck = 20012,
	[OriginalName("EliteQuest_Data_Req")]
	EliteQuestDataReq = 20041,
	[OriginalName("EliteQuest_Data_Ack")]
	EliteQuestDataAck = 20042,
	[OriginalName("EliteQuest_FightStart_Req")]
	EliteQuestFightStartReq = 20043,
	[OriginalName("EliteQuest_FightStart_Ack")]
	EliteQuestFightStartAck = 20044,
	[OriginalName("EliteQuest_FightResult_Req")]
	EliteQuestFightResultReq = 20045,
	[OriginalName("EliteQuest_FightResult_Ack")]
	EliteQuestFightResultAck = 20046,
	[OriginalName("EliteQuest_BuyCombat_Req")]
	EliteQuestBuyCombatReq = 20047,
	[OriginalName("EliteQuest_BuyCombat_Ack")]
	EliteQuestBuyCombatAck = 20048,
	[OriginalName("EliteQuest_ScoreAward_Req")]
	EliteQuestScoreAwardReq = 20051,
	[OriginalName("EliteQuest_ScoreAward_Ack")]
	EliteQuestScoreAwardAck = 20052,
	[OriginalName("EliteQuest_HelpRecordList_Req")]
	EliteQuestHelpRecordListReq = 20053,
	[OriginalName("EliteQuest_HelpRecordList_Ack")]
	EliteQuestHelpRecordListAck = 20054,
	[OriginalName("EliteQuest_HelpAward_Req")]
	EliteQuestHelpAwardReq = 20055,
	[OriginalName("EliteQuest_HelpAward_Ack")]
	EliteQuestHelpAwardAck = 20056,
	[OriginalName("EliteQuest_HelpAwardFast_Req")]
	EliteQuestHelpAwardFastReq = 20057,
	[OriginalName("EliteQuest_HelpAwardFast_Ack")]
	EliteQuestHelpAwardFastAck = 20058,
	[OriginalName("HeroTrial_Data_Req")]
	HeroTrialDataReq = 20081,
	[OriginalName("HeroTrial_Data_Ack")]
	HeroTrialDataAck = 20082,
	[OriginalName("HeroTrial_FightStart_Req")]
	HeroTrialFightStartReq = 20083,
	[OriginalName("HeroTrial_FightStart_Ack")]
	HeroTrialFightStartAck = 20084,
	[OriginalName("HeroTrial_FightResult_Req")]
	HeroTrialFightResultReq = 20085,
	[OriginalName("HeroTrial_FightResult_Ack")]
	HeroTrialFightResultAck = 20086,
	[OriginalName("ElementTrial_Data_Req")]
	ElementTrialDataReq = 20091,
	[OriginalName("ElementTrial_Data_Ack")]
	ElementTrialDataAck = 20092,
	[OriginalName("ElementTrial_FightStart_Req")]
	ElementTrialFightStartReq = 20093,
	[OriginalName("ElementTrial_FightStart_Ack")]
	ElementTrialFightStartAck = 20094,
	[OriginalName("ElementTrial_FightResult_Req")]
	ElementTrialFightResultReq = 20095,
	[OriginalName("ElementTrial_FightResult_Ack")]
	ElementTrialFightResultAck = 20096,
	[OriginalName("ElementTrial_BuyCombat_Req")]
	ElementTrialBuyCombatReq = 20097,
	[OriginalName("ElementTrial_BuyCombat_Ack")]
	ElementTrialBuyCombatAck = 20098,
	[OriginalName("ElementTrial_SaoDang_Req")]
	ElementTrialSaoDangReq = 20099,
	[OriginalName("ElementTrial_SaoDang_Ack")]
	ElementTrialSaoDangAck = 20100,
	[OriginalName("ElementTrial_Refresh_Req")]
	ElementTrialRefreshReq = 20061,
	[OriginalName("ElementTrial_Refresh_Ack")]
	ElementTrialRefreshAck = 20062,
	[OriginalName("ElementTrial_Upgrade_Req")]
	ElementTrialUpgradeReq = 20063,
	[OriginalName("ElementTrial_Upgrade_Ack")]
	ElementTrialUpgradeAck = 20064,
	[OriginalName("Idle_Data_Req")]
	IdleDataReq = 20101,
	[OriginalName("Idle_Data_Ack")]
	IdleDataAck = 20102,
	[OriginalName("Idle_Gain_Req")]
	IdleGainReq = 20103,
	[OriginalName("Idle_Gain_Ack")]
	IdleGainAck = 20104,
	[OriginalName("Idle_FastGain_Req")]
	IdleFastGainReq = 20105,
	[OriginalName("Idle_FastGain_Ack")]
	IdleFastGainAck = 20106,
	[OriginalName("EndlessHorde_Data_Req")]
	EndlessHordeDataReq = 20111,
	[OriginalName("EndlessHorde_Data_Ack")]
	EndlessHordeDataAck = 20112,
	[OriginalName("EndlessHorde_FightStart_Req")]
	EndlessHordeFightStartReq = 20113,
	[OriginalName("EndlessHorde_FightStart_Ack")]
	EndlessHordeFightStartAck = 20114,
	[OriginalName("EndlessHorde_FightForward_Req")]
	EndlessHordeFightForwardReq = 20115,
	[OriginalName("EndlessHorde_FightForward_Ack")]
	EndlessHordeFightForwardAck = 20116,
	[OriginalName("EndlessHorde_FightResult_Req")]
	EndlessHordeFightResultReq = 20117,
	[OriginalName("EndlessHorde_FightResult_Ack")]
	EndlessHordeFightResultAck = 20118,
	[OriginalName("EndlessHorde_BuyCombat_Req")]
	EndlessHordeBuyCombatReq = 20119,
	[OriginalName("EndlessHorde_BuyCombat_Ack")]
	EndlessHordeBuyCombatAck = 20120,
	[OriginalName("EndlessHorde_SaoDang_Req")]
	EndlessHordeSaoDangReq = 20121,
	[OriginalName("EndlessHorde_SaoDang_Ack")]
	EndlessHordeSaoDangAck = 20122,
	[OriginalName("Legendary_Data_Req")]
	LegendaryDataReq = 20131,
	[OriginalName("Legendary_Data_Ack")]
	LegendaryDataAck = 20132,
	[OriginalName("Legendary_FightStart_Req")]
	LegendaryFightStartReq = 20133,
	[OriginalName("Legendary_FightStart_Ack")]
	LegendaryFightStartAck = 20134,
	[OriginalName("Legendary_FightResult_Req")]
	LegendaryFightResultReq = 20135,
	[OriginalName("Legendary_FightResult_Ack")]
	LegendaryFightResultAck = 20136,
	[OriginalName("WarRoom_HeroSet_Req")]
	WarRoomHeroSetReq = 21203,
	[OriginalName("WarRoom_HeroSet_Ack")]
	WarRoomHeroSetAck = 21204,
	[OriginalName("WarRoom_UpgradeFast_Req")]
	WarRoomUpgradeFastReq = 21205,
	[OriginalName("WarRoom_UpgradeFast_Ack")]
	WarRoomUpgradeFastAck = 21206,
	[OriginalName("World_Data_Req")]
	WorldDataReq = 22201,
	[OriginalName("World_Data_Ack")]
	WorldDataAck = 22202,
	[OriginalName("World_ReconExplore_Req")]
	WorldReconExploreReq = 22211,
	[OriginalName("World_ReconExplore_Ack")]
	WorldReconExploreAck = 22212,
	[OriginalName("World_ReconExplored_Req")]
	WorldReconExploredReq = 22213,
	[OriginalName("World_ReconExplored_Ack")]
	WorldReconExploredAck = 22214,
	[OriginalName("World_ReconExploreCancel_Req")]
	WorldReconExploreCancelReq = 22217,
	[OriginalName("World_ReconExploreCancel_Ack")]
	WorldReconExploreCancelAck = 22218,
	[OriginalName("WorldResource_Activate_Req")]
	WorldResourceActivateReq = 22231,
	[OriginalName("WorldResource_Activate_Ack")]
	WorldResourceActivateAck = 22232,
	[OriginalName("WorldResource_IdleAward_Req")]
	WorldResourceIdleAwardReq = 22233,
	[OriginalName("WorldResource_IdleAward_Ack")]
	WorldResourceIdleAwardAck = 22234,
	[OriginalName("WorldDungeon_Open_Req")]
	WorldDungeonOpenReq = 22271,
	[OriginalName("WorldDungeon_Open_Ack")]
	WorldDungeonOpenAck = 22272,
	[OriginalName("WorldDungeon_Continue_Req")]
	WorldDungeonContinueReq = 22273,
	[OriginalName("WorldDungeon_Continue_Ack")]
	WorldDungeonContinueAck = 22274,
	[OriginalName("WorldDungeon_Close_Req")]
	WorldDungeonCloseReq = 22275,
	[OriginalName("WorldDungeon_Close_Ack")]
	WorldDungeonCloseAck = 22276,
	[OriginalName("WorldDungeon_LogInfo_Req")]
	WorldDungeonLogInfoReq = 22277,
	[OriginalName("WorldDungeon_LogInfo_Ack")]
	WorldDungeonLogInfoAck = 22278,
	[OriginalName("WorldDungeon_OpenCell_Req")]
	WorldDungeonOpenCellReq = 22281,
	[OriginalName("WorldDungeon_OpenCell_Ack")]
	WorldDungeonOpenCellAck = 22282,
	[OriginalName("WorldDungeon_Move_Req")]
	WorldDungeonMoveReq = 22283,
	[OriginalName("WorldDungeon_Move_Ack")]
	WorldDungeonMoveAck = 22284,
	[OriginalName("WorldDungeon_EventTrigger_Req")]
	WorldDungeonEventTriggerReq = 22291,
	[OriginalName("WorldDungeon_EventFightStart_Ack")]
	WorldDungeonEventFightStartAck = 22293,
	[OriginalName("WorldDungeon_EventModRand_Ack")]
	WorldDungeonEventModRandAck = 22295,
	[OriginalName("WorldDungeon_EventPopup_Ack")]
	WorldDungeonEventPopupAck = 22297,
	[OriginalName("WorldDungeon_EventFightResult_Req")]
	WorldDungeonEventFightResultReq = 22311,
	[OriginalName("WorldDungeon_EventFightResult_Ack")]
	WorldDungeonEventFightResultAck = 22312,
	[OriginalName("WorldDungeon_EventModSelect_Req")]
	WorldDungeonEventModSelectReq = 22313,
	[OriginalName("WorldDungeon_EventModSelect_Ack")]
	WorldDungeonEventModSelectAck = 22314,
	[OriginalName("WorldDungeon_EventPopupConfirm_Req")]
	WorldDungeonEventPopupConfirmReq = 22315,
	[OriginalName("WorldDungeon_EventPopupConfirm_Ack")]
	WorldDungeonEventPopupConfirmAck = 22316,
	[OriginalName("WorldEvent_Radar_Req")]
	WorldEventRadarReq = 22401,
	[OriginalName("WorldEvent_Radar_Ack")]
	WorldEventRadarAck = 22402,
	[OriginalName("WorldEvent_AwardBox_Req")]
	WorldEventAwardBoxReq = 22403,
	[OriginalName("WorldEvent_AwardBox_Ack")]
	WorldEventAwardBoxAck = 22404,
	[OriginalName("WorldEvent_FightStart_Req")]
	WorldEventFightStartReq = 22405,
	[OriginalName("WorldEvent_FightStart_Ack")]
	WorldEventFightStartAck = 22406,
	[OriginalName("WorldEvent_FightResult_Req")]
	WorldEventFightResultReq = 22407,
	[OriginalName("WorldEvent_FightResult_Ack")]
	WorldEventFightResultAck = 22408,
	[OriginalName("WorldLair_Data_Req")]
	WorldLairDataReq = 22501,
	[OriginalName("WorldLair_Data_Ack")]
	WorldLairDataAck = 22502,
	[OriginalName("WorldLair_FightStart_Req")]
	WorldLairFightStartReq = 22503,
	[OriginalName("WorldLair_FightStart_Ack")]
	WorldLairFightStartAck = 22504,
	[OriginalName("WorldLair_FightResult_Req")]
	WorldLairFightResultReq = 22505,
	[OriginalName("WorldLair_FightResult_Ack")]
	WorldLairFightResultAck = 22506,
	[OriginalName("WorldLair_SaoDang_Req")]
	WorldLairSaoDangReq = 22507,
	[OriginalName("WorldLair_SaoDang_Ack")]
	WorldLairSaoDangAck = 22508,
	[OriginalName("WorldLair_HelpData_Req")]
	WorldLairHelpDataReq = 22511,
	[OriginalName("WorldLair_HelpData_Ack")]
	WorldLairHelpDataAck = 22512,
	[OriginalName("WorldLair_HelpFightStart_Req")]
	WorldLairHelpFightStartReq = 22513,
	[OriginalName("WorldLair_HelpFightStart_Ack")]
	WorldLairHelpFightStartAck = 22514,
	[OriginalName("WorldLair_HelpFightResult_Req")]
	WorldLairHelpFightResultReq = 22515,
	[OriginalName("WorldLair_HelpFightResult_Ack")]
	WorldLairHelpFightResultAck = 22516,
	[OriginalName("WorldLair_Refresh_Req")]
	WorldLairRefreshReq = 22517,
	[OriginalName("WorldLair_Refresh_Ack")]
	WorldLairRefreshAck = 22518,
	[OriginalName("WorldLair_FightRecord_Req")]
	WorldLairFightRecordReq = 22521,
	[OriginalName("WorldLair_FightRecord_Ack")]
	WorldLairFightRecordAck = 22522,
	[OriginalName("WorldLair_BuyFightNum_Req")]
	WorldLairBuyFightNumReq = 22523,
	[OriginalName("WorldLair_BuyFightNum_Ack")]
	WorldLairBuyFightNumAck = 22524,
	[OriginalName("WorldBoss_Data_Req")]
	WorldBossDataReq = 22801,
	[OriginalName("WorldBoss_Data_Ack")]
	WorldBossDataAck = 22802,
	[OriginalName("WorldBoss_FightStart_Req")]
	WorldBossFightStartReq = 22803,
	[OriginalName("WorldBoss_FightStart_Ack")]
	WorldBossFightStartAck = 22804,
	[OriginalName("WorldBoss_FightReborn_Req")]
	WorldBossFightRebornReq = 22805,
	[OriginalName("WorldBoss_FightReborn_Ack")]
	WorldBossFightRebornAck = 22806,
	[OriginalName("WorldBoss_FightResult_Req")]
	WorldBossFightResultReq = 22807,
	[OriginalName("WorldBoss_FightResult_Ack")]
	WorldBossFightResultAck = 22808,
	[OriginalName("WorldBoss_SaoDang_Req")]
	WorldBossSaoDangReq = 22809,
	[OriginalName("WorldBoss_SaoDang_Ack")]
	WorldBossSaoDangAck = 22810,
	[OriginalName("WorldBoss_BuyCombat_Req")]
	WorldBossBuyCombatReq = 22811,
	[OriginalName("WorldBoss_BuyCombat_Ack")]
	WorldBossBuyCombatAck = 22812,
	[OriginalName("WorldBoss_RankList_Req")]
	WorldBossRankListReq = 22813,
	[OriginalName("WorldBoss_RankList_Ack")]
	WorldBossRankListAck = 22814,
	[OriginalName("WorldBoss_RecordList_Req")]
	WorldBossRecordListReq = 22815,
	[OriginalName("WorldBoss_RecordList_Ack")]
	WorldBossRecordListAck = 22816,
	[OriginalName("Feature_List_Req")]
	FeatureListReq = 22831,
	[OriginalName("Feature_List_Ack")]
	FeatureListAck = 22832,
	[OriginalName("HomelandV2_Data_Req")]
	HomelandV2DataReq = 23001,
	[OriginalName("HomelandV2_Data_Ack")]
	HomelandV2DataAck = 23002,
	[OriginalName("HomelandV2_DataSync_Req")]
	HomelandV2DataSyncReq = 23003,
	[OriginalName("HomelandV2_DataSync_Ack")]
	HomelandV2DataSyncAck = 23004,
	[OriginalName("HomelandV2_FloorUnlock_Req")]
	HomelandV2FloorUnlockReq = 23005,
	[OriginalName("HomelandV2_FloorUnlock_Ack")]
	HomelandV2FloorUnlockAck = 23006,
	[OriginalName("HomelandV2_RoomCreate_Req")]
	HomelandV2RoomCreateReq = 23011,
	[OriginalName("HomelandV2_RoomCreate_Ack")]
	HomelandV2RoomCreateAck = 23012,
	[OriginalName("HomelandV2_RoomRepair_Req")]
	HomelandV2RoomRepairReq = 23013,
	[OriginalName("HomelandV2_RoomRepair_Ack")]
	HomelandV2RoomRepairAck = 23014,
	[OriginalName("HomelandV2_RoomUpgrade_Req")]
	HomelandV2RoomUpgradeReq = 23015,
	[OriginalName("HomelandV2_RoomUpgrade_Ack")]
	HomelandV2RoomUpgradeAck = 23016,
	[OriginalName("HomelandV2_RoomUpgradeFast_Req")]
	HomelandV2RoomUpgradeFastReq = 23017,
	[OriginalName("HomelandV2_RoomUpgradeFast_Ack")]
	HomelandV2RoomUpgradeFastAck = 23018,
	[OriginalName("HomelandV2_RoomDestroy_Req")]
	HomelandV2RoomDestroyReq = 23021,
	[OriginalName("HomelandV2_RoomDestroy_Ack")]
	HomelandV2RoomDestroyAck = 23022,
	[OriginalName("HomelandV2_RoomMove_Req")]
	HomelandV2RoomMoveReq = 23023,
	[OriginalName("HomelandV2_RoomMove_Ack")]
	HomelandV2RoomMoveAck = 23024,
	[OriginalName("HomelandV2_RoomCompose_Req")]
	HomelandV2RoomComposeReq = 23025,
	[OriginalName("HomelandV2_RoomCompose_Ack")]
	HomelandV2RoomComposeAck = 23026,
	[OriginalName("HomelandV2_RoomEdit_Req")]
	HomelandV2RoomEditReq = 23027,
	[OriginalName("HomelandV2_RoomEdit_Ack")]
	HomelandV2RoomEditAck = 23028,
	[OriginalName("HomelandV2_ReplaceWorker_Req")]
	HomelandV2ReplaceWorkerReq = 23031,
	[OriginalName("HomelandV2_ReplaceWorker_Ack")]
	HomelandV2ReplaceWorkerAck = 23032,
	[OriginalName("HomelandV2_ReplaceWorkerSelect_Req")]
	HomelandV2ReplaceWorkerSelectReq = 23033,
	[OriginalName("HomelandV2_ReplaceWorkerSelect_Ack")]
	HomelandV2ReplaceWorkerSelectAck = 23034,
	[OriginalName("CrossLadder_Data_Req")]
	CrossLadderDataReq = 23501,
	[OriginalName("CrossLadder_Data_Ack")]
	CrossLadderDataAck = 23502,
	[OriginalName("CrossLadder_SeasonInit_Req")]
	CrossLadderSeasonInitReq = 23503,
	[OriginalName("CrossLadder_SeasonInit_Ack")]
	CrossLadderSeasonInitAck = 23504,
	[OriginalName("CrossLadder_CombatList_Req")]
	CrossLadderCombatListReq = 23511,
	[OriginalName("CrossLadder_CombatList_Ack")]
	CrossLadderCombatListAck = 23512,
	[OriginalName("CrossLadder_FightStart_Req")]
	CrossLadderFightStartReq = 23513,
	[OriginalName("CrossLadder_FightStart_Ack")]
	CrossLadderFightStartAck = 23514,
	[OriginalName("CrossLadder_FightResult_Req")]
	CrossLadderFightResultReq = 23515,
	[OriginalName("CrossLadder_FightResult_Ack")]
	CrossLadderFightResultAck = 23516,
	[OriginalName("CrossLadder_BuyCombat_Req")]
	CrossLadderBuyCombatReq = 23517,
	[OriginalName("CrossLadder_BuyCombat_Ack")]
	CrossLadderBuyCombatAck = 23518,
	[OriginalName("CrossLadder_UpgradeFightStart_Req")]
	CrossLadderUpgradeFightStartReq = 23521,
	[OriginalName("CrossLadder_UpgradeFightStart_Ack")]
	CrossLadderUpgradeFightStartAck = 23522,
	[OriginalName("CrossLadder_UpgradeFightResult_Req")]
	CrossLadderUpgradeFightResultReq = 23523,
	[OriginalName("CrossLadder_UpgradeFightResult_Ack")]
	CrossLadderUpgradeFightResultAck = 23524,
	[OriginalName("CrossLadder_UpgradeConfirm_Req")]
	CrossLadderUpgradeConfirmReq = 23525,
	[OriginalName("CrossLadder_UpgradeConfirm_Ack")]
	CrossLadderUpgradeConfirmAck = 23526,
	[OriginalName("CrossLadder_TargetAward_Req")]
	CrossLadderTargetAwardReq = 23531,
	[OriginalName("CrossLadder_TargetAward_Ack")]
	CrossLadderTargetAwardAck = 23532,
	[OriginalName("CrossLadder_RecordList_Req")]
	CrossLadderRecordListReq = 23533,
	[OriginalName("CrossLadder_RecordList_Ack")]
	CrossLadderRecordListAck = 23534,
	[OriginalName("CrossLadder_FriendList_Req")]
	CrossLadderFriendListReq = 23535,
	[OriginalName("CrossLadder_FriendList_Ack")]
	CrossLadderFriendListAck = 23536,
	[OriginalName("MineWar_Data_Req")]
	MineWarDataReq = 23551,
	[OriginalName("MineWar_Data_Ack")]
	MineWarDataAck = 23552,
	[OriginalName("MineWar_EmptyPoint_Req")]
	MineWarEmptyPointReq = 23553,
	[OriginalName("MineWar_EmptyPoint_Ack")]
	MineWarEmptyPointAck = 23554,
	[OriginalName("MineWar_PointData_Req")]
	MineWarPointDataReq = 23555,
	[OriginalName("MineWar_PointData_Ack")]
	MineWarPointDataAck = 23556,
	[OriginalName("MineWar_FightStart_Req")]
	MineWarFightStartReq = 23561,
	[OriginalName("MineWar_FightStart_Ack")]
	MineWarFightStartAck = 23562,
	[OriginalName("MineWar_FightResult_Req")]
	MineWarFightResultReq = 23563,
	[OriginalName("MineWar_FightResult_Ack")]
	MineWarFightResultAck = 23564,
	[OriginalName("MineWar_OccupyPoint_Req")]
	MineWarOccupyPointReq = 23565,
	[OriginalName("MineWar_OccupyPoint_Ack")]
	MineWarOccupyPointAck = 23566,
	[OriginalName("MineWar_BuyCombat_Req")]
	MineWarBuyCombatReq = 23567,
	[OriginalName("MineWar_BuyCombat_Ack")]
	MineWarBuyCombatAck = 23568,
	[OriginalName("MineWar_IdleAward_Req")]
	MineWarIdleAwardReq = 23571,
	[OriginalName("MineWar_IdleAward_Ack")]
	MineWarIdleAwardAck = 23572,
	[OriginalName("MineWar_RecordList_Req")]
	MineWarRecordListReq = 23573,
	[OriginalName("MineWar_RecordList_Ack")]
	MineWarRecordListAck = 23574,
	[OriginalName("MineWar_RecordSettle_Req")]
	MineWarRecordSettleReq = 23575,
	[OriginalName("MineWar_RecordSettle_Ack")]
	MineWarRecordSettleAck = 23576,
	[OriginalName("MineWar_SaoDang_Req")]
	MineWarSaoDangReq = 23577,
	[OriginalName("MineWar_SaoDang_Ack")]
	MineWarSaoDangAck = 23578,
	[OriginalName("CrossMine_ServerList_Req")]
	CrossMineServerListReq = 23601,
	[OriginalName("CrossMine_ServerList_Ack")]
	CrossMineServerListAck = 23602,
	[OriginalName("CrossMine_Data_Req")]
	CrossMineDataReq = 23603,
	[OriginalName("CrossMine_Data_Ack")]
	CrossMineDataAck = 23604,
	[OriginalName("CrossMine_PointData_Req")]
	CrossMinePointDataReq = 23605,
	[OriginalName("CrossMine_PointData_Ack")]
	CrossMinePointDataAck = 23606,
	[OriginalName("CrossMine_DestroyEmptyPoint_Req")]
	CrossMineDestroyEmptyPointReq = 23607,
	[OriginalName("CrossMine_DestroyEmptyPoint_Ack")]
	CrossMineDestroyEmptyPointAck = 23608,
	[OriginalName("CrossMine_FightStart_Req")]
	CrossMineFightStartReq = 23611,
	[OriginalName("CrossMine_FightStart_Ack")]
	CrossMineFightStartAck = 23612,
	[OriginalName("CrossMine_FightResult_Req")]
	CrossMineFightResultReq = 23613,
	[OriginalName("CrossMine_FightResult_Ack")]
	CrossMineFightResultAck = 23614,
	[OriginalName("CrossMine_RecordList_Req")]
	CrossMineRecordListReq = 23615,
	[OriginalName("CrossMine_RecordList_Ack")]
	CrossMineRecordListAck = 23616,
	[OriginalName("CrossPeak_Data_Req")]
	CrossPeakDataReq = 23651,
	[OriginalName("CrossPeak_Data_Ack")]
	CrossPeakDataAck = 23652,
	[OriginalName("CrossPeak_FightStart_Req")]
	CrossPeakFightStartReq = 23653,
	[OriginalName("CrossPeak_FightStart_Ack")]
	CrossPeakFightStartAck = 23654,
	[OriginalName("CrossPeak_FightResult_Req")]
	CrossPeakFightResultReq = 23655,
	[OriginalName("CrossPeak_FightResult_Ack")]
	CrossPeakFightResultAck = 23656,
	[OriginalName("CrossPeak_SaoDang_Req")]
	CrossPeakSaoDangReq = 23657,
	[OriginalName("CrossPeak_SaoDang_Ack")]
	CrossPeakSaoDangAck = 23658,
	[OriginalName("CrossPeak_BuyCombat_Req")]
	CrossPeakBuyCombatReq = 23661,
	[OriginalName("CrossPeak_BuyCombat_Ack")]
	CrossPeakBuyCombatAck = 23662,
	[OriginalName("CrossPeak_RecordList_Req")]
	CrossPeakRecordListReq = 23663,
	[OriginalName("CrossPeak_RecordList_Ack")]
	CrossPeakRecordListAck = 23664,
	[OriginalName("CrossPeak_Top_Req")]
	CrossPeakTopReq = 23665,
	[OriginalName("CrossPeak_Top_Ack")]
	CrossPeakTopAck = 23666,
	[OriginalName("Truck_Data_Req")]
	TruckDataReq = 23701,
	[OriginalName("Truck_Data_Ack")]
	TruckDataAck = 23702,
	[OriginalName("Truck_SlotUnlock_Req")]
	TruckSlotUnlockReq = 23703,
	[OriginalName("Truck_SlotUnlock_Ack")]
	TruckSlotUnlockAck = 23704,
	[OriginalName("Truck_TransportOpen_Req")]
	TruckTransportOpenReq = 23705,
	[OriginalName("Truck_TransportOpen_Ack")]
	TruckTransportOpenAck = 23706,
	[OriginalName("Truck_TransportRefresh_Req")]
	TruckTransportRefreshReq = 23707,
	[OriginalName("Truck_TransportRefresh_Ack")]
	TruckTransportRefreshAck = 23708,
	[OriginalName("Truck_TransportStart_Req")]
	TruckTransportStartReq = 23711,
	[OriginalName("Truck_TransportStart_Ack")]
	TruckTransportStartAck = 23712,
	[OriginalName("Truck_TransportView_Req")]
	TruckTransportViewReq = 23713,
	[OriginalName("Truck_TransportView_Ack")]
	TruckTransportViewAck = 23714,
	[OriginalName("Truck_TransportComplete_Req")]
	TruckTransportCompleteReq = 23715,
	[OriginalName("Truck_TransportComplete_Ack")]
	TruckTransportCompleteAck = 23716,
	[OriginalName("Truck_BuyTransport_Req")]
	TruckBuyTransportReq = 23717,
	[OriginalName("Truck_BuyTransport_Ack")]
	TruckBuyTransportAck = 23718,
	[OriginalName("Truck_TransportRecordList_Req")]
	TruckTransportRecordListReq = 23721,
	[OriginalName("Truck_TransportRecordList_Ack")]
	TruckTransportRecordListAck = 23722,
	[OriginalName("Truck_MatchList_Req")]
	TruckMatchListReq = 23731,
	[OriginalName("Truck_MatchList_Ack")]
	TruckMatchListAck = 23732,
	[OriginalName("Truck_RobFightStart_Req")]
	TruckRobFightStartReq = 23733,
	[OriginalName("Truck_RobFightStart_Ack")]
	TruckRobFightStartAck = 23734,
	[OriginalName("Truck_RobFightResult_Req")]
	TruckRobFightResultReq = 23735,
	[OriginalName("Truck_RobFightResult_Ack")]
	TruckRobFightResultAck = 23736,
	[OriginalName("Truck_BuyRob_Req")]
	TruckBuyRobReq = 23737,
	[OriginalName("Truck_BuyRob_Ack")]
	TruckBuyRobAck = 23738,
	[OriginalName("Truck_RobRecordList_Req")]
	TruckRobRecordListReq = 23741,
	[OriginalName("Truck_RobRecordList_Ack")]
	TruckRobRecordListAck = 23742,
	[OriginalName("Truck_AddWanted_Req")]
	TruckAddWantedReq = 23743,
	[OriginalName("Truck_AddWanted_Ack")]
	TruckAddWantedAck = 23744,
	[OriginalName("Train_Data_Req")]
	TrainDataReq = 23751,
	[OriginalName("Train_Data_Ack")]
	TrainDataAck = 23752,
	[OriginalName("Train_TransportOpen_Req")]
	TrainTransportOpenReq = 23753,
	[OriginalName("Train_TransportOpen_Ack")]
	TrainTransportOpenAck = 23754,
	[OriginalName("Train_TransportTake_Req")]
	TrainTransportTakeReq = 23755,
	[OriginalName("Train_TransportTake_Ack")]
	TrainTransportTakeAck = 23756,
	[OriginalName("Train_TransportTakeRefresh_Req")]
	TrainTransportTakeRefreshReq = 23757,
	[OriginalName("Train_TransportTakeRefresh_Ack")]
	TrainTransportTakeRefreshAck = 23758,
	[OriginalName("Train_TransportTakeConfirm_Req")]
	TrainTransportTakeConfirmReq = 23761,
	[OriginalName("Train_TransportTakeConfirm_Ack")]
	TrainTransportTakeConfirmAck = 23762,
	[OriginalName("Train_TransportAward_Req")]
	TrainTransportAwardReq = 23763,
	[OriginalName("Train_TransportAward_Ack")]
	TrainTransportAwardAck = 23764,
	[OriginalName("Train_RobMatch_Req")]
	TrainRobMatchReq = 23771,
	[OriginalName("Train_RobMatch_Ack")]
	TrainRobMatchAck = 23772,
	[OriginalName("Train_RobFightStart_Req")]
	TrainRobFightStartReq = 23773,
	[OriginalName("Train_RobFightStart_Ack")]
	TrainRobFightStartAck = 23774,
	[OriginalName("Train_RobFightResult_Req")]
	TrainRobFightResultReq = 23775,
	[OriginalName("Train_RobFightResult_Ack")]
	TrainRobFightResultAck = 23776,
	[OriginalName("LegionWar_Data_Req")]
	LegionWarDataReq = 23801,
	[OriginalName("LegionWar_Data_Ack")]
	LegionWarDataAck = 23802,
	[OriginalName("LegionWar_SignUp_Req")]
	LegionWarSignUpReq = 23803,
	[OriginalName("LegionWar_SignUp_Ack")]
	LegionWarSignUpAck = 23804,
	[OriginalName("LegionWar_SignUpList_Req")]
	LegionWarSignUpListReq = 23805,
	[OriginalName("LegionWar_SignUpList_Ack")]
	LegionWarSignUpListAck = 23806,
	[OriginalName("LegionWar_EnterWar_Req")]
	LegionWarEnterWarReq = 23811,
	[OriginalName("LegionWar_EnterWar_Ack")]
	LegionWarEnterWarAck = 23812,
	[OriginalName("LegionWar_MapInfo_Req")]
	LegionWarMapInfoReq = 23813,
	[OriginalName("LegionWar_MapInfo_Ack")]
	LegionWarMapInfoAck = 23814,
	[OriginalName("LegionWar_MapInfo_Ntf")]
	LegionWarMapInfoNtf = 23816,
	[OriginalName("LegionWar_MapMove_Ntf")]
	LegionWarMapMoveNtf = 23817,
	[OriginalName("LegionWar_PointInfo_Req")]
	LegionWarPointInfoReq = 23821,
	[OriginalName("LegionWar_PointInfo_Ack")]
	LegionWarPointInfoAck = 23822,
	[OriginalName("LegionWar_EnterPoint_Req")]
	LegionWarEnterPointReq = 23823,
	[OriginalName("LegionWar_EnterPoint_Ack")]
	LegionWarEnterPointAck = 23824,
	[OriginalName("LegionWar_Rest_Req")]
	LegionWarRestReq = 23827,
	[OriginalName("LegionWar_Rest_Ack")]
	LegionWarRestAck = 23828,
	[OriginalName("LegionWar_Goto_Req")]
	LegionWarGotoReq = 23831,
	[OriginalName("LegionWar_Goto_Ack")]
	LegionWarGotoAck = 23832,
	[OriginalName("LegionWar_PointAward_Req")]
	LegionWarPointAwardReq = 23833,
	[OriginalName("LegionWar_PointAward_Ack")]
	LegionWarPointAwardAck = 23834,
	[OriginalName("LegionWar_MarkPoint_Req")]
	LegionWarMarkPointReq = 23835,
	[OriginalName("LegionWar_MarkPoint_Ack")]
	LegionWarMarkPointAck = 23836,
	[OriginalName("LegionWar_FightStart_Req")]
	LegionWarFightStartReq = 23841,
	[OriginalName("LegionWar_FightStart_Ack")]
	LegionWarFightStartAck = 23842,
	[OriginalName("LegionWar_FightResult_Req")]
	LegionWarFightResultReq = 23843,
	[OriginalName("LegionWar_FightResult_Ack")]
	LegionWarFightResultAck = 23844,
	[OriginalName("LegionWar_FightCancel_Req")]
	LegionWarFightCancelReq = 23845,
	[OriginalName("LegionWar_FightCancel_Ack")]
	LegionWarFightCancelAck = 23846,
	[OriginalName("LegionWar_LegionRankList_Req")]
	LegionWarLegionRankListReq = 23851,
	[OriginalName("LegionWar_LegionRankList_Ack")]
	LegionWarLegionRankListAck = 23852,
	[OriginalName("LegionWar_PlayerRankList_Req")]
	LegionWarPlayerRankListReq = 23853,
	[OriginalName("LegionWar_PlayerRankList_Ack")]
	LegionWarPlayerRankListAck = 23854,
	[OriginalName("LegionWar_ModSelect_Req")]
	LegionWarModSelectReq = 23855,
	[OriginalName("LegionWar_ModSelect_Ack")]
	LegionWarModSelectAck = 23856,
	[OriginalName("LegionWar_ZhanLingLevel_Req")]
	LegionWarZhanLingLevelReq = 23861,
	[OriginalName("LegionWar_ZhanLingLevel_Ack")]
	LegionWarZhanLingLevelAck = 23862,
	[OriginalName("LegionWar_ZhanLingAward_Req")]
	LegionWarZhanLingAwardReq = 23863,
	[OriginalName("LegionWar_ZhanLingAward_Ack")]
	LegionWarZhanLingAwardAck = 23864,
	[OriginalName("LegionWar_TaskAward_Req")]
	LegionWarTaskAwardReq = 23865,
	[OriginalName("LegionWar_TaskAward_Ack")]
	LegionWarTaskAwardAck = 23866,
	[OriginalName("LegionWar_RecordList_Req")]
	LegionWarRecordListReq = 23867,
	[OriginalName("LegionWar_RecordList_Ack")]
	LegionWarRecordListAck = 23868,
	[OriginalName("LegionWar_WatchMapInfo_Req")]
	LegionWarWatchMapInfoReq = 23871,
	[OriginalName("LegionWar_WatchMapInfo_Ack")]
	LegionWarWatchMapInfoAck = 23872,
	[OriginalName("LegionWar_SettleLegionInfo_Req")]
	LegionWarSettleLegionInfoReq = 23873,
	[OriginalName("LegionWar_SettleLegionInfo_Ack")]
	LegionWarSettleLegionInfoAck = 23874,
	[OriginalName("LegionWar_SettlePlayerInfo_Req")]
	LegionWarSettlePlayerInfoReq = 23875,
	[OriginalName("LegionWar_SettlePlayerInfo_Ack")]
	LegionWarSettlePlayerInfoAck = 23876,
	[OriginalName("Facility_Data_Req")]
	FacilityDataReq = 23901,
	[OriginalName("Facility_Data_Ack")]
	FacilityDataAck = 23902,
	[OriginalName("Facility_Recommend_Req")]
	FacilityRecommendReq = 23903,
	[OriginalName("Facility_Recommend_Ack")]
	FacilityRecommendAck = 23904,
	[OriginalName("Facility_Create_Req")]
	FacilityCreateReq = 23905,
	[OriginalName("Facility_Create_Ack")]
	FacilityCreateAck = 23906,
	[OriginalName("Facility_Close_Req")]
	FacilityCloseReq = 23907,
	[OriginalName("Facility_Close_Ack")]
	FacilityCloseAck = 23908,
	[OriginalName("Facility_View_Req")]
	FacilityViewReq = 23909,
	[OriginalName("Facility_View_Ack")]
	FacilityViewAck = 23910,
	[OriginalName("Facility_RoomPreview_Req")]
	FacilityRoomPreviewReq = 23911,
	[OriginalName("Facility_RoomPreview_Ack")]
	FacilityRoomPreviewAck = 23912,
	[OriginalName("Facility_EventTrigger_Req")]
	FacilityEventTriggerReq = 23921,
	[OriginalName("Facility_EventCommon_Ack")]
	FacilityEventCommonAck = 23922,
	[OriginalName("Facility_EventLockedChest_Ack")]
	FacilityEventLockedChestAck = 23923,
	[OriginalName("Facility_EventMonster_Ack")]
	FacilityEventMonsterAck = 23924,
	[OriginalName("Facility_EventShop_Ack")]
	FacilityEventShopAck = 23925,
	[OriginalName("Facility_EventRolling_Ack")]
	FacilityEventRollingAck = 23926,
	[OriginalName("Facility_EventStair_Ack")]
	FacilityEventStairAck = 23927,
	[OriginalName("Facility_StairTeleport_Req")]
	FacilityStairTeleportReq = 23931,
	[OriginalName("Facility_StairTeleport_Ack")]
	FacilityStairTeleportAck = 23932,
	[OriginalName("Facility_OpenChest_Req")]
	FacilityOpenChestReq = 23933,
	[OriginalName("Facility_OpenChest_Ack")]
	FacilityOpenChestAck = 23934,
	[OriginalName("Facility_FightResult_Req")]
	FacilityFightResultReq = 23935,
	[OriginalName("Facility_FightResult_Ack")]
	FacilityFightResultAck = 23936,
	[OriginalName("Facility_Rolling_Req")]
	FacilityRollingReq = 23937,
	[OriginalName("Facility_Rolling_Ack")]
	FacilityRollingAck = 23938,
	[OriginalName("Facility_RollingEnd_Req")]
	FacilityRollingEndReq = 23939,
	[OriginalName("Facility_RollingEnd_Ack")]
	FacilityRollingEndAck = 23940,
	[OriginalName("Facility_ShopBuy_Req")]
	FacilityShopBuyReq = 23941,
	[OriginalName("Facility_ShopBuy_Ack")]
	FacilityShopBuyAck = 23942,
	[OriginalName("Facility_Quit_Req")]
	FacilityQuitReq = 23943,
	[OriginalName("Facility_Quit_Ack")]
	FacilityQuitAck = 23944,
	[OriginalName("Season_Enter_Req")]
	SeasonEnterReq = 24002,
	[OriginalName("Season_Enter_Ack")]
	SeasonEnterAck = 24003,
	[OriginalName("Season_Data_Req")]
	SeasonDataReq = 24004,
	[OriginalName("Season_Data_Ack")]
	SeasonDataAck = 24005,
	[OriginalName("Season_Map_Info_Req")]
	SeasonMapInfoReq = 24006,
	[OriginalName("Season_Map_Info_Ack")]
	SeasonMapInfoAck = 24007,
	[OriginalName("Season_View_Req")]
	SeasonViewReq = 24008,
	[OriginalName("Season_View_Ack")]
	SeasonViewAck = 24009,
	[OriginalName("Season_March_Start_Req")]
	SeasonMarchStartReq = 24010,
	[OriginalName("Season_March_Start_Ack")]
	SeasonMarchStartAck = 24011,
	[OriginalName("Season_Move_Ntf")]
	SeasonMoveNtf = 24014,
	[OriginalName("Season_March_Arrive_Ntf")]
	SeasonMarchArriveNtf = 24015,
	[OriginalName("Season_Map_Info_Ntf")]
	SeasonMapInfoNtf = 24016,
	[OriginalName("Season_City_Detail_Req")]
	SeasonCityDetailReq = 24017,
	[OriginalName("Season_City_Detail_Ack")]
	SeasonCityDetailAck = 24018,
	[OriginalName("Season_Res_Occupy_Req")]
	SeasonResOccupyReq = 24019,
	[OriginalName("Season_Res_Occupy_Ack")]
	SeasonResOccupyAck = 24020,
	[OriginalName("Season_Res_Abandon_Req")]
	SeasonResAbandonReq = 24021,
	[OriginalName("Season_Res_Abandon_Ack")]
	SeasonResAbandonAck = 24022,
	[OriginalName("Season_My_Territory_Req")]
	SeasonMyTerritoryReq = 24023,
	[OriginalName("Season_My_Territory_Ack")]
	SeasonMyTerritoryAck = 24024,
	[OriginalName("Season_Adjacent_Req")]
	SeasonAdjacentReq = 24025,
	[OriginalName("Season_Adjacent_Ack")]
	SeasonAdjacentAck = 24026,
	[OriginalName("Season_Res_Produce_Ntf")]
	SeasonResProduceNtf = 24027,
	[OriginalName("Season_Aoi_Ntf")]
	SeasonAoiNtf = 24028,
	[OriginalName("Season_Declare_Req")]
	SeasonDeclareReq = 24031,
	[OriginalName("Season_Declare_Ack")]
	SeasonDeclareAck = 24032,
	[OriginalName("Season_Declare_List_Req")]
	SeasonDeclareListReq = 24033,
	[OriginalName("Season_Declare_List_Ack")]
	SeasonDeclareListAck = 24034,
	[OriginalName("Season_CityWar_State_Ntf")]
	SeasonCityWarStateNtf = 24035,
	[OriginalName("Season_Garrison_Req")]
	SeasonGarrisonReq = 24036,
	[OriginalName("Season_Garrison_Ack")]
	SeasonGarrisonAck = 24037,
	[OriginalName("Season_Recruit_Req")]
	SeasonRecruitReq = 24038,
	[OriginalName("Season_Recruit_Ack")]
	SeasonRecruitAck = 24039,
	[OriginalName("Season_Guard_Info_Req")]
	SeasonGuardInfoReq = 24040,
	[OriginalName("Season_Guard_Info_Ack")]
	SeasonGuardInfoAck = 24041,
	[OriginalName("Season_City_Attack_Req")]
	SeasonCityAttackReq = 24042,
	[OriginalName("Season_City_Attack_Ack")]
	SeasonCityAttackAck = 24043,
	[OriginalName("Season_CityWar_Result_Ntf")]
	SeasonCityWarResultNtf = 24044,
	[OriginalName("Season_City_Owner_Ntf")]
	SeasonCityOwnerNtf = 24045,
	[OriginalName("Season_Online_Reward_Req")]
	SeasonOnlineRewardReq = 24046,
	[OriginalName("Season_Online_Reward_Ack")]
	SeasonOnlineRewardAck = 24047,
	[OriginalName("Season_CityWar_Info_Req")]
	SeasonCityWarInfoReq = 24048,
	[OriginalName("Season_CityWar_Info_Ack")]
	SeasonCityWarInfoAck = 24049,
	[OriginalName("Season_Merit_Rank_Req")]
	SeasonMeritRankReq = 24050,
	[OriginalName("Season_Merit_Rank_Ack")]
	SeasonMeritRankAck = 24051,
	[OriginalName("Season_Battle_Result_Req")]
	SeasonBattleResultReq = 24052,
	[OriginalName("Season_Battle_Result_Ack")]
	SeasonBattleResultAck = 24053,
	[OriginalName("Season_Repair_Req")]
	SeasonRepairReq = 24054,
	[OriginalName("Season_Repair_Ack")]
	SeasonRepairAck = 24055,
	[OriginalName("Season_Team_Pos_Req")]
	SeasonTeamPosReq = 24056,
	[OriginalName("Season_Team_Pos_Ack")]
	SeasonTeamPosAck = 24057,
	[OriginalName("Season_March_SpeedUp_Req")]
	SeasonMarchSpeedUpReq = 24058,
	[OriginalName("Season_March_SpeedUp_Ack")]
	SeasonMarchSpeedUpAck = 24059,
	[OriginalName("Season_Airdrop_List_Req")]
	SeasonAirdropListReq = 24071,
	[OriginalName("Season_Airdrop_List_Ack")]
	SeasonAirdropListAck = 24072,
	[OriginalName("Season_Airdrop_Refresh_Ntf")]
	SeasonAirdropRefreshNtf = 24073,
	[OriginalName("Season_Airdrop_Decrypt_Req")]
	SeasonAirdropDecryptReq = 24076,
	[OriginalName("Season_Airdrop_Decrypt_Ack")]
	SeasonAirdropDecryptAck = 24077,
	[OriginalName("Season_Airdrop_Progress_Ntf")]
	SeasonAirdropProgressNtf = 24078,
	[OriginalName("Season_Airdrop_Settle_Ntf")]
	SeasonAirdropSettleNtf = 24081,
	[OriginalName("Season_Airdrop_Enter_Ntf")]
	SeasonAirdropEnterNtf = 24082,
	[OriginalName("Season_Force_Info_Req")]
	SeasonForceInfoReq = 24091,
	[OriginalName("Season_Force_Info_Ack")]
	SeasonForceInfoAck = 24092,
	[OriginalName("Season_Force_Update_Req")]
	SeasonForceUpdateReq = 24093,
	[OriginalName("Season_Force_Update_Ack")]
	SeasonForceUpdateAck = 24094,
	[OriginalName("Season_Force_Members_Req")]
	SeasonForceMembersReq = 24095,
	[OriginalName("Season_Force_Members_Ack")]
	SeasonForceMembersAck = 24096,
	[OriginalName("Season_Office_Appoint_Req")]
	SeasonOfficeAppointReq = 24097,
	[OriginalName("Season_Office_Appoint_Ack")]
	SeasonOfficeAppointAck = 24098,
	[OriginalName("Season_Leader_Elect_Info_Req")]
	SeasonLeaderElectInfoReq = 24099,
	[OriginalName("Season_Leader_Elect_Info_Ack")]
	SeasonLeaderElectInfoAck = 24100,
	[OriginalName("Season_Leader_Impeach_Req")]
	SeasonLeaderImpeachReq = 24101,
	[OriginalName("Season_Leader_Impeach_Ack")]
	SeasonLeaderImpeachAck = 24102,
	[OriginalName("Season_Leader_Change_Ntf")]
	SeasonLeaderChangeNtf = 24105,
	[OriginalName("Season_Tech_Info_Req")]
	SeasonTechInfoReq = 24106,
	[OriginalName("Season_Tech_Info_Ack")]
	SeasonTechInfoAck = 24107,
	[OriginalName("Season_Tech_Upgrade_Req")]
	SeasonTechUpgradeReq = 24108,
	[OriginalName("Season_Tech_Upgrade_Ack")]
	SeasonTechUpgradeAck = 24109,
	[OriginalName("Season_Shop_List_Req")]
	SeasonShopListReq = 24110,
	[OriginalName("Season_Shop_List_Ack")]
	SeasonShopListAck = 24111,
	[OriginalName("Season_Shop_Buy_Req")]
	SeasonShopBuyReq = 24112,
	[OriginalName("Season_Shop_Buy_Ack")]
	SeasonShopBuyAck = 24113,
	[OriginalName("Season_Res_Attack_Req")]
	SeasonResAttackReq = 24116,
	[OriginalName("Season_Res_Attack_Ack")]
	SeasonResAttackAck = 24117,
	[OriginalName("Season_Force_Exp_Ntf")]
	SeasonForceExpNtf = 24118,
	[OriginalName("Season_Res_Settle_Req")]
	SeasonResSettleReq = 24119,
	[OriginalName("Season_Res_Settle_Ack")]
	SeasonResSettleAck = 24120,
	[OriginalName("Season_Stage_Task_Info_Req")]
	SeasonStageTaskInfoReq = 24121,
	[OriginalName("Season_Stage_Task_Info_Ack")]
	SeasonStageTaskInfoAck = 24122,
	[OriginalName("Season_Stage_Task_Award_Req")]
	SeasonStageTaskAwardReq = 24123,
	[OriginalName("Season_Stage_Task_Award_Ack")]
	SeasonStageTaskAwardAck = 24124,
	[OriginalName("Season_Stage_Task_Ntf")]
	SeasonStageTaskNtf = 24125
}
