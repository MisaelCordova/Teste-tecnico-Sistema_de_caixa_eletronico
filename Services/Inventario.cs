using System;
using Teste_tecnico_Sistema_de_caixa_eletronico.Estrategias;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teste_tecnico_Sistema_de_caixa_eletronico.Models;
using Teste_tecnico_Sistema_de_caixa_eletronico.Notificacoes;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Services
{
    public class Inventario
    {
        private readonly List<CedulaMoeda> _cedulas = new();
        private readonly INotificacao _notificacao;

        public IEstrategiaSaque Estrategia { get; private set; }
        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("pt-BR");


        public Inventario(INotificacao? notificacao = null, IEstrategiaSaque? estrategia = null)
        {
            _notificacao = notificacao ?? new Notificacao("operacoes.txt");
            Estrategia = estrategia ?? new MenorQuantidade();
        }

        private void RegistrarOperacao(string mensagem)
        {
            try
            {
                _notificacao.Registrar(mensagem);
            }
            catch (Exception erro) when (erro is IOException or UnauthorizedAccessException)
            {
            
                Console.Error.WriteLine($"Aviso: não foi possível registrar a operação no arquivo: {erro.Message}");
            }
        }
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

            RegistrarOperacao($"Carga: {quantidade} unidade(s) de {valor.ToString("C2", _cultura)}; total: {(valor * quantidade).ToString("C2", _cultura)}; saldo: {ValorTotal.ToString("C2", _cultura)}.");
        }
        public void Descarregar(decimal valor, long quantidade)
        {
            ValidarQuantidade(quantidade);

            var cedula = _cedulas.FirstOrDefault(c => c.Valor == valor);

            if (cedula == null)
                throw new InvalidOperationException(
                    "Essa cedula/moeda não existe no caixa.");

            if (cedula.Quantidade < quantidade)
                throw new InvalidOperationException(
                    "Quantidade insuficiente no caixa.");

            cedula.Quantidade -= quantidade;

            if (cedula.Quantidade == 0)
            {
                _cedulas.Remove(cedula);
            }

            RegistrarOperacao($"Descarga: {quantidade} unidade(s) de {valor.ToString("C2", _cultura)}; total: {(valor * quantidade).ToString("C2", _cultura)}; saldo: {ValorTotal.ToString("C2", _cultura)}.");
        }

        public void TrocarEstrategia(IEstrategiaSaque estrategia)
        {
            ArgumentNullException.ThrowIfNull(estrategia);

            Estrategia = estrategia;
        }

        public ResumoEstoque[] Consultar()
        {
            var resumo = _cedulas.Select(c => new ResumoEstoque
            {
                Cedula = c.Valor,
                Quantidade = c.Quantidade,
                ValorTotal = c.ValorTotal
            }).ToArray();

            RegistrarOperacao($"Consulta de estoque: {resumo.Length} tipo(s) de cédula/moeda; saldo: {ValorTotal.ToString("C2", _cultura)}.");
            return resumo;
        }
        private static void ValidarQuantidade(long quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantidade),
                      "A quantidade deve ser positiva.");
        }
            
        public void Saque(Conta conta, decimal valorSaque)
        {
            ArgumentNullException.ThrowIfNull(conta);
            ValidarValorSaque(valorSaque);

            if (valorSaque > conta.Saldo)
            {
                Console.WriteLine("Saldo insuficiente na conta.");
                return;
            }

            if (!ValidarSaque(valorSaque))
                return;

            var composicao = Estrategia.ComporNotas(
                valorSaque, _cedulas);

            if (composicao == null)
            {
                Console.WriteLine("O caixa possui saldo suficiente, mas não é possível compor o valor com as cédulas disponíveis.");
                ExibirSugestaoSaque(valorSaque);
                return;
            }

           
            conta.Debitar(valorSaque);
            foreach (var (valor, quantidade) in composicao)
            {
                Descarregar(valor, quantidade);
            }

            var notas = string.Join(", ", composicao.Select(nota =>
                $"{nota.Value} unidade(s) de {nota.Key.ToString("C2", _cultura)}"));

            RegistrarOperacao($"Conta: {conta.Id}; saque: {valorSaque.ToString("C2", _cultura)}; notas: {notas}; total de notas: {composicao.Values.Sum()}; saldo do caixa: {ValorTotal.ToString("C2", _cultura)}; saldo da conta: {conta.Saldo.ToString("C2", _cultura)}.");
            ExibirResumoSaque(composicao);
            Console.WriteLine($"Saldo da conta: {conta.Saldo.ToString("C2", _cultura)}");
        }

        private static void ExibirResumoSaque(Dictionary<long, long> composicao)
        {
         
            Console.WriteLine("Saque efetuado com sucesso com:");
            foreach (var (valor, quantidade) in composicao)
            {
                Console.WriteLine($"{quantidade} nota(s) de {valor.ToString("C2", _cultura)}");
            }

            Console.WriteLine($"Total de notas: {composicao.Values.Sum()}");
        }

        private void ExibirSugestaoSaque(decimal valorSaque)
        {
            var sugestao = Estrategia.ComporNotas(
                valorSaque, _cedulas,
                permitirValorMenor: true);

            if (sugestao == null)
            {
                Console.WriteLine("Não há um valor menor disponável para sugerir.");
                return;
            }

            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            decimal valorSugerido = sugestao.Sum(nota => (decimal)nota.Key * nota.Value);

            Console.WriteLine($"Sugestão: sacar {valorSugerido.ToString("C2", cultura)} com:");
            foreach (var (valor, quantidade) in sugestao)
            {
                Console.WriteLine($"{quantidade} nota(s) de {valor.ToString("C2", cultura)}");
            }

            Console.WriteLine("Para sacar esse valor, faça uma nova solicitação.");
        }

        private static void ValidarValorSaque(decimal valorSaque)
        {
            if (valorSaque <= 0 || decimal.Truncate(valorSaque) != valorSaque)
                throw new ArgumentOutOfRangeException(nameof(valorSaque), "O saque deve ser positivo e inteiro, pois o caixa não possui moedas de centavos.");
        }

        private bool ValidarSaque(decimal valorSaque)
        {
            if (valorSaque > ValorTotal)
            {
                Console.WriteLine("Saldo do caixa insuficiente");
                return false;
            }

            return true;
        }
       

    }
}

