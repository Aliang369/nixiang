-- 怪物
local base = lt.BattleLivingAgent
local ClassBattleAgent = class("BattleMonsterAgent", base)

local BattleAttr = gConst.Attr.Battle
local HeroAttr = gConst.Attr.Hero
local BaseAttr = gConst.Attr.Base
local Figure = gConst.Game.Figure
local ModulePowerSaving = gModule.PowerSaving

ClassBattleAgent.isTarget = false -- 标识是否自己的目标
ClassBattleAgent.agentType = gConst.Game.AGENT_MONSTER
ClassBattleAgent.relation = 0 -- 关系 0-未知 1-敌对 5-中立 9-友善 10-自身
ClassBattleAgent.actionId = gConst.Game.ACTION_INVALID_ID
ClassBattleAgent.faceDir = nil
ClassBattleAgent.position = nil
ClassBattleAgent.location = nil
ClassBattleAgent.faceDirection = nil

ClassBattleAgent.skillNeedUpdate = false

function ClassBattleAgent:ctor(heroData, x, y, direction)
    base.ctor(self)

    self.position = Vector2.zero
    self.location = Vector2.zero
    self.faceDirection = Vector2.zero
    self.cacheSkillData = {}

    self.heroData = heroData
    self:updateRelation()

    --副本怪，需要读偏移
    local modelId = heroData:getFigure():getModelId()
    if modelId then
        local cfg = gDao.Model:queryById(modelId)
        self.showOffsetX = cfg:getShowOffsetX() / 100
        self.showOffsetY = cfg:getShowOffsetY() / 100
        self.bDamageBigObject = cfg:getWidth() >= gConst.Game.BATTLE_DAMAGE_BIG_OBJECT_THRESHOLD
    else
        self.showOffsetX = 0
        self.showOffsetY = 0
    end

    self.heroActor = lt.BattleActorObjectPool:popMonsterActor(heroData)
    self.heroActor:setIsHumanMonster(false)
    
    -- self.uiInfo = lt.BattleUIObjectPool:popMonsterUI(heroData)
    -- self.uiInfo:setScale(0.01, 0.01, 1)
    -- self.uiInfo:setHideWhenFull(true)

    self.cmdQueue = lt.BattleCmdQueue.new(self)
    self.stateMachine = lt.BattleStateMachine.new(self)
    self.stateMachine:init(self:getInitAppearState())

    self:setPosition(x, y)
    self:setDirection(direction or Vector2Fast.down)

    AgentManager:addAgent(self)

    self:initAppearInfo()
    self:birthColorChange()
end

function ClassBattleAgent:dtor()
    AgentManager:removeAgent(self)
    
    self.stateMachine:del()
    self.stateMachine = nil

    self:markClear()    
    self.cmdQueue:del()
    self.cmdQueue = nil

    self.heroData = nil
    base.dtor(self)
end


local dieEffectPrefab = "Art/Effect/Scene/SceneEffect_BattleDie/SceneEffect_BattleDie.prefab"
function ClassBattleAgent:disappear()
    AgentManager:removeAgent(self)

    local worldAgent = AgentManager:getWorldAgent()
    
    if self:isSelfTarget() then
        worldAgent:removeTarget()
    end

    local actorPosition = self.heroActor:getPosition()

    self.heroActor:removeActorInst()
    self:markClear()

    if self:isStateDie() then
        -- local agentId = self:getAgentId()
        -- worldAgent:addDisappearAgent(agentId, self)
        -- worldAgent:removeDisappearAgent(agentId)
        self:del()

        worldAgent:getBattleEnvSystem():createOnceEnv(self:getEnvShowOrder(), dieEffectPrefab, Vector2Fast.new(actorPosition.x, -actorPosition.z), Vector2Fast.up)
    else
        self:del()
    end
end

function ClassBattleAgent:tryDisappear()
    if not gModule.Map:isNeedExecDieByHand() then
        self:disappear()
    else
        local worldAgent = AgentManager:getWorldAgent()
        local agentId = self:getAgentId()
        worldAgent:addDisappearAgent(agentId, self)
    end
end

