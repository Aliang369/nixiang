-- @Date    : 2021-03-26 14:38:14
-- @Author  : ZhaoCi
-- @Desc    :

-- local socket = require("socket")
local ipairs = ipairs
local pairs = pairs

local Game = {}

local Hero = import(".Model.Hero")
local Monster = import(".Model.Monster")
local BattleRobot = import(".Model.BattleRobot")
local BattlePet = import(".Model.BattlePet")
local BattleNpc = import(".Model.BattleNpc")
local Vision = import(".Model.Vision")
local SkillObj = import(".Model.SkillObj")
local TrapObject = import(".Model.TrapObject")
local Others = import(".Model.Others")
local SimpleHero = import(".Model.SimpleHero")

local STAGE_WORLD = gConst.Game.STAGE_WORLD
local STAGE_BATTLE = gConst.Game.STAGE_BATTLE
local STAGE_CITY = gConst.Game.STAGE_CITY
local STAGE_SECT = gConst.Game.STAGE_SECT
local STAGE_GUILDLAND = gConst.Game.STAGE_GUILDLAND
local STAGE_UNKNOW = gConst.Game.STAGE_UNKNOW

local CurrencyProIdx = gConst.Attr.CurrencyProIdx
local BindCurrencyProIdx = gConst.Attr.BindCurrencyProIdx
Game.stageMode = gConst.Game.STAGE_UNKNOW

local Setting = gModule.Setting

