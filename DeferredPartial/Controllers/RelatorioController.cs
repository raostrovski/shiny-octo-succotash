using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeferredPartial.MVC.Controllers
{
    public class RelatorioController : Controller
    {
        // GET: Relatorio
        public ActionResult Index()
        {
            var model = new RelatorioViewModel
            {
                Nome = "Relatório financeiro",
                Total = 15000
            };

            return View(model);
        }
    }

    public class RelatorioViewModel
    {
        public string Nome { get; set; }
        public int Total { get; set; }
    }
}