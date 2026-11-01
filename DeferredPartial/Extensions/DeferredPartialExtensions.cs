using DeferredPartial.CrossCutting.Contracts;
using DeferredPartial.CrossCutting.Helpers;
using DeferredPartial.CrossCutting.Models;
using DeferredPartial.Services.Resolvers;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DeferredPartial.MVC.Extensions
{
    /// <summary>
    /// Este é o método que será utilizado pelos desenvolvedores nas Views.
    /// </summary>
    public static class DeferredPartialExtensions
    {
        public static IHtmlString RenderDeferredPartial(this HtmlHelper html, string partialName, object model = null)
        {
            if (string.IsNullOrWhiteSpace(partialName))
            {
                throw new ArgumentException(
                    "O nome da Partial View é obrigatório.",
                    "partialName"
                );
            }

            var store = DependencyResolver.Current
                .GetService(typeof(IDeferredPartialStore))
                as IDeferredPartialStore;

            var resolver = DependencyResolver.Current
                .GetService(typeof(DeferredViewResolver))
                as DeferredViewResolver;

            if (store == null || resolver == null)
            {
                throw new InvalidOperationException(
                    "Serviços do DeferredPartial não registrados."
                );
            }

            var controllerContext =
                html.ViewContext.Controller.ControllerContext;

            var viewResult = resolver.Resolve(
                controllerContext,
                partialName
            );

            try
            {
                var httpContext = html.ViewContext.HttpContext;

                var identity = httpContext.User.Identity;

                var userName = identity != null &&
                               identity.IsAuthenticated
                    ? identity.Name
                    : "anonymous";

                if (httpContext.Session == null)
                {
                    throw new InvalidOperationException(
                        "Session não está disponível."
                    );
                }

                var owner = DeferredPartialOwner.GetOrCreate(
                    httpContext.Session,
                    userName
                );

                var entry = new DeferredPartialEntry
                {
                    Model = model,
                    Owner = owner,
                    PartialName = partialName,

                    RouteValues = new RouteValueDictionary(
                        controllerContext.RouteData.Values
                    ),

                    DataTokens = new RouteValueDictionary(
                        controllerContext.RouteData.DataTokens
                    )
                };

                var id = store.Store(entry);

                var urlHelper = new UrlHelper(
                    html.ViewContext.RequestContext
                );

                var url = urlHelper.Action(
                    "Render",
                    "Deferred",
                    new
                    {
                        area = "",
                        id = id
                    }
                );

                if (string.IsNullOrEmpty(url))
                {
                    viewResult.ViewEngine.ReleaseView(
                        controllerContext,
                        viewResult.View
                    );

                    throw new InvalidOperationException(
                        "Não foi possível gerar a URL do DeferredController."
                    );
                }

                var content = string.Format(
                    "<div data-partial-ajax-url=\"{0}\">Carregando...</div>",
                    HttpUtility.HtmlAttributeEncode(url)
                );

                return MvcHtmlString.Create(content);
            }
            finally
            {
                viewResult.ViewEngine.ReleaseView(
                    controllerContext,
                    viewResult.View
                );
            }
        }
    }
}