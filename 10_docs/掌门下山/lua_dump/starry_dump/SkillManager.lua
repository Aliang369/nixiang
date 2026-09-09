local table = table
local string = string

local pairs = pairs

local tonumber = tonumber
local pairs = pairs

local SkillManager = {}
local INNER_UNLOCK_CD = gConst.Skill.INNER_UNLOCK_CD
local BATTLE_HERO_SCALE = gConst.Game.BATTLE_HERO_SCALE

local SkillObjectKey = 0

--[[
Hit特效Delay指基于Attack特效播放起点的打击点时间

-- NOTE: 
一些特效时间点需要注意的地方

伤害特效的延迟时间:
    人物技能: 
        人物技能特效属于瞬发, 整套特效是连贯表现, 伤害延迟不允许策划进行配置.
    怪物技能: 
        产生的伤害延迟需要进行动态配置的, 伤害特效的表现(要求能够脱离此技能的前骤产生的特效表现和动作表现)
        此时Attack特效的延迟播放时间需要进行动态的推算. 结合特效的配置和数值的配置
        公式: 数值配置Delay - 特效配置hit特效Delay
        预警同理, 基于Attack的Delay时间往前推算

伤害特效的持续时间:
    人物技能: 
        部分人物技能会有动态时间持续性伤害,(要求Attack攻击特效为循环的)
        1. 数值配置中的持续时间(Duration)代表意义为产生伤害后, 持续多长时间, 
        2. 服务器下发的词条额外增加时间, local affixLoopTime = stepData.affixLoopTime / 1000 or 0
        3. 由于Attack特效打击效果时间点需要一段时间, 此时需要进行时间补偿, 在模板直接调整数值 字段变量为 local extraTime = 0.1
        
        公式: 数值配置持续时间(Duration) + affixLoopTime + extraTime
]]

-- 加载特效配置, 所有技能使用到的特效的持续时间和延迟, 部分不以特效配置, 需要结合数值配置(规则见NOTE)
function SkillManager:init()
    local asset = ResourcesManager:coLoadAsync("Art/Statistic/SkillEffectStatistic.bytes", UEType.TextAsset)

    self.effsData = {}
    local count = 1
    for k, tbl in pairs(Json.decode(asset.bytes)) do
        for path, v in pairs(tbl) do
            self.effsData[path] = v
        end
        count = count + 1
    end

    -- 加载技能动作帧数(用于怪物)
    local asset = ResourcesManager:coLoadAsync("Art/Statistic/FrameStatistic.bytes", UEType.TextAsset)

    self.monsterFrames = {}
    for id, list in pairs(Json.decode(asset.bytes)) do
        id = tonumber(id)
        self.monsterFrames[id] = {}

        for action, v in pairs(list) do
            self.monsterFrames[id][tonumber(action)] = v
        end
    end

    -- 加载技能spine动作时间 (用于怪物)
    local asset = ResourcesManager:coLoadAsync("Art/Statistic/SpineActionTimeStatistic.bytes", UEType.TextAsset)

    self.actionDurationMap = {}
    for id, list in pairs(Json.decode(asset.bytes)) do
        id = tonumber(id)
        self.actionDurationMap[id] = list
    end

    gConst.Game:setSpineActionDuration(self.actionDurationMap)

    self:initSkillTemplate()

    self:reset()
    
    self.skillObjectTable = {}
    SkillObjectKey = 0

    -- 技能执行functor
    self.skillUpdateFunctor = {}
end

-- 初始化技能模板
function SkillManager:initSkillTemplate()
    -- 技能实例池
    self.skillInstFuncMap = {}

    -- 人物技能模板池
    self.skillFuncMap = {}
    setmetatable(self.skillFuncMap, {
        __index = function(t, k)
            local success, func = pcall(require, "Stage.Battle.Skill.SkillFunc.SkillFunc" .. k)
            if not success then
                if DEBUG then 
                    printError(string.format("获取技能模板, 未找到对应模板: [%d]", k))
                end
                return
            end

            func:setFuncIndex(k)

            func:setEffData(self.effsData)

            t[k] = func

            return t[k]
        end
    })

    -- 天赋效果模板池
    self.innatFuncMap = {}
    setmetatable(self.innatFuncMap, {
        __index = function(t, k)
            local success, func = PCallSafe(require, "Stage.Battle.Skill.SkillFuncInnat.SkillFuncInnat" .. k)
            if not success then
                printError(string.format("获取天赋模板, 未找到对应模板: [%d]", k))
                return
            end

            func:setFuncIndex(k)

            func:setEffData(self.effsData)

            t[k] = func
            return t[k]
        end
    })
end

function SkillManager:uninit()
    self:reset()
end

