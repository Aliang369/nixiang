#!/usr/bin/env python3
# Real Git fixtures for incremental script lifecycle governance; no devices/builds.
# script-policy: {"purpose":"脚本治理真实Git回归", "inputs":"临时Git仓库及三入口源码", "outputs":"unittest断言结果", "boundary":"仅临时目录离线检查", "prior_art":"prior_art gen_scripts_index；04_scripts/_common/test_audit_experience_registry.py", "difference":"新增重复与删后调用的真实Git夹具，复用既有索引器而非第二治理实现", "evidence":"04_scripts/README.md#脚本长效治理"}
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

HERE = Path(__file__).resolve().parent


class GovernanceTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory(prefix="脚本治理 空格 ")
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        self.base = "04_scripts/测试 游戏/build.sh"
        self.put(self.base, '#!/bin/sh\nVERSION=1\necho "$VERSION"\n')
        self.put("README.md", "channel-review: official builds APK; other channel emits JSON\n")
        self.git("init", "-q")
        self.commit()

    def git(self, *args):
        return subprocess.run(["git", "-C", str(self.root), *args], check=True, capture_output=True)

    def commit(self):
        self.git("add", ".")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test", "commit", "-qm", "fixture")

    def put(self, name, text):
        p = self.root / name
        p.parent.mkdir(parents=True, exist_ok=True)
        p.write_text(text, encoding="utf-8")
        return p

    def check(self):
        return subprocess.run([sys.executable, "-B", str(HERE / "gen_scripts_index.py"),
                               "--governance", "--root", str(self.root)], capture_output=True, text=True)

    def annotation(self, difference="不同渠道输出JSON清单，不做官方APK组装",
                   evidence="README.md#channel-review"):
        meta = dict(purpose="channel manifest export", inputs="version and channel config",
                    outputs="JSON manifest output", boundary="alternate channel only",
                    prior_art="prior_art build; " + self.base, difference=difference,
                    evidence=evidence)
        return "# script-policy: " + json.dumps(meta, ensure_ascii=False) + "\n"

    def snapshot(self):
        return {str(p.relative_to(self.root)): (p.read_bytes(), p.stat().st_mtime_ns)
                for p in self.root.rglob("*") if p.is_file() and ".git" not in p.parts}

    def test_untracked_exact_copy(self):
        self.put("04_scripts/测试 游戏/copy.sh", (self.root / self.base).read_text())
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("exact-copy", result.stdout)
        self.assertIn("new=1", result.stdout)

    def test_second_tree_javascript_copy_fails(self):
        text = "console.log(123);\n"
        self.put("mod菜单/starry-mod-menu/scripts/channel.js", text)
        self.commit()
        self.put("mod菜单/starry-mod-menu/scripts/channel_new.js", text)
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("exact-copy", result.stdout)

    def test_version_only_blank_declaration_fails(self):
        self.put("04_scripts/测试 游戏/build_2.0.sh", self.annotation(" ") +
                 (self.root / self.base).read_text().replace("VERSION=1", "VERSION=2"))
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("version-only-copy", result.stdout)

    def test_version_only_nonempty_claim_still_fails(self):
        self.put("04_scripts/测试 游戏/build_2.0.sh", self.annotation() +
                 (self.root / self.base).read_text().replace("VERSION=1", "VERSION=2"))
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("version-only-copy", result.stdout)

    def test_numeric_algorithm_difference_with_review_evidence_passes(self):
        difference = "alternate channel VERSION selects manifest schema, not APK release; two dispatch algorithms"
        self.put("README.md", "channel-review: " + difference + "\n")
        self.put("04_scripts/测试 游戏/build_2.0.sh", self.annotation(difference) +
                 (self.root / self.base).read_text().replace("VERSION=1", "VERSION=2"))
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout)
        self.assertIn("REVIEW normalized candidate", result.stdout)

    def test_existing_entry_changed_to_version_copy_fails(self):
        p = "04_scripts/测试 游戏/build_3.0.sh"
        self.put(p, "echo distinct algorithm\n")
        self.commit()
        self.put(p, (self.root / self.base).read_text().replace("VERSION=1", "VERSION=3"))
        self.assertEqual(self.check().returncode, 1)

    def test_index_check_refs_drift_zero_write(self):
        common = self.root / "04_scripts/_common"
        common.mkdir()
        shutil.copyfile(HERE / "gen_scripts_index.py", common / "gen_scripts_index.py")
        self.put("04_scripts/_common/helper.sh", "# helper description\necho helper\n")
        command = [sys.executable, "-B", str(common / "gen_scripts_index.py")]
        subprocess.run(command, check=True, capture_output=True)
        index = common / "scripts_index.tsv"
        content = index.read_text()
        index.write_text(content.rsplit("\t0", 1)[0] + "\t999\n")
        before = self.snapshot()
        result = subprocess.run(command + ["--check"], capture_output=True, text=True)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertEqual(before, self.snapshot())

    def test_stable_config_update_passes(self):
        self.put(self.base, '#!/bin/sh\nVERSION=2\necho "$VERSION"\n')
        self.assertEqual(self.check().returncode, 0)

    def test_distinct_channel_evidence_passes(self):
        self.put("04_scripts/测试 游戏/build_2.0.sh", self.annotation() + 'echo "channel JSON"\n')
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout)
        self.assertIn("REVIEW family", result.stdout)

    def test_staged_copy_then_worktree_fix_still_fails(self):
        p = "04_scripts/测试 游戏/new.sh"
        self.put(p, (self.root / self.base).read_text())
        self.git("add", p)
        self.put(p, self.annotation() + "echo different\n")
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("staged snapshot", result.stdout)
        self.assertIn("exact-copy", result.stdout)

    def test_unstaged_modified_duplicate_fails(self):
        self.put("04_scripts/测试 游戏/other.sh", "echo distinct\n")
        self.commit()
        self.put("04_scripts/测试 游戏/other.sh", (self.root / self.base).read_text())
        self.assertIn("exact-copy", self.check().stdout)
        self.assertEqual(self.check().returncode, 1)

    def test_deleted_active_pipeline_fails(self):
        self.put("04_scripts/测试 游戏/pipeline.json", json.dumps({"stages": [{"command": 'bash "' + self.base + '"'}]}))
        self.commit()
        (self.root / self.base).unlink()
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("deleted-active-reference", result.stdout)

    def test_deleted_notes_and_negative_guard_pass(self):
        self.put("04_scripts/测试 游戏/pipeline.json", json.dumps({"notes": {"command": self.base},
                 "history": [{"cmd": self.base}], "cmd": 'test ! -f "' + self.base + '"'}))
        self.commit()
        (self.root / self.base).unlink()
        self.assertEqual(self.check().returncode, 0)

    def test_document_only_old_debt_passes(self):
        self.put("04_scripts/测试 游戏/old.sh", (self.root / self.base).read_text())
        self.commit()
        self.put("README.md", "docs-only change\n")
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout)
        self.assertIn("DEBT", result.stdout)

    def test_default_gate_read_only(self):
        before = self.snapshot()
        self.assertEqual(self.check().returncode, 0)
        self.assertEqual(before, self.snapshot())

    def test_three_entry_failures_precede_side_effects(self):
        common = self.root / "04_scripts/_common"
        common.mkdir(parents=True)
        for name in ("preflight.sh", "gen_scripts_index.py", "finalize_round.sh", "finalize_game_delivery.sh"):
            shutil.copyfile(HERE / name, common / name)
        # Sentinel downstream executable: real entry and gate run; only external effect replaced.
        sentinel = self.put("effect.sh", '#!/bin/sh\ntouch "' + str(self.root / 'EFFECT') + '"\n')
        sentinel.chmod(0o755)
        self.commit()
        self.put("04_scripts/测试 游戏/clone.sh", (self.root / self.base).read_text())
        before = self.snapshot()
        env = dict(os.environ, NIXIANG_ROOT=str(self.root), APKSIGNER=str(sentinel))
        cases = [("preflight.sh", "README.md"),
                 ("finalize_round.sh", "--game", "测试 游戏", "--apk", "fake.apk", "--apply", "--keep-going"),
                 ("finalize_game_delivery.sh", "--game", "测试 游戏", "--apk", "fake.apk", "--prune", "--apply"),
                 ("finalize_round.sh", "--game", "测试 游戏", "--apk", "fake.apk"),
                 ("finalize_game_delivery.sh", "--game", "测试 游戏", "--experience-only")]
        for name, *args in cases:
            with self.subTest(entry=name, args=args):
                result = subprocess.run(["bash", str(common / name), *args], cwd=self.root,
                                        env=env, capture_output=True, text=True)
                self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
                self.assertIn("exact-copy", result.stdout)
                self.assertNotIn("步骤 1:", result.stdout)
                self.assertFalse((self.root / "EFFECT").exists())
                self.assertEqual(before, self.snapshot())

    # ---------- A：活跃命令词法（解释器选项 / env / 相对路径 / 工作目录 / -c 嵌套）----------
    def assert_deleted_reference(self, command, data_extra=None):
        data = {"stages": [{"command": command}]}
        data.update(data_extra or {})
        self.put("04_scripts/测试 游戏/pipeline.json", json.dumps(data, ensure_ascii=False))
        self.commit()
        text = (self.root / self.base).read_text()
        (self.root / self.base).unlink()
        result = self.check()
        self.put(self.base, text)
        self.git("add", ".")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test",
                 "commit", "-qm", "restore", "--allow-empty")
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertIn("deleted-active-reference", result.stdout)

    def test_interpreter_options_and_call_forms_detected(self):
        cases = [
            ('bash -eu "04_scripts/测试 游戏/build.sh"', None),
            ('env bash "./04_scripts/测试 游戏/build.sh"', None),
            ("sh -e '04_scripts/测试 游戏/build.sh'", None),
            ('bash -c \'bash "04_scripts/测试 游戏/build.sh"\'', None),
            ("bash build.sh", {"workdir": "04_scripts/测试 游戏"}),
            ("bash -e build.sh", {"workdir": "04_scripts/测试 游戏"}),
        ]
        for command, extra in cases:
            with self.subTest(command=command, extra=extra):
                self.assert_deleted_reference(command, extra)

    def test_unresolvable_variable_is_review_not_pass_claim(self):
        self.put("04_scripts/测试 游戏/pipeline.json",
                 json.dumps({"stages": [{"command": 'bash "$UNKNOWN_DIR/tool.sh"'}]}))
        self.commit()
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("dynamic-reference", result.stdout)
        self.assertIn("人工复核", result.stdout)

    def test_guard_commands_are_not_treated_as_calls(self):
        self.put("04_scripts/测试 游戏/pipeline.json", json.dumps({"stages": [
            {"command": 'test ! -f "04_scripts/测试 游戏/build.sh"'},
            {"command": 'command -v "04_scripts/测试 游戏/build.sh" >/dev/null'},
            {"command": 'if [ -x "04_scripts/测试 游戏/build.sh" ]; then echo present; fi'},
        ]}))
        self.commit()
        (self.root / self.base).unlink()
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertNotIn("deleted-active-reference", result.stdout)

    # ---------- B：暂存检查必须整份读暂存快照 ----------
    def test_staged_duplicate_cannot_use_worktree_evidence(self):
        notes = "04_scripts/测试 游戏/notes.md"
        self.put(notes, "channel-review anchor only in worktree\n")
        self.put("04_scripts/测试 游戏/new.sh",
                 self.annotation(evidence=notes + "#channel-review") + 'echo "channel JSON"\n')
        self.git("add", "04_scripts/测试 游戏/new.sh")   # 证据文件故意不暂存
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertIn("staged snapshot", result.stdout)
        self.assertIn("staged index", result.stdout)

    def test_staged_evidence_content_is_the_staged_one(self):
        self.put("README.md", "# notes\nplaceholder line\n")
        self.git("add", "README.md")                     # 索引里没有 anchor
        self.put("README.md", "channel-review: alternate channel JSON\n")   # 工作树有
        self.put("04_scripts/测试 游戏/new.sh", self.annotation() + 'echo "channel JSON"\n')
        self.git("add", "04_scripts/测试 游戏/new.sh")
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertIn("staged index", result.stdout)

    # ---------- C：入口身份，NIXIANG_ROOT 不能指到别的干净仓库 ----------
    def entries(self):
        common = self.root / "04_scripts/_common"
        common.mkdir(parents=True, exist_ok=True)
        for name in ("preflight.sh", "gen_scripts_index.py", "finalize_round.sh",
                     "finalize_game_delivery.sh", "audit_experience_registry.py"):
            if (HERE / name).is_file():
                shutil.copyfile(HERE / name, common / name)
        sentinel = self.put("effect.sh", '#!/bin/sh\ntouch "' + str(self.root / 'EFFECT') + '"\n')
        sentinel.chmod(0o755)
        self.commit()
        return common, sentinel

    def clean_git_root(self):
        path = Path(tempfile.mkdtemp(prefix="另一 干净根 "))
        self.addCleanup(shutil.rmtree, path, True)
        (path / "README.md").write_text("clean tree\n", encoding="utf-8")
        for args in (["init", "-q"], ["add", "."],
                     ["-c", "user.name=C", "-c", "user.email=c@example.test", "commit", "-qm", "clean"]):
            subprocess.run(["git", "-C", str(path), *args], check=True, capture_output=True)
        return path

    def test_env_pointing_at_other_clean_root_is_not_a_bypass(self):
        common, sentinel = self.entries()
        self.put("04_scripts/测试 游戏/clone.sh", (self.root / self.base).read_text())
        clean = self.clean_git_root()
        before = self.snapshot()
        env = dict(os.environ, NIXIANG_ROOT=str(clean), APKSIGNER=str(sentinel))
        cases = [("preflight.sh", "README.md"),
                 ("finalize_round.sh", "--game", "测试 游戏", "--apk", "fake.apk"),
                 ("finalize_game_delivery.sh", "--game", "测试 游戏", "--experience-only")]
        for name, *args in cases:
            with self.subTest(entry=name):
                result = subprocess.run(["bash", str(common / name), *args], cwd=self.root,
                                        env=env, capture_output=True, text=True, timeout=180)
                self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
                self.assertIn("exact-copy", result.stdout + result.stderr)
                self.assertFalse((self.root / "EFFECT").exists())
        self.assertEqual(before, self.snapshot())

    def test_space_path_clean_root_passes_every_gate(self):
        common, sentinel = self.entries()          # 无重复、无脏树
        gate = subprocess.run([sys.executable, "-B", str(common / "gen_scripts_index.py"),
                               "--governance", "--root", str(self.root)],
                              capture_output=True, text=True)
        self.assertEqual(gate.returncode, 0, gate.stdout + gate.stderr)
        env = dict(os.environ, NIXIANG_ROOT=str(self.root), APKSIGNER=str(sentinel))
        round_out = subprocess.run(["bash", str(common / "finalize_round.sh"), "--game", "测试 游戏",
                                    "--apk", "fake.apk"], cwd=self.root, env=env,
                                   capture_output=True, text=True, timeout=180)
        joined = round_out.stdout + round_out.stderr
        self.assertNotIn("exact-copy", joined)
        self.assertNotIn("与入口仓库", joined)
        self.assertIn("[finalize]", joined)
        exp = subprocess.run(["bash", str(common / "finalize_game_delivery.sh"), "--game", "测试 游戏",
                              "--experience-only"], cwd=self.root, env=env,
                             capture_output=True, text=True, timeout=180)
        self.assertIn("--experience-only", exp.stdout)

    # ---------- D：本轮之前的未跟踪脚本只算既有欠账 ----------
    def test_preexisting_untracked_entry_is_debt_not_new(self):
        legacy = "04_scripts/测试 游戏/legacy.sh"
        self.put(legacy, "echo legacy only\n")
        self.put("04_scripts/测试 游戏/pipeline.json",
                 json.dumps({"stages": [{"command": "bash " + legacy}]}, ensure_ascii=False))
        self.git("add", "04_scripts/测试 游戏/pipeline.json")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test",
                 "commit", "-qm", "pipeline")
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("untracked-not-new", result.stdout)
        self.assertNotIn("metadata:", result.stdout)

    def test_prose_mention_does_not_exempt_new_entry(self):
        # 散文（notes）里出现文件名不算被调用：未跟踪新脚本必须照样补元信息，不能被洗白成旧账。
        legacy = "04_scripts/测试 游戏/legacy.sh"
        self.put(legacy, "echo legacy only\n")
        self.put("04_scripts/测试 游戏/pipeline.json",
                 json.dumps({"notes": "旧轮曾用 bash " + legacy + " 归档", "stages": []}, ensure_ascii=False))
        self.git("add", "04_scripts/测试 游戏/pipeline.json")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test",
                 "commit", "-qm", "notes")
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertNotIn("untracked-not-new", result.stdout)
        self.assertIn("metadata:", result.stdout)

    def test_versioned_baseline_call_exempts_stable_entry(self):
        # 基线活跃命令点的是带版本副本时，无版本稳定入口只记 DEBT（既有欠账），不要求补元信息。
        self.put("04_scripts/测试 游戏/tool_1.1.8.sh", "#!/bin/sh\nVERSION=1.1.8\necho $VERSION\n")
        self.put("04_scripts/测试 游戏/pipeline.json", json.dumps(
            {"stages": [{"command": "bash 04_scripts/测试 游戏/tool_1.1.8.sh"}]}, ensure_ascii=False))
        self.git("add", "04_scripts/测试 游戏")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test",
                 "commit", "-qm", "versioned")
        self.put("04_scripts/测试 游戏/tool.sh", "#!/bin/sh\necho stable entry\n")
        result = self.check()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("untracked-not-new: 04_scripts/测试 游戏/tool.sh", result.stdout)
        self.assertNotIn("metadata:", result.stdout)

    def test_new_versioned_copy_is_not_exempted(self):
        # 新造的带版本副本不能借同族基线调用豁免：必须当本轮新增受检（此处缺元信息 ⇒ FAIL）。
        self.put("04_scripts/测试 游戏/tool_1.1.8.sh", "#!/bin/sh\nVERSION=1.1.8\necho $VERSION\n")
        self.put("04_scripts/测试 游戏/pipeline.json", json.dumps(
            {"stages": [{"command": "bash 04_scripts/测试 游戏/tool_1.1.8.sh"}]}, ensure_ascii=False))
        self.git("add", "04_scripts/测试 游戏")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test",
                 "commit", "-qm", "versioned")
        self.put("04_scripts/测试 游戏/tool_1.1.9.sh", "#!/bin/sh\nVERSION=1.1.9\necho $VERSION\n")
        result = self.check()
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertNotIn("untracked-not-new: 04_scripts/测试 游戏/tool_1.1.9.sh", result.stdout)
        self.assertIn("metadata:", result.stdout)
    def test_empty_root_is_not_a_bypass(self):
        other = self.root / "empty"
        other.mkdir()
        result = subprocess.run([sys.executable, "-B", str(HERE / "gen_scripts_index.py"),
                                 "--governance", "--root", str(other)], capture_output=True, text=True)
        self.assertEqual(result.returncode, 2)


if __name__ == "__main__":
    unittest.main()
