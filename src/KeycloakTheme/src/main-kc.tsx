import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import { KcPage } from "./kc.gen";

import "./index.css";
import "./evently.css";

if (!window.kcContext) {
    throw new Error("No Keycloak context");
}

document.body.classList.add("evently-keycloak");

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <KcPage kcContext={window.kcContext} />
    </StrictMode>
);