function ClassBattleAgent:hideAgent()
    self:getActor():setActive(false)
end

function ClassBattleAgent:dealReconnect(heroData, x, y, direction, status)
    self.heroData = heroData

    local curX, curY = self.position.x, self.position.y
    if curX ~= x or curY ~= y then
        self:setPosition(x, y)
    end

    self:setDirection(direction or Vector2.zero)
    
    local isChangeState = status ~= self.stateMachine:getStateId()
    if isChangeState then 
        if status == gConst.Game.STATE_ID_DIE then 
            self:changeStateDie()

        elseif status == gConst.Game.STATE_ID_MOVE then 
            self:changeStateMove(gConst.Game.MOVE_MODE_DEFAULT, direction, 100)

        elseif status == gConst.Game.STATE_ID_IDLE then 
            self:changeStateIdle()

        else
            printWarning("==========> 服务器下发客户端没有处理的状态, 状态id:", status)
        end
    end
end

function ClassBattleAgent:initAppearInfo()
    local figure = self.heroData:getFigure()
    self.hitPoint, self.height, self.buffScale, self.showDamagePoint = lt.WorldCommonUtil:calcBattleAgentAppearInfo(figure)
end

function ClassBattleAgent:setDirection(direction)
    self:setDirectionXY(direction.x, direction.y)
end

-- 设置所在的格子
function ClassBattleAgent:setMN(m, n, updatePosition)
    if updatePosition then
        self:setPosition(m, n)
    else
        self.curM = math.floor(m)
        self.curN = math.floor(n)
    end
end

function ClassBattleAgent:getMN()
    return self.curM, self.curN
end


function ClassBattleAgent:setDirectionXY(x, y)
    -- 当朝向为0, 0 时, 人物方向不变
    self.direction:Set(x, y)

    local faceDir = Util.World:directionToLeftRight(self.direction) 
    self:changeFaceDir(faceDir)
end

function ClassBattleAgent:getFaceDir()
    return self.faceDir
end

-- 修改面向
function ClassBattleAgent:changeFaceDir(faceDir)
    if not self.faceDir or (self.faceDir ~= faceDir) then
        self.faceDir = faceDir
        self.faceDirection:Set(Util.World:fixTo4DirectionXY(self.direction))

        self.buffSystem:setDirection(self.faceDir)
        self.heroActor:setDirection(self.direction)

        if self.changeFaceDirectionFuns then
            for _, callback in pairs(self.changeFaceDirectionFuns) do
                callback()
            end
        end
    end
end

function ClassBattleAgent:getFaceDirection()
    return self.faceDirection
end

function ClassBattleAgent:setActionDirty(dirty)
    self.actionDirty = dirty
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
    self.heroActor:setSpeed(1)
    self.heroActor:actionIdle()
end

function ClassBattleAgent:actionRun()
    self.buffSystem:roleRun()
    self.heroActor:setSpeed(1)
    self.heroActor:actionRun()

end

function ClassBattleAgent:actionDie(direction)
    self.buffSystem:roleDie()
    self.heroActor:setSpeed(1)
    self.heroActor:actionDie(direction)
    self.heroActor:dissolveIfNeed()
end


function ClassBattleAgent:actionSkillStart(actionOffset)
    if actionOffset < 1 or actionOffset > gConst.Game.SKILL_ACTION_NUM then
        printWarning("ClassBattleAgent:actionSkillStart 错误的起手动作id: " .. (gConst.Game.ACTION_SKILL_START_ID + actionOffset))
        return
    end
    self.heroActor:actionSkillStart(actionOffset)
end

function ClassBattleAgent:actionSkillReady(actionOffset)
    if actionOffset < 1 or actionOffset > gConst.Game.SKILL_ACTION_NUM then
        printWarning("ClassBattleAgent:actionSkillReady 错误的吟唱动作id: " .. (gConst.Game.ACTION_SKILL_READY_ID + actionOffset))
        return
    end

    self.heroActor:actionSkillReady(actionOffset)
end

