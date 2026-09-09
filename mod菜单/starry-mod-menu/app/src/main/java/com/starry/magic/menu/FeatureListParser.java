package com.starry.magic.menu;

/**
 * 解析 Main.cpp g_featureList 条目（与 HTML 动态渲染约定一致）。
 * 支持：Toggle_ / CheckBox_ / Button_ / Category_ /
 * Slider_名:min:max[:default] / SeekBar_名_min_max[_default] /
 * Spinner_名:选项1,选项2 / InputText_ / InputValue_默认:名
 */
public final class FeatureListParser {

    public enum Kind {
        CATEGORY, TOGGLE, BUTTON, SEEK, SPINNER, INPUT, CHECKBOX, SKIP
    }

    public static final class Item {
        public final int featNum;
        public final Kind kind;
        public final String name;
        public final String raw;
        /** SEEK: [min, max, default]；SPINNER: 选项；INPUT: ["text"|"value", default] */
        public final String[] extras;

        Item(int featNum, Kind kind, String name, String raw, String[] extras) {
            this.featNum = featNum;
            this.kind = kind;
            this.name = name;
            this.raw = raw;
            this.extras = extras;
        }
    }

    private FeatureListParser() {}

    public static Item parse(int index, String spec) {
        if (spec == null || spec.isEmpty()) {
            return new Item(index, Kind.SKIP, "", spec, null);
        }
        if (spec.startsWith("Category_")) {
            return new Item(index, Kind.CATEGORY, spec.substring(9), spec, null);
        }
        if (spec.startsWith("RichWebView_") || spec.startsWith("RichTextView_")
                || spec.startsWith("Collapse_") || spec.startsWith("LaunchLink_")) {
            return new Item(index, Kind.SKIP, spec, spec, null);
        }
        if (spec.startsWith("Toggle_")) {
            return new Item(index, Kind.TOGGLE, spec.substring(7), spec, null);
        }
        if (spec.startsWith("CheckBox_")) {
            return new Item(index, Kind.CHECKBOX, spec.substring(9), spec, null);
        }
        if (spec.startsWith("Button_")) {
            return new Item(index, Kind.BUTTON, spec.substring(7), spec, null);
        }
        if (spec.startsWith("Slider_") || spec.startsWith("SeekBar_")) {
            return parseSeek(index, spec);
        }
        if (spec.startsWith("Spinner_")) {
            String rest = spec.substring(8);
            int colon = rest.indexOf(':');
            String name;
            String[] opts;
            if (colon >= 0) {
                name = rest.substring(0, colon).trim();
                String optPart = rest.substring(colon + 1);
                opts = optPart.isEmpty() ? new String[0] : optPart.split(",");
                for (int i = 0; i < opts.length; i++) {
                    opts[i] = opts[i].trim();
                }
            } else {
                name = rest.trim();
                opts = new String[0];
            }
            return new Item(index, Kind.SPINNER, name, spec, opts);
        }
        if (spec.startsWith("InputValue_")) {
            String rest = spec.substring(11);
            int colon = rest.indexOf(':');
            if (colon >= 0) {
                String def = rest.substring(0, colon).trim();
                String name = rest.substring(colon + 1).trim();
                return new Item(index, Kind.INPUT, name, spec, new String[]{"value", def});
            }
            return new Item(index, Kind.INPUT, rest.trim(), spec, new String[]{"value", ""});
        }
        if (spec.startsWith("InputText_")) {
            return new Item(index, Kind.INPUT, spec.substring(10).trim(), spec,
                    new String[]{"text", ""});
        }
        return new Item(index, Kind.TOGGLE, spec, spec, null);
    }

    /** Slider_移速:1:5 或 SeekBar_移速_1_5 或 SeekBar_Perfect概率_0_100_85 */
    private static Item parseSeek(int index, String spec) {
        boolean slider = spec.startsWith("Slider_");
        String rest = spec.substring(slider ? 7 : 8);
        String name;
        String min = "1";
        String max = "5";
        String def = min;
        if (rest.contains(":")) {
            String[] p = rest.split(":");
            name = p[0];
            if (p.length >= 2) min = p[1];
            if (p.length >= 3) max = p[2];
            if (p.length >= 4) def = p[3];
        } else {
            String[] p = rest.split("_");
            name = p.length > 0 ? p[0] : rest;
            if (p.length >= 2) min = p[1];
            if (p.length >= 3) max = p[2];
            if (p.length >= 4) def = p[3];
        }
        return new Item(index, Kind.SEEK, name, spec, new String[]{min, max, def});
    }

    /** 解析 SEEK 项当前应显示/下发的整数值（无偏好时用 default） */
    public static int seekCurrentValue(Item item) {
        if (item == null || item.kind != Kind.SEEK) return 0;
        int min = 1, max = 5, def = min;
        if (item.extras != null && item.extras.length >= 2) {
            try {
                min = Integer.parseInt(item.extras[0].trim());
                max = Integer.parseInt(item.extras[1].trim());
            } catch (Exception ignored) {}
        }
        if (item.extras != null && item.extras.length >= 3) {
            try {
                def = Integer.parseInt(item.extras[2].trim());
            } catch (Exception ignored) {
                def = min;
            }
        } else {
            def = min;
        }
        if (max <= min) max = min + 1;
        int cur = Preferences.contains(item.featNum)
                ? Preferences.loadPrefInt(item.featNum) : def;
        if (cur < min) cur = min;
        if (cur > max) cur = max;
        return cur;
    }

    public static int seekResetValue(String spec) {
        Item item = parse(0, spec);
        if (item.kind != Kind.SEEK || item.extras == null || item.extras.length < 2) return 0;
        try {
            if (item.extras.length >= 3) return Integer.parseInt(item.extras[2].trim());
            return Integer.parseInt(item.extras[0].trim());
        } catch (Exception ignored) {
            return 0;
        }
    }
}
