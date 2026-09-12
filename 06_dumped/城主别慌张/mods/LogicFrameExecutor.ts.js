/* ===== chunks:///_virtual/LogicFrameExecutor.ts ===== */
System.register("chunks:///_virtual/LogicFrameExecutor.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./RoomDef.ts", "./ILogicFrame.ts"], (function(t) {
    var e, n, u, i;
    return {
        setters: [function(t) {
            e = t.createForOfIteratorHelperLoose
        }, function(t) {
            n = t.cclegacy
        }, function(t) {
            u = t.InputType
        }, function(t) {
            i = t.isEmptyFrame
        }],
        execute: function() {
            n._RF.push({}, "0cf1exi5R1BgbjMlpArkJiB", "LogicFrameExecutor", void 0);
            t("LogicFrameExecutor", function() {
                function t() {
                    this.battleLogic = null, this.currentState = null, this.previousState = null, this.executedFrameCount = 0, this.onFrameExecuted = null
                }
                var n = t.prototype;
                return n.setBattleLogic = function(t) {
                    this.battleLogic = t
                }, n.executeFrame = function(t) {
                    if (this.previousState = this.currentState, !i(t))
                        for (var n, u = e(t.inputs); !(n = u()).done;) {
                            var a = n.value;
                            this.applyInput(a)
                        }
                    this.advanceLogic(), this.currentState = this.captureState(t.frameId), this.executedFrameCount++, this.onFrameExecuted && this.currentState && this.onFrameExecuted(t, this.currentState)
                }, n.applyInput = function(t) {
                    switch (t.inputType) {
                        case u.Move:
                            this.handleMoveInput(t);
                            break;
                        case u.Attack:
                            this.handleAttackInput(t);
                            break;
                        case u.Skill:
                            this.handleSkillInput(t);
                            break;
                        case u.Target:
                            this.handleTargetInput(t);
                            break;
                        case u.Pause:
                            this.handlePauseInput(t);
                            break;
                        case u.Resume:
                            this.handleResumeInput(t);
                            break;
                        default:
                            Logger.errorBusiness("[LogicFrameExecutor] Unknown input type: " + t.inputType)
                    }
                }, n.advanceLogic = function() {
                    this.battleLogic
                }, n.captureState = function(t) {
                    return this.battleLogic, {
                        frameId: t,
                        timestamp: Date.now(),
                        entities: []
                    }
                }, n.handleMoveInput = function(t) {}, n.handleAttackInput = function(t) {}, n.handleSkillInput = function(t) {}, n.handleTargetInput = function(t) {}, n.handlePauseInput = function(t) {}, n.handleResumeInput = function(t) {}, n.getCurrentState = function() {
                    return this.currentState
                }, n.getPreviousState = function() {
                    return this.previousState
                }, n.getExecutedFrameCount = function() {
                    return this.executedFrameCount
                }, n.setFrameExecutedCallback = function(t) {
                    this.onFrameExecuted = t
                }, n.reset = function() {
                    this.currentState = null, this.previousState = null, this.executedFrameCount = 0
                }, n.destroy = function() {
                    this.reset(), this.battleLogic = null, this.onFrameExecuted = null
                }, t
            }());
            n._RF.pop()
        }
    }
}));