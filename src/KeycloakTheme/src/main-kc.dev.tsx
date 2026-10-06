import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import { KcPage } from "./kc.gen";
import { getKcContextMock } from "./login/mocks/getKcContextMock";

import "./index.css";
import "./evently.css";

document.body.classList.add("evently-keycloak");

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <KcPage
            kcContext={getKcContextMock({
                pageId: "login.ftl",
                overrides: {}
            })}
        />
    </StrictMode>
);