function SkillManager:reset()
    -- 清空选中的技能Id
    self:clearSelectId()
    self:clearWJSelectId()

    -- 重置技能CD
    self.cdMap = {}

    -- 重置技能许可
    self.permitMap = {}

    -- 解除技能按钮引用
    self.skillButton = {}

    -- 重置记录的未收到服务器反馈的技能, 0.5秒(INNER_UNLOCK_CD)内未收到服务器包回包, 会解除等待
    self.waitMap = {}

    -- 重置 (一次CD技能可以多次分段进行手动释放的)技能 记录允许分段释放的结束时间, 到结束时间后技能进入CD
    self.againEndTimeMap = {}

    if self.skillInstFuncMap then
        for k, skillFuncMap in pairs(self.skillInstFuncMap) do
            for _, skillFunc in pairs(skillFuncMap) do
                if skillFunc:isInstantiate() then
                    skillFunc:onDestroy()
                end
            end
        end
        self.skillInstFuncMap = {}
    end

    self.skillObjectTable = {}
    self.skillUpdateFunctor = {}
    SkillObjectKey = 0

    --gModule.Skill:clearEffectData()
end

function SkillManager:onUpdate()
    -- 更新技能冷却
    local serverTime = gModule.Server:getSecond()

    for skillId, againEndTime in pairs(self.againEndTimeMap) do
        if againEndTime <= serverTime then
            self:cleanAgainEndTime(skillId)
        else
            local uiButton = self.skillButton[skillId]
            if uiButton then
                uiButton:updateAgainCoutDown()
            end
        end
    end

    for skillId, nextTime in pairs(self.cdMap) do
        if not self.againEndTimeMap[skillId] then
            if nextTime <= serverTime then
                self:cleanCdEnd(skillId)
            else
                local uiButton = self.skillButton[skillId]
                if uiButton then
                    uiButton:updateCountDown()
                end
            end
        end
    end

    -- 驱动客户端解锁 长时间未等到服务器回技能反馈包的锁定
    local deltaTime = Time.deltaTime
    for skillId, elapse in pairs(self.waitMap) do
        elapse = elapse + deltaTime

        if elapse >= INNER_UNLOCK_CD then
            self.waitMap[skillId] = nil
        else
            self.waitMap[skillId] = elapse
        end
    end

    PCallSafe(function()
        for k, skillObject in pairs(self.skillObjectTable) do
            if skillObject:isOver() then
                skillObject:del()
                self.skillObjectTable[k] = nil
            else
                skillObject:onUpdate()
            end
        end
    end)

    --  ??
    for onUpdateFunctor, params in pairs(self.skillUpdateFunctor) do
        if serverTime - params.startTime >= params.delay then
            onUpdateFunctor(params)
        end
    end
end

-- 设置技能Id 对应的ui组件
function SkillManager:setSkillBtn(skillId, uiButton)
    if not skillId then
        return
    end

    self.skillButton[skillId] = uiButton
end

-- 删除技能时, 或者技能按钮组件销毁时, 重置单个技能数据
function SkillManager:resetSkill(skillId)
    self.skillButton[skillId] = nil
    self.permitMap[skillId] = nil
    self.waitMap[skillId] = nil

    if skillId == self.selectId then
        self:clearSelectId()
    end
    if skillId == self.wjSelectId then
        self:clearWJSelectId()
    end
end

-- 使用技能 添加准许
function SkillManager:addPermit(skillId, priority)
    self.permitMap[skillId] = priority
end

-- 使用掉技能后, 移除准许
function SkillManager:removePermit(skillId)
    self.permitMap[skillId] = nil
end

-- 使用技能发包通知服务器, 等待服务器回包, 未回包前不允许再使用该技能
function SkillManager:addWait(skillId)
    self.waitMap[skillId] = 0
end

-- 移动等待回包
function SkillManager:removeWait(skillId)
    self.waitMap[skillId] = nil
end

-- 更新技能的CD时间
function SkillManager:updateCD(skillId, nextTime)
    local serverTime = gModule.Server:getSecond()
    nextTime = nextTime / 1000

    -- 无cd的技能不进入cdMap
    if nextTime > serverTime then
        self.cdMap[skillId] = nextTime
    else
        -- cd冷却结束
        self:cleanCdEnd(skillId)
    end
end

-- 技能Cd冷却结束
function SkillManager:cleanCdEnd(skillId)
    if self.cdMap[skillId] == nil then
        return
    end

    self.cdMap[skillId] = nil

    -- 驱动UI更新
    local uiButton = self.skillButton[skillId]
    if uiButton then
        uiButton:updateCountDown()
    end
end