function ClassBattleAgent:actionSkillChant(actionOffset)
    if actionOffset < 1 or actionOffset > gConst.Game.SKILL_ACTION_NUM then
        printWarning("ClassBattleAgent:actionSkillChant 错误的出手动作id: " .. (gConst.Game.ACTION_SKILL_CHANT_ID + actionOffset))
        return
    end

    self.heroActor:actionSkillChant(actionOffset)
end

function ClassBattleAgent:setModelSpeed(speed)
    self.heroActor:setSpeed(speed)
end

function ClassBattleAgent:actionSkillLoop(actionOffset)
    if actionOffset < 1 or actionOffset > gConst.Game.SKILL_ACTION_NUM then
        printWarning("ClassBattleAgent:actionSkillLoop 错误的持续施法动作id: " .. (gConst.Game.ACTION_SKILL_LOOP_ID + actionOffset))
        return
    end
    
    self.heroActor:actionSkillLoop(actionOffset)
end

function ClassBattleAgent:actionSkillLoopForce(actionOffset)
    if actionOffset < 1 or actionOffset > gConst.Game.SKILL_ACTION_NUM then
        printWarning("ClassBattleAgent:actionSkillLoopForce 错误的持续施法动作id: " .. (gConst.Game.ACTION_SKILL_LOOP_ID + actionOffset))
        return
    end
    
    self.heroActor:actionSkillLoopForce(actionOffset)
end

function ClassBattleAgent:actionSkillEnd(actionOffset)
    if actionOffset < 1 or actionOffset > gConst.Game.SKILL_ACTION_NUM then
        printWarning("ClassBattleAgent:actionSkillEnd 错误的结束动作id: " .. (gConst.Game.ACTION_SKILL_END_ID + actionOffset))
        return
    end

    self.heroActor:actionSkillEnd(actionOffset)
end

function ClassBattleAgent:initCmdQueue()
    self.cmdQueue = lt.BattleCmdQueue.new(self)
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
    --当前假死状态
    if self.buffSystem and self.buffSystem:isFadeDead() then
        return
    end
    self:changeState(self:getStateIdle(...))
end

function ClassBattleAgent:isStateIdle()
    return self:isState(gConst.Game.STATE_ID_IDLE)
end


function ClassBattleAgent:changeStateMove(moveMode, direction, distance)
    self:changeState(self:getStateMove(moveMode, direction, distance))
end

function ClassBattleAgent:isStateMove()
    return self:isState(gConst.Game.STATE_ID_MOVE)
end

function ClassBattleAgent:onMoveStart()
end

function ClassBattleAgent:onMoveStep()
    self:onLocation(true)
end

function ClassBattleAgent:onMoveEnd(forcedStop)
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
    self.heroActor:getFigureInst():setAlpha()
end

function ClassBattleAgent:resetAlpha()
    self.heroActor:getFigureInst():resetAlpha()
end

function ClassBattleAgent:setNavigateCallback(callback)
    self.navigateCallback = callback
end

function ClassBattleAgent:mpChange()
end

function ClassBattleAgent:onUpdateEverySec()
    -- 测试代码 test
    if true then
        local mapData = gModule.Map:getMapData()
        if gModule.Map:isBlock({x = self.curM, y = self.curN}) then
            local strs = {
                self:getDisplayName(),
                self:getObjId(),
                self.curM,
                self.curN,
            }
            MsgManager:popCenterMsg(table.concat(strs, ", "))
        end
    end
end

