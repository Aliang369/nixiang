-- @Date    : 2022-05-25 20:29:27
-- @Author  : 
-- @Desc    : 角色自己Agent

local base = lt.BattleLivingAgent
local ClassBattleAgent = class("BattleSelfAgent", base)

local ATTACK_MODE_PICE = gConst.Game.ATTACK_MODE_PICE
local ATTACK_MODE_ALL = gConst.Game.ATTACK_MODE_ALL
local ATTACK_MODE_GUILD = gConst.Game.ATTACK_MODE_GUILD
local ATTACK_MODE_TEAM = gConst.Game.ATTACK_MODE_TEAM

local RELATION_INVALID = gConst.Game.RELATION_INVALID
local RELATION_HOSTILE = gConst.Game.RELATION_HOSTILE
local RELATION_FRIENDLY = gConst.Game.RELATION_FRIENDLY

local BattleAttr = gConst.Attr.Battle
local HeroAttr = gConst.Attr.Hero
local BaseAttr = gConst.Attr.Base
local Figure = gConst.Game.Figure
local ModulePowerSaving = gModule.PowerSaving
local WeaponPos = gConst.Item.WeaponPos

ClassBattleAgent.isTarget = false -- 标识是否自己的目标

ClassBattleAgent.agentType = gConst.Game.AGENT_HERO
ClassBattleAgent.heroData = nil -- 角色数据
ClassBattleAgent.heroActor = nil -- 角色actor
ClassBattleAgent.stateMachine = nil --状态机
ClassBattleAgent.curM = 0 --当前所在M格子
ClassBattleAgent.curN = 0 --当前所在N格子

ClassBattleAgent.actionId = gConst.Game.ACTION_INVALID_ID
ClassBattleAgent.faceDir = nil
ClassBattleAgent.faceDirection = nil
ClassBattleAgent.holdingWeapon = nil -- 背的武器特效

ClassBattleAgent.isOutOfGame = nil
ClassBattleAgent.movingFrame = false
ClassBattleAgent.relation = gConst.Game.RELATION_SELF
ClassBattleAgent.position = nil
ClassBattleAgent.location = nil
ClassBattleAgent.isBanMoveClient = false
ClassBattleAgent.isBanCastSkill = false


ClassBattleAgent.RefreshWeapon = "Art/Effect/Scene/Refresh_Weapon/Refresh_Weapon.prefab"
ClassBattleAgent.RefreshFiveElem = "Art/Effect/Scene/Refresh_FiveElem/Refresh_FiveElem.prefab"

ClassBattleAgent.envInnatSkillIdMap = nil -- 天赋SkillId Env映射表

function ClassBattleAgent:ctor(heroData, x, y, direction)
    base.ctor(self)

    self.position = Vector2.zero
    self.location = Vector2.zero
    self.faceDirection = Vector2.new(0, 0)
    self.envInnatSkillIdMap = {}
    self.cacheSkillData = {}
    self.changeFaceDirectionFuns = {}

    self.heroData = heroData

    self:updateRelation()

    self.heroActor = lt.BattleHeroActor.new(heroData)

    self.cmdQueue = lt.BattleCmdQueue.new(self)
    
    self.uiInfo = lt.BattleHeroUIInfo.new(heroData)
    self.uiInfo:setScale(0.01, 0.01, 1)

    self.canTurn = true

    for _, data in pairs(gModule.Game:getInnatEffectMap(self.heroData:getObjId())) do
        self:updateInnatEffect(data)
    end

    self.stateMachine = lt.BattleStateMachine.new(self)
    self.stateMachine:init(self:getInitAppearState())

    self.lockTarget = lt.BattleTargetLock.new()

    self:setPosition(x, y)

    self:setDirection(direction or Vector2Fast.down)

    AgentManager:addAgent(self)

    self:initAppearInfo()

    self:resetActorColor()
end

function ClassBattleAgent:onComplete()
    -- 初始死亡 人物隐藏
    if self:isStateDie() then
        self:onDie()
        self.heroActor:setPosition(-100, 0, 100)
    end
end

function ClassBattleAgent:dtor()
    AgentManager:removeAgent(self)

    self:resetActionHandle()

    self.stateMachine:del()
    self.stateMachine = nil

    self:clearHoldingWeapon()
    self:clearHoldingRide()
    self:clearEnvTable()

    self.lockTarget:del()
    self.lockTarget = nil

    if self:isSelfTarget() then
        AgentManager:getWorldAgent():removeTarget()
    end

    self:resetActorColor()
    self.heroActor:del()
    self.heroActor = nil


    self.cmdQueue:dtor()
    self.cmdQueue = nil

    self.uiInfo:del()
    self.uiInfo = nil
    self.heroData = nil
    
    if self.skillEyeBuff then
        self.skillEyeBuff:setOver()
        self.skillEyeBuff = nil
    end

    base.dtor(self)
end


function ClassBattleAgent:initAppearInfo()
    local figure = self.heroData:getFigure()
    self.hitPoint, self.height, self.buffScale, self.showDamagePoint = lt.WorldCommonUtil:calcBattleAgentAppearInfo(figure)
    self:clearHoldingWeapon()
    self:createHoldingRide()
    self:createHoldingWeapon()
    self:checkAlpha()
    self:onSkillEyeStatus()
end

function ClassBattleAgent:clearHoldingWeapon()
    self.heroActor:clearHoldingWeapon()
end

function ClassBattleAgent:clearHoldingRide()
    self.heroActor:clearHoldingRide()

    local skillId = nil
    if self.battleRideId and self.battleRideId > 0 then
        local skillInfo = gModule.Skill:getSkillMap()
        if self.battleRideId == self.heroData:getRideId() then
            -- 之前是主战Id，中断主战坐骑技能
            local skillData = skillInfo[WeaponPos.RideSkill_Zd]
            if skillData ~= nil then
                skillId = skillData:getSkillId()
                lt.SkillManager:handleStopSkill(self, skillId)
            end
        elseif self.battleRideId == self.heroData:getSubRideId() then
            -- 之前是助战Id，中断助战坐骑技能
            local skillData2 = skillInfo[WeaponPos.SubRideSkill_Zd]
            if skillData2 ~= nil then
                skillId = skillData2:getSkillId()
                lt.SkillManager:handleStopSkill(self, skillId)
            end
        end
        self.battleRideId = nil
    end
    
    self:clearRideEffect(skillId)
