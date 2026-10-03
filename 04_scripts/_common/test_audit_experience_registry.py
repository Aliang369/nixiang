#!/usr/bin/env python3
"""Tests for experience metadata audit; use disposable markdown fixtures."""
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
import audit_experience_registry as audit


class ExperienceAuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.directory = self.root / "10_docs/_templates"
        self.directory.mkdir(parents=True)

    def write(self, name, text):
        path = self.directory / name
        path.write_text(text, encoding="utf-8")
        return path

    def test_legacy_card_is_reported_not_failed(self):
        result = audit.audit(self.write("经验_old.md", "# Old\n"))
        self.assertEqual(result["status"], "legacy")
        self.assertEqual(result["issues"], [])

    def test_candidate_metadata_passes(self):
        (self.root / "case.md").write_text("source\n", encoding="utf-8")
        (self.root / "evidence").mkdir()
        (self.root / "evidence/README.md").write_text("evidence\n", encoding="utf-8")
        (self.root / "skill").mkdir()
        (self.root / "skill/hook-rules.md").write_text("# Target\n", encoding="utf-8")
        path = self.write("经验_good.md", "---\nexperience_id: exp-20260101-demo\nstatus: candidate\nscope: Unity\nsource: case.md\nevidence: evidence/README.md\ntargets: skill/hook-rules.md\n---\n# Candidate\n")
        self.assertEqual(audit.audit(path)["status"], "ok")

    def test_merged_requires_approval_and_commit(self):
        path = self.write("经验_bad.md", "---\nexperience_id: exp-002\nstatus: merged\nscope: all\nsource: case.md\nevidence: evidence.md\ntargets: hook-rules.md\n---\n")
        result = audit.audit(path)
        self.assertEqual(result["status"], "invalid")
        self.assertTrue(any("approved_by" in item for item in result["issues"]))
        self.assertTrue(any("merged_commit" in item for item in result["issues"]))

    def test_duplicate_ids_fail_registry_audit(self):
        text = "---\nexperience_id: exp-dup\nstatus: candidate\nscope: x\nsource: y\nevidence: z\ntargets: t\n---\n"
        self.write("经验_a.md", text)
        self.write("经验_b.md", text)
        result = audit.audit_registry(self.root)
        self.assertEqual(result["invalid"], 2)
        self.assertTrue(all(any("重复 experience_id" in issue for issue in row["issues"])
                            for row in result["results"]))


if __name__ == "__main__":
    unittest.main()