-- 更新
function ClassBattleAgent:onUpdate()
    self.stateMachine:onUpdate()
    
    if ModulePowerSaving:isOpen() then
        return
    end
    if not self.heroActor then
        return
    end
    self.heroActor:onUpdate()
    self.buffSystem:onUpdate()

    if self.birthElapse then
        if self.heroActor:getFigureInst():isInst() then
            self.birthElapse = self.birthElapse + Time.deltaTime
            for idx, birthTime in ipairs(gConst.Game.BattleColorBirthTimeArray) do
                if self.birthElapse >= birthTime and self.birthColorIdx < idx then
                    self.birthColorIdx = idx
                    self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Alpha)
                    self.heroActor:colorChange(gConst.Color.BattleBirthArray[idx])
                    break
                end
            end
            if self.birthElapse >= self.birthRecoverTime then
                self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Default)
                self.heroActor:colorChange(gConst.Color.ModelDefaultColor)
                self.birthElapse = nil
            end
        end
    end

    if self.hitElapse then
        if self.heroActor:getFigureInst():isInst() then
            self.hitElapse = self.hitElapse + Time.deltaTime
            if self.hitElapse >= self.hitRecoverTime then
                self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Default)
                self.heroActor:colorChange(gConst.Color.ModelDefaultColor)
                self.hitElapse = nil
            end
        end
    end

    if not table.isEmpty(self.skillSoundHandelMap) then 
        for k, skillSoundHandel in pairs(self.skillSoundHandelMap) do
            if skillSoundHandel.isPlayOver then
                self.skillSoundHandelMap[k] = nil
            end
        end
    end

    if self.reliveUIInfo then
        if self.reliveUIInfo:onUpdate() then
            self.reliveUIInfo:del()
            self.reliveUIInfo = nil
        end
    end
end

--设置假死状态
function ClassBattleAgent:setFadeDead(buff)
    if self.heroData then
        if self.reliveUIInfo then
            self.reliveUIInfo:setBuff(buff)
        else
            self.reliveUIInfo = lt.BattleReliveUIInfo.new(self.heroData, buff)
            self.reliveUIInfo:setScale(0.01, 0.01, 1)
            local offsetX = self.position.x + self.showOffsetX
            local offsetY = self.position.y + self.showOffsetY
            self.reliveUIInfo:setPosition(offsetX, gConst.Game.BATTLE_UI_MONSTER_INFO_HEIGHT, -offsetY)
            local worldAgent = AgentManager:getWorldAgent()
            local mapActor = worldAgent:getMapAgent():getMapActor()
            mapActor:addUIActor(self.reliveUIInfo)
        end
    end
end

function ClassBattleAgent:onUpdateDisappear()
end



function ClassBattleAgent:isValid()
    return self.heroData ~= nil
end
------------------------------------------ 属性相关引起Agent变化
-- 更新外观属性
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
end

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

    -- 血量变化
    self:hpChange()
end

function ClassBattleAgent:onHpChange()
    -- 血量变化
    self:hpChange() 
end



------------------------------------------

function ClassBattleAgent:getRelation()
    return self.relation
end

function ClassBattleAgent:enterMap()
end

function ClassBattleAgent:onLocation(isMove)
    self:setLocationMN(math.floor(self.position.x), math.floor(self.position.y))
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
    return gConst.Game.OBJ_MONSTER
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

function ClassBattleAgent:getModelId()
    return self.heroData:getModelId()
end

function ClassBattleAgent:getFigureModelId()
    return self.heroData:getFigureModelId()
end

function ClassBattleAgent:isSelf()
    return false
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
        self:changeStateMove(gConst.Game.MOVE_MODE_DEFAULT, cmd:getDirection(), 100)
    elseif type == gConst.Game.CMD_ATTACK then
        -- 攻击指令
        return self:handleAttackCmd(cmd)
    elseif type == gConst.Game.CMD_DIE then
        if not gModule.Map:isNeedExecDieByHand() then
            -- 死亡指令
            self.heroData:setStatus(gConst.Global.DIE)

            self:changeStateDie(cmd:getData())
            if self:isSelfTarget() then
                AgentManager:getWorldAgent():removeTarget()
            end

            self:onDieCallback()
        else
            self.dieCmd = cmd
            self:changeStateIdle()
        end

    elseif type == gConst.Game.CMD_JUMP then
        -- 跳跃指令
        self:onJump(cmd:getData())
    elseif type == gConst.Game.CMD_CHARGE then
        -- 冲锋指令
        self:onCharge(cmd:getData())

    elseif type == gConst.Game.CMD_HIT then
        -- 强受击表现, 非死亡态都可强行进入受击态, 
        if (cmd:isStrongHit() and not self:isStateDie()) then
            -- Agent本身是否可以有受击表现
            local meta = self.heroData:getConfig()
            self:changeStateHit(cmd)
        end

    else
        printError("ClassBattleAgent:handleCmd 缺少对type:"..type.."的处理")
    end

    return true
