package com.starry.magic.menu;

/**
 * MenuService 空子类，仅用于 Manifest 等长劫持：
 * 部分游戏新版 Manifest 没有 33 字符广告 Service 槽（如追迹迷城 1.0.20260904，
 * 穿山甲槽变为 34 字符 com.byazt.iu.DownloadHandleService），外科脚本改为把该槽
 * 等长替换成 "com.starry.magic.menu.MenuService2"（同为 34 字符）。
 * 行为完全继承 MenuService；MenuService.start() 会探测本类是否被声明并自动切换。
 */
public class MenuService2 extends MenuService {
}
