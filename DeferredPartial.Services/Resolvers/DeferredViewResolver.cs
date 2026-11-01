using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DeferredPartial.Services.Resolvers
{
    /// <summary>
    /// Responsável por encontrar a Partial View utilizando o contexto original do Controller.
    /// Isso é importante para que o MVC consiga resolver corretamente Views dentro de Areas, pastas específicas e ViewEngines customizadas.
    /// </summary>
    public class DeferredViewResolver
    {
        public ViewEngineResult Resolve(
            ControllerContext controllerContext,
            string partialName)
        {
            var result = ViewEngines.Engines.FindPartialView(
                controllerContext,
                partialName
            );

            if (result.View != null)
                return result;

            var locations = string.Join(
                    Environment.NewLine,
                    result.SearchedLocations
                        ?? new string[0]
                );

            throw new InvalidOperationException(
                "Partial View não encontrada: " +
                partialName +
                Environment.NewLine +
                locations
            );
        }
    }
}
