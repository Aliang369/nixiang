
package offlinetest;

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentManager;
import android.content.Intent;
import android.util.Log;

import com.taptap.sdk.LoginRequest;
import com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity;
import com.taptap.sdk.ui.TapTapActivity;
import com.starry.magic.core.GameHook;
import com.starry.magic.core.GameHooks;
import com.starry.magic.core.TapTapWebLogin;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;

/**
 * 星光框架 TapTap 强制网页登录 + 单游戏 SPI 离线回归（纯 JVM，不碰设备/网络）。
 * 被测对象是真实源码：core/TapTapWebLogin.java、core/GameHook.java、core/GameHooks.java、
 * games/yntt/GameHookImpl.java，配合 fixtures/ 下的 Android/TapSDK 假件编译执行。
 */
public final class OfflineLoginTest {

    private static final String LEGACY = "com.starry.magic.sign.YnttLoginRequest";
    private static final String PKG_YNT = "com.qinglv.Immortal";

    private static int passed = 0;
    private static final List<String> failures = new ArrayList<>();

    public static void main(String[] args) throws Exception {
        Path menuRoot = Paths.get(System.getProperty("starry.menu.root", ".")).toAbsolutePath().normalize();
        Path repoRoot = Paths.get(System.getProperty("starry.repo.root", menuRoot.getParent().toString())).toAbsolutePath().normalize();
        System.out.println("== 星光登录离线回归 ==");
        System.out.println("   menuRoot = " + menuRoot);
        System.out.println("   repoRoot = " + repoRoot);
        System.out.println("   JDK      = " + System.getProperty("java.version") + " (" + System.getProperty("java.vm.name") + ")");

        t01_nullActivity();
        t02_nonLoginActivity();
        t03_legacyClassNullOrEmpty();
        t04_legacyRequestInjected();
        t05_legacyCopyProps();
        t06_legacyIdempotent();
        t07_newImplLoginTypeRewritten();
        t08_newImplAlreadyWeb();
        t09_sanitizeHandlers();
        t10_oldImplFallback();
        t11_newImplFallback();
        t12_missingPlumbingNoCrash();
        t13_spiResolvesGameImpl();
        t14_spiNativeWins();
        t15_spiIllegalAndEmpty();
        t16_spiNoSideEffectForOtherPackage();
        t17_r8RulesSemanticallyEqual(menuRoot);
        t18_smaliLegacyRequestReachable(menuRoot);
        t19_twoLifecycleEntries(menuRoot);
        t20_nativeJniPathsMatchJava(menuRoot);

        System.out.println();
        if (failures.isEmpty()) {
            System.out.println("[offline-login] PASS " + passed + " 组断言");
            return;
        }
        System.out.println("[offline-login] FAIL " + failures.size() + " 组（通过 " + passed + "）");
        for (String f : failures) System.out.println("  x " + f);
        System.exit(1);
    }

    private static void t01_nullActivity() {
        try {
            TapTapWebLogin.onPreCreated(null, null);
            TapTapWebLogin.onPreCreated(null, LEGACY);
            TapTapWebLogin.onCreated(null);
            ok("T01 null Activity：三条入口都静默返回，不抛");
        } catch (Throwable t) {
            bad("T01 null Activity 抛异常: " + t);
        }
    }

    private static void t02_nonLoginActivity() {
        Activity a = new Activity();
        a.setIntent(intentWith(baseRequest()));
        try {
            TapTapWebLogin.onPreCreated(a, LEGACY);
            TapTapWebLogin.onCreated(a);
            ok("T02 非 TapTap Activity：两条路都不动 intent、不抛");
        } catch (Throwable t) {
            bad("T02 非 TapTap Activity 抛异常: " + t);
        }
    }

