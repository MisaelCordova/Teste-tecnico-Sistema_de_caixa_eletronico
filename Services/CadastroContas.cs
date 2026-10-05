using System;
using System.Collections.Generic;
using System.Text;
using Teste_tecnico_Sistema_de_caixa_eletronico.Models;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Services
{
    public class CadastroContas
    {
        private readonly Dictionary<string, Conta> contas = new(StringComparer.Ordinal);

        public void Registrar(Conta conta)
        {
            ArgumentNullException.ThrowIfNull(conta);
            if (!contas.TryAdd(conta.Id, conta))
                throw new InvalidOperationException("Já existe uma conta com esse identificador");

        }

        public Conta Autenticar(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            return contas.TryGetValue(id.Trim(), out var conta) ? conta : throw new InvalidOperationException("Conta não encontrada. Informe um identificador cadastrado.");
        }
    }
}