end

function ClassBattleAgent:createHoldingWeapon()
    -- print("createHoldingWeapon", self.__cname)
    -- 后面背武器
    self.heroActor:createHoldingWeapon()
end

function ClassBattleAgent:createHoldingRide()
    -- 记录当前正在使用的Id
    self.battleRideId = self.heroData:getBattleRideId()
    self.heroActor:createHoldingRide(self.heroData:getBattleRideId())
end

-- 大世界的朝向 同为人物面向
function ClassBattleAgent:setDirection(direction, force)
    self:setDirectionXY(direction.x, direction.y, force)
end

function ClassBattleAgent:setDirectionXY(x, y, force)
    -- print("rotatedVector setDirectionXY", self.__cname, x, y)
    -- 当朝向为0, 0 时, 人物方向不变
    self.direction:Set(x, y)

    -- 自动转向怪物中
    if self.autoTurnDirection then
        return
    end

    local faceDir = Util.World:directionToFaceDir(self.direction) 
    self:changeFaceDir(faceDir, force)

    self.heroActor:setDirection(self.direction)
end

-- 技能施法引发的技能转向
function ClassBattleAgent:setDirectionBySkill(direction)
    local faceDir = Util.World:directionToFaceDir(direction) 
    self:changeFaceDir(faceDir, force)

    self.heroActor:setDirection(direction)
end

-- 修改面向
function ClassBattleAgent:changeFaceDir(faceDir, force)
    if not self.faceDir or force or (self.faceDir ~= faceDir) then
        self.faceDir = faceDir
        self.faceDirection:Set(Util.World:fixTo4DirectionXY(self.direction))

        self.buffSystem:setDirection(self.faceDir)
        

        -- 需要让外部知道面向发向变化
        EventManager:broadcast(GameEvent.BATTLE_AGENT_DIRECTION, self:getAgentId())

        for _, callback in pairs(self.changeFaceDirectionFuns) do
            callback()
        end
    end
end

function ClassBattleAgent:getFaceDir()
    return self.faceDir
end

function ClassBattleAgent:getFaceDirection()
    return self.faceDirection:Clone()
end

function ClassBattleAgent:getCmdQueue()
    return self.cmdQueue
end

function ClassBattleAgent:pushCmd(cmd)
    if self.cmdQueue == nil then
        return
    end

    self.cmdQueue:push(cmd)
end

function ClassBattleAgent:popCmd()
    if self.cmdQueue == nil then
        return nil
    end

    return self.cmdQueue:pop()
end


function ClassBattleAgent:getCurState()
    return self.stateMachine:getCurState()
end

function ClassBattleAgent:changeState(newState)
    self.stateMachine:changeState(newState)
end

function ClassBattleAgent:isState(stateId)
    return self.stateMachine:isState(stateId)
end

function ClassBattleAgent:changeStateIdle(...)
    self:changeState(self:getStateIdle(...))
end

function ClassBattleAgent:isStateIdle()
    return self:isState(gConst.Game.STATE_ID_IDLE)
end


function ClassBattleAgent:changeStateMove(...)
    self:changeState(self:getStateMove(...))
end

function ClassBattleAgent:isStateMove()
    return self:isState(gConst.Game.STATE_ID_MOVE)
end

function ClassBattleAgent:onMoveStart()
    if self.heroActor then
        self.heroActor:onMoveStart()
    end
end

function ClassBattleAgent:onMoveStep()
    self:onLocation(true)
end

function ClassBattleAgent:onMoveEnd(forcedStop)
    if self.heroActor then
        self.heroActor:onMoveEnd()
    end
    if not self.navigateCallback then
        return
    end

    if forcedStop then
        self.navigateCallback = nil
        return
    end

    if gModule.Navigate:arriveNavDest() then
        self.navigateCallback()
    end
    self.navigateCallback = nil
end

function ClassBattleAgent:checkAlpha()
    local isFade = self.buffSystem:hasBuffEffect(gConst.Game.BUFF_FADE)
    local isAlpha = self:isAlpha()

    if isFade ~= isAlpha then
        if isFade then
            self:setAlpha()
        else
            self:resetAlpha()
        end
    end
end

function ClassBattleAgent:isAlpha()
    return self.heroActor:getFigureInst():isAlpha()
end

function ClassBattleAgent:setAlpha()
    --清除被击状态
    self:resetActorColor()
    self.heroActor:getFigureInst():setAlpha()
end

function ClassBattleAgent:resetAlpha()
    self.heroActor:getFigureInst():resetAlpha()
end

function ClassBattleAgent:setNavigateCallback(callback)
    self.navigateCallback = callback
end

function ClassBattleAgent:isValid()
    return self.heroData ~= nil
end
------------------------------------------ 属性相关引起Agent变化
-- 更新外观属性

-- 移速变化
function ClassBattleAgent:onCurMoveSpeed()
    if self:isStateMove() then
        self:getCurState():updateMoveSpeed()
    end
end

-- 更新
function ClassBattleAgent:onAttrChange(data, needUpdate)
    if needUpdate or needUpdate == nil then
        self.heroData:setAttrArray(data.objprops)
    end

    local properCode
    local mpChange = false
    local hpChange = false
    for k, v in ipairs(data.objprops) do
        properCode = v.properCode
        if properCode == HeroAttr.Healthy then 
            -- self.uiInfo:updateHealth()
        elseif properCode == HeroAttr.HpMax or properCode == HeroAttr.CurHp or properCode == HeroAttr.Shield then
            hpChange = true
        elseif properCode == HeroAttr.MpMax or properCode == HeroAttr.CurMp then
            mpChange = true
        elseif properCode == HeroAttr.RideHpMax or properCode == HeroAttr.RideCurHp then
            hpChange = true
        end
    end

    if hpChange then
        -- 血量变化
        self:hpChange()
    end

    if mpChange then
        self:mpChange()
    end
end

function ClassBattleAgent:onCurRatioDef()
    if self.buffSystem then
        self.buffSystem:onRatioDefChange()
    end
    if self.uiInfo then
        self.uiInfo:ratioDefChange(self.heroData)
    end
