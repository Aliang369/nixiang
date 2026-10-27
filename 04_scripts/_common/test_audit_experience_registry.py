#!/usr/bin/env python3
"""Tests for experience metadata audit; use disposable markdown fixtures."""
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
import audit_experience_registry as audit

BASE = ("experience_id: exp-20260101-demo\n"
        "scope: Unity\nsource: case.md\nevidence: evidence/README.md\ntargets: skill/hook-rules.md\n")


class ExperienceAuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.directory = self.root / "10_docs/_templates"
        self.directory.mkdir(parents=True)
        (self.root / "case.md").write_text("source\n", encoding="utf-8")
        (self.root / "evidence").mkdir()
        (self.root / "evidence/README.md").write_text("evidence\n", encoding="utf-8")
        (self.root / "skill").mkdir()
        (self.root / "skill/hook-rules.md").write_text("# Target\n", encoding="utf-8")

    def write(self, name, text):
        path = self.directory / name
        path.write_text(text, encoding="utf-8")
        return path

    def card(self, name, extra, ident="exp-20260101-demo"):
        base = BASE.replace("exp-20260101-demo", ident)
        return self.write(name, "---\n" + base + extra + "---\n# Card\n")

    # ---- legacy -------------------------------------------------------
    def test_legacy_card_is_reported_not_failed(self):
        result = audit.audit(self.write("经验_old.md", "# Old\n"))
        self.assertEqual(result["status"], "legacy")
        self.assertEqual(result["issues"], [])

    def test_legacy_cards_do_not_fail_registry(self):
        self.write("经验_old.md", "# Old\n")
        payload = audit.audit_registry(self.root)
        self.assertEqual((payload["legacy"], payload["invalid"]), (1, 0))

    # ---- tech ---------------------------------------------------------
    def test_tech_candidate_passes(self):
        path = self.card("经验_tech.md", "class: tech\nstatus: candidate\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "ok", result["issues"])
        self.assertEqual(result["warnings"], [])

    def test_tech_approved_needs_approved_by(self):
        path = self.card("经验_tech2.md", "class: tech\nstatus: approved\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("approved_by" in i for i in result["issues"]))

    def test_tech_merged_needs_commit(self):
        path = self.card("经验_tech3.md", "class: tech\nstatus: merged\napproved_by: self-check\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("merged_commit" in i for i in result["issues"]))

    def test_tech_does_not_need_approval_ref(self):
        path = self.card("经验_tech4.md", "class: tech\nstatus: approved\napproved_by: audited\n")
        self.assertEqual(audit.audit(path)["status"], "ok")

    # ---- class 强制校验 ------------------------------------------------
    def test_missing_class_is_invalid_not_silent(self):
        path = self.card("经验_noclass.md", "status: candidate\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("缺少 class" in i for i in result["issues"]))

    def test_missing_class_is_not_counted_as_legacy(self):
        self.card("经验_noclass2.md", "status: candidate\n")
        payload = audit.audit_registry(self.root)
        self.assertEqual(payload["legacy"], 0)
        self.assertEqual(payload["invalid"], 1)

    def test_invalid_class_value_fails(self):
        path = self.card("经验_badclass.md", "class: policyish\nstatus: candidate\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("class 无效" in i for i in result["issues"]))

    # ---- policy -------------------------------------------------------
    def test_policy_candidate_passes_without_approval(self):
        path = self.card("经验_pol1.md", "class: policy\nstatus: candidate\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "ok", result["issues"])
        self.assertEqual(result["warnings"], [])

    def test_policy_approved_requires_machine_checkable_ref(self):
        path = self.card("经验_pol2.md", "class: policy\nstatus: approved\napproved_by: me\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("approval_ref" in i for i in result["issues"]))

    def test_policy_approved_rejects_prose_reference(self):
        path = self.card("经验_pol3.md",
                         "class: policy\nstatus: approved\napproved_by: me\napproval_ref: 用户说过可以这么改\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("approval_ref 格式无效" in i for i in result["issues"]))

    def test_policy_merged_with_message_id_requires_manual_review(self):
        commit = self._repo_commit()
        path = self.card("经验_pol4.md",
                         "class: policy\nstatus: merged\napproved_by: me\n"
                         f"approval_ref: m01296\nmerged_commit: {commit}\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "ok", result["issues"])
        self.assertEqual(result["warnings"], [audit.MANUAL_REVIEW])

    def test_policy_date_qualified_ref_accepted(self):
        path = self.card("经验_pol5.md",
                         "class: policy\nstatus: approved\napproved_by: me\napproval_ref: m01296@2026-10-27\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "ok", result["issues"])
        self.assertEqual(len(result["warnings"]), 1)

    def test_policy_rejected_needs_no_approval(self):
        path = self.card("经验_pol6.md", "class: policy\nstatus: rejected\n")
        self.assertEqual(audit.audit(path)["status"], "ok")

    # ---- 注册表级 ------------------------------------------------------
    def test_duplicate_ids_fail_registry_audit(self):
        text = "---\n" + BASE.replace("exp-20260101-demo", "exp-20260101-dup") + "class: tech\nstatus: candidate\n---\n"
        self.write("经验_a.md", text)
        self.write("经验_b.md", text)
        result = audit.audit_registry(self.root)
        self.assertEqual(result["invalid"], 2)
        self.assertTrue(all(any("重复 experience_id" in issue for issue in row["issues"])
                            for row in result["results"]))

    def test_registry_counts_manual_review(self):
        self.card("经验_ok.md", "class: tech\nstatus: candidate\n", "exp-20260101-ok")
        self.card("经验_pol.md",
                  "class: policy\nstatus: approved\napproved_by: me\napproval_ref: m00001\n",
                  "exp-20260101-pol")
        payload = audit.audit_registry(self.root)
        self.assertEqual(payload["manual_review"], 1)
        self.assertEqual(payload["invalid"], 0)

    def _repo_commit(self):
        """在临时目录里造一个真 git 仓库，返回它的 HEAD（merged_commit 校验走真实 git）。"""
        import subprocess
        for cmd in (["git", "init", "-q"], ["git", "add", "case.md"],
                    ["git", "-c", "user.email=t@t", "-c", "user.name=t", "commit", "-qm", "init"]):
            subprocess.run(cmd, cwd=self.root, check=True, capture_output=True)
        return subprocess.run(["git", "rev-parse", "HEAD"], cwd=self.root,
                              capture_output=True, text=True).stdout.strip()


if __name__ == "__main__":
    unittest.main()
