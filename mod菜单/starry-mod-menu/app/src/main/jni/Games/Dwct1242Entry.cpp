// 动物餐厅12.42：本地两开关，私有modern接线。
#include "ModEntry.h"
#include "DwctMod.h"
static const char* kFeatures[]={"Category_辅助","Toggle_免广告","Toggle_免内购"};
static void Tick(const bool* f,const int*){Dwct_Tick(f);}
static void Clear(){Dwct_OnFeatureChanged(DWCT_FEAT_NO_AD,false);Dwct_OnFeatureChanged(DWCT_FEAT_NO_IAP,false);}
extern "C" const StarryGameMod* const GameMod_Get(){
 static const StarryGameMod mod={
 .tag="Dwct1242",.authGameId="dwct",.featureList=kFeatures,.featureCount=3,
 .waitLib="libcocos2djs.so",.initDelayMs=1000000,.initRetryMax=120,
 .initRetryIntervalMs=1000000,.tickIntervalMs=500000,
 .onLoad=nullptr,.tryInit=Dwct_TryInit,.isReady=Dwct_IsReady,.tick=Tick,
 .onFeatureChanged=Dwct_OnFeatureChanged,.onValueChanged=nullptr,.onInputChanged=nullptr,
 .forceClear=Clear,.onApplyPlayPatch=nullptr};return &mod;
}
