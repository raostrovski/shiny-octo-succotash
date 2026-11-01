using System.Web.Mvc;
using System.Web.Routing;

namespace DeferredPartial.CrossCutting.Models
{
    /// <summary>
    /// Representa os dados necessários para renderizar a Partial View na segunda requisição.
    /// </summary>
    public class DeferredPartialEntry
    {
        public object Model { get; set; }

        public string Owner { get; set; }

        public string PartialName { get; set; }

        public RouteValueDictionary RouteValues { get; set; }

        public RouteValueDictionary DataTokens { get; set; }
    }
}
