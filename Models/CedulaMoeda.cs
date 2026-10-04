using System;
using System.Collections.Generic;
using System.Text;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Models
{
    public class CedulaMoeda
    {
        private static readonly decimal[] ValoresPermitidos =
        {
            1m, 2m, 5m, 10m, 20m, 50m, 100m, 200m
        };
        private decimal _valor;

        public decimal Valor
        {
            get => _valor;
            set
            {
                if (Array.IndexOf(ValoresPermitidos, value) < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Valor), value,
                        "Valor de cédula ou moeda inválido.");
                }

                _valor = value;
            }
        }
        public long Quantidade { get; set; }
        public decimal ValorTotal => Valor * Quantidade;
    }

    public class ResumoEstoque
    {
        public decimal Cedula { get; set; }
        public long Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