end


function ClassBattleAgent:getStateIdle(...)
    return lt.BattleMonsterIdleState.new(self, ...)
end

function ClassBattleAgent:getStateMove(moveMode, direction, distance)
    return lt.BattleMonsterMoveState.new(self, moveMode, direction, distance)
end

function ClassBattleAgent:getStateDie(...)
    return lt.BattleMonsterDieState.new(self, ...)
end

function ClassBattleAgent:changeStateDie(...)
    self:stopShake()

    self:changeState(self:getStateDie(...))
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
function ClassBattleAgent:onDie(diretion)
    self:actionDie(diretion)
    if not NativeManager:isAuditing() then
        -- AgentManager:getWorldAgent():getBattleEnvSystem():createOnceEnv(self:getEnvShowOrder(), "Art/Effect/Skill/Hit/death_hit.prefab",  
            -- Vector2Fast.zero, Vector2Fast.up, { parent = self })
    end
    -- self.uiInfo:onDie()
end

-- 角色复活
function ClassBattleAgent:onRevive()
    -- if not self.uiInfo then
    --     return
    -- end

    -- self.uiInfo:onRevive()
    self:hpChange()
end

function ClassBattleAgent:getCurHp()
    return self.heroData:getCurHp()
end

function ClassBattleAgent:getHpMax()
    return self.heroData:getHpMax()
end

function ClassBattleAgent:hpChange()
    -- self.uiInfo:hpChange(self.heroData)

    if self:isSelfTarget() then
        -- 目标血量变化
        EventManager:broadcast(GameEvent.WORLD_UPDATE_SELECT_ROLE, self.heroData)        
    end
end


function ClassBattleAgent:onHpMpChange(attackerId, data)
    -- dump(data, "onHpMpChange")
    -- print("onHpMpChange", self.__cname, attackerId, data)
    -- 更新人物血量数据
    if data.maxHP > 0 then
        self.heroData:setCurHp(data.curHP)
        self.heroData:setHpMax(data.maxHP)

        self:hpChange()
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
        gConst.Game.AGENT_ROBOT,
        gConst.Game.AGENT_NPC, 
        gConst.Game.AGENT_VISION)
    if not attackAgent then
        return
    end

    if data.skillId > 0 then
        -- 技能引发的伤害, 触发对象的受击效果
        if data.maxHP > 0 then
            lt.SkillManager:handleHurt(self, attackAgent, data.skillId, data.hpChange, data.flag, data.targetPos, data.hurtParam)
        end
    end

    if attackAgent:getRelation() == gConst.Game.RELATION_SELF then
        -- 玩家自己对怪物产生的伤害
        local meta = self.heroData:getConfig()
        local beHitType = meta:getBeHitType()
        if beHitType ~= gConst.Game.BeHitType.None then
            self:hitColorChange()
        end
        -- 正常变化
        self:popSelfFloatWord(data.hpChange, data.flag, data.damageDir, attackAgent)
    else
        -- 正常变化
        self:popFloatWord(data.hpChange, data.flag, data.damageDir, attackAgent)
    end
end

function ClassBattleAgent:hitColorChange()
    self.setHitColorArray = {}
    self.hitElapse = 0
    if self.heroActor then
        self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Blend)
        self.heroActor:colorChange(gConst.Color.BattleHit)
    end
    self.hitRecoverTime = gConst.Game.BattleColorRecoverTime
end

function ClassBattleAgent:birthColorChange()
    if gModule.Raid:getRaidType() == gConst.Logic.DuplicateType.Handup then
        return
    end
    self.birthElapse = 0
    self.birthColorIdx = 0
    self.birthColorIdx = self.birthColorIdx + 1
    self.heroActor:setBlendValue(gConst.Game.ShaderBlendValue.Alpha)
    self.heroActor:colorChange(gConst.Color.BattleBirthArray[self.birthColorIdx])
    self.birthRecoverTime = gConst.Game.BattleColorBirthTotalTime
end

