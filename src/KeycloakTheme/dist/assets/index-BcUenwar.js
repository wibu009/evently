import{r as y,B as x,j as e}from"./evently-C3WtrOYl.js";import{a as j}from"./id-D1fVGO1t.js";import{u as f,a as b,b as A,c as v}from"./KcPage-D9pouytT.js";import{u as B,T as S}from"./Template-Cd60yQwq.js";import{w}from"./waitForElementMountedOnDom-qpCjLZnq.js";import"./index-CKYewT40.js";function F(c){const{webAuthnButtonId:i}=c,{kcContext:n}=f();j(n.pageId==="login-passkeys-conditional-authenticate.ftl");const{msgStr:r,isFetchingTranslations:d}=b(),{insertScriptTags:m}=B({effectId:"LoginPasskeysConditionalAuthenticate",scriptTags:[{type:"module",textContent:()=>`
                    import { authenticateByWebAuthn } from "${x}keycloak-theme/login/js/webauthnAuthenticate.js";
                    import { initAuthenticate } from "${x}keycloak-theme/login/js/passkeysConditionalAuth.js";

                    const authButton = document.getElementById("${i}");
                    const input = {
                        isUserIdentified : ${n.isUserIdentified},
                        challenge : ${JSON.stringify(n.challenge)},
                        userVerification : ${JSON.stringify(n.userVerification)},
                        rpId : ${JSON.stringify(n.rpId)},
                        createTimeout : ${JSON.stringify(n.createTimeout)}
                    };
                    authButton.addEventListener("click", () => {
                        authenticateByWebAuthn({
                            ...input,
                            errmsg : ${JSON.stringify(r("webauthn-unsupported-browser-text"))}
                        });
                    }, { once: true });

                    initAuthenticate({
                        ...input,
                        errmsg : ${JSON.stringify(r("passkey-unsupported-browser-text"))}
                    }, available => {
                        const loginForm = document.getElementById("kc-form-login");
                        const passkeyButton = document.getElementById("kc-form-passkey-button");

                        if (!loginForm || !passkeyButton) {
                            return;
                        }

                        if (available) {
                            loginForm.style.display = "block";
                        } else {
                            passkeyButton.style.display = "block";
                        }
                    });
                `}]});y.useEffect(()=>{d||(async()=>(await w({elementId:i}),m()))()},[d])}function J(){const{kcContext:c}=f();j(c.pageId==="login-passkeys-conditional-authenticate.ftl");const{messagesPerField:i,login:n,url:r,usernameHidden:d,shouldDisplayAuthenticators:m,authenticators:l,registrationDisabled:I,realm:h}=c,{msg:o,msgStr:N,advancedMsg:g}=b(),{kcClsx:t}=A(),k="authenticateWebAuthnButton";return F({webAuthnButtonId:k}),e.jsxs(S,{headerNode:o("passkey-login-title"),infoNode:h.registrationAllowed&&!I&&e.jsx("div",{id:"kc-registration",children:e.jsxs("span",{children:["$",o("noAccount")," ",e.jsx("a",{tabIndex:6,href:r.registrationUrl,children:o("doRegister")})]})}),children:[e.jsxs("form",{id:"webauth",action:r.loginAction,method:"post",children:[e.jsx("input",{type:"hidden",id:"clientDataJSON",name:"clientDataJSON"}),e.jsx("input",{type:"hidden",id:"authenticatorData",name:"authenticatorData"}),e.jsx("input",{type:"hidden",id:"signature",name:"signature"}),e.jsx("input",{type:"hidden",id:"credentialId",name:"credentialId"}),e.jsx("input",{type:"hidden",id:"userHandle",name:"userHandle"}),e.jsx("input",{type:"hidden",id:"error",name:"error"})]}),e.jsxs("div",{className:t("kcFormGroupClass"),style:{marginBottom:0},children:[l!==void 0&&Object.keys(l).length!==0&&e.jsxs(e.Fragment,{children:[e.jsx("form",{id:"authn_select",className:t("kcFormClass"),children:l.authenticators.map((s,a)=>e.jsx("input",{type:"hidden",name:"authn_use_chk",readOnly:!0,value:s.credentialId},a))}),m&&e.jsxs(e.Fragment,{children:[l.authenticators.length>1&&e.jsx("p",{className:t("kcSelectAuthListItemTitle"),children:o("passkey-available-authenticators")}),e.jsx("div",{className:t("kcFormClass"),children:l.authenticators.map((s,a)=>e.jsxs("div",{id:`kc-webauthn-authenticator-item-${a}`,className:t("kcSelectAuthListItemClass"),children:[e.jsx("i",{className:v((()=>{const u=t(s.transports.iconClass);return u===s.transports.iconClass?t("kcWebAuthnDefaultIcon"):u})(),t("kcSelectAuthListItemIconPropertyClass"))}),e.jsxs("div",{className:t("kcSelectAuthListItemBodyClass"),children:[e.jsx("div",{id:`kc-webauthn-authenticator-label-${a}`,className:t("kcSelectAuthListItemHeadingClass"),children:g(s.label)}),s.transports!==void 0&&s.transports.displayNameProperties!==void 0&&s.transports.displayNameProperties.length!==0&&e.jsx("div",{id:`kc-webauthn-authenticator-transport-${a}`,className:t("kcSelectAuthListItemDescriptionClass"),children:s.transports.displayNameProperties.map((u,p,C)=>e.jsxs(y.Fragment,{children:[e.jsxs("span",{children:[" ",g(u)," "]},p),p!==C.length-1&&e.jsx("span",{children:", "})]},p))}),e.jsxs("div",{className:t("kcSelectAuthListItemDescriptionClass"),children:[e.jsx("span",{id:`kc-webauthn-authenticator-createdlabel-${a}`,children:o("passkey-createdAt-label")}),e.jsx("span",{id:`kc-webauthn-authenticator-created-${a}`,children:s.createdAt})]})]}),e.jsx("div",{className:t("kcSelectAuthListItemFillClass")})]},a))})]})]}),e.jsx("div",{id:"kc-form",children:e.jsxs("div",{id:"kc-form-wrapper",children:[h.password&&e.jsx("form",{id:"kc-form-login",action:r.loginAction,method:"post",style:{display:"none"},onSubmit:s=>{const{login:a}=s.target;return a!==void 0&&(a.disabled=!0),!0},children:!d&&e.jsxs("div",{className:t("kcFormGroupClass"),children:[e.jsx("label",{htmlFor:"username",className:t("kcLabelClass"),children:o("passkey-autofill-select")}),e.jsx("input",{tabIndex:1,id:"username","aria-invalid":i.existsError("username"),className:t("kcInputClass"),name:"username",defaultValue:n.username??"",autoComplete:"username webauthn",type:"text",autoFocus:!0}),i.existsError("username")&&e.jsx("span",{id:"input-error-username",className:t("kcInputErrorMessageClass"),"aria-live":"polite",children:i.get("username")})]})}),e.jsx("div",{id:"kc-form-passkey-button",className:t("kcFormButtonsClass"),style:{display:"none"},children:e.jsx("input",{id:k,type:"button",autoFocus:!0,value:N("passkey-doAuthenticate"),className:t("kcButtonClass","kcButtonPrimaryClass","kcButtonBlockClass","kcButtonLargeClass")})})]})})]})]})}export{J as default};