end
------------------------------------------

function ClassBattleAgent:getRelation()
    return self.relation
end

function ClassBattleAgent:enterMap()
end

function ClassBattleAgent:getRole()
    return self.heroData
end

function ClassBattleAgent:getObjId()
    if self.heroData == nil then
        return 0
    end
    return self.heroData:getObjId()
end

function ClassBattleAgent:getObjType()
    return gConst.Game.OBJ_HERO
end

function ClassBattleAgent:getDisplayName()
    if self.heroData == nil then
        return "Unknow"
    end
    return self.heroData:getDisplayName() 
end

function ClassBattleAgent:getModelBody()
    return 0.45
end

function ClassBattleAgent:getModelHead()
    return 0.9
end

function ClassBattleAgent:getFigureModelId()
    return self.heroData:getFigureModelId()
end

function ClassBattleAgent:isSelf()
    return true
end

function ClassBattleAgent:getMovingSpeed()
    if self.heroData == nil then
        return gConst.Game.DEFAULT_WORLD_MOVING_SPEED / 100
    end
    return self.heroData:getMovingSpeed() / 100
end

function ClassBattleAgent:getRoundPosition()
    return Vector2.new(self.position.x + (math.random() - 0.5) * 0.5, self.position.y + (math.random() - 0.5) * 0.5)
end

-- 指令, 返回值决定是否打断状态机后续 update流程
function ClassBattleAgent:handleCmd(cmd)
    local type = cmd:getType()
    if type ~= gConst.Game.CMD_HIT and Vector2.Distance(cmd:getPosition(), self:getPosition()) >= gConst.Game.MAX_DISTANCE_OFFSET and type ~= gConst.Game.CMD_ATTACK then
        -- 相对位置差别太大修正位置
        local pos = cmd:getPosition()
        self:setPosition(pos.x, pos.y)
    end

    if type == gConst.Game.CMD_IDLE then
        -- 待机指令
        self:setDirection(cmd:getDirection())
        self:changeStateIdle()
    elseif type == gConst.Game.CMD_MOVE then
        -- 移动指令
        self:changeStateMove(gConst.Game.MOVE_MODE_DEFAULT, cmd:getPosition() + cmd:getDirection() * 100)
    elseif type == gConst.Game.CMD_ATTACK then
        -- 攻击指令
        return self:handleAttackCmd(cmd)
    elseif type == gConst.Game.CMD_JUMP then
        -- 跳跃指令
        self:onJump(cmd:getData())
    elseif type == gConst.Game.CMD_CHARGE then
        -- 冲锋指令
        self:onCharge(cmd:getData())
    else
        printError("BattleSelfAgent:handleCmd 缺少对type:"..type.."的处理")
    end

    return true
end

function ClassBattleAgent:selfRideFigureChg(isAlive)
    
end

function ClassBattleAgent:changeStateDie()
    self:resetActionHandle()
    self:changeState(self:getStateDie())
end

function ClassBattleAgent:isStateDie()
    return self:isState(gConst.Game.STATE_ID_DIE)
end

-- 存活中
function ClassBattleAgent:isAlive()
    return not self:isStateDie()
end

-- 是否可攻击
function ClassBattleAgent:isAttackable()
    return not self:isStateDie()
end

-- 角色死亡
function ClassBattleAgent:onDie()
    self:clearHoldingWeapon()
    self:clearHoldingRide()
    self:clearAllSkillSound()
    self.uiInfo:onDie()
    EventManager:broadcast(GameEvent.HERO_DEAD_OPEN_RELIVE)

end

-- 角色复活
function ClassBattleAgent:onRevive()
    self.uiInfo:onRevive()
    self.heroActor:setPosition(self.position.x, 0, -self.position.y)
    self:hpChange()
    self:createHoldingRide()
    self:createHoldingWeapon()
end

function ClassBattleAgent:hpChange()
    local heroData = gModule.Game:getHero()
    local maxHp, curHp
    local shield = heroData:getShield()
    local isShowRideHp = self.heroData:isBattleOnTheMount()
    if isShowRideHp then 
        self.uiInfo:rideHpChange(self.heroData)
        maxHp = heroData:getRideHpMax()
        curHp = heroData:getRideCurHp()
        EventManager:broadcast(GameEvent.RIDE_CURHP_CHG)
    else
        self.uiInfo:hpChange(self.heroData)

        maxHp = heroData:getHpMax()
        curHp = heroData:getCurHp()
        if self:isSelfTarget() then
            -- 目标血量变化
            EventManager:broadcast(GameEvent.WORLD_UPDATE_SELECT_ROLE, self.heroData)
        end
        EventManager:broadcast(GameEvent.HERO_CURHP_CHG)
    end

    -- 血量变化更新组队ui血量
    EventManager:broadcast(GameEvent.TEAM_REFRESH_HP, self.heroData:getDisplayName(), curHp, maxHp, isShowRideHp, shield)

end

function ClassBattleAgent:mpChange()
    -- self.uiInfo:mpChange(self.heroData)

    EventManager:broadcast(GameEvent.HERO_CURMP_CHG)
end

function ClassBattleAgent:hitColorChange()
    --隐身中不触发受击效果
    if self:isFadeState() then
        return
    end
    self.setHitColorArray = {}
    self.hitElapse = 0
    if self.heroActor then
        self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Blend)
        self.heroActor:colorChange(gConst.Color.BattleHeroHit)
    end
    self.hitRecoverTime = gConst.Game.BattleColorRecoverTime
end

function ClassBattleAgent:resetActorColor()
    if self.heroActor then
        self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Default)
        self.heroActor:colorChange(gConst.Color.ModelDefaultColor)
    end
    self.hitElapse = nil
end