    private static void t03_legacyClassNullOrEmpty() {
        try {
            TapTapActivity act = oldActivityWithRequest();
            Object before = requestOf(act);
            TapTapWebLogin.onPreCreated(act, null);
            Object afterNull = requestOf(act);
            TapTapWebLogin.onPreCreated(act, "");
            Object afterEmpty = requestOf(act);
            if (before == afterNull && before == afterEmpty) {
                ok("T03 legacy 类名 null/空：跳过旧版替换（request 实例不变）");
            } else {
                bad("T03 legacy 类名 null/空 仍替换了 request");
            }
        } catch (Throwable t) {
            bad("T03 legacy 类名 null/空 抛异常: " + t);
        }
    }

    private static void t04_legacyRequestInjected() {
        try {
            TapTapActivity act = oldActivityWithRequest();
            TapTapWebLogin.onPreCreated(act, LEGACY);
            Object req = requestOf(act);
            if (req == null) {
                bad("T04 旧版路：request extra 变 null");
                return;
            }
            if (!LEGACY.equals(req.getClass().getName())) {
                bad("T04 旧版路：未替换成 " + LEGACY + "，实为 " + req.getClass().getName());
                return;
            }
            try {
                int code = ((LoginRequest) req).getRequestCode();
                bad("T04 旧版路：注入类 getRequestCode() 未抛异常（返回 " + code + "）");
            } catch (IllegalStateException expected) {
                ok("T04 旧版路：request 由参数注入并被替换，getRequestCode() 抛 IllegalStateException");
            }
        } catch (Throwable t) {
            bad("T04 旧版路 抛异常: " + t);
        }
    }

    private static void t05_legacyCopyProps() {
        try {
            LoginRequest base = baseRequest();
            base.setCodeChallenge("cc-123");
            base.setCodeChallengeMethod("S256");
            base.setResponseType("code");
            base.setRedirectUri("taptap://cb");
            TapTapActivity act = new TapTapActivity();
            act.setPackageNameForTest(PKG_YNT);
            act.setIntent(intentWith(base));
            act.loginRequest = base;
            TapTapWebLogin.onPreCreated(act, LEGACY);
            LoginRequest forced = requestOf(act);
            boolean sameState = "st-1".equals(forced.getState());
            boolean samePerms = forced.getPermissions() != null && forced.getPermissions().length == 1;
            boolean copied = "cc-123".equals(forced.getCodeChallenge())
                    && "S256".equals(forced.getCodeChallengeMethod())
                    && "code".equals(forced.getResponseType())
                    && "taptap://cb".equals(forced.getRedirectUri());
            if (sameState && samePerms && copied) {
                ok("T05 旧版路：state/permissions 走构造、字符串属性逐个搬运");
            } else {
                bad("T05 旧版路 属性搬运不完整 state=" + sameState + " perms=" + samePerms + " str=" + copied);
            }
        } catch (Throwable t) {
            bad("T05 旧版路 属性搬运 抛异常: " + t);
        }
    }

    private static void t06_legacyIdempotent() {
        try {
            TapTapActivity act = oldActivityWithRequest();
            TapTapWebLogin.onPreCreated(act, LEGACY);
            Object first = requestOf(act);
            TapTapWebLogin.onPreCreated(act, LEGACY);
            Object second = requestOf(act);
            if (first == second) {
                ok("T06 旧版路：重复调用幂等（已强制过就不再替换）");
            } else {
                bad("T06 旧版路：重复调用又换了一次实例（非幂等）");
            }
        } catch (Throwable t) {
            bad("T06 旧版路 幂等 抛异常: " + t);
        }
    }


