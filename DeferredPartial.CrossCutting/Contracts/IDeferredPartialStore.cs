using DeferredPartial.CrossCutting.Models;

namespace DeferredPartial.CrossCutting.Contracts
{
    /// <summary>
    /// O contrato define como armazenar e recuperar uma Partial View adiada.
    /// </summary>
    public interface IDeferredPartialStore
    {
        string Store(DeferredPartialEntry entry);

        DeferredPartialEntry Take(string id, string owner);
    }
}