local HeroAttr = gConst.Attr.Hero
-- 配置初始化广播的人物属性事件
Game.mapAttrEvt = {
    -- 人物基本属性事件
    [HeroAttr.Sex] = GameEvent.HERO_SEX_CHG,
    [HeroAttr.Level] = GameEvent.HERO_LEVEL_CHG,
    [HeroAttr.DisplayName] = GameEvent.HERO_DISPLAY_NAME_CHG,

    -- 人物独有属性事件
    [HeroAttr.CurExp] = GameEvent.HERO_CUREXP_CHG,
    [HeroAttr.PayCoin] = GameEvent.HERO_PAYCOIN_CHG,
    [HeroAttr.FairyCrystal] = GameEvent.HERO_FAIRYCRYSTAL_CHG,
    [HeroAttr.BindPayCoin] = GameEvent.HERO_BINDPAYCOIN_CHG,
    [HeroAttr.Coin] = GameEvent.HERO_COIN_CHG,
    [HeroAttr.BindCoin] = GameEvent.HERO_BINDCOIN_CHG,
    [HeroAttr.MaxPackage] = GameEvent.UPDATE_BAG_GRID,
    [HeroAttr.MaxStorage] = GameEvent.UPDATE_DEPOT_GRID,
    [HeroAttr.Lucky] = GameEvent.HERO_LUCKY_CHG,
    [HeroAttr.Age] = GameEvent.HERO_AGE_CHG,
    [HeroAttr.TechAlchemy] = GameEvent.HERO_TECHALCHEMY_CHG,
    [HeroAttr.TechRefiner] = GameEvent.HERO_TECHREFINER_CHG,
    [HeroAttr.TechMedicine] = GameEvent.HERO_TECHMEDICINE_CHG,
    [HeroAttr.TechFengshui] = GameEvent.HERO_TECHFENGSHUI_CHG,
    [HeroAttr.Comprehension] = GameEvent.HERO_COMPREHENSION_CHG,
    [HeroAttr.Healthy] = GameEvent.HERO_HEALTHY_CHG,
    [HeroAttr.Charm] = GameEvent.HERO_CHARM_CHG,
    [HeroAttr.Energy] = GameEvent.HERO_ENERGY_CHG,
    [HeroAttr.Contribute] = GameEvent.HERO_CONTRIBUTE_CHG,
    [HeroAttr.LastDrugId] = GameEvent.HERO_LASTDRUGID_CHG,
    [HeroAttr.DrugCount] = GameEvent.HERO_DRUGCOUNT_CHG,
    [HeroAttr.FirstRechargeAward] = GameEvent.HERO_FIRSTRECHAR_CHG,
    [HeroAttr.ExpDrugLimit] = GameEvent.HERO_EXPDRUGLIMIT_CHG,
    [HeroAttr.ReliveCount] = GameEvent.HERO_RELIVE_COUNT_CHG,
    [HeroAttr.ItemUseTimesWithAddAttr] = GameEvent.HERO_ITEMUSETIMESWITHADDATTR_CHG,
    [HeroAttr.HuntCoin] = GameEvent.HERO_HUNT_COIN_CHG,
    [HeroAttr.RechargeItemNum] = GameEvent.HERO_RECHARGE_ITEM_NUM,
    [HeroAttr.ExpSpeed] = GameEvent.HERO_EXPSPEED_CHG,
    [HeroAttr.CampLv] = GameEvent.CAMP_LV_CHG,
    [HeroAttr.GainAssembly] = GameEvent.HERO_GAINASSEMBLY_CHG,
    [HeroAttr.MagicFire] = GameEvent.HERO_MAGICFIRE_CHG,
    [HeroAttr.InnatCount] = GameEvent.INNAT_COUNT_CHG,
    [HeroAttr.Innat] = GameEvent.INNAT_CHG,
    [HeroAttr.Transfer] = GameEvent.TRANSFER_CHG,

    [HeroAttr.CampCount14] = GameEvent.CAMP_COUNT14_CHG,
    [HeroAttr.CampCount15] = GameEvent.CAMP_COUNT15_CHG,
    [HeroAttr.CampCount16] = GameEvent.CAMP_COUNT16_CHG,
    [HeroAttr.CampCount17] = GameEvent.CAMP_COUNT17_CHG,
    [HeroAttr.CampCount18] = GameEvent.CAMP_COUNT18_CHG,
    [HeroAttr.CampCount19] = GameEvent.CAMP_COUNT19_CHG,
    [HeroAttr.CampCount20] = GameEvent.CAMP_COUNT20_CHG,
    [HeroAttr.CampCount21] = GameEvent.CAMP_COUNT21_CHG,

    [HeroAttr.RepelCount] = GameEvent.REPEL_COUNT_CHG,

    [HeroAttr.MagicFireLotteryStartTime] = GameEvent.MAGIC_FIRE_LOTTERY_START_TIME_CHG,

    [HeroAttr.GodAreaMoneyCoin] = GameEvent.GOD_AREA_MONEY_COIN_CHG,

    [HeroAttr.BigAdventureCoin] = GameEvent.BIG_ADVENTURE_COIN_CHG,
    
    [HeroAttr.FashionLotteryCoin] = GameEvent.FASHION_LOTTERY_COIN_CHG,
    [HeroAttr.ExtraExp] = GameEvent.EXTRA_EXP_CHG,

    [HeroAttr.TowerNum] = GameEvent.TOWER_NUM_CHG,
    [HeroAttr.TowerReward] = GameEvent.TOWER_REWARD_CHG,
    [HeroAttr.TowerSweep] = GameEvent.TOWER_SWEEP_CHG,
    [HeroAttr.TowerSweepBuyTimes] = GameEvent.TOWER_SWEEP_BUY_TIMES_CHG,
    [HeroAttr.HExpLimit] = GameEvent.HAND_EXP_LIMIT_CHG,
    [HeroAttr.BodyRefine] = GameEvent.HERO_BODY_REFINE_CHG,
    [HeroAttr.EnergyGet] = GameEvent.ENERGY_GET_CHG,
    [HeroAttr.EnergyAddTime] = GameEvent.ENERGY_ADD_TIME_CHG,
    [HeroAttr.ExpAddBegin] = GameEvent.EXP_ADD_BEGIN,
    [HeroAttr.PetSearchBegin] = GameEvent.PET_SEARCH_BEGIN,
    [HeroAttr.SpiritPetFind] = GameEvent.SPIRIT_PET_FIND,
    [HeroAttr.BattlePlanId] = GameEvent.BATTLE_PLAN_ID_CHG,

    [HeroAttr.CurNature] = GameEvent.CUR_NATURE_CHG,
    [HeroAttr.BaseNature] = GameEvent.BASE_NATURE_CHG,

    [HeroAttr.BattleStatus] = GameEvent.HERO_BATTLE_STATUS,
    [HeroAttr.BattlingTime] = GameEvent.HERO_BATTLING_TIME,
    [HeroAttr.BattleCount] = GameEvent.HERO_BATTLE_COUNT,
    [HeroAttr.BattleBuyCount] = GameEvent.HERO_BATTLE_BUY_COUNT,
    [HeroAttr.BattleCrossRank] = GameEvent.HERO_BATTLE_CROSS_RANK,
    [HeroAttr.BattleDailyJoinCount] = GameEvent.HERO_BATTLE_DAILY_JOIN_COUNT,

    [HeroAttr.ActivityNum] = GameEvent.HERO_ACTIVITYNUM_CHG,
    [HeroAttr.GodAreaCoin] = GameEvent.HERO_GODAREACOIN_CHG,
    [HeroAttr.RideStatus] = GameEvent.HERO_RIDE_STATUS_CHG,
    [HeroAttr.RideDeathTime] = GameEvent.HERO_RIDE_DEATH_TIME_CHG,
    [HeroAttr.GodAreaScoreCoin] = GameEvent.HERO_GODAREASCORECOIN_CHG,
    [HeroAttr.GodAreaShopCoin] = GameEvent.HERO_GODAREASHOPCOIN_CHG,
    [HeroAttr.RideCurHp] = GameEvent.HERO_RIDE_CURHP_CHG,

    [HeroAttr.MetalEle] = GameEvent.HERO_METALELE_CHG,
    [HeroAttr.WoodEle] = GameEvent.HERO_WOODELE_CHG,
    [HeroAttr.WaterEle] = GameEvent.HERO_WATERELE_CHG,
    [HeroAttr.FireEle] = GameEvent.HERO_FIREELE_CHG,
    [HeroAttr.EarthEle] = GameEvent.HERO_EARTHELE_CHG,

    [HeroAttr.NewServerScore] = GameEvent.HERO_NEW_SERVER_SCORE_CHG,
    [HeroAttr.FairyCrystal] = GameEvent.HERO_FAIRY_CRYSTAL_CHG,

    [HeroAttr.CombatValue] = GameEvent.HERO_COMBAT_VALUE_CHG,
    [HeroAttr.ChapterLuckyRound] = GameEvent.CHAPTER_RAFFLE_ROUND_CHG,

    [HeroAttr.CaveLevel] = GameEvent.CAVE_LEVEL_CHG,
    [HeroAttr.RideState] = GameEvent.HERO_RIDE_STAE_CHG,
    [HeroAttr.BattleRideState] = GameEvent.HERO_BATTLE_RIDE_STAE_CHG,

    [HeroAttr.LocalBattleStatus] = GameEvent.HERO_LOCAL_BATTLE_STATUS,
    [HeroAttr.LocalBattleDailyJoinCount] = GameEvent.HERO_LOCAL_BATTLE_DAILY_JOIN_COUNT,
    [HeroAttr.LocalBattlingTime] = GameEvent.HERO_LOCAL_BATTLE_BATTLING_TIME,
    [HeroAttr.LocalBattleCount] = GameEvent.HERO_LOCAL_BATTLE_COUNT,
    [HeroAttr.LocalBattleBuyCount] = GameEvent.HERO_LOCAL_BATTLE_BUY_COUNT,
    [HeroAttr.LocalBattleScoreMoney] = GameEvent.LOCAL_BATTLE_MONEY_CHG,
    [HeroAttr.HandUpCoin] = GameEvent.HERO_HAND_UP_COIN_CHG,

    [HeroAttr.GmFuncUnlockOpen] = GameEvent.HERO_GM_FUNC_UNLOCK_STAE_CHG,
    [HeroAttr.PrivilegeCardId] = GameEvent.HERO_PRIVILEGE_CARD_CHG,

    [HeroAttr.CurHeroTitleId] = GameEvent.HERO_CUR_TITLE_CHG,
    [HeroAttr.GodAreaBattleOrderExp] = GameEvent.HERO_GOD_AREA_ORDER_EXP_CHG,
    [HeroAttr.GodAreaLotteryTimes] = GameEvent.GODAREA_LOTTERY_TIMES_CHG,
    [HeroAttr.GodAreaLotteryGuarantee] = GameEvent.GODAREA_LOTTERY_GUARANTEE_CHG,
    [HeroAttr.MagicFireSkillId] = GameEvent.MAGIC_FIRE_SKILL_CHG,
    [HeroAttr.TowerHangUpBuyCount] = GameEvent.TOWER_BUY_HANG_COUNT_CHG,
    [HeroAttr.TotalMailCount] = GameEvent.TOTAL_MAIL_NUM_CHG,

    [HeroAttr.AncientLotteryWishId] = GameEvent.ANCIENT_LOTTERY_WISH_CHG,
    [HeroAttr.DuplicateMatchId] = GameEvent.HERO_DUPLICATE_MATCH_ID_CHG,
    [HeroAttr.DuplicateMatchStartTime] = GameEvent.HERO_DUPLICATE_MATCH_TIME_CHG,
    [HeroAttr.FreeGiftStatus] = GameEvent.FREE_GIFT_STATUS_CHG,
    [HeroAttr.RechargeRebateValue] = GameEvent.RECHARGE_REBATE_VALUE_CFG,
    [HeroAttr.AncientLotteryNextFreeTime] = GameEvent.ANCIENT_LOTTERY_NEXT_FREE_TIME_CHG,
    [HeroAttr.UserBagStoredTimes] = GameEvent.PUBLIC_SPACE_COUNT_CHG,
    [HeroAttr.DestinyResetStatus] = GameEvent.HERO_DESTINY_RESET,
    [HeroAttr.RepelEvilChallengeTimes] = GameEvent.REPEL_EVIL_CHALLENGE_TIMES_CHG,
    [HeroAttr.RepelEvilDailyWin] = GameEvent.REPEL_EVIL_DAILY_WIN_CHG,
    [HeroAttr.CrossRepelEvilCount] = GameEvent.REPEL_EVIL_CROSS_COUNT_CHG,
    [HeroAttr.GuildBossChallengeTimes] = GameEvent.GUILD_BOSS_CHALLENGE_TIMES_CHG,
    [HeroAttr.SpringActivityCoin] = GameEvent.SPRING_ACTIVITY_COIN_CHG,
    [HeroAttr.TowerCoin] = GameEvent.TOWER_COIN_CHG,
    [HeroAttr.LadderCredits] = GameEvent.LADDER_CREDITS_CHG,
    [HeroAttr.HeroGuildScore] = GameEvent.GUILDE_SCORE_CHG,
    [HeroAttr.BigAdventurePassScore] = GameEvent.BIGADVENTUREPASS_SCORE_CHG,
    [HeroAttr.ThousandStrikesPassScore] = GameEvent.THOUSAND_STRIKES_PASS_SCORE_CHG,

    [HeroAttr.GuildDonateCountOne] = GameEvent.GUILDE_DONATECOUNTChange,
    [HeroAttr.GuildDonateCountTwo] = GameEvent.GUILDE_DONATECOUNTChange,
    [HeroAttr.GuildDonateCountThree] = GameEvent.GUILDE_DONATECOUNTChange,

    [HeroAttr.TowerMaxNum] = GameEvent.TOWER_MAX_NUM_CHG,
    [HeroAttr.TowerSweepAt] = GameEvent.TOWER_SWEEP_AT_CHG,
    [HeroAttr.TowerRewardNum] = GameEvent.TOWER_REWARD_NUM_CHG,
    [HeroAttr.TowerQuickSweepLeftCount] = GameEvent.TOWER_QUICK_SWEEP_LEFT_COUNT_CHG,
    [HeroAttr.TowerQuickSweepAt] = GameEvent.TOWER_QUICK_SWEEP_AT_CHG,
    [HeroAttr.TowerSweepStatus] = GameEvent.TOWER_SWEEP_STATUS_CHG,
    [HeroAttr.TowerResetCount] = GameEvent.TOWER_RESET_COUNT_CHG,

    [HeroAttr.HandUpLeftCount] = GameEvent.HANDUP_LEFT_COUNT_CHG,
    [HeroAttr.HandUpLeftCountAt] = GameEvent.HANDUP_AT_CHG,

    [HeroAttr.DuanWuPrivilegeStatus] = GameEvent.DUAN_WU_PRIVILEGE_STATUS_CHG,
    [HeroAttr.DuanWuPrivilegeOpenTime] = GameEvent.DUAN_WU_PRIVILEGE_OPEN_TIME_CHG,
    [HeroAttr.DuanWuPrivilegeDailyStatus] = GameEvent.DUAN_WU_PRIVILEGE_DAILY_STATUS_CHG,
    [HeroAttr.DuanWuPrivilegePackStatus] = GameEvent.DUAN_WU_PRIVILEGE_PACK_STATUS_CHG,

    [HeroAttr.LimitLotteryCount] = GameEvent.LIMIT_LOTTERY_COUNT_CHG,
    [HeroAttr.LimitLotteryGuaranteeCount] = GameEvent.LIMIT_LOTTERY_GUARANTEE_CHG,
    [HeroAttr.LimitLotteryPage] = GameEvent.LIMIT_LOTTERY_PAGE_CHG,
    [HeroAttr.BattlePassStatActive] = GameEvent.BATTLE_PASS_STAT_ACTIVE_CHG,
    [HeroAttr.BattlePassStatCampDailyTask] = GameEvent.CAMP_TASK_COMPLETE_NUM_CHG,
    [HeroAttr.MysticBossMaxLayer] = GameEvent.MYSTIC_MAX_NUM_CHG,
    [HeroAttr.MysticBossMajor] = GameEvent.MYSTIC_BOSS_MAJOR_CHG,
    [HeroAttr.MysticBossDailyRewardTime] = GameEvent.MYSTIC_BOSS_DAILY_REWARD_TIME_CHG,
    [HeroAttr.MainRideFashionId] = GameEvent.RIDE_FASHION_ID_CHG,
    [HeroAttr.SubRideFashionId] = GameEvent.SUB_RIDE_FASHION_ID_CHG,
    [HeroAttr.GuildOreStatus] = GameEvent.HERO_GUILDORE_BATTLE_STATUS,
    [HeroAttr.CavePetLevel] = GameEvent.CAVE_PET_LEVEL_CHG,

    [HeroAttr.RoleRechargeCount] = GameEvent.ROLE_RECHARGE_COUNT_CHG,

    [HeroAttr.UseChatBubbleId] = GameEvent.USE_CHATBUBBLE_ID_CHG,
    [HeroAttr.UseAvatarFrameId] = GameEvent.USE_AVATAR_FRAME_ID_CHG,
    [HeroAttr.GuildGardenHelpCoin] = GameEvent.GUILD_GARDEN_HELP_COIN_CHG,

    [HeroAttr.GuildPetCatchId] = GameEvent.GUILDPET_CATCH_ID_CHG,
    [HeroAttr.GuildPetCatchTime] = GameEvent.GUILDPET_CATCH_TIME,
    
    [HeroAttr.TargetGuildId] = GameEvent.TARGET_GUILDID_CHG,
    [HeroAttr.IsSneak] = GameEvent.IS_SNEAK_CHG,
    [HeroAttr.SneakGuildLookCount] = GameEvent.SNEAK_GUILD_LOOK_COUNT_CHG,
    [HeroAttr.SneakGuildStealGrowCount] = GameEvent.SNEAK_GUILD_STEAL_GROW_COUNT_CHG,
    [HeroAttr.SneakGuildStealBoxCount] = GameEvent.SNEAK_GUILD_STEAL_BOX_COUNT_CHG,
    [HeroAttr.SneakGuildBoxStatus] = GameEvent.SNEAK_GUILD_BOX_STATUS_CHG,
    [HeroAttr.FateEleGuarantee1] = GameEvent.HERO_FATE_ELEGUARANTEE_CHANGE,
    [HeroAttr.FateEleGuarantee2] = GameEvent.HERO_FATE_ELEGUARANTEE_CHANGE,
    [HeroAttr.FateScore] = GameEvent.HERO_FATE_SCORE_CHANGE,
    [HeroAttr.FateLotteryCount] = GameEvent.HERO_FATE_LOTTERYCOUNT_CHANGE,
    [HeroAttr.ArenaBoxStatus] = GameEvent.ARENA_BATTLE_STATUS,
    [HeroAttr.ArenaBoxBattleTime] = GameEvent.ARENA_BATTLE_BATTLING_TIME,

}
for k, eventArray in pairs(gConst.AttrToEvent.BattleEvent) do
    Game.mapAttrEvt[k] = eventArray.Hero
end

