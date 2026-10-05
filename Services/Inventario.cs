using System;
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
        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("pt-BR");


        public Inventario(INotificacao? notificacao= null)
        {
            _notificacao = notificacao ?? new Notificacao("operacoes.txt");
        }

        private void RegistrarOperacao(string mensagem)
        {
            try
            {
                _notificacao.Registrar(mensagem);
            }
            catch (Exception erro) when (erro is IOException or UnauthorizedAccessException)
            {
                // A operação já aconteceu; falha no registro não deve sugerir repeti-la.
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
                    "Essa cédula/moeda não existe no caixa.");

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
            
        public void Saque(decimal valorSaque)
        {
            if (!ValidarSaque(valorSaque))
                return;

            var composicao = ComporNotas(
                valorSaque, _cedulas, EstrategiaSaque.PreservarMaioresValores);

            if (composicao == null)
            {
                Console.WriteLine("O caixa possui saldo suficiente, mas não é possível compor o valor com as cédulas disponíveis.");
                ExibirSugestaoSaque(valorSaque);
                return;
            }

            foreach (var (valor, quantidade) in composicao)
            {
                Descarregar(valor, quantidade);
            }

            var notas = string.Join(", ", composicao.Select(nota =>
                $"{nota.Value} unidade(s) de {nota.Key.ToString("C2", _cultura)}"));

            RegistrarOperacao($"Saque: {valorSaque.ToString("C2", _cultura)}; notas: {notas}; total de notas: {composicao.Values.Sum()}; saldo: {ValorTotal.ToString("C2", _cultura)}.");
            ExibirResumoSaque(composicao);
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
            var sugestao = ComporNotas(
                valorSaque, _cedulas, EstrategiaSaque.PreservarMaioresValores,
                permitirValorMenor: true);

            if (sugestao == null)
            {
                Console.WriteLine("Não há um valor menor disponível para sugerir.");
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

        private bool ValidarSaque(decimal valorSaque)
        {
            if (valorSaque <= 0 || decimal.Truncate(valorSaque) != valorSaque)
                throw new ArgumentOutOfRangeException(nameof(valorSaque), "O saque deve ser positivo e inteiro, pois o caixa não possui moedas de centavos.");

            if (valorSaque > ValorTotal)
            {
                Console.WriteLine("Saldo do caixa insuficiente");
                return false;
            }

            return true;
        }
       

        private Dictionary<long, long>? ComporNotas(
            decimal valor, List<CedulaMoeda> cedulas, EstrategiaSaque estrategia,
            bool permitirValorMenor = false)
        {
            ArgumentNullException.ThrowIfNull(cedulas);

            long valorSaque = checked((long) valor);
            long tamanho = valorSaque + 1;
            var combinacoes = new Dictionary<long, long>?[tamanho];
            long[] totalNotas = new long[tamanho];

            combinacoes[0] = new Dictionary<long, long>();

            foreach(var cedula in cedulas.OrderBy(c => c.Valor))
            {
                long valorNota = checked((long)(cedula.Valor));

                for (long valorAtual = valorSaque; valorAtual >= 0; valorAtual--) 
                {
                    var combinacaoAtual = combinacoes[valorAtual];
                    if (combinacaoAtual == null) continue;

                    long valorRestante = valorSaque - valorAtual;
                    long quantidadeQueCabe = valorRestante / valorNota;
                    long quantidadeMaxima = Math.Min(cedula.Quantidade, quantidadeQueCabe);

                    for(long quantidade = 1; quantidade <= quantidadeMaxima; quantidade++)
                    {
                        long novoValor = valorAtual + valorNota * quantidade; ;
                        long novaQuantidadeNotas = totalNotas[valorAtual] + quantidade;

                        bool guardarCombinacao;

                        if (combinacoes[novoValor] == null)
                        {
                            guardarCombinacao = true; // é a primeira solução para esse valor

                        } 
                        else if (estrategia == EstrategiaSaque.PreservarMaioresValores)
                        {
                            guardarCombinacao = false; // a solução anterior usa menos notas maior valor
                        }
                        else
                        {
                            guardarCombinacao = novaQuantidadeNotas < totalNotas[novoValor];
                        }

                        if (guardarCombinacao)
                        {
                            var novaCombinacao = new Dictionary<long, long>(combinacaoAtual);
                            long quantidadeAnterior = novaCombinacao.GetValueOrDefault(valorNota);
                            novaCombinacao[valorNota] = quantidadeAnterior + quantidade;

                            combinacoes[novoValor] = novaCombinacao;
                            totalNotas[novoValor] = novaQuantidadeNotas;
                        }
                    }
                }
            }
    
            if (combinacoes[valorSaque] != null || !permitirValorMenor)
                return combinacoes[valorSaque];

            // As combinações já foram calculadas. Procuramos a maior abaixo do saque.
            for (long valorMenor = valorSaque - 1; valorMenor > 0; valorMenor--)
            {
                if (combinacoes[valorMenor] != null)
                    return combinacoes[valorMenor];
            }

            return null;
        }
    }
}
