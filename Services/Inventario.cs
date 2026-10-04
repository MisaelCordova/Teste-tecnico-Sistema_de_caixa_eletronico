using System;
using System.Collections.Generic;
using System.Text;
using Teste_tecnico_Sistema_de_caixa_eletronico.Models;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Services
{
    public class Inventario
    {
        private readonly List<CedulaMoeda> _cedulas = new();
        public decimal ValorTotal => _cedulas.Sum(c => c.ValorTotal);

        public void Carregar(decimal valor, long quantidade)
        {
            ValidarQuantidade(quantidade);

            var cedula = _cedulas.FirstOrDefault(c => c.Valor == valor);

            if (cedula == null)
            {
                _cedulas.Add(new CedulaMoeda
                {
                    Valor = valor,
                    Quantidade = quantidade
                });
            }
            else
            {
                cedula.Quantidade = checked(cedula.Quantidade + quantidade);
            }
        }
        public void Descarregar(decimal valor, long quantidade)
        {
            ValidarQuantidade(quantidade);

            var cedula = _cedulas.FirstOrDefault(c => c.Valor == valor);

            if (cedula == null)
                throw new InvalidOperationException(
                    "Essa cédula/moeda não existe no caixa.");

            if (cedula.Quantidade < quantidade)
                throw new InvalidOperationException(
                    "Quantidade insuficiente no caixa.");

            cedula.Quantidade -= quantidade;

            if (cedula.Quantidade == 0)
            {
                _cedulas.Remove(cedula);
            }

        }

        public ResumoEstoque[] Consultar()
        {
            return _cedulas.Select(c => new ResumoEstoque
            {
                Cedula = c.Valor,
                Quantidade = c.Quantidade,
                ValorTotal = c.ValorTotal
            }).ToArray();
        }
        private static void ValidarQuantidade(long quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantidade),
                      "A quantidade deve ser positiva.");
        }
    }
}
