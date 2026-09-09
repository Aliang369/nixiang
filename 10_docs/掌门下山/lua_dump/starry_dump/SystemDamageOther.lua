-- @Date    : 2023-12-21 21:03:14
-- @Author  : cjx
-- @Desc    :  

local base = import(".ElementBase2")
local SystemDamageOther = class("SystemDamageOther", base)
SystemDamageOther.PrefabPath = "Art/WorldMap/Element/SystemDamageOther.prefab"

function SystemDamageOther:setText(value)
    base.setText(self, string.format("%.0f", math.abs(value)))
end

return SystemDamageOther