function Game:init()
    print("Game:init", self.stageMode)

    SocketEventManager:addEvt(self, SCmd.HERO_DATA_INFO, Bind(self, self.onHeroData))
    SocketEventManager:addEvt(self, SCmd.GET_TARGET_PLAYER_DATA, Bind(self, self.onOthersData))
    SocketEventManager:addEvt(self, SCmd.HERO_ATTR_CHANGE, Bind(self, self.onHeroAttrChange))
    SocketEventManager:addEvt(self, SCmd.OBJ_APPEARANCE_CHANGE, Bind(self, self.onObjAppearanceChange))
    SocketEventManager:addEvt(self, SCmd.GET_BUFF_LIST_NTF, Bind(self, self.onBuffList))
    SocketEventManager:addEvt(self, SCmd.UPDATE_BUFF_NTF, Bind(self, self.onUpdateBuff))

    -- npc pet 死亡等字段更新
    SocketEventManager:addEvt(self, SCmd.SPIRIT_PET_UPDATE_NTF, Bind(self, self.onBattlePetUpdateNtf))
    SocketEventManager:addEvt(self, SCmd.NPC_UPDATE_NTF, Bind(self, self.onBattleNpcUpdateNtf))

    -- 战斗舞台 通讯协议
    SocketEventManager:addEvt(self, SCmd.OBJ_APPEAR, Bind(self, self.onObjAppear))
    SocketEventManager:addEvt(self, SCmd.OBJ_DISAPPEAR, Bind(self, self.onObjDisappear))
    
    SocketEventManager:addEvt(self, SCmd.IDLE, Bind(self, self.onIdleResp))
    SocketEventManager:addEvt(self, SCmd.OBJ_IDLE, Bind(self, self.onObjIdle))

    SocketEventManager:addEvt(self, SCmd.MOVE, Bind(self, self.onMoveResp))
    SocketEventManager:addEvt(self, SCmd.OBJ_MOVE, Bind(self, self.onObjMove))
    SocketEventManager:addEvt(self, SCmd.OBJ_DIRECTION_NTF, Bind(self, self.onObjDirection))
    SocketEventManager:addEvt(self, SCmd.OBJ_RELIVE_NTF, Bind(self, self.onReliveNtf))
    SocketEventManager:addEvt(self, SCmd.OBJ_DEFEAT_POS_SYNC_NTF, Bind(self, self.onDefeatSyncPos))

    SocketEventManager:addEvt(self, SCmd.ATTACK, Bind(self, self.onAttackResp))
    SocketEventManager:addEvt(self, SCmd.OBJ_ATTACK, Bind(self, self.onObjAttack))
    SocketEventManager:addEvt(self, SCmd.ATTACK_TARGETS_NTF, Bind(self, self.onAttackTargets))
    SocketEventManager:addEvt(self, SCmd.OBJ_BE_HIT_NTF, Bind(self, self.onBeHitNtf))

    SocketEventManager:addEvt(self, SCmd.OBJ_SOUND_NTF, Bind(self, self.onPlaySound))

    SocketEventManager:addEvt(self, SCmd.OBJ_ATTACK_SYNC, Bind(self, self.onObjAttackSync))

    SocketEventManager:addEvt(self, SCmd.BATTLE_RESET_NTF, Bind(self, self.onBattleResetNtf))

    SocketEventManager:addEvt(self, SCmd.OBJ_CHARGE_MOVE, Bind(self, self.onObjCharge))

    SocketEventManager:addEvt(self, SCmd.OBJ_JUMP, Bind(self, self.onObjJump))
    SocketEventManager:addEvt(self, SCmd.OBJ_DIE, Bind(self, self.onObjDie))

    SocketEventManager:addEvt(self, SCmd.BUSY_STATUS_SETTING, Bind(self, self.onSetBusyStatus))

    SocketEventManager:addEvt(self, SCmd.HP_MP_CHANGE, Bind(self, self.onHpMpChange))

    SocketEventManager:addEvt(self, SCmd.JUMP_POINT, Bind(self, self.onScreenJumpPoint))
    SocketEventManager:addEvt(self, SCmd.JUMP_MAP_NTF, Bind(self, self.onJumpMapNtf))

    SocketEventManager:addEvt(self, SCmd.TRAP_ALARM_NTF, Bind(self, self.onTrapAlarm))
    SocketEventManager:addEvt(self, SCmd.TRAP_TRIGGER_NTF, Bind(self, self.onTrapTrigger))

    SocketEventManager:addEvt(self, SCmd.SKILL_COUNT_NTF, Bind(self, self.onSkillCountNtf))

    SocketEventManager:addEvt(self, SCmd.SERVER_EFFECT_NTF, Bind(self, self.onServerEffectNtf))
    SocketEventManager:addEvt(self, SCmd.INNAT_START_NTF, Bind(self, self.onInnatStartNtf))

    -- 走格子模式通讯协议 (世界 城镇 门派)
    SocketEventManager:addEvt(self, SCmd.WHOLE_OBJ_APPEAR, Bind(self, self.onWholeObjAppear))
    SocketEventManager:addEvt(self, SCmd.WHOLE_OBJ_DISAPPEAR, Bind(self, self.onWholeObjDisappear))
    SocketEventManager:addEvt(self, SCmd.WHOLE_IDLE, Bind(self, self.onWholeIdleResp))
    SocketEventManager:addEvt(self, SCmd.WHOLE_OBJ_IDLE, Bind(self, self.onWholeObjIdle))
    SocketEventManager:addEvt(self, SCmd.WHOLE_MOVE, Bind(self, self.onWholeMoveResp))
    SocketEventManager:addEvt(self, SCmd.WHOLE_OBJ_MOVE, Bind(self, self.onWholeObjMove))
    SocketEventManager:addEvt(self, SCmd.FASHION_JUMP_POS, Bind(self, self.onFashionJumpPos))

    SocketEventManager:addEvt(self, SCmd.BIG_SCREEN_NEARBY_OBJ, Bind(self, self.onWholeNearbyObj))


    SocketEventManager:addEvt(self, SCmd.SWITCH_TERRAIN_STATE, Bind(self, self.onSwitchTerrainState))
    SocketEventManager:addEvt(self, SCmd.SWITCH_TERRAIN_STATE_NTF, Bind(self, self.onObjSwitchTerrain))

    SocketEventManager:addEvt(self, SCmd.BOOK_GET_EXP, Bind(self, self.onGetExp))

    SocketEventManager:addEvt(self, SCmd.UN_KNOW_ERROR_NTF, Bind(self, self.onUnKnowError))

    SocketEventManager:addEvt(self, SCmd.OBJ_PASSIVE_SYNC_NTF, Bind(self, self.onSyncObjPassive))
    SocketEventManager:addEvt(self, SCmd.OBJ_SKILL_STOP_SYNC_NTF, Bind(self, self.onSyncObjSKillStop))

    self.execListPrepareChangeStage = {}
    self.execListPrepareChangeStageTagSet = {}
    
    -- 不同stage间切换需要执行的行为
    self.execListAfterChangeStage = {
        function()
        end,
    }

    self.serverEffectMap = {}
    self.innatEffectBeginTimeMap = {}

    -- 标识是否切换地形中
    self.switchingTerrain = false

    -- 标识是否在跳地图
    self.jumpingMap = false
end

function Game:uninit()
    self.switchingTerrain = false

    SocketEventManager:clearEvt(self)
    self.hero = nil    
    self.__attrEvt = nil
    self.stageMode = STAGE_UNKNOW
end

function Game:reconnectClear()
    self.serverEffectMap = {}
    self.innatEffectBeginTimeMap = {}
    self.jumpingMap = false

    self.switchingTerrain = false
end


function Game:getMapAttrEvt()
    return self.mapAttrEvt
end

function Game:isAttrEvt(event)
    if not self.__attrEvt then
        self.__attrEvt = {}
        for _, v in pairs(self.mapAttrEvt) do
            self.__attrEvt[v] = true
        end
    end

    return self.__attrEvt[event] or false
end

function Game:initAttrEvt()

end

local GAME_RESP_CODE = {
    OK = 0,
    ERROR = 1, --坐标与服务器不同步
    BLOCK = 2, --阻挡点
    MOVE_TOO_FAST = 3, --移动速度不同步, 过快
    MOVE_DIE = 4, --死亡
    -- USE_SKILL = 5, --技能释放中, 客户端不会收到, 收到即有问题
    BUFF_BAN_MOVE = 6, --身上带有buff, 无法移动
}

function Game:reqOthersInfo(playerId, serverId, tipType)
    -- 如果有serverId需要转场合服后的服务器id
    if serverId then 
        serverId = gModule.Server:getAfterServerIdById(serverId)
    end
    local msgData = {
        playerId = playerId,
        serverId = serverId,
        type = tipType,
    }

    SocketManager:send(CCmd.GET_TARGET_PLAYER_DATA, msgData)
end

function Game:reqSetBusyStatus()
    SocketManager:send(CCmd.BUSY_STATUS_SETTING)
end

function Game:reqOthersSimpleInfo(playerId, serverId, tipType)
    -- 如果有serverId需要转场合服后的服务器id
    if serverId then 
        serverId = gModule.Server:getAfterServerIdById(serverId)
    end
    local msgData = {
        playerId = playerId,
        serverId = serverId,
        isSimple = true,
        type = tipType,
    }

    SocketManager:send(CCmd.GET_TARGET_PLAYER_DATA, msgData)
end

function Game:reqOpenPlayerTip(playerId, serverId, tipType, params)
    self:reqOthersSimpleInfo(playerId, serverId, tipType)
    self.playerTipParams = params
end

function Game:onOpenPlayerTip(type, data)
    if type == gConst.Logic.OtherInfoTipType.FriendTip then
        UIManager:openWindow(lt.FriendTipWnd, data, self.playerTipParams)
    elseif type == gConst.Logic.OtherInfoTipType.ChatRoleInfo then
        UIManager:openWindow(lt.ChatRoleInfoTipWnd, data, self.playerTipParams)
    elseif type == gConst.Logic.OtherInfoTipType.TeamInvite then
        UIManager:openWindow(lt.TeamInviteOtherWnd, data, self.playerTipParams)
    elseif type == gConst.Logic.OtherInfoTipType.OtherRole then
        UIManager:openWindow(lt.UIOtherRoleTipWnd, data, self.playerTipParams)
    elseif type == gConst.Logic.OtherInfoTipType.GuildMember then
        UIManager:openWindow(lt.GuildMemberInfoWnd, data, self.playerTipParams)
    elseif type == gConst.Logic.OtherInfoTipType.Arena then
        UIManager:openWindow(lt.UIOtherRoleTipWnd, data, self.playerTipParams)
    end
end

function Game:onHeroData(data)
    print("Game:onHeroData")
    SocketManager:clearWaitCmd()

    -- 断线重连执行
    self:heroAttrChange(data)

    self.hero = Hero.new(data)
    self.hero:setIsMe()
    
    EventManager:broadcast(GameEvent.ENTER_GAME)
    gModule.Moment:record(gConst.Logic.MOMENT_HERODATA) --记录人物数据下发的时间

    
    gModule.Guide:updateToggle() --更新引导开启状态
    gModule.Moment:record(gConst.Logic.MOMENT_HERODATA)

    if self.hero:getExpAddBegin() == 1 then 
        gModule.Moment:record(gConst.Logic.MOMENT_EXPBEGIN)

        self.hero:setClientExp(self.hero:getValue(gConst.Attr.Hero.CurExp))
    end

    -- 日志设置用户
    CS.YF.Logger.SetUserId(self.hero:getPlayerId())


    if self.stageMode == STAGE_UNKNOW then
        return
    end

    if #data.objprops < 1 then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    
    worldAgent:onHeroDataRefresh(self.hero, data)