--[[
更新多次可以多次分段进行手动释放的结束时间
    castCount 释放次数
    firstCastTime, 首次释放的时间
]]
function SkillManager:updateAgainEndTime(skillId, castCount, firstCastTime)
    local serverTime = gModule.Server:getSecond()

    if castCount > 0 then
        -- firstCastTime 在一次技能释放周期内, 不会变化, 只需要记录一次
        if self.againEndTimeMap[skillId] == nil then
            local skillEffect = lt.SkillManager:getSkillEffect(skillId)
            local againTime = skillEffect:getAgainTime() / 1000

            self.againEndTimeMap[skillId] = firstCastTime / 1000 + againTime
        end
    else
        -- 0代表不在分段释放的状态(结束了), 有可能进入技能Cd, 也可能是没有过释放技能
        self:cleanAgainEndTime(skillId)
    end
end

-- 允许技能可以多次分段进行手动释放的时间结束
function SkillManager:cleanAgainEndTime(skillId)
    if self.againEndTimeMap[skillId] == nil then
        return
    end

    self.againEndTimeMap[skillId] = nil

    -- 驱动UI更新
    local uiButton = self.skillButton[skillId]
    if uiButton then
        uiButton:updateAgainCoutDown()
    end
end

-- 更新CD时间 仅处理进入CD
function SkillManager:updateCDEnterCD(skillId, nextTime)
    local serverTime = gModule.Server:getSecond()
    nextTime = nextTime / 1000

    if self.againEndTimeMap[skillId] then
        self.againEndTimeMap[skillId] = nil
    end

    -- 无cd的技能不进入cdMap
    if nextTime > serverTime or self.cdMap[skillId] then
        self.cdMap[skillId] = nextTime
    end
end

-- 技能是否处理于CD
function SkillManager:skillIsOnCd(skillId)
    if not self.cdMap[skillId] then
        return false
    end

    return true
end

-- 获得技能Cd的结束时间
function SkillManager:getCdMap(skillId)
    return self.cdMap[skillId]
end

-- 获得技能(一次CD技能可以多次分段进行手动释放的) 允许分段释放的结束时间
function SkillManager:getAgainEndTime(skillId)
    return self.againEndTimeMap[skillId]
end

-- 通过技能Id 获取对应的技能配置
function SkillManager:getConfig(skillId)
    local skillMeta = gModule.Skill:combineQueryById(skillId)
    if skillMeta == nil then
        printError(string.format("找不到技能对应的技能配置 id[%d]", skillId))
    end

    return skillMeta
end

-- 通过技能Id 获取对应的技能模板
function SkillManager:getFunc(skillId, agentId)
    local skillMeta = self:getConfig(skillId)

    if not skillMeta then
        return
    end

    local funcIndex = skillMeta:getIndex()

    -- 空技能模板
    if gConst.Skill.EmptyTemplate[funcIndex] then
        return
    end
    
    local func = self.skillFuncMap[funcIndex]

    if not func then
        printError(string.format("找不到技能对应的技能模板 技能Id[%d] 模板Id [%d]", skillId, funcIndex))
    end
    
    if func and agentId then
        if func:isEnableInst() then
            if not self.skillInstFuncMap[agentId] then
                self.skillInstFuncMap[agentId] = {}
            end
            if not self.skillInstFuncMap[agentId][skillId] then
                self.skillInstFuncMap[agentId][skillId] = func.new()
            end
            return self.skillInstFuncMap[agentId][skillId]
        end
    end
    
    return func
end

--获取用户当前实例技能
function SkillManager:getVaildFunc(agentId, skillId)
    local skillMeta = self:getConfig(skillId)

    if not skillMeta then
        return nil
    end

    local funcIndex = skillMeta:getIndex()
    local func = self.skillFuncMap[funcIndex]

    if not func then
        printError(string.format("找不到技能对应的技能模板 技能Id[%d] 模板Id [%d]", skillId, funcIndex))
    end

    if func and agentId then
        if func:isEnableInst() then
            if not self.skillInstFuncMap[agentId] then
                return nil
            end
            return self.skillInstFuncMap[agentId][skillId]
        end
    end

    return func
end

