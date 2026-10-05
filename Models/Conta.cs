using System;
using System.Collections.Generic;
using System.Text;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Models
{
    public class Conta
    {
        public Conta(string id, decimal saldoInicial)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            if (saldoInicial < 0 || decimal.Round(saldoInicial, 2) != saldoInicial)
                throw new ArgumentOutOfRangeException(nameof(saldoInicial), "O saldo inicial deve ser não negativo e ter até duas casas decimais.");

            Id = id.Trim();
            saldo = saldoInicial;
        }

        public string Id { get;  }
        private decimal saldo;
        public decimal Saldo { get { return saldo; } }

        internal void Debitar(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentOutOfRangeException(nameof(valor), "O débito deve ser positivo.");
            if (valor > saldo)
                throw new InvalidOperationException("Saldo insuficiente na conta.");
            saldo -= valor;
        }
       
    }
}