    private static void t07_newImplLoginTypeRewritten() {
        try {
            TapTapLoginActivity act = new TapTapLoginActivity();
            act.setPackageNameForTest(PKG_YNT);
            LoginRequest req = baseRequest();
            req.loginType = LoginRequest.MOBILE_CLIENT;
            act.request = req;
            Intent it = new Intent();
            it.putExtra("com_taptap_sdk_login_request", req);
            act.setIntent(it);
            TapTapWebLogin.onPreCreated(act, null);
            if (LoginRequest.EMBEDDED_WEB.equals(req.loginType)) {
                ok("T07 新版路：onCreate 前 loginType mobile_client → embedded_web");
            } else {
                bad("T07 新版路：loginType 仍为 " + req.loginType);
            }
        } catch (Throwable t) {
            bad("T07 新版路 抛异常: " + t);
        }
    }

    private static void t08_newImplAlreadyWeb() {
        try {
            TapTapLoginActivity act = new TapTapLoginActivity();
            act.setPackageNameForTest(PKG_YNT);
            LoginRequest req = baseRequest();
            req.loginType = LoginRequest.EMBEDDED_WEB;
            Intent it = new Intent();
            it.putExtra("com_taptap_sdk_login_request", req);
            act.setIntent(it);
            Log.LINES.clear();
            TapTapWebLogin.onPreCreated(act, null);
            boolean unchanged = LoginRequest.EMBEDDED_WEB.equals(req.loginType);
            boolean noRewrite = true;
            for (String s : Log.LINES) if (s.contains("-> embedded_web")) noRewrite = false;
            if (unchanged && noRewrite) {
                ok("T08 新版路：已是 embedded_web 时不重写（幂等）");
            } else {
                bad("T08 新版路 已 web 仍重写 unchanged=" + unchanged + " log=" + Log.LINES);
            }
        } catch (Throwable t) {
            bad("T08 新版路 幂等 抛异常: " + t);
        }
    }

    private static void t09_sanitizeHandlers() {
        try {
            com.taptap.sdk.login.ui.TapTapLoginActivity act = legacyHandlersActivity();
            Object client = Class.forName("com.taptap.sdk.login.internal.handlers.client.ClientLoginHandler")
                    .getDeclaredConstructor().newInstance();
            Object phone = Class.forName("com.taptap.sdk.login.internal.handlers.phone.PhoneLoginHandler")
                    .getDeclaredConstructor().newInstance();
            Object cloud = Class.forName("com.taptap.sdk.login.internal.handlers.cloud.CloudLoginHandler")
                    .getDeclaredConstructor().newInstance();
            act.impl.loginHandlers = new Object[]{cloud, client, phone};
            TapTapWebLogin.onPreCreated(act, null);
            Object[] hs = act.impl.loginHandlers;
            boolean cloudKept = hs[0] == cloud;
            boolean clientDead = canHandleFalse(hs[1]);
            boolean phoneDead = canHandleFalse(hs[2]);
            if (cloudKept && clientDead && phoneDead) {
                ok("T09 sanitize：Client/Phone handler 变 canHandle=false，Cloud 原样保留");
            } else {
                bad("T09 sanitize 不符 cloud=" + cloudKept + " client=" + clientDead + " phone=" + phoneDead);
            }
        } catch (Throwable t) {
            bad("T09 sanitize 抛异常: " + t);
        }
    }

    private static boolean canHandleFalse(Object handler) throws Exception {
        if (handler == null) return false;
        Class<?> itf = Class.forName("com.taptap.sdk.login.internal.handlers.LoginHandler");
        if (!itf.isInstance(handler)) return false;
        Object r = itf.getMethod("canHandle", LoginRequest.class).invoke(handler, baseRequest());
        return Boolean.FALSE.equals(r);
    }

