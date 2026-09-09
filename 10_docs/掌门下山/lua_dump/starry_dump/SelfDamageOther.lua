-- @Date    : 2021-12-17 17:39:01
-- @Author  : ZhaoCi
-- @Desc    :

local base = import(".ElementBase2")
local SelfDamageOther = class("SelfDamageOther", base)
SelfDamageOther.PrefabPath = "Art/WorldMap/Element/SelfDamageOther.prefab"

function SelfDamageOther:setText(value)
    base.setText(self, string.format("%.0f", math.abs(value)))
end

return SelfDamageOther
