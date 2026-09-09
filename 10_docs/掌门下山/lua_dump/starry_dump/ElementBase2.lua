-- @Date    : 2024-07-09 15:47:28
-- @Author  : mars
-- @Desc    : 新做的element无animator


local UnityLayers = UnityLayers
local POOL_POSITION = Vector2.new(-10, -10)

local function innerSetPosition(self)
    if self.position == nil or not self.__instantiate then
        return
    end

    self.transform:SetLocalPositionXYZ(self.position.x, (self.height or 0), -self.position.y)
end

local function innerSetText(self)
    if self.text == nil or self.unity_text == nil then
        return
    end

    self.unity_text.text = self.text
end

local function innerStartAnim(self)
    if not self.anim or self.unity_BattleDamageMono == nil then
        return
    end

    self.unity_BattleDamageMono:ShowDamage(self.damageDir.x, -self.damageDir.y, self.position.x, -self.position.y, self.height, self.curDuration, self.scale)
end

local ElementBase2 = class("ElementBase2")
ElementBase2.__instantiate = nil
ElementBase2.running = false
ElementBase2.duration = 1
ElementBase2.shortDuration = 0.4
ElementBase2.curDuration = 0
ElementBase2.prefix = "/" -- 会被字体中进行转换面对应的前缀
ElementBase2.scale = 1

function ElementBase2:ctor()
    self.__instantiate = false
    GameObjectPool:getGameObjectAsync(self.PrefabPath, Bind(self, self.onGoInstantiate))
end

function ElementBase2:dtor()
    self:onDestroy()
end

function ElementBase2:recycle()
    self:setPosition(POOL_POSITION)
end

function ElementBase2:onGoInstantiate(gameObject)
    if gameObject == nil or self.__instantiate == nil then
        return false
    end

    self:onInstantiate(gameObject)

    return true
end

function ElementBase2:onInstantiate(gameObject)
    if self.__instantiate == nil then
        return
    end

    local worldAgent = AgentManager:getWorldAgent()
    local parentTrans
    if worldAgent and worldAgent.getDamageTransform and self:isCrit() then 
        parentTrans = worldAgent:getCritDamageTransform()
    elseif worldAgent and worldAgent.getDamageTransform then
        parentTrans = worldAgent:getDamageTransform()
    end

    self.__instantiate = true

    self.gameObject = gameObject
    self.transform = self.gameObject.transform
    self.gameObject:SetAllLayer(UnityLayers.SceneL5)
    self.transform:SetParent(parentTrans)

    self.unity_BattleDamageMono = self.gameObject:GetComponent(CSharpType.BattleDamageMono)
    assert(not IsNull(self.unity_BattleDamageMono), "ElementBase2:onInstantiate unity_BattleDamageMono init error")

    self:initText()

    innerSetText(self)
    innerStartAnim(self)

    return true
end

function ElementBase2:onEnable(position, height, text, damageDir, isBigObject)
    self.position = position
    self.height = height
    self:setText(text or self.text)

    if self.unity_BattleDamageMono then
        self.unity_BattleDamageMono.enabled = true
    end
    self.elapse = 0
    self.curDuration = AgentManager:getWorldAgent():getMonsterNumber() >= 10 and self.shortDuration or self.duration
    -- if not damageDir then
    --     printWarning("ElementBase2 onEnable no direction", self.__cname, position, height, text, damageDir, isBigObject)
    -- end
    self.damageDir = damageDir or Vector3.zero

    self:startAnim()

    if isBigObject then
        self.scale = 1
    elseif isBigObject == false then
        self.scale = 0.7
    else
        printWarning("ElementBase2 onEnable no isBigObject value", self.__cname, position, height, text, damageDir, isBigObject)
    end
end

function ElementBase2:onDisable()
end

function ElementBase2:onRecycle()
    self.running = false
    if self.unity_BattleDamageMono then
        self.unity_BattleDamageMono.enabled = false
    end
    self.anim = false
    lt.BattleElementManager:recycleElement(self)
end

function ElementBase2:onDestroy()
    if not IsNull(self.gameObject) then
        GameObjectPool:recycleGameObject(self.PrefabPath, self.gameObject)
    end

    self.gameObject = nil
    self.transform = nil

    self.unity_text = nil
    self.unity_BattleDamageMono = nil
end

function ElementBase2:setPosition(position, height)
    self.position = position
    self.height = height

    innerSetPosition(self)
end

function ElementBase2:isCrit()
    return false
end

function ElementBase2:initText()
    self.unity_text = self.gameObject:Find("Text"):GetComponent(UEType.TextMesh)

    assert(not IsNull(self.unity_text), "ElementBase2:initText unity_text init error")
end

function ElementBase2:onUpdate()
    if not self.__instantiate then
        return
    end
    
    self.elapse = self.elapse + Time.deltaTime
end

function ElementBase2:isOver()
    return self.elapse >= self.curDuration
end

function ElementBase2:setText(text)
    self.text = text

    innerSetText(self)
end

function ElementBase2:startAnim()
    self.anim = true
    innerStartAnim(self)
end

return ElementBase2
