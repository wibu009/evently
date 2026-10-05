import{r as m,j as e}from"./evently-C3WtrOYl.js";import{a as y}from"./id-D1fVGO1t.js";import{a as u,u as p,b as v,c as i}from"./KcPage-D9pouytT.js";import{u as C,T as f}from"./Template-Cd60yQwq.js";import{w as h}from"./waitForElementMountedOnDom-qpCjLZnq.js";import{L as g}from"./LogoutOtherSessions-DMEfCVkh.js";import"./index-CKYewT40.js";function x(r){const{olRecoveryCodesListId:t}=r,{msgStr:o,isFetchingTranslations:c}=u(),{insertScriptTags:n}=C({effectId:"LoginRecoveryAuthnCodeConfig",scriptTags:[{type:"text/javascript",textContent:()=>`

                    /* copy recovery codes  */
                    function copyRecoveryCodes() {
                        var tmpTextarea = document.createElement("textarea");
                        var codes = document.querySelectorAll("#${t} li");
                        for (i = 0; i < codes.length; i++) {
                            tmpTextarea.value = tmpTextarea.value + codes[i].innerText + "\\n";
                        }
                        document.body.appendChild(tmpTextarea);
                        tmpTextarea.select();
                        document.execCommand("copy");
                        document.body.removeChild(tmpTextarea);
                    }

                    var copyButton = document.getElementById("copyRecoveryCodes");
                    copyButton && copyButton.addEventListener("click", function () {
                        copyRecoveryCodes();
                    });

                    /* download recovery codes  */
                    function formatCurrentDateTime() {
                        var dt = new Date();
                        var options = {
                            month: 'long',
                            day: 'numeric',
                            year: 'numeric',
                            hour: 'numeric',
                            minute: 'numeric',
                            timeZoneName: 'short'
                        };

                        return dt.toLocaleString('en-US', options);
                    }

                    function parseRecoveryCodeList() {
                        var recoveryCodes = document.querySelectorAll("#${t} li");
                        var recoveryCodeList = "";

                        for (var i = 0; i < recoveryCodes.length; i++) {
                            var recoveryCodeLiElement = recoveryCodes[i].innerText;
                            recoveryCodeList += recoveryCodeLiElement + "\\r\\n";
                        }

                        return recoveryCodeList;
                    }

                    function buildDownloadContent() {
                        var recoveryCodeList = parseRecoveryCodeList();
                        var dt = new Date();
                        var options = {
                            month: 'long',
                            day: 'numeric',
                            year: 'numeric',
                            hour: 'numeric',
                            minute: 'numeric',
                            timeZoneName: 'short'
                        };

                        return fileBodyContent =
                            ${JSON.stringify(o("recovery-codes-download-file-header"))} + "\\n\\n" +
                            recoveryCodeList + "\\n" +
                            ${JSON.stringify(o("recovery-codes-download-file-description"))} + "\\n\\n" +
                            ${JSON.stringify(o("recovery-codes-download-file-date"))} + " " + formatCurrentDateTime();
                    }

                    function setUpDownloadLinkAndDownload(filename, text) {
                        var el = document.createElement('a');
                        el.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
                        el.setAttribute('download', filename);
                        el.style.display = 'none';
                        document.body.appendChild(el);
                        el.click();
                        document.body.removeChild(el);
                    }

                    function downloadRecoveryCodes() {
                        setUpDownloadLinkAndDownload('kc-download-recovery-codes.txt', buildDownloadContent());
                    }

                    var downloadButton = document.getElementById("downloadRecoveryCodes");
                    downloadButton && downloadButton.addEventListener("click", downloadRecoveryCodes);

                    /* print recovery codes */
                    function buildPrintContent() {
                        var recoveryCodeListHTML = document.getElementById('${t}').innerHTML;
                        var styles =
                            \`@page { size: auto;  margin-top: 0; }
                            body { width: 480px; }
                            div { list-style-type: none; font-family: monospace }
                            p:first-of-type { margin-top: 48px }\`;

                        return printFileContent =
                            "<html><style>" + styles + "</style><body>" +
                            "<title>kc-download-recovery-codes</title>" +
                            "<p>" + ${JSON.stringify(o("recovery-codes-download-file-header"))} + "</p>" +
                            "<div>" + recoveryCodeListHTML + "</div>" +
                            "<p>" + ${JSON.stringify(o("recovery-codes-download-file-description"))} + "</p>" +
                            "<p>" + ${JSON.stringify(o("recovery-codes-download-file-date"))} + " " + formatCurrentDateTime() + "</p>" +
                            "</body></html>";
                    }

                    function printRecoveryCodes() {
                        var w = window.open();
                        w.document.write(buildPrintContent());
                        w.print();
                        w.close();
                    }

                    var printButton = document.getElementById("printRecoveryCodes");
                    printButton && printButton.addEventListener("click", printRecoveryCodes);
                `}]});m.useEffect(()=>{c||(async()=>(await h({elementId:t}),n()))()},[c])}function N(){const{kcContext:r}=p();y(r.pageId==="login-recovery-authn-code-config.ftl");const{kcClsx:t}=v(),{recoveryAuthnCodesConfigBean:o,isAppInitiatedAction:c}=r,{msg:n,msgStr:a}=u(),d="kc-recovery-codes-list";return x({olRecoveryCodesListId:d}),e.jsxs(f,{headerNode:n("recovery-code-config-header"),children:[e.jsxs("div",{className:i("pf-c-alert","pf-m-warning","pf-m-inline",t("kcRecoveryCodesWarning")),"aria-label":"Warning alert",children:[e.jsx("div",{className:"pf-c-alert__icon",children:e.jsx("i",{className:"pficon-warning-triangle-o","aria-hidden":"true"})}),e.jsxs("h4",{className:"pf-c-alert__title",children:[e.jsx("span",{className:"pf-screen-reader",children:"Warning alert:"}),n("recovery-code-config-warning-title")]}),e.jsx("div",{className:"pf-c-alert__description",children:e.jsx("p",{children:n("recovery-code-config-warning-message")})})]}),e.jsx("ol",{id:d,className:t("kcRecoveryCodesList"),children:o.generatedRecoveryAuthnCodesList.map((s,l)=>e.jsxs("li",{children:[e.jsxs("span",{children:[l+1,":"]})," ",s.slice(0,4),"-",s.slice(4,8),"-",s.slice(8)]},l))}),e.jsxs("div",{className:t("kcRecoveryCodesActions"),children:[e.jsxs("button",{id:"printRecoveryCodes",className:i("pf-c-button","pf-m-link"),type:"button",children:[e.jsx("i",{className:"pficon-print","aria-hidden":"true"})," ",n("recovery-codes-print")]}),e.jsxs("button",{id:"downloadRecoveryCodes",className:i("pf-c-button","pf-m-link"),type:"button",children:[e.jsx("i",{className:"pficon-save","aria-hidden":"true"})," ",n("recovery-codes-download")]}),e.jsxs("button",{id:"copyRecoveryCodes",className:i("pf-c-button","pf-m-link"),type:"button",children:[e.jsx("i",{className:"pficon-blueprint","aria-hidden":"true"})," ",n("recovery-codes-copy")]})]}),e.jsxs("div",{className:t("kcFormOptionsClass"),children:[e.jsx("input",{className:t("kcCheckInputClass"),type:"checkbox",id:"kcRecoveryCodesConfirmationCheck",name:"kcRecoveryCodesConfirmationCheck",onChange:s=>{document.getElementById("saveRecoveryAuthnCodesBtn").disabled=!s.target.checked}}),e.jsx("label",{htmlFor:"kcRecoveryCodesConfirmationCheck",children:n("recovery-codes-confirmation-message")})]}),e.jsxs("form",{action:r.url.loginAction,className:t("kcFormGroupClass"),id:"kc-recovery-codes-settings-form",method:"post",children:[e.jsx("input",{type:"hidden",name:"generatedRecoveryAuthnCodes",value:o.generatedRecoveryAuthnCodesAsString}),e.jsx("input",{type:"hidden",name:"generatedAt",value:o.generatedAt}),e.jsx("input",{type:"hidden",id:"userLabel",name:"userLabel",value:a("recovery-codes-label-default")}),e.jsx(g,{}),c?e.jsxs(e.Fragment,{children:[e.jsx("input",{type:"submit",className:t("kcButtonClass","kcButtonPrimaryClass","kcButtonLargeClass"),id:"saveRecoveryAuthnCodesBtn",value:a("recovery-codes-action-complete"),disabled:!0}),e.jsx("button",{type:"submit",className:t("kcButtonClass","kcButtonDefaultClass","kcButtonLargeClass"),id:"cancelRecoveryAuthnCodesBtn",name:"cancel-aia",value:"true",children:n("recovery-codes-action-cancel")})]}):e.jsx("input",{type:"submit",className:t("kcButtonClass","kcButtonPrimaryClass","kcButtonBlockClass","kcButtonLargeClass"),id:"saveRecoveryAuthnCodesBtn",value:a("recovery-codes-action-complete"),disabled:!0})]})]})}export{N as default};