end

function Game:onOthersData(data)
    if Util.Funs:hasError(data) then 
        return
    end

    if data.type == 0 then
        local othersData = Others.new(data.roleInfo)
        gModule.Others:recordOthersData(othersData)
        UIManager:openWindow(lt.OthersWnd, othersData)
    elseif table.keyof(gConst.Logic.OtherInfoTipType, data.type) then
        if data.type == gConst.Logic.OtherInfoTipType.Arena then
            if data.roleInfo then
                local othersData = Others.new(data.roleInfo)
                gModule.Others:recordOthersData(othersData)
                UIManager:openWindow(lt.ArenaRobotOthersWnd, othersData)
            else
                local simpleHero = SimpleHero.new(data.simpleRoleInfo)
                self:onOpenPlayerTip(data.type, simpleHero)
            end
        else
            local simpleHero = SimpleHero.new(data.simpleRoleInfo)
            self:onOpenPlayerTip(data.type, simpleHero)
        end
    end
end

function Game:onHeroAttrChange(data)
    self:heroAttrChange(data.prop, data.isShow)
end

function Game:onSetBusyStatus(data)
    if Util.Funs:hasError(data) then 
        return
    end

    EventManager:broadcast(GameEvent.BUSY_IS_SETTING)
end

function Game:heroAttrChange(data, showFloat)
    if not self.hero then
        return
    end

    if not data or #data.objprops < 1 then
        return
    end
    
    self.hero:setAttrArray(data.objprops)

    local isMutil, array = self:testAttrChange(data.objprops, showFloat)
    if isMutil then
        -- 多条属性同时更新
        EventManager:broadcast(GameEvent.UPDATE_HERO_ATTRS, array, showFloat)
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    -- 自己属性发生变化
    local selfAgent = worldAgent:getSelfAgent()
    if selfAgent and selfAgent.onAttrChange then
        selfAgent:onAttrChange(data, false)
    end
end

