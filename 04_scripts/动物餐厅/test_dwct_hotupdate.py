import os,sys,subprocess,shutil,json
from pathlib import Path
root=Path(__file__).resolve().parents[2];work=root/'07_patched/动物餐厅/dwct1242';stage=Path(os.environ.get('LOCALAPPDATA','/tmp'))/'Temp/dwct1242_hot_test';stage.mkdir(parents=True,exist_ok=True)
def put(p,t):
 p=stage/p;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(t,encoding='utf8')
put(Path('android/content/Context.java'),'package android.content; public abstract class Context {public abstract String getPackageName();public abstract java.io.File getFilesDir();public abstract android.content.res.AssetManager getAssets();}')
put(Path('android/content/res/AssetManager.java'),'package android.content.res; public abstract class AssetManager {public abstract java.io.InputStream open(String s)throws java.io.IOException;}')
put(Path('android/util/Log.java'),'package android.util; public class Log {public static int i(String a,String b){System.out.println(b);return 0;}public static int w(String a,String b){System.out.println(b);return 0;}public static int e(String a,String b){System.out.println(b);return 0;}}')
put(Path('android/system/Os.java'),'package android.system; public class Os {public static boolean fail=false;public static void rename(String a,String b)throws Exception{if(fail)throw new java.io.IOException("injected rename failure");java.nio.file.Files.move(java.nio.file.Path.of(a),java.nio.file.Path.of(b),java.nio.file.StandardCopyOption.REPLACE_EXISTING,java.nio.file.StandardCopyOption.ATOMIC_MOVE);}}')
put(Path('org/json/JSONObject.java'),'package org.json; public class JSONObject {public static boolean fail=false;public static int visits=0;public JSONObject(String s){visits++;if(fail)throw new RuntimeException("injected manifest failure");}public JSONObject optJSONObject(String s){return null;}public boolean has(String s){return false;}public JSONObject getJSONObject(String s){return null;}public void put(String s,Object v){} }')
put(Path('com/starry/magic/cocos/DwctIapGate.java'),(root/'mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/cocos/DwctIapGate.java').read_text(encoding='utf8'))
put(Path('Test.java'),'''import java.nio.file.*;import java.io.*;import java.util.*;
public class Test {static Path work,base;static boolean bad=false;static int checks=0;
 static void eq(byte[] a,byte[] b){if(!Arrays.equals(a,b))throw new AssertionError();checks++;}
 public static void main(String[] args)throws Exception{work=Path.of(args[0]);base=Path.of(args[1]);Files.createDirectories(base.resolve("restaurant-hot-update/assets/Script"));Files.createDirectories(base.resolve("restaurant-hot-update/assets/main"));
 android.content.Context ctx=new android.content.Context(){public String getPackageName(){return "inland.twgame.restaurant";}public File getFilesDir(){return base.toFile();}public android.content.res.AssetManager getAssets(){return new android.content.res.AssetManager(){public InputStream open(String s)throws IOException{if(bad)return new ByteArrayInputStream(new byte[]{1});String n=s.contains("Script")?"Script.hot.patched.jsc":"main.hot.patched.jsc";return Files.newInputStream(work.resolve(n));}};}};
 Path a=base.resolve("restaurant-hot-update/assets/Script/index.jsc"),b=base.resolve("restaurant-hot-update/assets/main/index.jsc");byte[] oa=Files.readAllBytes(work.resolve("device.hotupdate.jsc")),ob=Files.readAllBytes(work.resolve("device.main.jsc")),pa=Files.readAllBytes(work.resolve("Script.hot.patched.jsc")),pb=Files.readAllBytes(work.resolve("main.hot.patched.jsc"));
 Files.write(a,oa);Files.write(b,ob);com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),pa);eq(Files.readAllBytes(b),pb);
 com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),pa);eq(Files.readAllBytes(b),pb);
 Files.write(b,ob);com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),pa);eq(Files.readAllBytes(b),pb);
 Files.write(a,new byte[]{4,5});com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),new byte[]{4,5});
 Files.write(a,oa);bad=true;com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),oa);bad=false;
 android.system.Os.fail=true;com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),oa);if(Files.exists(Path.of(a+".starry1242.tmp")))throw new AssertionError();checks++;
 android.system.Os.fail=false;com.starry.magic.cocos.DwctIapGate.apply(ctx);eq(Files.readAllBytes(a),pa);
 Files.writeString(base.resolve("restaurant-hot-update/project.manifest"),"{}");org.json.JSONObject.fail=true;com.starry.magic.cocos.DwctIapGate.apply(ctx);int visits=org.json.JSONObject.visits;org.json.JSONObject.fail=false;com.starry.magic.cocos.DwctIapGate.apply(ctx);if(org.json.JSONObject.visits<=visits)throw new AssertionError("patched branch did not retry manifest");checks++;System.out.println("JAVA_HOTUPDATE_TEST_PASS checks="+checks+" deviceWrites=0");
 }}''')
jdk=Path(os.environ['NIXIANG_JAVA17']);src=list(stage.rglob('*.java'));classes=stage/'classes';classes.mkdir(exist_ok=True)
subprocess.run([str(jdk/'bin/javac'),'-encoding','UTF-8','-d',str(classes)]+list(map(str,src)),check=True)
subprocess.run([str(jdk/'bin/java'),'-cp',str(classes),'Test',str(work),str(stage/'fixture')],check=True)