    private static void t10_oldImplFallback() {
        try {
            com.taptap.sdk.login.ui.TapTapLoginActivity act = legacyHandlersActivity();
            TapTapWebLogin.onCreated(act);
            boolean called = act.impl.fallbackCalls == 1;
            com.taptap.sdk.login.ui.TapTapLoginActivity act2 = legacyHandlersActivity();
            act2.fm = new FragmentManager() {
                @Override public Fragment findFragmentByTag(String tag) {
                    return new com.taptap.sdk.login.internal.handlers.web.WebLoginFragment();
                }
            };
            TapTapWebLogin.onCreated(act2);
            boolean skipped = act2.impl.fallbackCalls == 0;
            if (called && skipped) {
                ok("T10 onCreated 旧版兜底：非 Web 时 fallbackToWeb，已是 Web 时跳过");
            } else {
                bad("T10 onCreated 旧版兜底不符 called=" + called + " skipped=" + skipped);
            }
        } catch (Throwable t) {
            bad("T10 onCreated 旧版兜底 抛异常: " + t);
        }
    }

    private static void t11_newImplFallback() {
        try {
            TapTapLoginActivity act = new TapTapLoginActivity();
            act.setPackageNameForTest(PKG_YNT);
            LoginRequest req = baseRequest();
            act.request = req;
            TapTapWebLogin.onCreated(act);
            boolean called = act.fallbackCalls == 1 && act.fallbackArg == req;
            TapTapLoginActivity act2 = new TapTapLoginActivity();
            act2.setPackageNameForTest(PKG_YNT);
            act2.request = baseRequest();
            act2.fm = new FragmentManager() {
                @Override public Fragment findFragmentByTag(String tag) {
                    return new com.taptap.sdk.login.impl.ui.fragment.WebLoginFragment();
                }
            };
            TapTapWebLogin.onCreated(act2);
            boolean skipped = act2.fallbackCalls == 0;
            if (called && skipped) {
                ok("T11 onCreated 新版兜底：非 Web 时 fallbackToWeb(request)，已是 Web 时跳过");
            } else {
                bad("T11 onCreated 新版兜底不符 called=" + called + " skipped=" + skipped);
            }
        } catch (Throwable t) {
            bad("T11 onCreated 新版兜底 抛异常: " + t);
        }
    }

    private static void t12_missingPlumbingNoCrash() {
        try {
            TapTapActivity oldAct = new TapTapActivity();
            oldAct.setPackageNameForTest(PKG_YNT);
            oldAct.impl = null;
            TapTapWebLogin.onCreated(oldAct);
            TapTapWebLogin.onPreCreated(oldAct, LEGACY);

            TapTapLoginActivity newAct = new TapTapLoginActivity();
            newAct.setPackageNameForTest(PKG_YNT);
            newAct.request = null;
            TapTapWebLogin.onCreated(newAct);
            TapTapWebLogin.onPreCreated(newAct, null);

            TapTapActivity noIntent = new TapTapActivity();
            noIntent.setPackageNameForTest(PKG_YNT);
            TapTapWebLogin.onPreCreated(noIntent, LEGACY);
            ok("T12 半初始化态（impl=null / request=null / intent=null）不崩、静默降级");
        } catch (Throwable t) {
            bad("T12 半初始化态 抛异常: " + t);
        }
    }


    private static void t13_spiResolvesGameImpl() {
        try {
            setGame("yntt", null);
            GameHook h = GameHooks.get();
            String cn = h.getClass().getName();
            if (!"com.starry.magic.games.yntt.GameHookImpl".equals(cn)) {
                bad("T13 SPI：game_id=yntt 未解析到 GameHookImpl，实为 " + cn);
                return;
            }
            if (!PKG_YNT.equals(h.targetPackage())) {
                bad("T13 SPI：targetPackage() 期望 " + PKG_YNT + "，实为 " + h.targetPackage());
                return;
            }
            ok("T13 SPI：game_id=yntt → games.yntt.GameHookImpl，targetPackage 正确");
        } catch (Throwable t) {
            bad("T13 SPI 解析 抛异常: " + t);
        }
    }

