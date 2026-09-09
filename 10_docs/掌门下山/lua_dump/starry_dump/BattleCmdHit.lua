-- @Date    : 2024-06-17 15:22:02
-- @Author  : Zhaoci (82552284@qq.com)
-- @Desc    : 

local BeHitType = gConst.Game.BeHitType
local HitActionType = gConst.Game.HitActionType

local BattleCmdHit = class("BattleCmdHit")

function BattleCmdHit:ctor(data)
    self.data = data
end

function BattleCmdHit:getData()
    return self.data
end

function BattleCmdHit:getType()
    return gConst.Game.CMD_HIT
end

function BattleCmdHit:getBeHitType()
    return self.data.beHitType
end

function BattleCmdHit:getSkillId()
    return self.data.skillId
end

function BattleCmdHit:isWeakHit()
    return self.data.beHitType == BeHitType.Weak
end

function BattleCmdHit:isStrongHit()
    return self.data.beHitType == BeHitType.Strong
end

function BattleCmdHit:useOnceAction()
    return self.data.hitActionType == HitActionType.Once
end

function BattleCmdHit:useLoopAction()
    return self.data.hitActionType == HitActionType.Loop
end

function BattleCmdHit:getLoopActionDuration()
    if DEBUG and self:useOnceAction() then
        printError("BattleCmdHit:getLoopDuration 非循环动作不能获取循环动作的时间")
        return 0
    end

    return self.data.loopActionDuration
end

return BattleCmdHit
