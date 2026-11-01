using DeferredPartial.CrossCutting.Contracts;
using DeferredPartial.CrossCutting.Helpers;
using DeferredPartial.MVC.Results;
using DeferredPartial.Services.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace DeferredPartial.MVC.Controllers
{
    /// <summary>
    /// O Controller fica no projeto MVC, fora de qualquer Area.
    /// </summary>
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class DeferredController : Controller
    {
        private readonly IDeferredPartialStore _store;
        private readonly DeferredViewResolver _resolver;

        public DeferredController(
            IDeferredPartialStore store,
            DeferredViewResolver resolver)
        {
            _store = store;
            _resolver = resolver;
        }

        [HttpGet]
        public ActionResult Render(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(400);
            }

            var identity = HttpContext.User.Identity;

            var userName = identity != null &&
                           identity.IsAuthenticated
                ? identity.Name
                : "anonymous";

            if (Session == null)
            {
                return new HttpStatusCodeResult(400);
            }

            var owner = DeferredPartialOwner.Get(
                Session,
                userName
            );

            if (owner == null)
            {
                return HttpNotFound();
            }

            var entry = _store.Take(id, owner);

            if (entry == null)
            {
                return HttpNotFound();
            }

            return new DeferredPartialViewResult(
                entry,
                _resolver
            );
        }
    }
}