import{r as l,B as d,j as e}from"./evently-C3WtrOYl.js";import{a}from"./id-D1fVGO1t.js";import{u as m,T as p}from"./Template-Cd60yQwq.js";import{w as g}from"./waitForElementMountedOnDom-qpCjLZnq.js";import{u as o,a as c,b as h}from"./KcPage-D9pouytT.js";import{L as y}from"./LogoutOtherSessions-DMEfCVkh.js";import"./index-CKYewT40.js";function f(i){const{webAuthnButtonId:n}=i,{kcContext:t}=o();a(t.pageId==="webauthn-register.ftl");const{msgStr:s,isFetchingTranslations:r}=c(),{insertScriptTags:u}=m({effectId:"LoginRecoveryAuthnCodeConfig",scriptTags:[{type:"module",textContent:()=>`
                    import { registerByWebAuthn } from "${d}keycloak-theme/login/js/webauthnRegister.js";
                    const registerButton = document.getElementById('${n}');
                    registerButton.addEventListener("click", function() {
                        const input = {
                            challenge : ${JSON.stringify(t.challenge)},
                            userid : ${JSON.stringify(t.userid)},
                            username : ${JSON.stringify(t.username)},
                            signatureAlgorithms : ${JSON.stringify(t.signatureAlgorithms)},
                            rpEntityName : ${JSON.stringify(t.rpEntityName)},
                            rpId : ${JSON.stringify(t.rpId)},
                            attestationConveyancePreference : ${JSON.stringify(t.attestationConveyancePreference)},
                            authenticatorAttachment : ${JSON.stringify(t.authenticatorAttachment)},
                            requireResidentKey : ${JSON.stringify(t.requireResidentKey)},
                            residentKey : ${JSON.stringify(t.residentKey)},
                            userVerificationRequirement : ${JSON.stringify(t.userVerificationRequirement)},
                            createTimeout : ${JSON.stringify(t.createTimeout)},
                            excludeCredentialIds : ${JSON.stringify(t.excludeCredentialIds)},
                            initLabel : ${JSON.stringify(s("webauthn-registration-init-label"))},
                            initLabelPrompt : ${JSON.stringify(s("webauthn-registration-init-label-prompt"))},
                            errmsg : ${JSON.stringify(s("webauthn-unsupported-browser-text"))}
                        };
                        registerByWebAuthn(input);
                    }, { once: true });
                `}]});l.useEffect(()=>{r||(async()=>(await g({elementId:n}),u()))()},[r])}function k(){const{kcContext:i}=o();a(i.pageId==="webauthn-register.ftl");const{kcClsx:n}=h(),{msg:t,msgStr:s}=c(),r="authenticateWebAuthnButton";return f({webAuthnButtonId:r}),e.jsxs(p,{headerNode:e.jsxs(e.Fragment,{children:[e.jsx("span",{className:n("kcWebAuthnKeyIcon")}),t("webauthn-registration-title")]}),children:[e.jsx("form",{id:"register",className:n("kcFormClass"),action:i.url.loginAction,method:"post",children:e.jsxs("div",{className:n("kcFormGroupClass"),children:[e.jsx("input",{type:"hidden",id:"clientDataJSON",name:"clientDataJSON"}),e.jsx("input",{type:"hidden",id:"attestationObject",name:"attestationObject"}),e.jsx("input",{type:"hidden",id:"publicKeyCredentialId",name:"publicKeyCredentialId"}),e.jsx("input",{type:"hidden",id:"authenticatorLabel",name:"authenticatorLabel"}),e.jsx("input",{type:"hidden",id:"transports",name:"transports"}),e.jsx("input",{type:"hidden",id:"authenticatorAttachment",name:"authenticatorAttachment"}),e.jsx("input",{type:"hidden",id:"error",name:"error"}),e.jsx(y,{})]})}),e.jsx("input",{type:"submit",className:n("kcButtonClass","kcButtonPrimaryClass","kcButtonBlockClass","kcButtonLargeClass"),id:r,value:s("doRegisterSecurityKey")}),!i.isSetRetry&&i.isAppInitiatedAction&&e.jsx("form",{action:i.url.loginAction,className:n("kcFormClass"),id:"kc-webauthn-settings-form",method:"post",children:e.jsx("button",{type:"submit",className:n("kcButtonClass","kcButtonDefaultClass","kcButtonBlockClass","kcButtonLargeClass"),id:"cancelWebAuthnAIA",name:"cancel-aia",value:"true",children:t("doCancel")})})]})}export{k as default};
