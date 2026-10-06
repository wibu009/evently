/**
 * Builds the Keycloakify theme JAR.
 *
 * With a real Apache Maven on PATH, `keycloakify build` runs its normal flow.
 * Without one (our dev boxes don't ship a Java toolchain), a shim `mvn.cmd` is
 * injected that produces the same artifact — Keycloakify's POM for this project
 * has zero dependencies, so the JAR is exactly a zip of src/main/resources.
 * CI runners (GitHub-hosted) have Maven preinstalled and won't touch the shim.
 */
import { execFileSync, spawnSync } from "node:child_process";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { readFileSync } from "node:fs";

const probe = spawnSync("cmd", ["/d", "/s", "/c", "mvn --version"], { encoding: "utf8" });
if (!(probe.stdout?.includes("Apache Maven"))) {
    const shimDir = path.dirname(fileURLToPath(import.meta.url));
    const mavenShimDir = path.join(shimDir, "maven-shim");

    const packageJson = JSON.parse(readFileSync(path.join(shimDir, "..", "package.json"), "utf8"));
    process.env.KC_SHIM_ARTIFACT_ID = "evently-keycloak-theme";
    process.env.KC_SHIM_VERSION = packageJson.version;
    process.env.PATH = `${mavenShimDir}${path.delimiter}${process.env.PATH ?? ""}`;
}

execFileSync("cmd", ["/d", "/s", "/c", "npx keycloakify build"], {
    stdio: "inherit"
});