    private static void t14_spiNativeWins() {
        try {
            // prefs 指向一个没有实现的 id，native 报 yntt ⇒ 能解析出 yntt 就证明 native 优先
            setGame("nosuchgame", "yntt");
            String cn = GameHooks.get().getClass().getName();
            if ("com.starry.magic.games.yntt.GameHookImpl".equals(cn)) {
                ok("T14 SPI：native 烘焙值优先于 prefs（prefs=无实现 id，native=yntt 仍解析成功）");
            } else {
                bad("T14 SPI：native 优先级失效，解析到 " + cn);
            }
        } catch (Throwable t) {
            bad("T14 SPI native 优先 抛异常: " + t);
        }
    }

    private static void t15_spiIllegalAndEmpty() {
        try {
            Set<String> seen = new LinkedHashSet<>();
            String[] ids = new String[]{null, "", "Yntt", "yntt/../x", "yntt-x", "nosuchgame"};
            for (String gid : ids) {
                setGame(gid, null);
                GameHook h = GameHooks.get();
                if (h == null) {
                    bad("T15 SPI：" + gid + " 解析返回 null（契约要求永不 null）");
                    return;
                }
                seen.add(h.getClass().getName());
            }
            boolean allEmpty = seen.size() == 1 && seen.contains("com.starry.magic.core.GameHooks$1");
            if (!allEmpty) {
                bad("T15 SPI：非法/未知 game_id 未全部退化为空实现，实为 " + seen);
                return;
            }
            setGame(null, null);
            GameHook empty = GameHooks.get();
            Activity a = new Activity();
            empty.onActivityPreCreated(a);
            empty.onActivityCreated(a);
            empty.onAttachBaseContext(new android.app.Application());
            empty.onApplicationCreate(new android.app.Application());
            if (empty.onHelperAttach(null) || !"".equals(empty.targetPackage()) || empty.usesPackagedSoAlias()) {
                bad("T15 SPI：空默认实现的默认值不符（应 false / 空串）");
                return;
            }
            ok("T15 SPI：null/空/大写/路径注入/未知 id 一律退化空实现，默认方法可安全调用");
        } catch (Throwable t) {
            bad("T15 SPI 非法 id 抛异常: " + t);
        }
    }

    private static void t16_spiNoSideEffectForOtherPackage() {
        try {
            setGame("yntt", null);
            GameHook h = GameHooks.get();
            TapTapActivity target = oldActivityWithRequest();
            target.setPackageNameForTest(PKG_YNT);
            h.onActivityPreCreated(target);
            boolean injected = LEGACY.equals(requestOf(target).getClass().getName());
            TapTapActivity other = oldActivityWithRequest();
            other.setPackageNameForTest("com.other.game");
            h.onActivityPreCreated(other);
            boolean untouched = !LEGACY.equals(requestOf(other).getClass().getName());
            if (injected && untouched) {
                ok("T16 SPI：目标包生效、非目标包零副作用（request 类名由 games/yntt 注入）");
            } else {
                bad("T16 SPI 生效判据不符 injected=" + injected + " untouched=" + untouched);
            }
        } catch (Throwable t) {
            bad("T16 SPI 副作用 抛异常: " + t);
        }
    }

    private static void t17_r8RulesSemanticallyEqual(Path menuRoot) {
        Path app = menuRoot.resolve("app");
        Path a = app.resolve("proguard-rules.pro");
        Path b = app.resolve("inject-dex-rules.pro");
        try {
            Set<String> ra = keepRules(a);
            Set<String> rb = keepRules(b);
            if (ra.isEmpty() || rb.isEmpty()) {
                bad("T17 R8：规则文件里没有 core/games 的 keep 条目（a=" + ra.size() + " b=" + rb.size() + "）");
                return;
            }
            if (!ra.equals(rb)) {
                Set<String> onlyA = new LinkedHashSet<>(ra);
                onlyA.removeAll(rb);
                Set<String> onlyB = new LinkedHashSet<>(rb);
                onlyB.removeAll(ra);
                bad("T17 R8：两文件不一致 only-in-proguard=" + onlyA + " only-in-inject=" + onlyB);
                return;
            }
            String joined = String.join("\n", ra);
            boolean keepnamesGames = joined.contains("-keepnames class com.starry.magic.games.**");
            boolean keepWeb = joined.contains("-keep class com.starry.magic.core.TapTapWebLogin");
            boolean noLegacy = !joined.contains("TapLoginForceWeb");
            if (keepnamesGames && keepWeb && noLegacy) {
                ok("T17 R8：两文件 core/games keep 集相同（" + ra.size() + " 条），含 games.** keepnames 与 core.TapTapWebLogin，无旧类");
            } else {
                bad("T17 R8：内容断言不符 keepnamesGames=" + keepnamesGames + " keepWeb=" + keepWeb + " noLegacy=" + noLegacy);
            }
        } catch (Throwable t) {
            bad("T17 R8 读取失败: " + t);
        }
    }

