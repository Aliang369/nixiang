-- @Date    : 2023-12-22 14:34:21
-- @Author  : cjx
-- @Desc    :  
local base = import(".ElementBase2")
local HeroDamage = class("HeroDamage", base)
HeroDamage.PrefabPath = "Art/WorldMap/Element/OtherDamageSelf.prefab"

function HeroDamage:setText(value)
    base.setText(self, string.format("%.0f", math.abs(value)))
end

return HeroDamage