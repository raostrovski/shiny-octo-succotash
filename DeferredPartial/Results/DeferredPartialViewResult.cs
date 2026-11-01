using DeferredPartial.CrossCutting.Models;
using DeferredPartial.Services.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DeferredPartial.MVC.Results
{
    /// <summary>
    /// Responsável por renderizar a View que foi resolvida e armazenada na primeira requisição.
    /// </summary>
    public class DeferredPartialViewResult : ActionResult
    {
        private readonly DeferredPartialEntry _entry;
        private readonly DeferredViewResolver _resolver;

        public DeferredPartialViewResult(
            DeferredPartialEntry entry,
            DeferredViewResolver resolver)
        {
            _entry = entry;
            _resolver = resolver;
        }

        public override void ExecuteResult(
            ControllerContext controllerContext)
        {
            if (_entry == null)
            {
                throw new InvalidOperationException(
                    "Entrada da Partial não encontrada."
                );
            }

            var routeData = new RouteData();

            foreach (var item in _entry.RouteValues)
            {
                routeData.Values[item.Key] = item.Value;
            }

            foreach (var item in _entry.DataTokens)
            {
                routeData.DataTokens[item.Key] = item.Value;
            }

            var originalContext = new ControllerContext(
                controllerContext.HttpContext,
                routeData,
                controllerContext.Controller
            );

            ViewEngineResult viewResult = null;

            try
            {
                viewResult = _resolver.Resolve(
                    originalContext,
                    _entry.PartialName
                );

                var viewData = new ViewDataDictionary(
                    _entry.Model
                );

                var tempData = new TempDataDictionary();

                var response =
                    controllerContext.HttpContext.Response;

                response.ContentType = "text/html";

                var viewContext = new ViewContext(
                    originalContext,
                    viewResult.View,
                    viewData,
                    tempData,
                    response.Output
                );

                viewResult.View.Render(
                    viewContext,
                    response.Output
                );
            }
            finally
            {
                if (viewResult != null &&
                    viewResult.View != null &&
                    viewResult.ViewEngine != null)
                {
                    viewResult.ViewEngine.ReleaseView(
                        originalContext,
                        viewResult.View
                    );
                }
            }
        }
    }
}