#pragma once
#ifdef __cplusplus
extern "C" {
#endif
/** Official 1.25.31 in-memory ACE 4-point soft patch. Call ASAP in JNI_OnLoad. */
void Ltzj_AceSoftPatch12531(void);
/** Soft-bypass sectionhash/filecrc/HashBySyscall BLRs (CaveBoot / H5h). */
void Ltzj_AceIntegritySoft12531(void);
/** Block kill(SIGBUS/SIGSEGV) via GOT + ignore SIGBUS (CaveBoot / H5i). */
void Ltzj_AceSuicideSoft12531(void);
#ifdef __cplusplus
}
#endif