-- 战斗飘字, 他人造成的伤害
function ClassBattleAgent:popFloatWord(value, flag, damageDir, attackAgent)
    if gModule.PowerSaving:isOpen() then
        return
    end
    
    local height = self.height + gConst.Game.BATTLE_SELF_DAMAGE_OFFY
    local randomOffx = math.random(-10, 10) / 20
    local position = Vector2.new(randomOffx + self:getPosition().x, self:getPosition().y)
    
    if value > 0 then
        -- 回血
        lt.BattleElementManager:popOtherHealSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        return
    end

    if value == 0 then
        -- 闪避
        if Util.World:isDodge(flag) then
            lt.BattleElementManager:popOtherDodgeSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        end

        return
    end

    -- 伤害
    
    if Util.World:isSolve(flag) then
        -- 化解
        lt.BattleElementManager:popOtherSolveSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isCrit(flag) then
        -- 暴击
        local offsetX, offsetY = Util.Funs:getCritOffset(self, attackAgent, height)
        height = offsetY
        position = Vector2.new(offsetX + self:getPosition().x, self:getPosition().y)
        lt.BattleElementManager:popOtherCritSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isLuckyStrike(flag) then
        -- 会心
        lt.BattleElementManager:popOtherLuckyStrikeSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    else
        -- 普通伤害
        height = self.height * 3 / 4
        -- position = self:getPosition()
        lt.BattleElementManager:popHeroDamage(position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    end
end

function ClassBattleAgent:popSystemFloatWord(value, flag, damageDir)
    if gModule.PowerSaving:isOpen() then
        return
    end

    local height = self.height + gConst.Game.BATTLE_OTHER_DAMAGE_OFFY
    local position = self:getPosition()
    
    if value > 0 then
        -- 回血
        lt.BattleElementManager:popSystemHealOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        return
    end

    if value == 0 then
        -- 闪避
        if Util.World:isDodge(flag) then
            lt.BattleElementManager:popSystemDodgeOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        end

        return
    end

    -- 伤害
    
    if Util.World:isSolve(flag) then
        -- 化解
        lt.BattleElementManager:popSystemSolveOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isCrit(flag) then
        -- 暴击
        lt.BattleElementManager:popSystemCritOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isLuckyStrike(flag) then
        -- 会心
        lt.BattleElementManager:popSystemLuckyStrikeOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    else
        -- 普通伤害
        lt.BattleElementManager:popHeroDamage(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    end
end

function ClassBattleAgent:popSelfFloatWord(value, flag, damageDir, attackAgent)
    if gModule.PowerSaving:isOpen() then
        return
    end

    local height = self.height + gConst.Game.BATTLE_SELF_DAMAGE_OFFY
    local randomOffx = math.random(-10, 10) / 20
    local position = Vector2.new(randomOffx + self:getPosition().x, self:getPosition().y)

    if value > 0 then
        -- 回血
        lt.BattleElementManager:popSelfHealSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        return
    end

    if value == 0 then
        -- 闪避
        if Util.World:isDodge(flag) then
            lt.BattleElementManager:popSelfDodgeSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        end

        return
    end

    -- 伤害
    if Util.World:isSolve(flag) then
        -- 化解
        lt.BattleElementManager:popSelfSolveSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isCrit(flag) then
        -- 暴击
        local offsetX, offsetY = Util.Funs:getCritOffset(self, attackAgent, height)
        height = offsetY
        position = Vector2.new(offsetX + self:getPosition().x, self:getPosition().y)
        lt.BattleElementManager:popSelfCritSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isLuckyStrike(flag) then
        -- 会心
        lt.BattleElementManager:popSelfLuckyStrikeSelf(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    else
        -- 普通伤害
        height = self.height * 3 / 4
        position = self:getPosition()
        lt.BattleElementManager:popHeroDamage(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    end
end


function ClassBattleAgent:canMove()
    return self.heroData:canMove() and not self.buffSystem:banMove() and not self.jumping and not self.isBanMoveClient
end

function ClassBattleAgent:canCastSkill()
    return not self.isBanCastSkill
end

function ClassBattleAgent:isBlooding()
    return false
end

function ClassBattleAgent:setSettlemented(isEnd)
    self.isBanMoveClient = isEnd
end

function ClassBattleAgent:resetOnSameMap()
    self:setSettlemented(false)
    self:readyToFlash(false)
end

function ClassBattleAgent:setBossComming(value)
    self.isBanMoveClient = value
    self.isBanCastSkill = value
end

-- 正在跳跃位移中
function ClassBattleAgent:setJumping(jumping)
    self.jumping = jumping
end


function ClassBattleAgent:setSpecialData(data)
    self.specialData = data
end

function ClassBattleAgent:getSpecialData()
    return self.specialData
end

function ClassBattleAgent:onJump(data)
    if data.skillId then
        -- 技能瞬移
        data.specialId = gConst.Skill.SPECIAL_JUMP
        lt.SkillManager:handleSpecial(self, data.skillId, data)
    else
        -- 其他瞬移
        self:setPosition(data.x, data.y)
        self:setDirectionXY(data.directionX, data.directionY)
        self:changeStateIdle()
    end
end

function ClassBattleAgent:onReset(data)
    -- 重置到目标为止
    self:setPosition(data.x, data.y)
    self:setDirectionXY(data.directionX, data.directionY)
    self:changeStateIdle()
    -- 清除Buff
    self:clearBuff()
end

function ClassBattleAgent:onCharge(data)
    if not self:isStateSkill() then
        printError("怪物不在技能态, 服务器下发怪物冲锋")
        return
    end

    data.specialId = gConst.Skill.SPECIAL_CHARGE
    data.skillId = self:getStateSkillId()

    self:setDirectionXY(data.directionX, data.directionY)

    lt.SkillManager:handleSpecial(self, data.skillId, data, self:getCurState())
end

function ClassBattleAgent:getInitAppearState()
    -- 出现时为死亡状态
    if self.heroData:isDead() then
        return self:getStateDie()
    end

    return self:getStateIdle()
end

function ClassBattleAgent:getBuffSystem()
    return self.buffSystem
end

-- 受击点
function ClassBattleAgent:getHitPoint()
    return self.hitPoint
end

--模型高度
function ClassBattleAgent:getHeight() 
    return self.height  
end

--buff的特效缩放
function ClassBattleAgent:getBuffScale()
    return self.buffScale
end

function ClassBattleAgent:getMidHeight()
    return self.height / 2 
end

function ClassBattleAgent:setAsTarget(isTarget)
    self.isTarget = isTarget
end

function ClassBattleAgent:isSelfTarget()
    return self.isTarget
end

-- 无法作为目标
function ClassBattleAgent:cantBeAsTarget()
    return self.heroData:isDead()
end

function ClassBattleAgent:canBeAsTarget()
    return not self:cantBeAsTarget()
end

------------------------------------------ 属性相关引起Agent变化
function ClassBattleAgent:onStatus()
    if self:isStateDie() and self.heroData:isAlive() then
        -- 复活
        self:changeStateIdle()
        EventManager:broadcast(GameEvent.BATTLE_SELF_RELIVE)
    elseif not self.heroData:isAlive() and self:isAlive() then
        self:onDie()
    end
end

-- 设置所在的格子
function ClassBattleAgent:setMN(m, n, updatePosition)
    if m <= 0 or n <= 0 then
        print("BattleSelfAgent:setMN less then zero")
    end

    if updatePosition then
        -- 取单元格的中心点为人物所在的格子
        self:setPosition(m + 0.5, n + 0.5)
    else
        self.curM = math.floor(m)
        self.curN = math.floor(n)
    end
end

function ClassBattleAgent:updateFigure(figure)    
    self.heroActor:updateFigure(figure)
end

function ClassBattleAgent:onFigureDirty()
    self.figureDirty = true
end

function ClassBattleAgent:onGuildInfoChg()
    local mapMeta = gDao.Map:queryById(gModule.Map:getMapCode())
    local attackMode = mapMeta:getAttackMode()
    local mapType = mapMeta:getMapType()
    if mapType == gConst.Game.STAGE_BATTLE and attackMode == gConst.Game.ATTACK_MODE_GUILD then
        -- self.uiInfo:onGuildInfoChg() 
    end
end

function ClassBattleAgent:onLvChange()
    -- self.uiInfo:onLvChange(self:getRole())
end


function ClassBattleAgent:onAppearanceChange(data)
    self.heroData:setAttrArray(data.objprops)

    -- 需要配置属性变化的回调
    local callback
    for k,v in ipairs(data.objprops) do
        callback = self.attrCallback[v.properCode]
        if callback then
            callback(self)
        end
    end

    if self.figureDirty then
        self.heroData:updateFigure()
        self:updateFigure(self.heroData:getFigure())
        self.figureDirty = false
    end
end

function ClassBattleAgent:onSkillEyeStatus()
    if self.heroData:getSkillEyeStatus() == 1 then
        if self.skillEyeBuff == nil then
            local prefabsPath = lt.SkillFuncPrefabsPath:getSkillPrefabsPathByIndex(23201)
            local battleEnvSystem = AgentManager:getWorldAgent():getBattleEnvSystem()
            self.skillEyeBuff = battleEnvSystem:createLoopEnv(prefabsPath.attackPrefab, Vector2Fast.zero, Vector2Fast.down,{
                parent = self
            })
        end
    else
        if self.skillEyeBuff then
            self.skillEyeBuff:setOver()
            self.skillEyeBuff = nil
        end
    end
end

function ClassBattleAgent:onEquipSelfMagicShowChg()
    self:clearHoldingWeapon()
    self:createHoldingWeapon()
end

function ClassBattleAgent:onRideStatusChg()
    if self.heroData:isBattleOnTheMount() then
        self.uiInfo:setPosition(self.position.x, gConst.Game.BATTLE_UI_SELF_INFO_HEIGHT + 0.5, -self.position.y)
        self:createHoldingRide()
        self.heroActor:setPosition(self.position.x, 0, -self.position.y)
    else
        self:clearHoldingRide()
        self.uiInfo:setPosition(self.position.x, gConst.Game.BATTLE_UI_SELF_INFO_HEIGHT, -self.position.y)
        self.heroActor:setPosition(self.position.x, 0, -self.position.y)
    end
    EventManager:broadcast(GameEvent.HERO_BATTLE_RIDE_STAE_CHG)
    self:hpChange()

    if self:isStateIdle() then
        self:actionIdle()
    elseif self:isStateMove() then 
        self:actionRun()
    end
end

function ClassBattleAgent:onHeroTitleChg()
    self.uiInfo:onCurHeroTitleChg()
end

-- 更新关系
function ClassBattleAgent:updateRelation()
    self.relation = RELATION_SELF
end

function ClassBattleAgent:onUpdate()
    -- 生物数据为空时, 未出现或者消失了不执行update流程
    -- if self.heroData == nil then
    --     return
    -- end

    self.stateMachine:onUpdate()

    self.uiInfo:onUpdate()

    if ModulePowerSaving:isOpen() then
        return
    end

    self.heroActor:onUpdate()

    if self.hitElapse then
        if self.heroActor:getFigureInst():isInst() then
            self.hitElapse = self.hitElapse + Time.deltaTime
            if self.hitElapse >= self.hitRecoverTime then
                self:resetActorColor()
            end
        end
    end

    if self.actionDirty then
        self:innerAction()
    end
    self.buffSystem:onUpdate()

    if not table.isEmpty(self.skillSoundHandelMap) then 
        for k, skillSoundHandel in pairs(self.skillSoundHandelMap) do
            if skillSoundHandel.isPlayOver then
                self.skillSoundHandelMap[k] = nil
            end
        end
    end
end

-- 状态
function ClassBattleAgent:getStateIdle(...)
    return lt.BattleSelfIdleState.new(self, ...)
end

function ClassBattleAgent:getStateMove(...)
    return lt.BattleSelfMoveState.new(self, ...)
end

function ClassBattleAgent:getStateDie()
    return lt.BattleSelfDieState.new(self)
end

function ClassBattleAgent:setBuffList(buffList)
    self.heroData:setBuffList(buffList)
    self.buffSystem:updateBuff()

    if self:isSelfTarget() then
        -- 目标buff变化
        EventManager:broadcast(GameEvent.TARGET_BUFF_CHG)        
    end

    local showNameBuff = self.buffSystem:hasShowNameBuff()
    -- self.uiInfo:showBuffName(showNameBuff)
end

function ClassBattleAgent:updateBuff(buffData)
    self.heroData:addBuffList(buffData.addBuffArray)
    self.heroData:removeBuffList(buffData.delBuffArray)
    self.buffSystem:updateBuff()
    
    if self:isSelfTarget() then
        -- 目标buff变化
        EventManager:broadcast(GameEvent.TARGET_BUFF_CHG)        
    end

    local showNameBuff = self.buffSystem:hasShowNameBuff()
    -- self.uiInfo:showBuffName(showNameBuff)

end

function ClassBattleAgent:clearBuff()
    self:setBuffList({})
end

function ClassBattleAgent:updateInnatEffect(data)
    self:clearInnatEffect(data.id)
    lt.SkillManager:handleExecInnatSkill(self, data.id, data)
end

-- 释放技能
local RELATION_SELF = gConst.Game.RELATION_SELF
function ClassBattleAgent:updateCastFunStep(stepData)
    local skillId = stepData.skillId
    lt.SkillManager:removeWait(skillId)

    if stepData.step ~= gConst.Skill.STEP_END then
        printError(string.format("服务器错误下发技能阶段数据 skill: %d step : %d", skillId, stepData.step))
        return 
    end

    lt.SkillManager:handleExec(self, skillId, stepData)

    --是否是new出来的新技能--new出来的不占用原来的cd和状态 0不是 1是
    if stepData.isNewSkill and stepData.isNewSkill == 1 then
        --被动触发的技能
    else
        -- stepData.count不为0时, 代表处于技能多次释放状态, 此时不需要更新技能的CD
        if stepData.count == 0 then
            lt.SkillManager:updateCD(skillId, stepData.nextTime)
        end
    end
end


-- 执行技能物
function ClassBattleAgent:updateCastFunDuration(stepData)
    local skillId = stepData.skillId

    if stepData.step ~= gConst.Skill.STEP_END then
        printError(string.format("服务器错误下发技能阶段数据 skill: %d step : %d", skillId, stepData.step))
        return 
    end

    lt.SkillManager:handleExecDuration(self, skillId, stepData)
end

-- 人物处理攻击指令
function ClassBattleAgent:handleAttackCmd(cmd)
    self:updateCastFunDuration(cmd:getData())

    -- 返回false不打断状态机流程 支持移动中施法 
    return false
end

function ClassBattleAgent:onHpMpChange(attackerId, data)
    -- 更新人物血量数据 如果蓝量变化 curHp 和 maxHp 都为0, curHp为0时代表死亡 需要通过maxHp字段更新血量
    if Util.World:isRide(data.flag) then
        if data.maxHP > 0 then
            self.heroData:setRideCurHp(data.curHP)
            self.heroData:setRideHpMax(data.maxHP)
    
            self:hpChange()
        end
    else
        if data.maxHP > 0 then
            self.heroData:setCurHp(data.curHP)
            self.heroData:setHpMax(data.maxHP)
    
            self:hpChange()
        end
    
        if data.maxMP > 0 then
            self.heroData:setCurMp(data.curMP)
            self.heroData:setMpMax(data.maxMP)
    
            self:mpChange()
        end        
    end


    if Util.World:isCost(data.flag) then
        -- 技能消耗的血蓝, 无受击表现及飘字
        return
    end

    --断血特效
    if data.bloodBroken then
        self:ShowBreakHpBarEffect()
    end

    if attackerId <= 0 then
        -- 来自buff和system伤害, 没有攻击者信息, 只显示伤害飘字
        self:popSystemFloatWord(data.hpChange, data.flag, data.damageDir)
        return
    end

    local attackAgent = AgentManager:getWorldAgent():queryAgentByObjId(data.attackerId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_VISION
    )
    if not attackAgent then
        return
    end

    if data.skillId > 0 then
        -- 技能引发的伤害, 触发对象的受击效果
        lt.SkillManager:handleHurt(self, attackAgent, data.skillId, data.hpChange, data.flag, data.targetPos, data.hurtParam)
    end

    if attackAgent:getRelation() == gConst.Game.RELATION_SELF then
        -- 玩家自己产生的伤害
        self:popSelfFloatWord(data.hpChange, data.flag, data.damageDir, attackAgent)
    else
        -- 受击变白
        self:hitColorChange()
        -- 正常变化
        self:popFloatWord(data.hpChange, data.flag, data.damageDir, attackAgent)
    end
end

-- 设置Agent位置
function ClassBattleAgent:setPosition(x, y)
    -- printMobile("setPosition", self.__cname, x, y)
    if type(x) == "table" then
        printError("BattleSelfAgent:setPosition", x, y)
    end
    if gConst.Game.SHOW_POSITION then
        self.uiInfo:setAddtionString(string.format("%.2f_%.2f", x, y))
    else
        --self.uiInfo:setNoneAddtionString()
    end
    
    self.curM = math.floor(x)
    self.curN = math.floor(y)

    self.position:Set(x, y)

    self.heroActor:setPosition(x, 0, -y)
    self.buffSystem:setLocalPosition(self.position)
    if self.heroData:isBattleOnTheMount() then
        self.uiInfo:setPosition(x, gConst.Game.BATTLE_UI_SELF_INFO_HEIGHT + 0.5, -y)  
    else
        self.uiInfo:setPosition(x, gConst.Game.BATTLE_UI_SELF_INFO_HEIGHT, -y)
    end
    EventManager:broadcast(GameEvent.CHANGE_SELF_AGENT, self.position)
end

function ClassBattleAgent:flashToPosition(x, y, directionX, directionY)
    self.readyToFlashFlag = false
    self:setPosition(x, y)
    self.heroActor:refreshPosition()
    self:setDirectionXY(directionX, directionY)
    if self:isStateMove() then
        self:getCurState():updateNewPosition()
    end
end

function ClassBattleAgent:readyToFlash(value)
    self.readyToFlashFlag = value
end

function ClassBattleAgent:canChangeDirection()
    return not self.readyToFlashFlag
end

-- 施法时能否进行转向
function ClassBattleAgent:setCanTurn(value)
    self.canTurn = value
end

-- 施法时能否进行转向
function ClassBattleAgent:getCanTurn()
    return self.canTurn
end

function ClassBattleAgent:getPosition()
    return self.position:Clone()
end

function ClassBattleAgent:getPositionXY()
    return self.position:Get()
end

function ClassBattleAgent:getLocation()
    return self.location
end

function ClassBattleAgent:getLocationMN()
    return self.location:Get()
end

function ClassBattleAgent:getMN()
    return self.curM, self.curN
end

function ClassBattleAgent:getDirection()
    return self.direction:Clone()
end

function ClassBattleAgent:getDirectionXY()
    return self.direction:Get()
end

function ClassBattleAgent:getActor()
    return self.heroActor
end

function ClassBattleAgent:innerAction()
    if self.actionId == gConst.Game.ACTION_INVALID_ID then
        return
    end

    if self.active == false then
        return
    end

    self.heroActor:action(self.actionId, self.faceDir, self.actionOffset)
    self.actionDirty = false
end

function ClassBattleAgent:actionIdle()
    self.buffSystem:roleIdle()
    if self.heroActor then
        self.heroActor:actionIdle()
    end
end

function ClassBattleAgent:actionRun()
    self.buffSystem:roleRun()
    self.heroActor:actionRun()
end

function ClassBattleAgent:actionDie()
    self.buffSystem:roleDie()
    self.heroActor:actionDie()
end

--[[ 
角色技能, 播放技能出手动作
    在播放出手动作时, 客户端可能朝技能的施法方向进行转向, 或者施法地点进行转向
    由配置表决定能否自动转向, 部分技能特效播放期间, 不允许转向, 会设置agent当前能否进行自动转向
]]
function ClassBattleAgent:actionSkillChant(skillId, stepData)
    -- 自动转向
    if self:getCanTurn() then

        local skillMeta = gModule.Skill:combineQueryById(skillId)

        if skillMeta:getCanTurn() then
            local turnDir

            if #stepData.targetDirs > 0 then
                turnDir = stepData.targetDirs[1]

            elseif #stepData.targetPoss > 0 then
                turnDir = stepData.targetPoss[1] - self:getPosition().normalized
            end

            if turnDir then
                -- 修改角色的面向
                self:setDirectionBySkill(turnDir)
                self.autoTurnDirection = true
            end

            if self.hasLockTarget then
                self.heroActor:lookAtTarget(self.lockTarget:getTransform())
                self.autoTurnDirection = true
            end
        end
    end

    if self:isOnTheMountAndAlive() then
        self.skillOffset = gConst.Logic.DefaultSkillChant
    else
        self.skillOffset = gConst.Game:randomSkillOffset(self.skillOffset, gConst.Game.HERO_SKILL_CHANT_COUNT)
    end
    -- self.skillOffset = gConst.Logic.DefaultSkillChant--gConst.Game:randomSkillOffset(self.skillOffset, gConst.Game.HERO_SKILL_CHANT_COUNT)

    self.heroActor:actionSkillChant(self.skillOffset)
end

function ClassBattleAgent:getSkillOffset()
    return self.skillOffset
end

function ClassBattleAgent:onActionSkillChantEnd()
    if self.autoTurnDirection then
        -- 注意: self.autoTurnDirection需要先设置为false, 否则 setDirectionXY不生效
        self:cancelAutoTurnDirection()
    end
    -- 由于C#层控制了锁定目标, 此时lua层的朝向此时不准确, 需要进行一次同步
    self:setDirectionXY(self.heroActor:getRotateDirectionXY())
    self:getCurState():actionCurAction()
end

-- 取消自动转向
function ClassBattleAgent:cancelAutoTurnDirection()
    if self.autoTurnDirection then
        self.autoTurnDirection = false

        self.heroActor:clearLookAtTarget()
    end
end

function ClassBattleAgent:resetActionHandle()
    if self.actionHandler then
        Schedule:removeUpdate(self.actionHandler)

        self.actionHandler = nil
    end
end

function ClassBattleAgent:castSkill(skillId, stepData)
    if self.heroData == nil then
        return
    end

    if self.heroData:isDead() then
        return
    end

    local state = self:getCurState()
    if not state.castSkill then 
        print("=========> 状态", state:getStateId())
    end

    -- 播放施法动作
    self:actionSkillChant(skillId, stepData)
    
    self:resetActionHandle()
    self.actionHandler = Schedule:addDelay(gConst.Game:getSkillOffsetTime(self), function()
        self:resetActionHandle()

        self:onActionSkillChantEnd()
    end)
end

function ClassBattleAgent:onLocation(isMove)
    self:setLocationMN(math.floor(self.position.x), math.floor(self.position.y))

    if self.heroData == nil then
        return
    end

    -- 处理地图地块显示
    AgentManager:getWorldAgent():onLocation()

    -- 更新定位
    EventManager:broadcast(GameEvent.WORLD_LOCATION_UPDATE, self.location)
end

function ClassBattleAgent:setLocation(location, updatePosition)
    self:setLocationMN(location.x, location.y, updatePosition)
end

function ClassBattleAgent:setLocationMN(m, n, updatePosition)
    self.location:Set(m, n)

    if updatePosition then
        self:setPosition(m + 0.5, n + 0.5)
    end
end

function ClassBattleAgent:getTransform()
    return self.heroActor:getTransform()
end

function ClassBattleAgent:getEnvTransform()
    return self.heroActor:getEnvTransform()
end

function ClassBattleAgent:getBuffFollowTransform()
    return self.heroActor:getBuffFollowTransform()
end

function ClassBattleAgent:getRotateTransform()
    return self.heroActor:getRotateTransform()
end

function ClassBattleAgent:popLookNode()
    if not self.lookNodePool then
        self.lookNodePool = {}
    end

    if #self.lookNodePool > 0 then
        local lookNode = self.lookNodePool[#self.lookNodePool]
        self.lookNodePool[#self.lookNodePool] = nil
        return lookNode
    else
        return lt.BattleLookNode.new(self)
    end
end

function ClassBattleAgent:pushLookNode(lookNode)
    self.lookNodePool[#self.lookNodePool + 1] = lookNode
end

function ClassBattleAgent:getUIActor()
    return self.uiInfo
end

function ClassBattleAgent:setLockTargetPosition(x, y)
    self.hasLockTarget = true
    self.lockTarget:setPosition(x, 0, -y)
end

function ClassBattleAgent:hideLockTarget()
    self.hasLockTarget = false
    self.heroActor:clearLookAtTarget()
    self.lockTarget:setPosition(-10, 0, -10)
end

function ClassBattleAgent:setActiveLockTarget(active)
    self.lockTarget:setActive(active)
end

function ClassBattleAgent:setLockTargetScale(x, y, z)
    self.lockTarget:setScale(x, y, z)
end

function ClassBattleAgent:getlockTarget()
    return self.lockTarget
end

function ClassBattleAgent:playFiveElementLvUp()
    local posX, posY = self:getPosition()
    local envSystem = AgentManager:getWorldAgent():getBattleEnvSystem()
    envSystem:createOnceEnv(self:getEnvShowOrder(), self.RefreshFiveElem, Vector2Fast.zero, Vector2Fast.up, {eulerAngleX = 45, parent = self:getActor().transform})
end

function ClassBattleAgent:playTechLvUp()
    local posX, posY = self:getPosition()
    local envSystem = AgentManager:getWorldAgent():getBattleEnvSystem()
    envSystem:createOnceEnv(self:getEnvShowOrder(), self.RefreshWeapon, Vector2Fast.zero, Vector2Fast.up, {eulerAngleX = 45, parent = self:getActor().transform})
end

function ClassBattleAgent:execDie()
    self:changeStateDie()
    self:onDieCallback()
    EventManager:broadcast(GameEvent.BATTLE_SELF_DIE)
end

function ClassBattleAgent:hasBuffByBuffType(buffType)
    return self.buffSystem:hasBuffByBuffType(buffType)
end

function ClassBattleAgent:getBuffLevelByBuffType(buffType)
    return self.buffSystem:getBuffLevelByBuffType(buffType)
end

function ClassBattleAgent:refreshHeroData(heroData)
    self.heroData = heroData
    if self.heroActor then
        self.heroActor:refreshHeroData(heroData)
    end
end

--[[
    记录释放技能过程, 需要临时缓存的数据

    注意 : 瞬发的技能(放完技能后续特效不要之前的数据)不应该存调用此接口, 此接口是为了一些持续技能后续播放特效需要一些数据,
    所以把需要的数据存到agent中.(数据不能存到模板中因为模板是公用的数据会错乱)

    调用此接口技能结束时要清楚缓存的数据
--]]
function ClassBattleAgent:setCacheSkillData(skillId, key, value)
    if not self.cacheSkillData then
        self.cacheSkillData = {}
    end

    if not self.cacheSkillData[skillId] then
        self.cacheSkillData[skillId] = {}
    end

    self.cacheSkillData[skillId][key] = value
end

-- 返回技能过程, 需要临时缓存的数据
function ClassBattleAgent:getCacheSkillData(skillId, key)
    if not self.cacheSkillData then
        return nil
    end

    if not self.cacheSkillData[skillId] then
        return nil
    end

    return self.cacheSkillData[skillId][key]
end

function ClassBattleAgent:clearCacheById(skillId)
    self.cacheSkillData[skillId] = nil
end

function ClassBattleAgent:registerChangeFaceDirection(key, callback)
    if not self.changeFaceDirectionFuns then
        self.changeFaceDirectionFuns = {}
    end

    self.changeFaceDirectionFuns[key] = callback
end

function ClassBattleAgent:unregisterChangeFaceDirection(key)
    self.changeFaceDirectionFuns[key] = nil
end

function ClassBattleAgent:canShake()
    return false
end

function ClassBattleAgent:getEnvShowOrder()
    return gConst.Game.EnvNeedShow.Order10
end

function ClassBattleAgent:bindEffectWithInnatSkillId(env, skillId)
    if skillId then
        if not self.envInnatSkillIdMap[skillId] then
            self.envInnatSkillIdMap[skillId] = {}
        end
        local list = self.envInnatSkillIdMap[skillId]
        list[#list + 1] = env
    end
end

function ClassBattleAgent:clearInnatEffect(innatSkillId)
    if not innatSkillId then
        return
    end
    local envTbl = self.envInnatSkillIdMap[innatSkillId]
    if not envTbl then
        return
    end
    for k, v in pairs(envTbl) do
        if v == env then
            envTbl[k] = nil
            break
        end
    end
end

function ClassBattleAgent:clearEffect(env, skillId, innatSkillId)
    base.clearEffect(self, env, skillId)
    self:clearInnatEffect(innatSkillId)
end

function ClassBattleAgent:clearEnvTable()
    base.clearEnvTable(self)
    self.envInnatSkillIdMap = {}
end

function ClassBattleAgent:onDieCallback()
    for k1, effetList in pairs(self.envInnatSkillIdMap) do
        for k2, v in pairs(effetList) do
            if v.longEffect or v.needClear then
                effetList[k2] = nil
            end
        end
    end
    print("=========> 死亡")
    base.onDieCallback(self)
end

function ClassBattleAgent:getSex()
    return self.heroActor:getSex()
end

function ClassBattleAgent:isOnTheMountAndAlive()
    return self.heroData:isBattleOnTheMount()
end

-- 配置回调
ClassBattleAgent.attrCallback = {
    [BattleAttr.CurMoveSpeed] = ClassBattleAgent.onCurMoveSpeed,
    [BaseAttr.Eye] = ClassBattleAgent.onFigureDirty,
    [BaseAttr.HairId] = ClassBattleAgent.onFigureDirty,
    [BaseAttr.BodyId] = ClassBattleAgent.onFigureDirty,
    [BaseAttr.ModelId] = ClassBattleAgent.onFigureDirty,
    [HeroAttr.GuildName] = ClassBattleAgent.onGuildInfoChg,
    [HeroAttr.GuildOfficeId] = ClassBattleAgent.onGuildInfoChg,
    [BaseAttr.Level] = ClassBattleAgent.onLvChange,
    [BattleAttr.Status] = ClassBattleAgent.onStatus,
    [HeroAttr.BattleRideState] = ClassBattleAgent.onRideStatusChg,
    [HeroAttr.CurHeroTitleId] = ClassBattleAgent.onHeroTitleChg,
    [HeroAttr.EquipSelfMagicShow] = ClassBattleAgent.onEquipSelfMagicShowChg,
    [HeroAttr.SkillEyeStatus] = ClassBattleAgent.onSkillEyeStatus,
    [BattleAttr.CurRatioDef] = ClassBattleAgent.onCurRatioDef,
}

return ClassBattleAgent
