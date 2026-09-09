-- @Date    : 2022-05-25 20:29:27
-- @Author  : 
-- @Desc    : 战斗生物 Agent基类 只有所有类型的生物都使用此方法才可在此处写方法

local base = lt.BattleBaseAgent
local ClassBattleAgent = class("BattleLivingAgent", base)

ClassBattleAgent.envTable = nil -- 部分env拥有管理权 执行销毁
ClassBattleAgent.envSkillIdMap = nil -- SkillId Env映射表

ClassBattleAgent.direction = nil
ClassBattleAgent.buffSystem = nil
ClassBattleAgent.bDamageBigObject = false
ClassBattleAgent.agentRunning = false

function ClassBattleAgent:ctor()
    base.ctor(self)
    self.direction = Vector2.zero
    self.envTable = {}
    self.envSkillIdMap = {}
    self.rideEnvSkillIdMap = {}
    self.skillSoundHandelMap = {}
    self.agentRunning = true
    self.buffSystem = lt.BuffSystem.new(self)
end

function ClassBattleAgent:dtor()
    self.agentRunning = false
    self.buffSystem:del()
    self.buffSystem = nil
    self:clearAllSkillSound()
    if self.reliveUIInfo then
        self.reliveUIInfo:del()
        self.reliveUIInfo = nil
    end

    base.dtor(self)
end

function ClassBattleAgent:bindEffect(env, isLongEffect, needClear, skillId)
    self.envTable[env] = {longEffect = isLongEffect, needClear = needClear}
end

function ClassBattleAgent:bindEffectWithSkillId(env, skillId)
    if skillId then
        if not self.envSkillIdMap[skillId] then
            self.envSkillIdMap[skillId] = {}
        end
        local list = self.envSkillIdMap[skillId]
        list[#list + 1] = env
    end
end

function ClassBattleAgent:bindRideEffectBySkillId(env, skillId)
    if skillId then
        if not self.rideEnvSkillIdMap[skillId] then
            self.rideEnvSkillIdMap[skillId] = {}
        end
        local list = self.rideEnvSkillIdMap[skillId]
        list[#list + 1] = env
    end
end

function ClassBattleAgent:clearRideEffect(skillId)
    for loopSkillId, effetList in pairs(self.rideEnvSkillIdMap) do
        if skillId == nil or skillId == loopSkillId then
            for _, effect in pairs(effetList) do
                -- 当前effect可能已经结束, 再次setOver是没有问题的
                effect:setOver()
            end
            self.rideEnvSkillIdMap[loopSkillId] = nil
        end
    end
end

function ClassBattleAgent:clearEffect(env, skillId)
    if self.envTable then
        self.envTable[env] = nil
    end

    if not skillId then
        return
    end

    local envTbl = self.envSkillIdMap[skillId]
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

function ClassBattleAgent:clearEnvTable()
    for env, v in pairs(self.envTable) do
        env:setOver()
        if env.onDisable then
            env:onDisable()
        else
            printError("ClassBattleAgent:clearEnvTable()", env.__cname)
        end
    end

    self.envTable = {}
    self.envSkillIdMap = {}
end

function ClassBattleAgent:onDieCallback()
    --死亡时，清理特效
    for k1, effetList in pairs(self.envSkillIdMap) do
        for k2, v in pairs(effetList) do
            if v.longEffect or v.needClear then
                effetList[k2] = nil
            end
        end
    end
    for effect, v in pairs(self.envTable) do
        if v.longEffect or v.needClear then
            effect:setOver()
        end
        self.envTable[effect] = nil
    end
    self:clearRideEffect()
end

function ClassBattleAgent:getEnvBySkillId(skillId)
    if not self.envSkillIdMap[skillId] then
        return
    end
    local env
    for k, v in pairs(self.envSkillIdMap[skillId]) do
        env = v
        break
    end
    return env
end

function ClassBattleAgent:markClear()
    self.agentRunning = false
    self.buffSystem:markClear()
    self:clearEnvTable()
end

function ClassBattleAgent:attachBuffModel(buffData)
    local directionId = Util.World:directionToFaceDir(self:getDirection())
    self.buffSystem:attachBuffModel(buffData:getBuffId())
    self.buffSystem:setDirection(directionId)
end

function ClassBattleAgent:deAttachBuffModel(buffData)
    self.buffSystem:deAttachBuffModel(buffData:getBuffId())
end

function ClassBattleAgent:getDirection()
    return self.direction:Clone()
end

function ClassBattleAgent:handleAttackPassiveCmd(stepData)
    lt.SkillManager:handleExecPassive(self, stepData.skillId, stepData)
    
    -- 返回false不打断状态机流程
    return false
end

function ClassBattleAgent:addSkillSoundHandle(skillSoundHandel)
    table.insert(self.skillSoundHandelMap, skillSoundHandel)
end

function ClassBattleAgent:clearAllSkillSound()
    for _, skillSoundHandel in pairs(self.skillSoundHandelMap) do
        AudioManager:stopBattleSound(skillSoundHandel)
    end 

    self.skillSoundHandelMap = {}
end

function ClassBattleAgent:actionHit()
    self.heroActor:actionHit()
end

function ClassBattleAgent:actionHitDown1()
    self.heroActor:actionHitDown1()
end

function ClassBattleAgent:actionHitDown2()
    self.heroActor:actionHitDown2()
end

function ClassBattleAgent:actionHitDown3()
    self.heroActor:actionHitDown3()
end

function ClassBattleAgent:isDamageBigObject()
    return self.bDamageBigObject
end

function ClassBattleAgent:isAgentRunning()
    return self.agentRunning
end

-- 播放断血效果
function ClassBattleAgent:ShowBreakHpBarEffect()
    local battleEnvSystem = AgentManager:getWorldAgent():getBattleEnvSystem()
    battleEnvSystem:createOnceEnv(gConst.Game.EnvNeedShow.OrderHit, "Art/Effect/Skill/Other/common_break_hpbar/common_break_hpbar.prefab", 
            Vector2Fast.zero, Vector2Fast.down, {
        duration = 1.2,
        parent = self,
        height = self:getHeight() + 0.1,
    })
end

function ClassBattleAgent:isFadeState()
    if self.buffSystem then
        return self.buffSystem:hasBuffEffect(gConst.Game.BUFF_FADE)
    end
    return false
end

--设置假死状态
function ClassBattleAgent:setFadeDead(buff)
end

return ClassBattleAgent