    private static Set<String> keepRules(Path p) throws IOException {
        Set<String> out = new LinkedHashSet<>();
        for (String raw : Files.readAllLines(p, StandardCharsets.UTF_8)) {
            String line = raw.trim();
            if (!line.startsWith("-keep")) continue;
            String norm = line.replaceAll("\\s+", " ");
            if (norm.contains("com.starry.magic.core") || norm.contains("com.starry.magic.games")) out.add(norm);
        }
        return out;
    }

    private static void t18_smaliLegacyRequestReachable(Path menuRoot) {
        Path smali = menuRoot.resolve("yntt_smali/com/starry/magic/sign/YnttLoginRequest.smali");
        Path impl = menuRoot.resolve("app/src/main/java/com/starry/magic/games/yntt/GameHookImpl.java");
        try {
            if (!Files.isRegularFile(smali)) {
                bad("T18 smali：缺 " + smali);
                return;
            }
            String s = Files.readString(smali, StandardCharsets.UTF_8);
            String src = Files.readString(impl, StandardCharsets.UTF_8);
            String declared = fieldString(src, "LEGACY_REQUEST_CLASS");
            if (declared == null) {
                bad("T18 smali：GameHookImpl 里找不到 LEGACY_REQUEST_CLASS 字面量");
                return;
            }
            String descriptor = ".class public L" + declared.replace('.', '/') + ";";
            boolean cls = s.contains(descriptor);
            boolean sup = s.contains(".super Lcom/taptap/sdk/LoginRequest;");
            boolean ctor = s.contains(" constructor <init>(Ljava/lang/String;[Ljava/lang/String;)V");
            boolean throwsInCode = smaliMethod(s, "public getRequestCode()I").contains("throw ");
            if (!(cls && sup && ctor && throwsInCode)) {
                bad("T18 smali 结构不符 cls=" + cls + " sup=" + sup + " ctor=" + ctor + " throw=" + throwsInCode);
                return;
            }
            Class<?> c = Class.forName(declared);
            c.getConstructor(String.class, String[].class);
            ok("T18 smali：" + declared + " 在 yntt_smali 可达，super/ctor/抛异常结构与反射调用一致");
        } catch (Throwable t) {
            bad("T18 smali 校验 抛异常: " + t);
        }
    }

