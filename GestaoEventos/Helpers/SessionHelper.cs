using GestaoEventos.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GestaoEventos.Helpers
{
    public static class SessionHelper
    {
        private const string ChaveCarrinho = "carrinho";

        public static List<CarrinhoItem> ObterCarrinho(ISession session)
        {
            var json = session.GetString(ChaveCarrinho);
            if (string.IsNullOrEmpty(json)) return new List<CarrinhoItem>();
            return JsonSerializer.Deserialize<List<CarrinhoItem>>(json) ?? new();
        }

        public static void SalvarCarrinho(ISession session, List<CarrinhoItem> carrinho)
            => session.SetString(ChaveCarrinho, JsonSerializer.Serialize(carrinho));

        public static void LimparCarrinho(ISession session)
            => session.Remove(ChaveCarrinho);

        public static void AdicionarItem(ISession session, CarrinhoItem novoItem)
        {
            var carrinho = ObterCarrinho(session);
            var existente = carrinho.FirstOrDefault(i => i.IngressoId == novoItem.IngressoId);
            if (existente != null) existente.Quantidade += novoItem.Quantidade;
            else carrinho.Add(novoItem);
            SalvarCarrinho(session, carrinho);
        }
        public static void RemoverItem(ISession session, int ingressoId)
        {
            var carrinho = ObterCarrinho(session);
            carrinho.RemoveAll(i => i.IngressoId == ingressoId);
            SalvarCarrinho(session, carrinho);
        }
        public static int TotalItens(ISession session)
            => ObterCarrinho(session).Sum(i => i.Quantidade);
    }
}