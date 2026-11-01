using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeferredPartial.CrossCutting.Helpers
{
    /// <summary>
    /// Gera uma identificação que vincula o conteúdo armazenado ao usuário e à sessão que fizeram a requisição original.
    /// </summary>
    public static class DeferredPartialOwner
    {
        private const string SessionKey =
            "__DeferredPartialOwner";

        public static string GetOrCreate(
            HttpSessionStateBase session,
            string userName)
        {
            if (session == null)
                throw new InvalidOperationException(
                    "Session não disponível."
                );

            var ownerId = session[SessionKey] as string;

            if (string.IsNullOrEmpty(ownerId))
            {
                ownerId = Guid.NewGuid().ToString("N");

                // Persiste um identificador na Session.
                session[SessionKey] = ownerId;
            }

            var user = string.IsNullOrWhiteSpace(userName)
                ? "anonymous"
                : userName;

            return user + "|" + ownerId;
        }

        public static string Get(
            HttpSessionStateBase session,
            string userName)
        {
            if (session == null)
                return null;

            var ownerId = session[SessionKey] as string;

            if (string.IsNullOrEmpty(ownerId))
                return null;

            var user = string.IsNullOrWhiteSpace(userName)
                ? "anonymous"
                : userName;

            return user + "|" + ownerId;
        }
    }
}