-- 处理按钮点击技能
function SkillManager:handleAction(action, skillId, skillConfig)
    --print("SkillManager:handleAction", skillId)
    local selfAgent = AgentManager:getSelfAgent()
    if not selfAgent then
        return
    end
    if not selfAgent:canCastSkill() then
        return
    end
    
    if action == gConst.Game.SKILL_PRESSED then
        -- 长按模式, press时无法释放技能相关提示
        local res, expendAttr, noticeId = self:readyForUse(skillId, skillConfig)
        if not res then
            if expendAttr == "CurHp" then
                MsgManager:popNotice(130)
                return
            elseif expendAttr == "CurMp" then
                MsgManager:popNotice(129)
                return
            elseif expendAttr == "RideCurHp" then
                MsgManager:popNotice(574)
                return
            end
            if noticeId then 
                MsgManager:popNotice(noticeId)
                return
            end
        end

        if skillConfig:getSkillType() == gConst.Item.WeaponPos.Wj then
            -- 武技 长按能够连续释放
            self:setWJSelectId(skillId)
        else
            -- 长按能够连续释放
            self:setSelectId(skillId)
        end

        if not res then
            return
        end

        -- 尝试驱动处理释放技能
        self:handleCast()

    elseif action == gConst.Game.SKILL_RELEASE then
        if skillConfig:getSkillType() == gConst.Item.WeaponPos.Wj then
            -- 武技技能松开
            self:clearWJSelectId()
        else
            if skillConfig:getLongPressCast() then
                self:clearSelectId()
            else
                -- 单次释放技能模式, release时无法释放技能相关提示
                local res, expendAttr, noticeId = self:readyForUse(skillId, skillConfig)

                if not res then
                    if expendAttr == "CurHp" then
                        MsgManager:popNotice(130)
                        return
                    elseif expendAttr == "CurMp" then
                        MsgManager:popNotice(129)
                        return
                    elseif expendAttr == "RideCurHp" then
                        MsgManager:popNotice(574)
                        return
                    end
                    if noticeId then 
                        MsgManager:popNotice(noticeId)
                        return
                    end
                    
                end

                -- 立即释放技能
                self:setSelectId(skillId)
                self:handleCast()
                self:clearSelectId()
            end
        end
    end
end

function SkillManager:notifyObjHit(objId)
    gModule.Game:notifyObjAttackHit(objId)
end

function SkillManager:readyForUse(skillId, skillConfig, skillType)
    -- 等待服务器反馈中
    if self.waitMap[skillId] ~= nil then
        return false
    end

    -- 技能处于cd中
    if not self.againEndTimeMap[skillId] and self.cdMap[skillId] ~= nil then
        return false
    end

    local noticeId
    local selfAgent = AgentManager:getSelfAgent()
    --当前不在场景中
    if selfAgent == nil then
        return false
    end

    --坐骑技能时需要判断是否有其他坐骑技能正在释放
    if skillType and skillConfig:getSkillType() == gConst.Item.WeaponPos.RideSkill_Zd then
        local hero = gModule.Game:getHero()
        --当前在坐骑状态(坐骑技能持续中)
        if hero:isBattleOnTheMount() then
            return false
        end
        local otherRideSkill = nil
        if skillType == gConst.Item.WeaponPos.RideSkill_Zd then
            --主技能时，判断副技能
            otherRideSkill = gModule.Skill:getRideSkillByPos(2)
        elseif skillType == gConst.Item.WeaponPos.SubRideSkill_Zd then
            --副技能时，判断主技能
            otherRideSkill = gModule.Skill:getRideSkillByPos(1)
        end
        if otherRideSkill then
            -- 等待服务器反馈中
            if self.waitMap[otherRideSkill:getSkillId()] ~= nil then
                return false
            end
        end
    end

    --当前为韧性状态，不受控制
    local isRationDef = false
    local roleData = selfAgent:getRole()
    if roleData then
        if roleData:getCurRatioDef() > 0 then
            isRationDef = true
        end
    end
    if not isRationDef then
        local noLimit = skillConfig.getNoLimit and skillConfig:getNoLimit() == 1 or false
        local buffSystem = selfAgent:getBuffSystem()
        if buffSystem and (not noLimit) then 
            if buffSystem:hasBuffEffect(gConst.Game.BUFF_STUNS) then 
                noticeId = 251
                return false, nil, noticeId
            elseif buffSystem:hasBuffEffect(gConst.Game.BUFF_SILENT) then 
                --沉默只能放武技
                if skillConfig:getSkillType() ~= gConst.Item.WeaponPos.Wj then
                    noticeId = 250
                    return false, nil, noticeId
                end
            elseif buffSystem:hasBuffEffect(gConst.Game.BUFF_INVINCIBLE) then 
                noticeId = 603
                return false, nil, noticeId
            end
        end
    end

    --设置技能消耗
    local expendInfoList = {}
    --技能效果消耗
    local skillIndex = skillConfig:getIndex()
    local effectCostArray = gModule.Skill:getEffectCost(skillIndex)
    if effectCostArray then
        for index, value in ipairs(effectCostArray) do
            expendInfoList[index] = value
        end
    end
    --技能配置消耗
    local expend = skillConfig:getCostArray()
    local isHas = false
    for _, value in ipairs(expendInfoList) do
        if value[1] == expend[1] then
            isHas = true
            value[2] = value[2] + expend[2]
            break
        end
    end
    if not isHas then
        if expend[1] ~= nil and expend[2] ~= nil then
            table.insert(expendInfoList, {expend[1], expend[2]})
        end
    end

    -- 无技能消耗
    if #expendInfoList == 0 then
        return true
    end

    for _, value in ipairs(expendInfoList) do
        local isEnough, expendAttr = self:isCostEnough(value[1], value[2], skillId)
        if not isEnough then
            return false, expendAttr
        end
    end

    return true
