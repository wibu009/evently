import{r as c,B as d,j as t}from"./evently-C3WtrOYl.js";import{a as l}from"./id-D1fVGO1t.js";import{u,a as m,b as g,k as x,c as f}from"./KcPage-D9pouytT.js";import{P as k}from"./PasswordWrapper-C1FBBuL6.js";import{u as j,T as y}from"./Template-Cd60yQwq.js";import{w as b}from"./waitForElementMountedOnDom-qpCjLZnq.js";import"./index-CKYewT40.js";function w(e){const{webAuthnButtonId:s}=e,{kcContext:a}=u();l(a.pageId==="login-password.ftl");const{msgStr:n,isFetchingTranslations:i}=m(),{insertScriptTags:r}=j({effectId:"LoginPassword",scriptTags:[{type:"module",textContent:()=>`
                    import { authenticateByWebAuthn } from "${d}keycloak-theme/login/js/webauthnAuthenticate.js";
                    import { initAuthenticate } from "${d}keycloak-theme/login/js/passkeysConditionalAuth.js";

                    const authButton = document.getElementById("${s}");
                    const input = {
                        isUserIdentified : ${a.isUserIdentified},
                        challenge : ${JSON.stringify(a.challenge)},
                        userVerification : ${JSON.stringify(a.userVerification)},
                        rpId : ${JSON.stringify(a.rpId)},
                        createTimeout : ${JSON.stringify(a.createTimeout)},
                        mediation : ${JSON.stringify(a.mediation)},
                        authenticatorAttachment : ${JSON.stringify(a.authenticatorAttachment)}
                    };
                    authButton.addEventListener("click", () => {
                        authenticateByWebAuthn({
                            ...input,
                            errmsg : ${JSON.stringify(n("webauthn-unsupported-browser-text"))}
                        });
                    }, { once: true });

                    initAuthenticate({
                        ...input,
                        errmsg : ${JSON.stringify(n("passkey-unsupported-browser-text"))}
                    });
                `}]});c.useEffect(()=>{i||a.enableWebAuthnConditionalUI!==!0||(async()=>(await b({elementId:s}),r()))()},[i])}function v(){const{kcContext:e}=u();l(e.pageId==="login-password.ftl");const{kcClsx:s}=g(),{msg:a,msgStr:n}=m(),[i,r]=c.useState(!1),o="authenticateWebAuthnButton";return w({webAuthnButtonId:o}),t.jsxs(y,{headerNode:a("doLogIn"),displayMessage:!e.messagesPerField.existsError("password"),children:[t.jsx("div",{id:"kc-form",children:t.jsx("div",{id:"kc-form-wrapper",children:t.jsxs("form",{id:"kc-form-login",onSubmit:()=>(r(!0),!0),action:e.url.loginAction,method:"post",children:[t.jsxs("div",{className:f(s("kcFormGroupClass"),"no-bottom-margin"),children:[t.jsx("hr",{}),t.jsx("label",{htmlFor:"password",className:s("kcLabelClass"),children:a("password")}),t.jsx(k,{passwordInputId:"password",toggleVisibilityButtonTabId:3,children:t.jsx("input",{tabIndex:2,id:"password",className:s("kcInputClass"),name:"password",type:"password",autoFocus:!0,autoComplete:"on","aria-invalid":e.messagesPerField.existsError("username","password")})}),e.messagesPerField.existsError("password")&&t.jsx("span",{id:"input-error-password",className:s("kcInputErrorMessageClass"),"aria-live":"polite",dangerouslySetInnerHTML:{__html:x(e.messagesPerField.get("password"))}})]}),t.jsx("div",{className:s("kcFormGroupClass","kcFormSettingClass"),children:t.jsx("div",{id:"kc-form-options",children:t.jsx("div",{className:s("kcFormOptionsWrapperClass"),children:e.realm.resetPasswordAllowed&&t.jsx("span",{children:t.jsx("a",{tabIndex:5,href:e.url.loginResetCredentialsUrl,children:a("doForgotPassword")})})})})}),t.jsx("div",{id:"kc-form-buttons",className:s("kcFormGroupClass"),children:t.jsx("input",{tabIndex:4,className:s("kcButtonClass","kcButtonPrimaryClass","kcButtonBlockClass","kcButtonLargeClass"),name:"login",id:"kc-login",type:"submit",value:n("doLogIn"),disabled:i})})]})})}),e.enableWebAuthnConditionalUI&&t.jsxs(t.Fragment,{children:[t.jsxs("form",{id:"webauth",action:e.url.loginAction,method:"post",children:[t.jsx("input",{type:"hidden",id:"clientDataJSON",name:"clientDataJSON"}),t.jsx("input",{type:"hidden",id:"authenticatorData",name:"authenticatorData"}),t.jsx("input",{type:"hidden",id:"signature",name:"signature"}),t.jsx("input",{type:"hidden",id:"credentialId",name:"credentialId"}),t.jsx("input",{type:"hidden",id:"userHandle",name:"userHandle"}),t.jsx("input",{type:"hidden",id:"error",name:"error"})]}),e.authenticators!==void 0&&e.authenticators.authenticators.length!==0&&t.jsx(t.Fragment,{children:t.jsx("form",{id:"authn_select",className:s("kcFormClass"),children:e.authenticators.authenticators.map((p,h)=>t.jsx("input",{type:"hidden",name:"authn_use_chk",readOnly:!0,value:p.credentialId},h))})}),t.jsx("br",{})," ",t.jsx("input",{id:o,type:"button",className:s("kcButtonClass","kcButtonDefaultClass","kcButtonBlockClass","kcButtonLargeClass"),value:n("passkey-doAuthenticate")})]})]})}export{v as default};
