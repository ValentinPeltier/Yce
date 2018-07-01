;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Yce"
  OutFile "Yce_setup_x64.exe"

  ;Default installation folder
  InstallDir "$DOCUMENTS\Yce"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Yce" ""

  ;Request application privileges for Windows Vista and Windows 7
  RequestExecutionLevel user

;--------------------------------
;UI Configuration

  !define MUI_ICON "setup.ico"
  !define MUI_UNICON "uninstall.ico"
  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "setup.bmp"
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "French"

;--------------------------------
;Installer Section

Section

  SetOutPath "$INSTDIR"
  
  File /r "Yce\*"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Yce" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ;Create shortcut
  CreateShortCut "$DESKTOP\Yce.lnk" "$INSTDIR\Yce.exe"
  CreateShortCut "$SMPROGRAMS\Yce.lnk" "$INSTDIR\Yce.exe"

SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  Delete "$INSTDIR\uninstall.exe"

  Delete "$DESKTOP\Yce.lnk"
  Delete "$SMPROGRAMS\Yce.lnk"

  RMDir /r "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\Yce"

SectionEnd