end

function SkillManager:isCostEnough(expendAttr, expendValue, skillId)
    if gConst.Attr.Hero[expendAttr] == nil then
        -- 特殊消耗
        if expendAttr == "CurHpPct" then    --当前血量百分比, 必定能够释放
            return true
        end
    end
    
    local hero = gModule.Game:getHero()
    local shield = hero:getShield()
    --消耗血量 但是有护盾就能放技能
    if expendAttr == "CurHp" and shield > 0 then
        return true
    end

    local isRide = hero:isBattleOnTheMount()
    if expendAttr == "CurHp" and isRide then
        expendAttr = "RideCurHp"
    end

    local key = gConst.Attr.Hero[expendAttr]
    if not key then
        printWarning("Unexpected ERROR SkillManager:readyForUse", skillId, expendAttr)
        return false
    end
    if not gModule.Game:getHero():enoughValue(key, expendValue) then
        return false, expendAttr
    end

    return true
end

-- 处理技能是否准备好了
function SkillManager:handleReady(skillId)
    if (self.againEndTimeMap[skillId] == nil and self.cdMap[skillId] ~= nil) or self.waitMap[skillId] ~= nil then
        return false
    end

    return true
end

-- 处理技能释放
function SkillManager:handleCast()
    local skillId = self:getTopPriorityId()
    if skillId == nil then
        return
    end

    local selfAgent = AgentManager:getWorldAgent():getSelfAgent()
    local selfPosition = selfAgent:getPosition()

    local skillMeta = gModule.Skill:combineQueryById(skillId)
    local skillFunc = self.skillFuncMap[skillMeta:getIndex()]
    if not skillFunc then
        return
    end

    skillFunc:onHandleCast(skillId, skillMeta, selfAgent)

    local paramType, param = skillFunc:onSelectTarget(skillId, skillMeta, selfPosition)
    local paramType3, param3 = skillFunc:onSelectTarget3(skillId, skillMeta, selfPosition)

    gModule.Game:sendAttack(skillId, selfPosition, {
        {paramType, param},
        {paramType3, param3}
    })

    self:addWait(skillId)

    return
end