-- 战斗飘字, 他人造成的伤害
function ClassBattleAgent:popFloatWord(value, flag, damageDir, attackAgent)
    if gModule.PowerSaving:isOpen() then
        return
    end

    local height = self.height + gConst.Game.BATTLE_OTHER_DAMAGE_OFFY
    local randomOffx = math.random(-10, 10) / 20
    local position = Vector2.new(randomOffx + self:getPosition().x, self:getPosition().y)
    
    if value > 0 then
        -- 回血
        lt.BattleElementManager:popOtherHealOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        return
    end

    if value == 0 then
        -- 闪避
        if Util.World:isDodge(flag) then
            lt.BattleElementManager:popOtherDodgeOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        end

        return
    end

    -- 伤害
    
    if Util.World:isSolve(flag) then
        -- 化解
        lt.BattleElementManager:popOtherSolveOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isCrit(flag) then
        -- 暴击
        local offsetX, offsetY = Util.Funs:getCritOffset(self, attackAgent, height)
        height = offsetY
        position = Vector2.new(offsetX + self:getPosition().x, self:getPosition().y)
        lt.BattleElementManager:popOtherCritOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isLuckyStrike(flag) then
        -- 会心
        local offsetX, offsetY = Util.Funs:getCritOffset(self, attackAgent, height)
        height = offsetY
        position = Vector2.new(offsetX + self:getPosition().x, self:getPosition().y)
        lt.BattleElementManager:popOtherLuckyStrikeOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    else
        -- 普通伤害
        lt.BattleElementManager:popOtherDamageOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
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
        lt.BattleElementManager:popSystemDamageOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    end
end

function ClassBattleAgent:popSelfFloatWord(value, flag, damageDir, attackAgent)
    
    if gModule.PowerSaving:isOpen() then
        return
    end

    local height = self.height + gConst.Game.BATTLE_OTHER_DAMAGE_OFFY
    local randomOffx = math.random(-10, 10) / 20
    local position = Vector2.new(randomOffx + self:getPosition().x, self:getPosition().y)

    if value > 0 then
        -- 回血
        lt.BattleElementManager:popSelfHealOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        return
    end

    if value == 0 then
        -- 闪避
        if Util.World:isDodge(flag) then
            lt.BattleElementManager:popSelfDodgeOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        end

        return
    end

    -- 伤害
    if Util.World:isSolve(flag) then
        -- 化解
        lt.BattleElementManager:popSelfSolveOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    elseif Util.World:isCrit(flag) then
        -- 暴击
        local offsetX, offsetY = Util.Funs:getCritOffset(self, attackAgent, height)
        height = offsetY
        position = Vector2.new(offsetX + self:getPosition().x, self:getPosition().y)
        lt.BattleElementManager:popSelfCritOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
        
    elseif Util.World:isLuckyStrike(flag) then
        -- 会心
        local offsetX, offsetY = Util.Funs:getCritOffset(self, attackAgent, height)
        height = offsetY
        position = Vector2.new(offsetX + self:getPosition().x, self:getPosition().y)
        lt.BattleElementManager:popSelfLuckyStrikeOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    else
        -- 普通伤害
        lt.BattleElementManager:popSelfDamageOther(self.position, self.showDamagePoint, value, damageDir, self:isDamageBigObject())
    end
end

function ClassBattleAgent:setBuffList(buffList)
    self.heroData:setBuffList(buffList)
    self.buffSystem:updateBuff()

    if self:isSelfTarget() then
        -- 目标buff变化
        EventManager:broadcast(GameEvent.TARGET_BUFF_CHG)        
    end
end

function ClassBattleAgent:updateBuff(buffData)
    self.heroData:addBuffList(buffData.addBuffArray)
    self.heroData:removeBuffList(buffData.delBuffArray)
    self.buffSystem:updateBuff()
    
    if self:isSelfTarget() then
        -- 目标buff变化
        EventManager:broadcast(GameEvent.TARGET_BUFF_CHG)        
    end
end

function ClassBattleAgent:canMove()
    return self.heroData:canMove() and not self.buffSystem:banMove() and not self.jumping
end

function ClassBattleAgent:isBlooding()
    return false
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

function ClassBattleAgent:onCharge(data)
    if not self:isStateSkill() then
        printWarning("怪物不在技能态, 服务器下发怪物冲锋")
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

