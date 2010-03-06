; BOOL UpdateDriverForPlugAndPlayDevices(HWND, PSTR, PSTR, DWORD, PBOOL);
!define sysUpdateDriverForPlugAndPlayDevices "newdev::UpdateDriverForPlugAndPlayDevices(i, t, t, i, *i) i"
; the masked value of ERROR_NO_SUCH_DEVINST is 523
!define ERROR_NO_SUCH_DEVINST -536870389

;BOOL SetupCopyOEMInf(PSTR, PSTR, DWORD, DWORD, PSTR, DWORD, PDWORD, PSTR);
!define sysSetupCopyOEMInf "setupapi::SetupCopyOEMInf(t, t, i, i, i, i, *i, t) i"
!define SPOST_NONE 0
!define SPOST_PATH 1
!define SPOST_URL 2
!define SP_COPY_DELETESOURCE 0x1
!define SP_COPY_REPLACEONLY 0x2
!define SP_COPY_NOOVERWRITE 0x8
!define SP_COPY_OEMINF_CATALOG_ONLY 0x40000