function SkillManager:handleCreate(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end

    if not skillFunc.enableInst then
        return
    end

    -- 已实例化 返回
    if skillFunc:isInstantiate() then
        return
    end

    print("SkillManager:handleCreate", agent, skillId, ...)

    return skillFunc:onCreate(agent, skillId, ...)
end


function SkillManager:handleDestroy(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end
    -- 已销毁 返回
    if not skillFunc:isInstantiate() then
        return
    end

    -- print("SkillManager:handleDestroy", agent, skillId, ...)

    skillFunc:onDestroy(agent, ...)
    self.skillInstFuncMap[agent:getAgentId()][skillId] = nil
end

function SkillManager:handleStopSkill(agent, skillId)
    local instSkill = self:getVaildFunc(agent:getAgentId(), skillId)
    if instSkill == nil then
        return false
    end
    print("SkillManager:handleStopSkill ", agent, skillId)
    instSkill:onStopSkill(agent, skillId)

    if instSkill:isEnableInst() then
        if self.skillInstFuncMap[agent:getAgentId()] then
            self.skillInstFuncMap[agent:getAgentId()][skillId] = nil
        end
    end
end

--调用初始化被动技能方法
function SkillManager:handleInitPassive(skillId)
    local skillMeta = self:getConfig(skillId)

    if not skillMeta then
        return
    end

    local funcIndex = skillMeta:getIndex()

    -- 不存在秘术技能模板
    if not gConst.Skill.MysticSkillTemplate[funcIndex] then
        return
    end

    local skillFunc = self.skillFuncMap[funcIndex]
    if skillFunc == nil then
        return
    end
    if not skillFunc.onInitPassive then
        return
    end
    skillFunc:onInitPassive(skillId)
end

function SkillManager:handleExecPassive(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end

    return skillFunc:onExec(agent, ...)
end

-- 无技能状态机 处理技能释放效果
function SkillManager:handleExec(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end

    -- 驱动agent播放技能出手动作
    agent:castSkill(skillId, ...)

    return skillFunc:onExec(agent, ...)
end

-- 处理技能释放次数的同步
function SkillManager:handleSyncCount(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end

    return skillFunc:onExecSyncCount(agent, ...)
end

-- 进入技能状态机 处理技能释放效果
function SkillManager:handleStateExec(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end

    -- print("SkillManager:handleStateExec", agent, skillId, ...)
    return skillFunc:onExec(agent, ...)
end

-- 释放技能后 技能的后续行为
function SkillManager:handleExecDuration(agent, skillId, ...)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end
    return skillFunc:onExecDuration(agent, ...)
end

-- 处理命中目标
function SkillManager:handleHit()

end

-- 处理目标受到伤害
function SkillManager:handleHurt(targetAgent, attackAgent, skillId, hpChange, ...)
    local skillFunc = self:getFunc(skillId, targetAgent:getAgentId())

    if skillFunc == nil then
        return false
    end

    if hpChange < 0 then
        -- 受击表现
        self:handleHitPerformance(targetAgent, skillId)
    end

    if targetAgent:canShake() then
        targetAgent:startShake(targetAgent, attackAgent)
    end

    --[[
        ...:
            flag, targetPos, hurtParam
    ]]
    return skillFunc:onHurt(targetAgent, attackAgent, skillId, ...)
end

function SkillManager:handleDie(targetAgent, attackAgent, skillId, ...)
    local skillFunc = self:getFunc(skillId, targetAgent:getAgentId())

    if skillFunc == nil then
        return false
    end

    return skillFunc:onDie(targetAgent, attackAgent, skillId, ...)
end


function SkillManager:handleSpecial(agent, skillId, data, state)
    local skillFunc = self:getFunc(skillId, agent:getAgentId())
    if skillFunc == nil then
        return false
    end

    agent:setSpecialData(data)

    return skillFunc:onSpecial(agent, data, state)
end

-- 处理受击表现
function SkillManager:handleHitPerformance(agent, skillId)
    local agentType = agent:getAgentType()
    local roleData = agent:getRole()

    local cmd
    if agentType == gConst.Game.AGENT_MONSTER then
        local meta = roleData:getConfig()

        local beHitType = meta:getBeHitType()
        -- 无受击表现
        if beHitType == gConst.Game.BeHitType.None then
            return
        end

        local data = {
            beHitType = beHitType,
            skillId = skillId, 
            hitActionType = gConst.Game.HitActionType.Once,
        }

        cmd = lt.BattleCmdHit.new(data)
    elseif agentType == gConst.Game.AGENT_HERO then
        cmd = lt.BattleCmdHit.new({
            beHitType = gConst.Game.BeHitType.Weak,
            skillId = skillId, 
            hitActionType = gConst.Game.HitActionType.Once,
        })
    end

    if cmd then 
        agent:pushCmd(cmd)
    end
end

function SkillManager:getSkillObj(skillObj)
    local sourceObjId = skillObj:getSourceObjId()
    local agent = AgentManager:getWorldAgent():queryAgentByObjId(sourceObjId, 
        gConst.Game.AGENT_HERO, 
        gConst.Game.AGENT_MONSTER, 
        gConst.Game.AGENT_PET, 
        gConst.Game.AGENT_NPC,
        gConst.Game.AGENT_VISION
    )

    if not agent then
        return nil
    end

    local skillFunc = self:getFunc(skillObj:getSkillId(), agent:getAgentId())
    if skillFunc == nil then
        return nil
    end

    if skillFunc.getSkillObj == nil then
        return nil
    end

    return skillFunc:getSkillObj(skillObj)
end

-- 获得最高优先级的技能Id
function SkillManager:getTopPriorityId()
    -- 非武技技能优先级高
    if self.selectId ~= nil and self:handleReady(self.selectId) then
        return self.selectId
    end
    -- 武技按着就释放
    if self.wjSelectId ~= nil and self:handleReady(self.wjSelectId) then
        return self.wjSelectId
    end
    return nil
end

-- 武技技能选取
function SkillManager:setWJSelectId(skillId)
    self.wjSelectId = skillId
end

function SkillManager:clearWJSelectId()
    self.wjSelectId = nil
end

-- 技能选取
function SkillManager:setSelectId(skillId)
    self.selectId = skillId
end

function SkillManager:clearSelectId()
    self.selectId = nil
end

function SkillManager:getStartStepDuration(modelId, offset)
    local modelMeta = gDao.Model:queryById(modelId)

    if modelMeta:getResType() == gConst.Logic.FigureType.UnitSpine then
        return self:getSpineStartStepDuration(modelId, offset)
    end

    local frame = self.monsterFrames[modelId][gConst.Game.ACTION_SKILL_START_ID + (offset - 1) * 10] or 0
    return frame / 30
end

function SkillManager:getLoopStepDuration(modelId, offset)
    local modelMeta = gDao.Model:queryById(modelId)

    if modelMeta:getResType() == gConst.Logic.FigureType.UnitSpine then
        return self:getSpineLoopStepDuration(modelId, offset)
    end

    local frame = self.monsterFrames[modelId][gConst.Game.ACTION_SKILL_LOOP_ID + (offset - 1) * 10] or 0
    return frame / 30
end

function SkillManager:getChantStepDuration(modelId, offset)
    local modelMeta = gDao.Model:queryById(modelId)
    if modelMeta:getResType() == gConst.Logic.FigureType.UnitSpine then
        return self:getSpineChantStepDuration(modelId, offset)
    end
    
    local frame = self.monsterFrames[modelId][gConst.Game.ACTION_SKILL_CHANT_ID + (offset - 1) * 10] or 0
    return frame / 30
end

function SkillManager:getReadyStepDuration(modelId, offset)
    local modelMeta = gDao.Model:queryById(modelId)
    if modelMeta:getResType() == gConst.Logic.FigureType.UnitSpine then
        return self:getSpineReadyStepDuration(modelId, offset)
    end
    
    local frame = self.monsterFrames[modelId][gConst.Game.ACTION_SKILL_READY_ID + (offset - 1) * 10] or 0
    return frame / 30
end

local StartActionNameTable = {
    [1] = "Skill1_Start",
    [2] = "Skill2_Start",
    [3] = "Skill3_Start",
    [4] = "Skill4_Start",
    [5] = "Skill5_Start",
    [6] = "Skill6_Start",
    [7] = "Skill7_Start",
    [8] = "Skill8_Start",
    [9] = "Skill9_Start",
    [10] = "Skill10_Start",
}
function SkillManager:getSpineStartStepDuration(modelId, offset)
    local actionName = StartActionNameTable[offset]
    
    return self.actionDurationMap[modelId][actionName] or 0
end

local ChantActionNameTable = {
    [1] = "Skill1_Chant",
    [2] = "Skill2_Chant",
    [3] = "Skill3_Chant",
    [4] = "Skill4_Chant",
    [5] = "Skill5_Chant",
    [6] = "Skill6_Chant",
    [7] = "Skill7_Chant",
    [8] = "Skill8_Chant",
    [9] = "Skill9_Chant",
    [10] = "Skill10_Chant",
}
function SkillManager:getSpineChantStepDuration(modelId, offset)
    local actionName = ChantActionNameTable[offset]
    
    return self.actionDurationMap[modelId][actionName] or 0
end

local EndActionNameTable = {
    [1] = "Skill1_End",
    [2] = "Skill2_End",
    [3] = "Skill3_End",
    [4] = "Skill4_End",
    [5] = "Skill5_End",
    [6] = "Skill6_End",
    [7] = "Skill7_End",
    [8] = "Skill8_End",
    [9] = "Skill9_End",
    [10] = "Skill10_End",
}
function SkillManager:getSpineEndStepDuration(modelId, offset)
    local actionName = EndActionNameTable[offset]
    
    return self.actionDurationMap[modelId][actionName] or 0
end

local ReadyActionNameTable = {
    [1] = "Skill1_Ready",
    [2] = "Skill2_Ready",
    [3] = "Skill3_Ready",
    [4] = "Skill4_Ready",
    [5] = "Skill5_Ready",
    [6] = "Skill6_Ready",
    [7] = "Skill7_Ready",
    [8] = "Skill8_Ready",
    [9] = "Skill9_Ready",
    [10] = "Skill10_Ready",
}
function SkillManager:getSpineReadyStepDuration(modelId, offset)
    local actionName = ReadyActionNameTable[offset]
    
    return self.actionDurationMap[modelId][actionName] or 0
end

local LoopActionNameTable = {
    [1] = "Skill1_Loop",
    [2] = "Skill2_Loop",
    [3] = "Skill3_Loop",
    [4] = "Skill4_Loop",
    [5] = "Skill5_Loop",
    [6] = "Skill6_Loop",
    [7] = "Skill7_Loop",
    [8] = "Skill8_Loop",
    [9] = "Skill9_Loop",
    [10] = "Skill10_Loop",
}
--[[
    少数boss 22702 loop叫Skill1_Loop1 Skill1_Loop2
]]
function SkillManager:getSpineLoopStepDuration(modelId, offset, suffix)
    local actionName = LoopActionNameTable[offset]
    
    return self.actionDurationMap[modelId][actionName] or 0
end

function SkillManager:getSpineDieDuration(modelId)
    return self.actionDurationMap[modelId]["Die"] or 0
end

-- 挂机时, 人物当前状态是否满足技能释放的释放条件
function SkillManager:canUseSkillWhileHangup(position, injuryRange, skillMeta)
    if not skillMeta then
        return false
    end
    
    local selfAgent = AgentManager:getSelfAgent()
    if not selfAgent then
        return false
    end

    -- 注意是否攻击范围内, 大部分技能可以简单的判断, 是否在技能的攻击距离内, 此时判断范围为攻击距离的圆形范围
    local distance = (skillMeta:getDistance() + injuryRange) * 0.01
    local direction = position - selfAgent:getPosition()
    local satisfy = direction:SqrMagnitude() < distance * distance

    -- 由于部分技能在攻击时不进行自动转向, 挂机时满足了攻击距离, 但是不自动转向, 部分技能表现上会打不到怪, 这时需要进行转向使其面向怪物
    if satisfy and not skillMeta:getCanTurn() then
        selfAgent:setDirection(direction)
    end

    return satisfy
end

function SkillManager:getSkillEffect(skillId)
    return gModule.Skill:combineQueryById(skillId):getSkillEffect()
end

function SkillManager:addSkillObject(skillObject)
    SkillObjectKey = SkillObjectKey + 1
    self.skillObjectTable[SkillObjectKey] = skillObject
    return SkillObjectKey
end

function SkillManager:getSkillObject(key)
    return self.skillObjectTable[key]
end

function SkillManager:addOnUpdateFunctor(functor, delay)
    self.skillUpdateFunctor[functor] = {delay = delay or 0, startTime = gModule.Server:getSecond()}
end

function SkillManager:removeOnUpdateFunctor(functor)
    self.skillUpdateFunctor[functor] = nil
end

-- 客户端是否存在天赋模板
function SkillManager:existInnatFunc(funcIndex)
    return gConst.Game.InnatFunc[funcIndex] and true or false
end


-- 通过天赋Id 获取对应的天赋模板
function SkillManager:getInnatFunc(innatId)
    local innatMeta = gDao.Innat:queryById(innatId)
    if not innatMeta then
        return
    end

    local funcIndex = innatMeta:getIndex()
    if not self:existInnatFunc(funcIndex) then
        return
    end

    local func = self.innatFuncMap[funcIndex]

    if not func then
        printWarning(string.format("找不到天赋对应的天赋模板 天赋Id[%d] 模板Id [%d]", innatId, funcIndex))
    end

    return func
end

--[[
服务器通知 执行天赋技能
]]
function SkillManager:handleExecInnatSkill(agent, innatOnlyId, extraParams)
    local innatLvMeta = gDao.InnatLv:queryById(innatOnlyId)
    if not innatLvMeta then
        return
    end

    local innatId = innatLvMeta:getInnatId()

    local innatFunc = self:getInnatFunc(innatId)
    if not innatFunc then
        return
    end

    innatFunc:onExec(agent, innatLvMeta:getInnatEffect(), extraParams)
end


function SkillManager:getEffectData(prefabPath)
    return self.effsData[prefabPath]
end

local SkillObjectLaunchMode = gConst.Skill.SkillObjectLaunchMode
local tempVector2 = Vector2Fast.new(0, 0)
--[[
计算发射点位置
mode 发射点模式
index 索引
count 发射总数
offsetLenMax
offsetAngle
ptrAngle 
]]
function SkillManager:calcFireLaunchPosition(launchParam)
    if not launchParam then
        return 0, 0
    end

    local launchMode = launchParam.mode
    if not launchMode then
        return 0, 0
    end

    if launchMode == SkillObjectLaunchMode.CircleEquallyDivide then
        local currentAngle = 360 / launchParam.count * (launchParam.index - 1) + launchParam.offsetAngle

        -- 计算出发射朝向
        tempVector2:Set(Mathf.Sin(Mathf.Deg2Rad * currentAngle), Mathf.Cos(Mathf.Deg2Rad * currentAngle))
        tempVector2:SetNormalize()

        -- 计算出发射目标点
        tempVector2:Mul(launchParam.offsetLenMax)
        tempVector2:Div(BATTLE_HERO_SCALE)

        return tempVector2.x, tempVector2.y
    
    end

    return 0, 0
end

--[[
计算发射点位置
mode 发射点模式
index 索引
count 发射总数
offsetLenMax
offsetAngle
ptrAngle 
direction
]]
function SkillManager:calcFireLaunchDirection(launchParam)
    if not launchParam then
        return 0, 0, 0
    end

    local launchMode = launchParam.mode
    if not launchMode then
        return 0, 0, 0
    end

    if launchMode == SkillObjectLaunchMode.PointToEquallyAngle then
        local angleLen
        local index = launchParam.index - 1
        if index == 0 then
            angleLen = 0
        else
            if index % 2 == 0 then
                angleLen = -math.floor(index / 2)
            else
                angleLen = math.floor(index / 2) + 1
            end
        end

        local currentAngle = (launchParam.ptrAngle / launchParam.count) * angleLen
        local direction = launchParam.direction

        local tempVector3 = CSharp.GeometryAlgorithmHelp.PointToEquallyAngle(0, currentAngle + launchParam.offsetAngle, 0, direction.x, direction.y, direction.z)

        return tempVector3.x, tempVector3.y, tempVector3.z
    end

    return 0, 0, 0
end

return SkillManager