-- 设为不可选中状态
function ClassBattleAgent:setUnselectable(value)
    if value then
        local worldAgent = AgentManager:getWorldAgent()
        if self:isSelfTarget() then
            worldAgent:removeTarget()
        end
    end
    self.unSelectable = value
end


function ClassBattleAgent:canBeAsTarget()
    return not self:cantBeAsTarget() and not self.unSelectable
end

function ClassBattleAgent:getInjuryRange()
    return self.heroData:getInjuryRange()
end

function ClassBattleAgent:needShowLockCircle()
    return true
end

------------------------------------------ 属性相关引起Agent变化
function ClassBattleAgent:onStatus()
    if self:isStateDie() and self.heroData:isAlive() then
        -- 复活
        self:changeStateIdle()
    end
end


-- 状态-技能
function ClassBattleAgent:getStateSkill(...)
    return lt.BattleMonsterSkillState.new(self, ...)
end

function ClassBattleAgent:changeStateSkill(...)
    self:changeState(self:getStateSkill(...))
end

function ClassBattleAgent:isStateSkill()
    return self:isState(gConst.Game.STATE_ID_SKILL)
end

function ClassBattleAgent:getStateSkillId()
    return self:getCurState():getSkillId()
end

function ClassBattleAgent:getStateHit(...)
    return lt.BattleMonsterHitState.new(self, ...)
end

function ClassBattleAgent:changeStateHit(...)
    self:changeState(self:getStateHit(...))
end

function ClassBattleAgent:isStateHit()
    return self:isState(gConst.Game.STATE_ID_HIT)
end


local RELATION_INVALID = gConst.Game.RELATION_INVALID
local RELATION_HOSTILE = gConst.Game.RELATION_HOSTILE
function ClassBattleAgent:updateRelation()
    if self.heroData == nil then
        self.relation = RELATION_INVALID
    else
        self.relation = RELATION_HOSTILE
    end
end

-- 怪物处理攻击指令
function ClassBattleAgent:handleAttackCmd(cmd)
    self:updateCastFunStep(cmd:getData())
   return true 
end

function ClassBattleAgent:updateCastFunStep(stepData)
    local skillId = stepData.skillId

    local castFunData = self.castFun
    if not castFunData then
        castFunData = {
            skillId = skillId,
            stepMap = {},
            step = stepData.step,
        }
        castFunData.stepMap[stepData.step] = stepData
        self.castFun = castFunData
    else
        castFunData.step = stepData.step
        castFunData.stepMap[stepData.step] = stepData
        castFunData.skillId = skillId
    end

    if self:isStateSkill() then 
        self:getCurState():onStep(castFunData)
    else
        -- 非技能释放中
        lt.SkillManager:handleStateExec(self, skillId, stepData, castFunData, false)
    end
end

function ClassBattleAgent:getCastFun()
    return self.castFun
end

function ClassBattleAgent:castFunEnd()
    self.castFun = nil
end

function ClassBattleAgent:setSkillUpdate(skillNeedUpdate)
    self.skillNeedUpdate = skillNeedUpdate
end

function ClassBattleAgent:inSkillUpdate()
    return self.skillNeedUpdate or false
end

function ClassBattleAgent:innerSetPosition()
    local offsetX = self.position.x + self.showOffsetX
    local offsetY = self.position.y + self.showOffsetY
    self.heroActor:setPosition(offsetX, 0, -offsetY)
    -- self.uiInfo:setPosition(offsetX, gConst.Game.BATTLE_UI_MONSTER_INFO_HEIGHT, -offsetY)
    if self.reliveUIInfo then
        self.reliveUIInfo:setPosition(offsetX, gConst.Game.BATTLE_UI_MONSTER_INFO_HEIGHT, -offsetY)
    end
end

function ClassBattleAgent:getActor()
    return self.heroActor
end


function ClassBattleAgent:getUIActor()
    return self.uiInfo
end