    private static void t19_twoLifecycleEntries(Path menuRoot) {
        Path base = menuRoot.resolve("app/src/main/java/com/starry/magic");
        String[] entries = {"StarryApp.java", "StarryInitProvider.java"};
        List<String> problems = new ArrayList<>();
        for (String name : entries) {
            String s;
            try {
                s = Files.readString(base.resolve(name), StandardCharsets.UTF_8);
            } catch (IOException e) {
                problems.add(name + " 读不到: " + e);
                continue;
            }
            String pre = bodyOf(s, "void onActivityPreCreated(");
            String cre = bodyOf(s, "void onActivityCreated(");
            if (pre.isEmpty() || !pre.contains("GameHooks.get()") || !pre.contains("onActivityPreCreated(a)")) {
                problems.add(name + " 的 onActivityPreCreated 未经 SPI 调用");
            }
            if (cre.isEmpty() || !cre.contains("GameHooks.get()") || !cre.contains("onActivityCreated(a)")) {
                problems.add(name + " 的 onActivityCreated 未经 SPI 调用");
            }
            if (!pre.contains("catch (Throwable") || !cre.contains("catch (Throwable")) {
                problems.add(name + " 两个回调缺 Throwable 兜底（生命周期裸抛＝开游戏闪退）");
            }
            if (s.contains("TapTapWebLogin") || s.contains("TapLoginForceWeb")) {
                problems.add(name + " 仍直接引用登录实现类（框架层不许直调）");
            }
        }
        if (problems.isEmpty()) {
            ok("T19 两个生命周期入口都只经 SPI 调、都兜 Throwable、都不直引登录类");
        } else {
            for (String p : problems) bad("T19 " + p);
        }
    }

    /**
     * T20 native/Java 一致性（2026-11-11 隔离搬迁）：DwctMod.cpp / DwdlmMod.cpp 里的 JNI 类全名，
     * 必须逐个能在 app/src/main/java 找到同名类且 package 声明一致；已迁走的两类不得再指向 cocos/。
     * 判据只读源码（不编 native、不改共享核心），用于挡住「Java 迁了包、JNI 字符串没跟」的半成品。
     */
    private static void t20_nativeJniPathsMatchJava(Path menuRoot) {
        String[] mods = {"Games/DwctMod.cpp", "Games/DwdlmMod.cpp"};
        String[] moved = {"com/starry/magic/games/dwct/DwctIapGate", "com/starry/magic/games/dwdlm/DwdlmHooks"};
        String[] legacy = {"com/starry/magic/cocos/DwctIapGate", "com/starry/magic/cocos/DwdlmHooks"};
        java.util.regex.Pattern ref = java.util.regex.Pattern.compile("com/starry/magic/[a-z0-9]+(?:/[a-z0-9]+)*/[A-Za-z0-9_]+");
        List<String> problems = new ArrayList<>();
        int checked = 0;
        for (String rel : mods) {
            Path cpp = menuRoot.resolve("app/src/main/jni").resolve(rel);
            String src;
            try {
                src = Files.readString(cpp, StandardCharsets.UTF_8);
            } catch (IOException e) {
                problems.add(rel + " 读不到: " + e);
                continue;
            }
            for (String l : legacy) {
                if (src.contains(l)) problems.add(rel + " 仍引用旧包名 " + l + "（该类已迁入 games/，JNI 会 FindClass 失败）");
            }
            java.util.regex.Matcher m = ref.matcher(src);
            Set<String> refs = new LinkedHashSet<>();
            while (m.find()) refs.add(m.group());
            if (refs.isEmpty()) {
                problems.add(rel + " 内无 com/starry/magic/... 类全名（JNI FindClass 目标缺失）");
                continue;
            }
            for (String r : refs) {
                checked++;
                String cls = r.substring(r.lastIndexOf('/') + 1);
                String pkg = r.substring(0, r.lastIndexOf('/')).replace('/', '.');
                Path jf = menuRoot.resolve("app/src/main/java").resolve(r + ".java");
                if (!Files.isRegularFile(jf)) {
                    problems.add(rel + " 引用 " + r + " 但 Java 源不存在（" + menuRoot.relativize(jf) + "）");
                    continue;
                }
                String js;
                try {
                    js = Files.readString(jf, StandardCharsets.UTF_8);
                } catch (IOException e) {
                    problems.add(r + " Java 源读不到: " + e);
                    continue;
                }
                boolean pkgOk = js.contains("package " + pkg + ";");
                boolean clsOk = java.util.regex.Pattern
                        .compile("\\b(?:class|interface|enum)\\s+" + java.util.regex.Pattern.quote(cls) + "\\b")
                        .matcher(js).find();
                if (!pkgOk || !clsOk) {
                    problems.add(rel + " 引用 " + r + " 与 Java 声明不符 pkg=" + pkgOk + " class=" + clsOk);
                }
            }
        }
        for (String mv : moved) {
            Path jf = menuRoot.resolve("app/src/main/java").resolve(mv + ".java");
            if (!Files.isRegularFile(jf)) problems.add("迁移目标缺文件：" + mv + ".java");
        }
        if (problems.isEmpty()) {
            ok("T20 native/Java 一致：" + mods.length + " 个 Mod.cpp 的 " + checked + " 处 JNI 类全名都在 Java 侧存在且包名/类名对得上，无 cocos/ 旧引用");
        } else {
            for (String p : problems) bad("T20 " + p);
        }
    }

