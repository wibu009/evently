@echo off
rem Apache Maven shim (fallback only).
rem
rem Keycloakify packages the theme with a plain `mvn clean install` whose POM has
rem no dependencies in this project's configuration, so the produced JAR is
rem exactly a zip of src/main/resources at the archive root. This shim provides
rem that behavior on machines without a Java/Maven toolchain:
rem   1. robocopy dereferences the node_modules symlinks Keycloakify stages,
rem   2. python -m zipfile packs the dereferenced tree into the ZIP.
rem If a real `mvn` is on PATH, keycloakify uses it instead — CI runners do.

if "%~1" == "--version" (
    echo Apache Maven 3.9.9 ^(shim^)
    exit /b 0
)

rem Runs in Keycloakify's staging directory (pom.xml + src/main/resources).
set PROJECT_DIR=%CD%
rem Coordinates are injected by tools/build-jar.mjs (matches the vite.config options).
set ARTIFACT_ID=%KC_SHIM_ARTIFACT_ID%
set THEME_VERSION=%KC_SHIM_VERSION%

set TARGET_DIR=%PROJECT_DIR%\target
if exist "%TARGET_DIR%" rmdir /s /q "%TARGET_DIR%"
mkdir "%TARGET_DIR%"

set JAR_PATH=%TARGET_DIR%\%ARTIFACT_ID%-%THEME_VERSION%.jar
set RES_SOURCE=%PROJECT_DIR%\src\main\resources
set RES_STAGE=%TEMP%\evently-kc-jar-resources

if exist "%RES_STAGE%" rmdir /s /q "%RES_STAGE%"
robocopy "%RES_SOURCE%" "%RES_STAGE%" /E /NFL /NDL /NJH /NJS /NP
if errorlevel 8 exit /b 1

pushd "%RES_STAGE%"
where python >nul 2>nul
if errorlevel 1 (
    echo maven-shim: python not found; cannot pack the jar 1>&2
    popd
    exit /b 1
)
python -m zipfile -c "%JAR_PATH%" theme META-INF
if errorlevel 1 (
    echo maven-shim: python zipfile failed 1>&2
    popd
    exit /b 1
)
popd
exit /b 0