-- 设置Agent位置
function ClassBattleAgent:setPosition(x, y)
    if gConst.Game.DebugBattleObjId == self:getObjId() then
        printMobile("DebugBattleObjId setPosition", self.__cname, x, y)
    end
    -- if gConst.Game.SHOW_POSITION then
    --     self.uiInfo:setAddtionString(string.format("%.2f_%.2f", x, y))
    -- end
    self.curM = math.floor(x)
    self.curN = math.floor(y)

    self.position:Set(x, y)

    self.buffSystem:setLocalPosition(self.position)
    self:innerSetPosition()
end

function ClassBattleAgent:getPosition()
    return self.position:Clone()
end

function ClassBattleAgent:getPositionXY()
    return self.position:Get()
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

function ClassBattleAgent:getLocation()
    return self.location
end

function ClassBattleAgent:getLocationMN()
    return self.location:Get()
end


function ClassBattleAgent:getDirection()
    return self.direction:Clone()
end

function ClassBattleAgent:getDirectionXY()
    return self.direction:Get()
end


function ClassBattleAgent:flashToPosition(x, y, directionX, directionY)
    self:setPosition(x, y)
    self:setDirectionXY(directionX, directionY)
    if self:isStateMove() then
        self:getCurState():updateNewPosition()
    end
end

function ClassBattleAgent:readyToFlash(value)
end

function ClassBattleAgent:setPositionFigure(x, y, z)
    if self.heroActor then
        self.heroActor:setPositionFigure(x, y, z)
        lt.ShadowFactory:setActive(self.heroActor, false)
    end
    -- if self.uiInfo then
    --     self.uiInfo:setActive(false)
    -- end
end

function ClassBattleAgent:resetPositionFigure()
    if self.heroActor then
        self.heroActor:setPositionFigure(0, 0, 0)
        lt.ShadowFactory:setActive(self.heroActor, true)
    end

    -- if self.uiInfo then
    --     self.uiInfo:setActive(true)
    -- end
end

function ClassBattleAgent:isBoss()
    return false
end

--选择目标的优先级(数越小优先级越高)
function ClassBattleAgent:getSelectTargetOrder()
    if self.heroData:isGeneral() then
        --精英怪
        return 2
    else
        return 3
    end
end

function ClassBattleAgent:markClear()
    base.markClear(self)

    if self.heroActor then
        lt.BattleActorObjectPool:pushMonsterActor(self.heroActor)
        self.heroActor = nil
    end
    -- if self.uiInfo then
    --     lt.BattleUIObjectPool:pushMonsterUI(self.uiInfo)
    --     self.uiInfo = nil
    -- end
    self.isClear = true
end

function ClassBattleAgent:isCleared()
    return self.isClear
end

function ClassBattleAgent:execDie()
    self.heroData:setStatus(gConst.Global.DIE)

    self:changeStateDie(self.dieCmd:getData())
    self.dieCmd = nil
    
    if self:isSelfTarget() then
        AgentManager:getWorldAgent():removeTarget()
    end

    self:onDieCallback()
    local agentId = self:getAgentId()
    AgentManager:getWorldAgent():removeDisappearAgent(agentId)
end

function ClassBattleAgent:canShake()
    local meta = self.heroData:getConfig()
    local beHitType = meta:getBeHitType()
    
    return beHitType ~= gConst.Game.BeHitType.None
end

function ClassBattleAgent:startShake(targetAgent, attackAgent)
    self.heroActor:startShake(targetAgent, attackAgent)
end

function ClassBattleAgent:stopShake()
    self.heroActor:stopShake()
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

function ClassBattleAgent:getEnvShowOrder()
    return gConst.Game.EnvNeedShow.Order30
end

function ClassBattleAgent:getEnvTransform()
    return self.heroActor:getEnvTransform()
end

function ClassBattleAgent:getBuffFollowTransform()
    return self.heroActor:getEnvTransform()
end

-- 配置回调
ClassBattleAgent.attrCallback = {
    [BattleAttr.CurMoveSpeed] = ClassBattleAgent.onCurMoveSpeed,
    [BattleAttr.HpMax] = ClassBattleAgent.onHpChange,
    [BattleAttr.Shield] = ClassBattleAgent.onHpChange,
    [BattleAttr.Status] = ClassBattleAgent.onStatus
}



return ClassBattleAgent
