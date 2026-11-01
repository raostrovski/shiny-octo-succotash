(function (window, document) {
    "use strict";

    var loadedScripts = new Map();

    function isExecutableScript(script) {
        var type = (script.type || "").trim().toLowerCase();

        return type === "" ||
            type === "module" ||
            type === "text/javascript" ||
            type === "application/javascript" ||
            type === "text/ecmascript" ||
            type === "application/ecmascript";
    }

    function isModule(script) {
        return (script.type || "").trim().toLowerCase() === "module";
    }

    function getScriptKey(script) {
        var url = new URL(script.src, document.baseURI).href;

        var type = isModule(script)
            ? "module"
            : "classic";

        return type + "|" + url;
    }

    function registerExistingScripts() {
        Array.prototype.forEach.call(
            document.scripts,
            function (script) {
                if (script.src && isExecutableScript(script)) {
                    loadedScripts.set(
                        getScriptKey(script),
                        Promise.resolve()
                    );
                }
            }
        );
    }

    function copyAttributes(source, target) {
        Array.prototype.forEach.call(
            source.attributes,
            function (attribute) {
                var name = attribute.name.toLowerCase();

                if (
                    name === "src" ||
                    name === "async" ||
                    name === "defer"
                ) {
                    return;
                }

                target.setAttribute(
                    attribute.name,
                    attribute.value
                );
            }
        );
    }

    function loadExternalScript(oldScript) {
        var key = getScriptKey(oldScript);

        if (loadedScripts.has(key)) {
            return loadedScripts.get(key);
        }

        var promise = new Promise(function (resolve, reject) {
            var newScript = document.createElement("script");

            copyAttributes(oldScript, newScript);

            newScript.async = false;

            newScript.onload = resolve;

            newScript.onerror = function () {
                reject(
                    new Error(
                        "Erro ao carregar: " + oldScript.src
                    )
                );
            };

            newScript.src = oldScript.src;

            document.head.appendChild(newScript);
        });

        loadedScripts.set(key, promise);

        promise.catch(function () {
            if (loadedScripts.get(key) === promise) {
                loadedScripts.delete(key);
            }
        });

        return promise;
    }

    function executeInlineScript(oldScript) {
        return new Promise(function (resolve, reject) {
            var newScript = document.createElement("script");

            copyAttributes(oldScript, newScript);

            if (isModule(oldScript)) {
                newScript.onload = resolve;

                newScript.onerror = function () {
                    reject(
                        new Error("Erro ao executar módulo inline.")
                    );
                };

                newScript.textContent = oldScript.textContent;

                oldScript.parentNode.replaceChild(
                    newScript,
                    oldScript
                );

                return;
            }

            newScript.textContent = oldScript.textContent;

            oldScript.parentNode.replaceChild(
                newScript,
                oldScript
            );

            resolve();
        });
    }

    async function executeScripts(container) {
        var scripts = Array.prototype.slice.call(
            container.querySelectorAll("script")
        );

        var errors = [];

        for (var i = 0; i < scripts.length; i++) {
            var script = scripts[i];

            if (!isExecutableScript(script)) {
                continue;
            }

            try {
                if (script.src) {
                    await loadExternalScript(script);
                    script.remove();
                } else {
                    await executeInlineScript(script);
                }
            } catch (error) {
                errors.push(error);

                console.error(
                    "[DeferredRendering] Script error:",
                    error
                );

                if (script.parentNode) {
                    script.remove();
                }
            }
        }

        return errors;
    }

    window.DeferredScriptExecutor = {
        execute: executeScripts
    };

    async function loadPartial(element) {
        var url = element.getAttribute(
            "data-partial-ajax-url"
        );

        try {
            var response = await fetch(url, {
                method: "GET",
                credentials: "same-origin",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error("HTTP " + response.status);
            }

            var html = await response.text();

            element.innerHTML = html;

            var errors = await executeScripts(element);

            element.removeAttribute(
                "data-partial-ajax-url"
            );

            document.dispatchEvent(
                new CustomEvent("deferredPartialLoaded", {
                    detail: {
                        element: element,
                        errors: errors
                    }
                })
            );
        } catch (error) {
            console.error(
                "[DeferredRendering] Partial error:",
                error
            );

            element.innerHTML = "Erro ao carregar conteúdo.";

            document.dispatchEvent(
                new CustomEvent("deferredPartialError", {
                    detail: {
                        element: element,
                        error: error
                    }
                })
            );
        }
    }

    function loadAll() {
        var elements = document.querySelectorAll(
            "[data-partial-ajax-url]"
        );

        Array.prototype.forEach.call(
            elements,
            function (element) {
                loadPartial(element);
            }
        );
    }

    window.DeferredPartialLoader = {
        load: loadPartial,
        loadAll: loadAll
    };

    function initialize() {
        registerExistingScripts();
        loadAll();
    }

    if (document.readyState === "complete") {
        initialize();
    } else {
        window.addEventListener(
            "load",
            initialize,
            { once: true }
        );
    }

})(window, document);