    /** 4.7.x 那代：impl.loginHandlers 数组路由。 */
    private static com.taptap.sdk.login.ui.TapTapLoginActivity legacyHandlersActivity() {
        LoginRequest req = baseRequest();
        com.taptap.sdk.login.ui.TapTapLoginActivity act = new com.taptap.sdk.login.ui.TapTapLoginActivity();
        act.setPackageNameForTest(PKG_YNT);
        act.setIntent(intentWith(req));
        act.loginRequest = req;
        return act;
    }

    /** 切出 smali 方法体（.method ... .end method；smali 没有大括号）。 */
    private static String smaliMethod(String src, String signature) {
        int i = src.indexOf(".method " + signature);
        if (i < 0) return "";
        int end = src.indexOf(".end method", i);
        return end < 0 ? src.substring(i) : src.substring(i, end);
    }

    private static void setGame(String prefsId, String nativeId) {
        com.starry.magic.menu.Preferences.authGameId = prefsId == null ? "" : prefsId;
        com.starry.magic.bridge.ModBridge.nativeGameId = nativeId;
        GameHooks.invalidate();
    }

    private static LoginRequest baseRequest() {
        return new LoginRequest("st-1", new String[]{"user_info"});
    }

    private static Intent intentWith(LoginRequest req) {
        Intent it = new Intent();
        it.putExtra("request", req);
        return it;
    }

    private static TapTapActivity oldActivityWithRequest() {
        LoginRequest req = baseRequest();
        TapTapActivity act = new TapTapActivity();
        act.setPackageNameForTest(PKG_YNT);
        act.setIntent(intentWith(req));
        act.loginRequest = req;
        return act;
    }

    private static LoginRequest requestOf(Activity act) {
        Intent it = act.getIntent();
        return it == null ? null : (LoginRequest) it.getParcelableExtra("request");
    }

    private static String bodyOf(String src, String anchor) {
        int i = src.indexOf(anchor);
        if (i < 0) return "";
        int brace = src.indexOf('{', i);
        if (brace < 0) return "";
        int depth = 0;
        for (int j = brace; j < src.length(); j++) {
            char ch = src.charAt(j);
            if (ch == '{') depth++;
            else if (ch == '}') {
                depth--;
                if (depth == 0) return src.substring(brace, j + 1);
            }
        }
        return "";
    }

    private static String fieldString(String src, String name) {
        int i = src.indexOf(name);
        while (i >= 0) {
            int eq = src.indexOf('=', i);
            if (eq < 0) return null;
            int q1 = src.indexOf('"', eq);
            int q2 = q1 < 0 ? -1 : src.indexOf('"', q1 + 1);
            if (q1 > 0 && q2 > q1) return src.substring(q1 + 1, q2);
            i = src.indexOf(name, i + 1);
        }
        return null;
    }

    private static void ok(String msg) {
        passed++;
        System.out.println("  ok " + msg);
    }

    private static void bad(String msg) {
        failures.add(msg);
        System.out.println("  XX " + msg);
    }

    private OfflineLoginTest() {}
}