function Game:testAttrChange(attrs, showFloat)
    local evts = {}
    local code = nil

    --货币属性变化
    local expendAttrChanged = false
    for _, v in ipairs(attrs) do
        code = v.properCode
        if self.mapAttrEvt[code] then
            evts[#evts+1] = self.mapAttrEvt[code]
        end

        if not expendAttrChanged and (CurrencyProIdx[code] or BindCurrencyProIdx[code]) then
            expendAttrChanged = true
        end
    end

    if expendAttrChanged then
        EventManager:broadcast(GameEvent.EXPEND_ATTR_CHANGE)
    end

    if #evts == 1 then
        EventManager:broadcast(evts[1], showFloat)
        return false
    elseif #evts == 0 then
        return false
    else
        table.sort(evts)
        return true, evts
    end
end

function Game:onObjAppearanceChange(data)
    if not data or #data.objprops < 1 then
        return
    end
    
    if self.stageMode == STAGE_UNKNOW then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local agent = worldAgent:queryAgentByObjId(data.objId)
    if not agent then
        return
    end

    agent:onAppearanceChange(data)
end

function Game:onObjAppear(data)
    if self.stageMode == STAGE_UNKNOW then
        return
    end

    local objType = data.objType

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    if objType == gConst.Game.OBJ_HERO then
        -- 玩家信息
        if data.objId == self.hero:getObjId() then
            -- 自己
            self.hero:setAttrArray(data.objprops)
            -- local x = 20
            -- local y = 9
            local x = data.x
            local y = data.y
            worldAgent:createSelfAgent(self.hero, x, y, Vector2.new(data.directionX, data.directionY))
        else
            -- 其他人 
            worldAgent:createHeroAgent(Hero.new(data), data.x, data.y, Vector2.new(data.directionX, data.directionY), data.status)
        end

    elseif objType == gConst.Game.OBJ_MONSTER then
        -- 怪物
        worldAgent:createMonsterAgent(Monster.new(data), data.x, data.y, Vector2.new(data.directionX, data.directionY), data.status)
    elseif objType == gConst.Game.OBJ_ROBOT then
        -- 机器人
        worldAgent:createRobotAgent(BattleRobot.new(data), data.x, data.y, Vector2.new(data.directionX, data.directionY), data.status)        
    elseif objType == gConst.Game.OBJ_VISION then
        worldAgent:createVisionAgent(Vision.new(data), data.x, data.y, Vector2.new(data.directionX, data.directionY), data.status)

    elseif objType == gConst.Game.OBJ_SKILL then
        -- 技能物
        local skillObj = SkillObj.new(data)

        worldAgent:createSkillAgent(skillObj)
    elseif objType == gConst.Game.OBJ_PET then
        worldAgent:createPetAgent(BattlePet.new(data), data.x, data.y, Vector2.new(data.directionX, data.directionY), data.status)
    elseif objType == gConst.Game.OBJ_NPC then
        -- Npc
        worldAgent:createNpcAgent(BattleNpc.new(data), data.x, data.y, Vector2.new(data.directionX, data.directionY), data.status)
    else
        printError("onObjAppear 缺少 objType:", objType)
    end
end

function Game:onObjDisappear(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    if data.ret == gConst.Logic.ObjDisappearType.Fade then 
        worldAgent:fadeAgent(data.objId)
    else
        worldAgent:removeAgent(data.objId)
    end
end

function Game:onIdleResp(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    if data.code == GAME_RESP_CODE.OK then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    -- 服务器发送数据异常
    local selfAgent = worldAgent:getSelfAgent()

    -- 异常处理
    if data.code == GAME_RESP_CODE.ERROR then
        -- 校验异常
        selfAgent:setPosition(data.x, data.y)
        selfAgent:setDirectionXY(data.directionX, data.directionY)

        if selfAgent:getCurState().setWait then
            -- 寻路中等待一下
            print("TODO: Game:onIdleResp 寻路中 等一下")
        else
            selfAgent:changeStateIdle(1)
        end
    elseif data.code == GAME_RESP_CODE.BLOCK then
        -- 碰到阻挡点了
        selfAgent:setPosition(data.x, data.y)
        selfAgent:setDirectionXY(data.directionX, data.directionY)
        selfAgent:changeStateIdle(1)
        worldAgent:focusNow()
    else
        printWarning("Game:onIdleResp 缺少对code:"..data.code.."的处理")
    end
end

function Game:onObjIdle(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    if gConst.Game.DebugBattleObjId == data.objId then
        dump(data, "DebugBattleObjId Game:onObjIdle")
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION,
        gConst.Game.AGENT_SERVER_SKILL
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    if roleAgent:getAgentType() == gConst.Game.AGENT_SERVER_SKILL then 
        roleAgent:serverSkillIdle(data)
        return
    end

    roleAgent:pushCmd(lt.BattleCmdIdle.new(data))
end

local ExceptionCode = {
    [GAME_RESP_CODE.ERROR] = true,
    [GAME_RESP_CODE.BLOCK] = true,
    [GAME_RESP_CODE.MOVE_TOO_FAST] = true,
}
function Game:onMoveResp(data)
    -- dump(data, "SocketManager:onProcessMsg broadcast msgId:MOVE")
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local code = data.code

    if code == GAME_RESP_CODE.OK then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    -- 服务器发送数据异常
    local selfAgent = worldAgent:getSelfAgent()

    -- 异常处理
    if ExceptionCode[code] then
        selfAgent:setPosition(data.x, data.y)
        selfAgent:setDirectionXY(data.directionX, data.directionY)
        selfAgent:changeStateIdle(1)
        worldAgent:focusNow()

        gModule.Navigate:reset()
    elseif GAME_RESP_CODE.BUFF_BAN_MOVE == code then
        selfAgent:setPosition(data.x, data.y)
        selfAgent:setDirectionXY(data.directionX, data.directionY)
        selfAgent:changeStateIdle(1)
        gModule.Navigate:reset()
    else
        printWarning("Game:onMoveResp 缺少对code:"..data.code.."的处理")
    end
end

function Game:onObjMove(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    
    if gConst.Game.DebugBattleObjId == data.objId then
        dump(data, "DebugBattleObjId Game:onObjMove")
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION,
        gConst.Game.AGENT_SERVER_SKILL
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    if roleAgent:getAgentType() == gConst.Game.AGENT_SERVER_SKILL then 
        roleAgent:serverSkillMove(data)
        return
    end

    roleAgent:pushCmd(lt.BattleCmdMove.new(data))
end

function Game:onObjDirection(data)
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    
    if gConst.Game.DebugBattleObjId == data.objId then
        dump(data, "DebugBattleObjId Game:onObjMove")
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER, 
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION,
        gConst.Game.AGENT_SERVER_SKILL
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    roleAgent:setDirection(Vector2Fast.new(data.directionX, data.directionY))
end

function Game:onReliveNtf(data)
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    
    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    -- 播复活特效
    local envSystem = AgentManager:getWorldAgent():getBattleEnvSystem()
    envSystem:createOnceEnv(gConst.Game.EnvNeedShow.Order10, "Art/Effect/Scene/SceneEffect_Relive/SceneEffect_Relive.prefab", Vector2Fast.zero, Vector2Fast.down, {
        parent = roleAgent,
    })
end

function Game:onDefeatSyncPos(data)
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    local objId, posX, posY
    for _, defeatPos in ipairs(data.posList) do
        objId = defeatPos.objId
        local roleAgent = worldAgent:queryAgentByObjId(objId, 
            gConst.Game.AGENT_HERO, 
            gConst.Game.AGENT_MONSTER, 
            gConst.Game.AGENT_ROBOT,
            gConst.Game.AGENT_PET, 
            gConst.Game.AGENT_NPC,
            gConst.Game.AGENT_VISION,
            gConst.Game.AGENT_SERVER_SKILL
        )
        if not roleAgent then
            printWarning("未找到目标 或目标不是人物")
        else
            posX = defeatPos.x
            posY = defeatPos.y
            print("======> 同步位置", posX, posY) 
            -- Schedule:addDelay(0.5, function ()
            --     roleAgent:setPosition(posX, posY)
            -- end)
            roleAgent:setPosition(posX, posY)
        end
    end
end

function Game:onObjJump(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    if roleAgent:isSelf() then
        -- 自己
        roleAgent:onJump(data)
    else
        -- 其他人释放技能
        roleAgent:pushCmd(lt.BattleCmdJump.new(data))
    end
    
end

local WORLD_ATTACK_RESP_CODE = {
    OK = 0,
    CD = 1, -- 技能冷却中
    TOO_FAR = 2, -- 距离过远
    MP = 3, -- 蓝不够
    HP = 4, -- 血不够
    INVALID_POINT = 5, -- 目标点不对
    RELEASING = 6,     -- 释放中
    CAN_NOT_USE = 7, -- 无法攻击
    NOT_LEARN = 8, -- 未学习
    PREPARE_TIME = 9, --副本准备期无法攻击
}

function Game:onAttackResp(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local code = data.code
    if code ~= WORLD_ATTACK_RESP_CODE.OK then
        -- 异常处理
        if code == WORLD_ATTACK_RESP_CODE.NOT_LEARN then
            printWarning("Game:onAttackResp 尚未学习对应技能 使用 @learnallskill")
        else
            if code >= WORLD_ATTACK_RESP_CODE.CD and code <= WORLD_ATTACK_RESP_CODE.PREPARE_TIME then 
                MsgManager:popNotice(240 + code)
            else
                printWarning("Game:onAttackResp 缺少对code:"..code.."的处理")
            end
        end
        local selfAgent = worldAgent:getSelfAgent()
        selfAgent:readyToFlash(false)
        return
    end

    local selfAgent = worldAgent:getSelfAgent()
    selfAgent:updateCastFunStep(data)
end

function Game:onObjAttack(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    -- 攻击指令
    local combineSkillMeta = gModule.Skill:combineQueryById(data.skillId)
    if combineSkillMeta and gModule.Skill:isPassiveSkill(data.skillId) then
        roleAgent:handleAttackPassiveCmd(data)
    else
        roleAgent:pushCmd(lt.BattleCmdAttack.new(data))
    end

end

function Game:onPlaySound(data)
    local soundId = data.soundId
    AudioManager:playSound(Util.Res:getBossSoundName(soundId))
end

function Game:onAttackTargets(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    local skillFunc = lt.SkillManager:getFunc(data.skillId, roleAgent:getAgentId())
    if not skillFunc then 
        return
    end

    skillFunc:onAttackTargets(roleAgent, data)
end

--[[message CS_ObjBeHitNtf {
  uint32 objId = 1;//受击objId
  uint32 type = 2;//动作类型
  uint32 skillId = 3;//受击技能id
  fixed64 startAt = 4;//开始时间
}]]
function Game:onBeHitNtf(data)
    -- if self.stageMode ~= STAGE_BATTLE then
    --     return
    -- end

    -- local worldAgent = AgentManager:getWorldAgent()
    -- if not worldAgent then
    --     return
    -- end

    -- local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
    --     -- gConst.Game.AGENT_HERO, 
    --     gConst.Game.AGENT_MONSTER 
    --     -- gConst.Game.AGENT_PET, 
    --     -- gConst.Game.AGENT_NPC,
    -- )

    -- if not roleAgent then
    --     printWarning("未找到目标 或目标不是人物")
    --     return
    -- end

    -- -- 标识数据来源自服务器
    -- data.fromServer = true
    -- lt.SkillManager:handleHitPerformance(roleAgent, data)
end

function Game:onObjAttackSync(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("技能同步, 无法找到服务器下发的同步目标")
        return
    end

    local skillFunc = lt.SkillManager:getFunc(data.skillId, roleAgent:getAgentId())
    if not skillFunc then 
        return
    end

    skillFunc:onAttackSync(roleAgent, data)
end

function Game:onSyncObjPassive(data)
    if gModule.Game:getStageMode() ~= gConst.Game.STAGE_BATTLE then
        return
    end

    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId,
        gConst.Game.AGENT_HERO,
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_PET,
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("技能被动数据同步, 无法找到服务器下发的同步目标")
        return
    end

    local skillFunc = lt.SkillManager:getFunc(data.skillId, roleAgent:getAgentId())
    if not skillFunc or not skillFunc.onObjPassiveSync then
        return
    end

    skillFunc:onObjPassiveSync(roleAgent, data)
end

function Game:onSyncObjSKillStop(data)
    if gModule.Game:getStageMode() ~= gConst.Game.STAGE_BATTLE then
        return
    end

    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local roleAgent = worldAgent:queryAgentByObjId(data.objId,
        gConst.Game.AGENT_HERO,
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_PET,
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("技能停止释放时同步, 无法找到服务器下发的同步目标")
        return
    end

    local skillFunc = lt.SkillManager:getFunc(data.skillId, roleAgent:getAgentId())
    if not skillFunc or not skillFunc.onObjSKillStopSync then
        return
    end

    skillFunc:onObjSKillStopSync(roleAgent, data)
end

function Game:onBattleResetNtf(data)
    print("zwjzwj Game:onBattleResetNtf")
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if worldAgent and worldAgent.onReset then
        worldAgent:onReset(data)
    end
    
    -- 更新技能Cd
    lt.SkillManager:reset()
    -- 清理药物Cd
    gModule.Item:clearDrugTime()
    EventManager:broadcast(GameEvent.UPDATE_SKILL_CD)
end

function Game:onObjCharge(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    

    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    local objType = roleAgent:getRole():getObjType()
    if objType ~= gConst.Game.OBJ_MONSTER and objType ~= gConst.Game.OBJ_PET then
        printWarning("Game:onObjCharge 目前只有怪物能够发起冲锋!!!")
        return
    end

    -- 冲锋指令
    roleAgent:pushCmd(lt.BattleCmdCharge.new(data))
end

function Game:onHpMpChange(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end
    
    local roleAgent = worldAgent:queryAgentByObjId(data.objId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )

    if not roleAgent then
        -- 当前目标不关心血量变换
        return
    end
    roleAgent:onHpMpChange(data.attackerId, data)
end

function Game:onObjDie(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    --死亡清除天赋特效数据
    self.innatEffectBeginTimeMap[data.objId] = {}

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    -- 自己死亡
    if data.objId == self.hero:getObjId() then
        if not gModule.Map:isNeedExecDieByHand() then
            local selfAgent = worldAgent:getSelfAgent()
            selfAgent:execDie()
        end

        return
    end

    local roleAgent = AgentManager:getWorldAgent():queryAgentByObjId(data.objId,
        gConst.Game.AGENT_HERO,
        gConst.Game.AGENT_MONSTER,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_PET,
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )
    if not roleAgent then
        return
    end

    roleAgent:pushCmd(lt.BattleCmdDie.new(data))
end

local JumpPointErrorCode = {
    MAP_NOT_EXIST = 1, -- 地图不存在
    MAP_TOO_FAR = 2, -- 离跳转点太远
    MAP_ERROR = 3, -- 地图错误
    MAP_NEED_PASS_DUPLICATE = 4 -- 未通关指定副本
}
function Game:onScreenJumpPoint(data)
    self.jumpingMap = false
    if data.code == JumpPointErrorCode.MAP_NOT_EXIST then
        MsgManager:popNotice(20020)
    elseif data.code == JumpPointErrorCode.MAP_TOO_FAR then
        MsgManager:popNotice(20021)
    elseif data.code == JumpPointErrorCode.MAP_ERROR then
        MsgManager:popNotice(20022)
    elseif data.code == JumpPointErrorCode.MAP_NEED_PASS_DUPLICATE then
        MsgManager:popNotice(557)
    elseif data.code == gConst.Global.SUCCESS then 
        local selfAgent = AgentManager:getSelfAgent()
        local m, n = selfAgent:getMN()
        local canTrigger, meetId = gModule.Meet:canTriggerMeet(Vector2.new(m, n))
        if canTrigger then
            gModule.Meet:reqCreateMeetId(meetId) 
        end
    end
end

function Game:onFashionJumpPos(data)
    self:setJumpingMap(false)
    if Util.Funs:hasError(data) then
        return
    end
end

function Game:onJumpMapNtf(data)
    if Util.Funs:hasError(data) then
        return
    end
end



--[[
此协议接收的机关 并没有objId, 表现上即为只有个人能够看到, 不会与其他玩家同步
]]
function Game:onTrapAlarm(data)
    if self.stageMode ~= STAGE_BATTLE then
        return 
    end

    for i, v in ipairs(data.posArray) do
        lt.TrapManager:onTrapAlarm(TrapObject.new(v, false))
        -- roleAgent:updateSkillStep(data)
    end
end

--[[
此协议接收的机关数据 并没有objId, 同onTrapAlarm
]]
function Game:onTrapTrigger(data)
    if self.stageMode ~= STAGE_BATTLE then
        return 
    end

    for i, v in ipairs(data.posArray) do
        lt.TrapManager:onTrapTrigger(TrapObject.new(v, false))
    end
end

function Game:onWholeObjAppear(data)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    if data.objType == gConst.Game.OBJ_HERO then
        -- 玩家信息
        if data.objId == self.hero:getObjId() then
            -- 自己
            self.hero:setAttrArray(data.objprops)

            local worldAgent = AgentManager:getWorldAgent()
            if not worldAgent then
                return
            end

            worldAgent:createSelfAgent(self.hero, data.m, data.n, data.direction, data.ret)
        else
            local worldAgent = AgentManager:getWorldAgent()
            if not worldAgent then
                return
            end

            if worldAgent:getHeroAgentCount() >= Setting:getMaxOnScreenPeople() then
                return
            end

            local moveList = nil
            if #data.moveList > 0 then
                moveList = {}
                for _, spot in ipairs(data.moveList) do
                    table.insert(moveList, Vector2.new(spot.newM, spot.newN))
                end
            end

            -- 其他人
            worldAgent:createHeroAgent(Hero.new(data), data.m, data.n, data.direction, moveList)
        end

    else
        printWarning("onWholeObjAppear 缺少 objType:", data.objType)
    end
end

function Game:onWholeObjDisappear(data)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    worldAgent:deleteAgent(data.objId, data.ret)
end

function Game:onWholeIdleResp(data)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    if data.code == GAME_RESP_CODE.OK then
        local selfAgent = AgentManager:getSelfAgent()
        if not selfAgent then
            return
        end
        local m, n = selfAgent:getMN()
        selfAgent:checkStandHuntPos()
        return
    end

    -- 异常处理
    local selfAgent = AgentManager:getSelfAgent()
    if not selfAgent then
        return
    end

    -- 异常处理
    if data.code == GAME_RESP_CODE.ERROR or data.code == GAME_RESP_CODE.BLOCK or data.code == GAME_RESP_CODE.MOVE_TOO_FAST then
        -- 碰到阻挡点了
        selfAgent:setMN(data.m, data.n, true)
        selfAgent:changeStateIdle(1)
        -- worldAgent:focusNow()

        gModule.Navigate:reset()
    else
        printError("Game:onWholeIdleResp 缺少对code:"..data.code.."的处理")
    end
end

function Game:onWholeObjIdle(data)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local agent = worldAgent:queryAgentByObjId(data.objId)
    if not agent then
        return
    end
    if not agent.pushCmd then 
        printError("=======> onWholeObjIdle 找到错误的agent:", agent.agentType)
        return
    end
    agent:pushCmd(lt.WorldIdleCmd.new(data.m, data.n, data.direction))
end

function Game:onWholeMoveResp(data)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    if data.code == GAME_RESP_CODE.OK then
        return
    end

    local selfAgent = AgentManager:getSelfAgent()
    if not selfAgent then
        return
    end

    -- 异常处理
    if data.code == GAME_RESP_CODE.ERROR or data.code == GAME_RESP_CODE.BLOCK or data.code == GAME_RESP_CODE.MOVE_TOO_FAST then
        -- 碰到阻挡点了
        selfAgent:setMN(data.m, data.n, true)
        selfAgent:changeStateIdle(1)
        -- worldAgent:focusNow()

        gModule.Navigate:reset()
    else
        printError("Game:onWholeMoveResp 缺少对code:"..data.code.."的处理")
    end
end

function Game:onWholeObjMove(data)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local agent = worldAgent:getAgent(data.objId)
    if not agent then
        return
    end

    if agent:isSelf() then
        printWarning("Game:onWholeObjMove 服务器错误下发协议, 自已的移动回包不应该走到其他人的广播")
        return
    end
    local locationArray = {}
    for _, spot in ipairs(data.moveList) do
        table.insert(locationArray, Vector2.new(spot.newM, spot.newN))
    end
    agent:pushCmd(lt.WorldMoveCmd.new(data.m, data.n, locationArray))
end

function Game:reqWholeNearbyObj(count)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    local excludeObjIdArray = {}

    local msgData = {
        num = count,
        excludeObjIdArray = excludeObjIdArray,
    }

    SocketManager:send(CCmd.BIG_SCREEN_NEARBY_OBJ, msgData)
end

function Game:onWholeNearbyObj(dataArray)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local curWholeMapPeopleNum = worldAgent:getHeroAgentCount()

    local heroMap = {}
    for k, data in ipairs(dataArray.objArray) do
        heroMap[data.objId] = true
    end

    -- 清除服务器未发的
    local serverAgentTable = worldAgent:getServerAgentTable()
    local myObjId = self.hero and self.hero:getObjId()
    for objId, agent in pairs(serverAgentTable) do
        if objId ~= myObjId and not heroMap[objId] then
            worldAgent:deleteAgent(objId)
            curWholeMapPeopleNum = curWholeMapPeopleNum - 1
        else
            -- 无需创建
            heroMap[objId] = false
        end
    end

    for k, data in ipairs(dataArray.objArray) do
        if data.objType == gConst.Game.OBJ_HERO then
            -- 玩家信息
            if data.objId ~= myObjId and heroMap[data.objId] then
                if curWholeMapPeopleNum >= Setting:getMaxOnScreenPeople() then
                    return
                end
                curWholeMapPeopleNum = curWholeMapPeopleNum + 1

                local moveList = nil
                if #data.moveList > 0 then
                    moveList = {}
                    for _, spot in ipairs(data.moveList) do
                        table.insert(moveList, Vector2.new(spot.newM, spot.newN))
                    end
                end

                -- 其他人
                worldAgent:createHeroAgent(Hero.new(data), data.m, data.n, data.direction, moveList)
            end
        else
            printWarning("onWholeNearbyObj 缺少 objType:", data.objType)
        end
    end
end

function Game:onBuffList(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    local buffArray = data.buffArray

    if #buffArray == 0 then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local objId = data.objId

    if objId == self.hero:getObjId() then
        local selfAgent = worldAgent:getSelfAgent()
        if selfAgent then
            selfAgent:setBuffList(buffArray)
        end

        EventManager:broadcast(GameEvent.BUFF_UPDATE)
    else
        local roleAgent = AgentManager:getWorldAgent():queryAgentByObjId(objId, 
            gConst.Game.AGENT_HERO, 
            gConst.Game.AGENT_MONSTER,
            gConst.Game.AGENT_ROBOT,
            gConst.Game.AGENT_PET, 
            gConst.Game.AGENT_NPC,
            gConst.Game.AGENT_VISION
        )
        if roleAgent then
            roleAgent:setBuffList(buffArray)
        else
            printWarning("Game:onBuffList cann't find role agent", objId)
        end
    end
end

function Game:onUpdateBuff(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    local objId = data.objId

    if objId == self.hero:getObjId() then
        local selfAgent = worldAgent:getSelfAgent()
        selfAgent:updateBuff(data)
        EventManager:broadcast(GameEvent.BUFF_UPDATE)
    else
        local roleAgent = AgentManager:getWorldAgent():queryAgentByObjId(objId, 
            gConst.Game.AGENT_HERO, 
            gConst.Game.AGENT_MONSTER,
            gConst.Game.AGENT_ROBOT, 
            gConst.Game.AGENT_PET, 
            gConst.Game.AGENT_NPC, 
            gConst.Game.AGENT_VISION
        )
        if roleAgent then
            roleAgent:updateBuff(data)
        end
    end
end

function Game:onInnatStartNtf(data)
    local objId = data.objId
    for i, kv in ipairs(data.innatCdList) do
        if not self.innatEffectBeginTimeMap[objId] then
            self.innatEffectBeginTimeMap[objId] = {}
        end
        self.innatEffectBeginTimeMap[objId][kv.id] = kv
    end

    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    if objId == self.hero:getObjId() then
        local selfAgent = worldAgent:getSelfAgent()
        if selfAgent then
            for  i, kv in ipairs(data.innatCdList) do
                selfAgent:updateInnatEffect(kv)
            end
        end
    else
        local roleAgent = AgentManager:getWorldAgent():queryAgentByObjId(objId, 
            gConst.Game.AGENT_HERO
        )
        if roleAgent then
            for  i, kv in ipairs(data.innatCdList) do
                roleAgent:updateInnatEffect(kv)
            end
        end
    end
end

function Game:getInnatEffectMap(objId)
    return self.innatEffectBeginTimeMap[objId] or {}
end

function Game:onBattlePetUpdateNtf(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    for i, unitPet in ipairs(data.petList) do
        local petProperty = unitPet.property
        local roleAgent = worldAgent:queryAgentByObjId(petProperty.objId, 
            gConst.Game.AGENT_PET
        )
        if roleAgent then
            roleAgent:onAttrChange(petProperty)
        end
    end

    EventManager:broadcast(GameEvent.BATTLE_PET_REFRESH)
end

function Game:onBattleNpcUpdateNtf(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    for i,npcProperty in ipairs(data.npcList) do
        local objProperty = npcProperty.property
        local roleAgent = worldAgent:queryAgentByObjId(objProperty.objId, 
            gConst.Game.AGENT_NPC
        )
        if roleAgent then
            roleAgent:onAttrChange(objProperty)
        end
    end

    EventManager:broadcast(GameEvent.BATTLE_NPC_REFRESH)
end

function Game:onSkillCountNtf(data)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end
    
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return
    end

    for _, unit in ipairs(data.attackCounts) do
        local objId = unit.objId
        local roleAgent = worldAgent:queryAgentByObjId(objId,
            gConst.Game.AGENT_HERO, 
            gConst.Game.AGENT_MONSTER, 
            gConst.Game.AGENT_PET,
            gConst.Game.AGENT_ROBOT, 
            gConst.Game.AGENT_NPC, 
            gConst.Game.AGENT_VISION
        )

        if roleAgent then
            lt.SkillManager:handleSyncCount(roleAgent, unit.skillId, unit)
        else
            printWarning("同步技能释放次数时, 找不到施法对象")
        end
    end
end

function Game:onServerEffectNtf(data)
    local isAddEffect = data.status == 1 -- 0 删除 1 增加
    local effectType = data.effect
    if isAddEffect then 
        self.serverEffectMap[effectType] = true
        EventManager:broadcast(GameEvent.SERVER_ADD_EFFECT, effectType)
    else
        self.serverEffectMap[effectType] = nil
        EventManager:broadcast(GameEvent.SERVER_DELETE_EFFECT, effectType)
    end
end

function Game:getMonsterNumber()
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return 0
    end

    return worldAgent:getMonsterNumber()
end

function Game:getBossNumber()
    local worldAgent = AgentManager:getWorldAgent()
    if not worldAgent then
        return 0
    end

    return worldAgent:getBossNumber()
end

-- 获得由服务器触发了哪些效果
function Game:getServerEffectMap()
    return self.serverEffectMap
end

function Game:hasServerEffectByType(effectType)
    return self.serverEffectMap[effectType] and true or false
end

function Game:slowMotionScale()
    CS.UnityEngine.Time.timeScale = gConst.Game.SlowDownTimeScale
    SocketManager:setHeartbeatover(30)
end

function Game:resetSlowDownTimeScale()
    CS.UnityEngine.Time.timeScale = 1
    SocketManager:resetHeartbeatover()
end

function Game:resetServerEffectMap()
    self.serverEffectMap = {}
end

function Game:onSwitchTerrainState(data)
    if Util.Funs:hasError(data) then
        return
    end

    if self.stageMode == STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    local selfAgent = worldAgent:getSelfAgent()

    if data.isChanged then
        local x, y = selfAgent:getPosition()
        local prefabPath = "Art/Effect/Scene/SceneEffect_Boat_Change/SceneEffect_Boat_Change.prefab"
        local envSystem = worldAgent:getEnvSystem()
        envSystem:createOnceEnv(prefabPath, Vector2Fast.new(x, y), Vector2Fast.up, {
            duration = 4,
        })
    end

    local type = data.type
    local m, n = data.m, data.n
    
    selfAgent:setMN(m, n, true)
    selfAgent:updateDisplayFigure()
    selfAgent:changeStateIdle()

    if gModule.Navigate:navigateInCrossMap() then
        gModule.Navigate:resumeCrossMapNavigate()
    elseif gModule.Navigate:navigateInCurMap() then
        gModule.Navigate:resumeCurMapNavigate()
    end

    if type == gConst.Logic.SwitchTerrainType.FromeSeaToLand then
        selfAgent:fromSeaToLand()
    end
end

function Game:onObjSwitchTerrain(data)
    --dump(data, "Game:onObjSwitchTerrain")
    if self.stageMode == STAGE_BATTLE then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    local roleAgent = worldAgent:queryAgentByObjId(data.objId)
    if not roleAgent then
        printWarning("未找到目标 或目标不是人物")
        return
    end

    if data.isChanged then
        local x, y = roleAgent:getPosition()
        local prefabPath = "Art/Effect/Scene/SceneEffect_Boat_Change/SceneEffect_Boat_Change.prefab"
        local envSystem = worldAgent:getEnvSystem()
        envSystem:createOnceEnv(prefabPath, Vector2Fast.new(x, y), Vector2Fast.up, {
            duration = 4,
        })
    end

    local type = data.type
    local m, n = data.m, data.n
    roleAgent:setMN(m, n, true)
    roleAgent:updateDisplayFigure()
    roleAgent:changeStateIdle()

    
    if type == gConst.Logic.SwitchTerrainType.FromeSeaToLand then
        roleAgent:fromSeaToLand()
    end
end

--------------------------------------------------------------------------------
-- 战斗地图请求
function Game:sendIdle(position, direction)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local msgData = {
        objId = self.hero:getObjId(),
        objType = gConst.Game.OBJ_HERO,
        x = position.x,
        y = position.y,
        directionX = direction.x,
        directionY = direction.y,
    }

    if gModule.Map:getPlayingMethod() == gConst.Attr.LunJian then
        -- 位于跨服中
        LunJianSocketManager:send(CCmd.IDLE, msgData)
    else
        -- 位于主服中
        SocketManager:send(CCmd.IDLE, msgData)
    end
end

function Game:sendMove(position, direction)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local msgData = {
        objId = self.hero:getObjId(),
        objType = gConst.Game.OBJ_HERO,
        x = position.x,
        y = position.y,
        directionX = direction.x,
        directionY = direction.y,
    }

    if gModule.Map:getPlayingMethod() == gConst.Attr.LunJian then
        -- 位于跨服中
        LunJianSocketManager:send(CCmd.MOVE, msgData)
    else
        -- 位于主服中
        SocketManager:send(CCmd.MOVE, msgData)
    end
end

function Game:sendAttack(skillId, position, loopParams)
    if self.stageMode ~= STAGE_BATTLE then
        return
    end

    local msgData = {
        objId = self.hero:getObjId(),
        skillId = skillId,
        x = position.x,
        y = position.y,
    }

    local paramType = loopParams[1][1]

    if not paramType then
        if gModule.Map:getPlayingMethod() == gConst.Attr.LunJian then
            -- 位于跨服中
            LunJianSocketManager:send(CCmd.ATTACK, msgData)
        else
            -- 位于主服中
            SocketManager:send(CCmd.ATTACK, msgData)
        end
        return
    end
    
    for k, tbl in ipairs(loopParams) do
        local paramType = tbl[1]
        local param = tbl[2]
        if paramType == gConst.Skill.PARAM_TYPE_POSITION then
            -- 参数为坐标
            -- 矫正参数
            for i, v in ipairs(param) do
                local pos = param[i]
                local x, y = pos.x, pos.y
                local direction = Vector2.new(pos.x - position.x, pos.y - position.y).normalized
        
                local loopCount = 1
                while true do
                    if not gModule.Map:isBlockPosition(pos) then
                        param[i].x = pos.x
                        param[i].y = pos.y
                        break
                    end
                    loopCount = loopCount + 1
                    if loopCount >= 20 then
                        break
                    end
                    pos = pos - direction
                end
                -- 不改变方向
                local newDirection = Vector2.new(pos.x - position.x, pos.y - position.y).normalized
                if not newDirection:EqualXY(direction.x, direction.y) then
                    param[i].x = position.x + direction.x
                    param[i].y = position.y + direction.y
                end
                param[i].x = math.max(param[i].x, 0)
                param[i].y = math.max(param[i].y, 0)
            end

            msgData.targetPoss = param
        elseif paramType == gConst.Skill.PARAM_TYPE_POSITION_RAW then
            --参数为坐标 不需要矫正阻挡点
            msgData.targetPoss = param
        elseif paramType == gConst.Skill.PARAM_TYPE_DIRECTION then
            -- 参数为向量
            msgData.targetDirs = param
        elseif paramType == gConst.Skill.PARAM_TYPE_TARGETID then
            -- 参数为向量
            msgData.targetIds = param
        end
    end

    if gModule.Map:getPlayingMethod() == gConst.Attr.LunJian then
        -- 位于跨服中
        LunJianSocketManager:send(CCmd.ATTACK, msgData)
    else
        -- 位于主服中
        SocketManager:send(CCmd.ATTACK, msgData)
    end
end

function Game:notifyObjAttackHit(objId)
    local msgData = {
        value1 = objId,
    }
    SocketManager:send(CCmd.SKILL_OBJ_HIT_RPT, msgData)
end

function Game:sendJumpPoint(linkIdx)
    local targetMapCode = gDao.MapLink:queryById(linkIdx):getCode2()
    local mapNeedLevel = gDao.Map:queryById(targetMapCode):getNeedLevel()
    if self.hero:getLevel() < mapNeedLevel then
        MsgManager:popNotice(310, gModule.LvUp:getLevelNameByNeedLevel(mapNeedLevel))
        return
    end
    --加个点击间隔
    if self.clickJumpPointTime and (gModule.Server:getTime() - self.clickJumpPointTime) < 0.1 then 
        return
    end
    self.clickJumpPointTime = gModule.Server:getTime()


    SilentDownloadManager:checkMapResourceDownloaded(targetMapCode, function()
        local msgData = {
            linkIdx = tostring(linkIdx)
        }

        self.jumpingMap = true
        if gModule.Map:getPlayingMethod() == gConst.Attr.LunJian then
            -- 0.1s 位于跨服中
            LunJianSocketManager:send(CCmd.JUMP_POINT, msgData)
        else
            -- 0.1s 位于主服中
            SocketManager:send(CCmd.JUMP_POINT, msgData)
        end
    end)

end

function Game:relive(mode)
    if gModule.Map:getPlayingMethod() == gConst.Attr.LunJian then
        -- 位于跨服中
        LunJianSocketManager:send(CCmd.RELIVE)
    else
        -- 位于主服中
        SocketManager:send(CCmd.RELIVE)
    end
end


-- 大地图协议
function Game:sendWholeIdle(m, n, directionVal)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    if self.jumpingMap then
        printError("=========> Game:sendWholeIdle", m, n, gModule.Map:getMapCode())
        return
    end

    local msgData = {
        objId = self.hero:getObjId(),
        m = m,
        n = n,
        direction = directionVal or 4
    }
    SocketManager:send(CCmd.WHOLE_IDLE, msgData)
end

function Game:reqFashionJumpPos(m, n, routeArray)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    if self.jumpingMap then
        printError("=========> Game:reqFashionJumpPos", m, n, gModule.Map:getMapCode())
        return
    end

    self:setJumpingMap(true)
    local moveList = {}
    for _, pathNode in ipairs(routeArray) do
        table.insert(moveList, {newM = pathNode.x, newN = pathNode.y})
    end
    local msgData = {
        m = m,
        n = n,
        moveList = moveList,
    }
    SocketManager:send(CCmd.FASHION_JUMP_POS, msgData)
end

--[[
    syncOneGrid:
        遥感操作走一格同步一格
]]

function Game:sendWholeMove(m, n, routeLocationArray, syncOneGrid)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    if self.jumpingMap then
        printError("=========> Game:sendWholeMove", m, n, gModule.Map:getMapCode())
        return
    end


    local mapData = gModule.Map:getMapData()
    -- 寻路移动
    local moveList = {}
    local startM = m
    local startN = n
    local needBreak = false
    local count = 0
    
    -- 裁剪路线发送给服务器
    for _, routeLocation in ipairs(routeLocationArray) do
        local targetM = routeLocation.x
        local targetN = routeLocation.y

        local newM = nil
        local newN = nil
        if startM == targetM then
            newM = targetM
        end
        if startN == targetN then
            newN = targetN
        end
        -- 遇到第一个block即停止
        local addOrSub = targetM > startM and 1 or -1
        for curM = startM + addOrSub, targetM, addOrSub do
            if mapData:isBlockMN(curM, startN) then
                needBreak = true
                break
            end

            newM = curM
            count = count + 1
            if count >= 3 and syncOneGrid then
                needBreak = true
                break
            end
        end

        local addOrSub = targetN > startN and 1 or -1
        for curN = startN + addOrSub, targetN, addOrSub do
            if mapData:isBlockMN(startM, curN) then
                needBreak = true
                break
            end

            newN = curN
            count = count + 1
            if count >= 3 and syncOneGrid then
                needBreak = true
                break
            end
        end

        if needBreak then
            if syncOneGrid then
                if newM == nil then
                    newM = targetM
                end
                if newN == nil then
                    newN = targetN
                end
            end
            if newM and newN then
                table.insert(moveList, {newM = newM, newN = newN})
            end
            break
        end
        startM = targetM
        startN = targetN

        table.insert(moveList, {newM = targetM, newN = targetN})
    end

    local msgData = {
        objId = self.hero:getObjId(),
        m = m,
        n = n,
        moveList = moveList,
    }
    SocketManager:send(CCmd.WHOLE_MOVE, msgData)
end

-- 是否正在跳地图
function Game:isJumpingMap()
    return self.jumpingMap
end

function Game:setJumpingMap(value)
    self.jumpingMap = value
end

function Game:sendWholeEnterSea(m, n, rideState)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    local msgData = {
        state = rideState,
        m = m,
        n = n,
        type = 1, --陆地到海
    }

    SocketManager:send(CCmd.SWITCH_TERRAIN_STATE, msgData)
end

function Game:sendWholeExitSea(m, n, rideState)
    if self.stageMode == STAGE_BATTLE or self.stageMode == STAGE_UNKNOW then
        return
    end

    local msgData = {
        state = rideState,
        m = m,
        n = n,
        type = 2,
    }

    SocketManager:send(CCmd.SWITCH_TERRAIN_STATE, msgData)
end

--  对外部模块api
function Game:getHero()
    return self.hero
end

function Game:hasHero()
    return self.hero ~= nil
end

function Game:getHeroGrade()
    local heroData = self:getHero()
    return gDao.Role:queryById(heroData:getLevel()):getGrade()
end

function Game:getHeroStage()
    local heroData = self:getHero()
    return gDao.Role:queryById(heroData:getLevel()):getStage()
end

function Game:getHeroDrugLimit()
    local metaDrugLimit = gDao.Role:queryById(self.hero:getLevel()):getDrugLimit()
    local totalNum = metaDrugLimit + gModule.Sponsor:getCurAddNumByType(gConst.Logic.SponsorType.DailyUseDrugNum)
    return totalNum
end

function Game:getAttrEvt(attr)
    return self.mapAttrEvt[attr]
end

function Game:execPrepareChangeStage()
    table.sort(self.execListPrepareChangeStage, function(a, b)
        return a.sortorder < b.sortorder
    end)
    for k, info in ipairs(self.execListPrepareChangeStage) do
        local callback = info.callback
        callback()
    end
    self.execListPrepareChangeStage = {}
    self.execListPrepareChangeStageTagSet = {}
end

function Game:addExecFunPrepareChangeStage(callback, sortorder, tag)
    if tag ~= nil then
        -- 回调方法标记，确保某些逻辑只会被执行一次
        if self.execListPrepareChangeStageTagSet[tag] then
            -- 本次逻辑中已经添加过了，目前找到问题，服务器死亡会触发2次
            return
        end
        self.execListPrepareChangeStageTagSet[tag] = true
    end

    self.execListPrepareChangeStage[#self.execListPrepareChangeStage + 1] = {callback = callback, sortorder = sortorder or 0}
end

function Game:execAfterChangeStage()
    local execList = {}
    for k, callback in ipairs(self.execListAfterChangeStage) do
        execList[#execList + 1] = callback
    end
    self.execListAfterChangeStage = {}

    for i, callback in ipairs(execList) do
        callback()
    end
end

function Game:addExecFunAfterChangeStage(callback)
    self.execListAfterChangeStage[#self.execListAfterChangeStage + 1] = callback
end

function Game:clearBossComingFlag()
    self:addExecFunAfterChangeStage(function()
        PrefsManager:setBool(gConst.Prefs.isPlayedBossComing, false)
    end)
end

-- 设置
function Game:setStageMode(stageMode)
    self.stageMode = stageMode
end

function Game:getStageMode()
    return self.stageMode
end

function Game:isWorld()
    return self.stageMode == STAGE_WORLD
end

function Game:isBattle()
    return self.stageMode == STAGE_BATTLE
end
    
function Game:isSect()
    return self.stageMode == STAGE_SECT
end

function Game:isCity()
    return self.stageMode == STAGE_CITY
end

function Game:isGuildLand()
    return self.stageMode == STAGE_GUILDLAND
end

-- 是否是内部地图
function Game:isInnerMap()
    if self:isSect() then 
        return true
    end

    if self:isCity() then 
        return true
    end

    if self:isGuildLand() then 
        return true
    end

    return false
end

function Game:isUnKnow()
    return self.stageMode == STAGE_UNKNOW
end

function Game:resetSwitchingTerrain()
    self.switchingTerrain = false
end

-- 确认是否进入海域 返回值用于状态机判断是否需要停止移动，如果需要弹窗或者切Progress态，就需要停止移动
function Game:confirmEnterSea(targetM, targetN)
    local boatUnlock = gModule.FuncUnlock:unlocked(gConst.FuncId.BOAT)
    local rideUnlock = gModule.FuncUnlock:unlocked(gConst.FuncId.RIDE_FLY)

    if not boatUnlock and not rideUnlock then
        gModule.FuncUnlock:unlockedPopNotice(gConst.FuncId.BOAT)
        return true
    end

    local canBoardBoat = gModule.Boat:canBoardBoat()
    local canMountRide = gModule.Ride:canMountRide()

    local key = gConst.Prefs.SWITCH_TERRAIN_TIP
    if PrefsManager:judegeIsToday(key) and (canBoardBoat or canMountRide) then
        if self.hero:isOnTheMount() then
            return false
        end

        if canBoardBoat then 
            self:enterSea(targetM, targetN, gConst.Logic.RideState.OnTheBoat)
            return true
        end

        if canMountRide then
            self:enterSea(targetM, targetN, gConst.Logic.RideState.OnTheMount)
            return true
        end
    end

    if self.switchingTerrain then
        return true
    end
    self.switchingTerrain = true

    UIManager:openWindow(lt.SwitchTerrainMsgWnd, {
        msg = "ENTER_SEA_AREA",
        toggleStr = LangsManager:getText("NO_TIPS_TODAY"),
        clicks = {
            Bind(self, self.enterSeaByBoardBoat, targetM, targetN),
            Bind(self, self.enterSeaByMountRide, targetM, targetN)
        },
        texts = {
            "GOTO_BY_BOAT",
            "GOTO_BY_MOUNT",
        },
        toggleChangeCallback = function(isOn)
            if isOn then
                PrefsManager:setBool(key, true, { isRecordTime = true })
            end
        end,
        noNeedClick = true,
        gray = (canBoardBoat and 0 or 1) | (canMountRide and 0 or 2)
    })
    EventManager:broadcast(GameEvent.MENU_STICK_OFF)

    return true
end

function Game:enterSeaByBoardBoat(targetM, targetN)
    local canBoardBoat = gModule.Boat:canBoardBoat()

    if not canBoardBoat then
        gModule.Boat:noticeCantBoardBoat()
        return 
    end

    self:enterSea(targetM, targetN, gConst.Logic.RideState.OnTheBoat)
end

function Game:enterSeaByMountRide(targetM, targetN)
    local canMountRide = gModule.Ride:canMountRide()

    if not canMountRide then
        gModule.Ride:noticeCantMountRide()
        return
    end
    if self.hero:isOnTheMount() then 
        self:sendWholeEnterSea(targetM, targetN, gConst.Logic.RideState.OnTheMount) 
    else
        self:enterSea(targetM, targetN, gConst.Logic.RideState.OnTheMount)
    end
end

function Game:autoEnterSea(seaM, seaN, targetM, targetN)
    if self.hero:isOnTheMount() then 
        return
    end

    local canBoardBoat = gModule.Boat:canBoardBoat()
    if canBoardBoat then 
        self:enterSeaHaveNavigate(seaM, seaN, targetM, targetN, gConst.Logic.RideState.OnTheBoat)

        return
    end

    local canMountRide = gModule.Ride:canMountRide()
    if canMountRide then
        self:enterSeaHaveNavigate(seaM, seaN, targetM, targetN, gConst.Logic.RideState.OnTheMount)

        return
    end

    -- 弹提示
    gModule.Boat:noticeCantBoardBoat()
end

-- 进入海域, 目标点坐标
function Game:enterSea(targetM, targetN, rideState)
    -- 读条时, 不要让玩家进行任何操作
    local selfAgent = AgentManager:getSelfAgent()
    if selfAgent then
        UIManager:maskOn()
        selfAgent:changeStateProgress(1, function()
            self:sendWholeEnterSea(targetM, targetN, rideState) 

            UIManager:maskOff()
        end, true)
    else
        printError("Unexpected ERROR Game:enterSea", selfAgent, targetM, targetN, rideState)
    end
end

-- 进入海域带有寻路
function Game:enterSeaHaveNavigate(seaM, seaN, targetM, targetN, rideState)
    -- 读条时, 不要让玩家进行任何操作
    local selfAgent = AgentManager:getSelfAgent()
    if selfAgent then
        UIManager:maskOn()
        -- 读条时间不能改成0, 改成0会导致记录的寻路数据被清楚
        selfAgent:changeStateProgress(1, function()
            self:sendWholeEnterSea(seaM, seaN, rideState) 

            if gModule.Navigate:navigateInCrossMap() then 
                local targetMapId = gModule.Navigate:getTargetMapId()
                gModule.Navigate:recordCrossMapDestIfHaveValue(targetMapId, targetM, targetN)
            elseif gModule.Navigate:navigateInCurMap() then 
                local targetMapId = gModule.Map:getMapCode()
                gModule.Navigate:recordCurMapDestIfHaveValue(targetMapId, targetM, targetN)
            end
            UIManager:maskOff()
        end, true)
    else
        printError("Unexpected ERROR Game:enterSea", selfAgent, targetM, targetN, rideState)
    end

end

-- 确认是否退出海域 返回值用于状态机判断是否需要停止移动，如果需要弹窗或者切Progress态，就需要停止移动
function Game:confirmExitSea(targetM, targetN)
    -- 非乘船状态, 不应该调用此接口
    if not self.hero:isOnTheBoat() then
        printError("非乘船状态, 不应该弹出退出海域确认框")
        return false
    end

    local key = gConst.Prefs.SWITCH_TERRAIN_TIP
    if PrefsManager:judegeIsToday(key) then
        self:exitSea(targetM, targetN)
        return true
    end

    if self.switchingTerrain then
        return true
    end
    self.switchingTerrain = true

    UIManager:openWindow(lt.SwitchTerrainMsgWnd, {
        msg = "EXIT_SEA_AREA",
        toggleStr = LangsManager:getText("NO_TIPS_TODAY"),
        clicks = {
            function()
                self:exitSea(targetM, targetN)
            end,
        },
        texts = {
            "BUTTON_EXIT",
        },
        toggleChangeCallback = function(isOn)
            if isOn then
                PrefsManager:setBool(key, true, { isRecordTime = true })
            end
        end,
        noNeedClick = true,
    })
    EventManager:broadcast(GameEvent.MENU_STICK_OFF)

    return true
end

-- 退出海域, 目标点坐标
function Game:exitSea(targetM, targetN)
    -- 读条时, 不要让玩家进行任何操作
    UIManager:maskOn()

    local selfAgent = AgentManager:getSelfAgent()
    selfAgent:changeStateProgress(1, function()
        self:sendWholeExitSea(targetM, targetN, gConst.Logic.RideState.None)

        UIManager:maskOff()
    end, true)
end

-- 退出海域带有寻路
function Game:exitSeaHaveNavigate(landM, landN, targetM, targetN)
    -- 读条时, 不要让玩家进行任何操作
    UIManager:maskOn()

    local selfAgent = AgentManager:getSelfAgent()
    selfAgent:changeStateProgress(1, function()
        self:sendWholeExitSea(landM, landN, gConst.Logic.RideState.None)
        if gModule.Navigate:navigateInCrossMap() then 
            local targetMapId = gModule.Navigate:getTargetMapId()
            gModule.Navigate:recordCrossMapDestIfHaveValue(targetMapId, targetM, targetN)
        elseif gModule.Navigate:navigateInCurMap() then 
            local targetMapId = gModule.Map:getMapCode()
            gModule.Navigate:recordCurMapDestIfHaveValue(targetMapId, targetM, targetN)
        end

        
        UIManager:maskOff()
    end, true)

end

function Game:isSwitchingTerrain()
    return self.switchingTerrain
end

-- 确认是否进入迷雾
function Game:confirmEnteFog(targetM, targetN)
    local mapId = gModule.Map:getMapCode()
    local mapData = gModule.Map:getMapData()
    if mapData ~= nil then
        -- 切图时mapData可能已经被置空
        local zoneId = mapData:getZonePid(targetM, targetN)
        UIManager:openWindow(lt.MapFogWnd, mapId, zoneId)
    else
        printMobile("Game:confirmEnteFog mapData == nil")
    end
    gModule.Navigate:reset()
end

function Game:reqGetBookExp(bookType)
    local msgData = {
        value1 = bookType
    }

    SocketManager:send(CCmd.BOOK_GET_EXP, msgData)
end

function Game:onGetExp(data)
    if Util.Funs:hasError(data) then
        return
    end

    EventManager:broadcast(GameEvent.BOOK_GET_EXP, data.value)
end

function Game:onUnKnowError(msg)
    if msg then 
        printError("======> 上报日志:"..msg)
    else
        printError("======> 上报日志")
    end
end

return Game