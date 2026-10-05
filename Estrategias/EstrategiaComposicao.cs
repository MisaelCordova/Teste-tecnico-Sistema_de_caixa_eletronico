using Teste_tecnico_Sistema_de_caixa_eletronico.Models;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Estrategias
{
    
    public abstract class EstrategiaComposicao : IEstrategiaSaque
    {
        public abstract string Nome { get; }
        protected abstract bool DeveSubstituir(long novaQuantidade, long quantidadeAnterior);

        public Dictionary<long, long>? ComporNotas(
            decimal valor, IEnumerable<CedulaMoeda> cedulas,
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
                long valorNota = checked((long)cedula.Valor);

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

                        bool guardarCombinacao = combinacoes[novoValor] == null ||
                            DeveSubstituir(novaQuantidadeNotas, totalNotas[novoValor]);

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

            for (long valorMenor = valorSaque - 1; valorMenor > 0; valorMenor--)
            {
                if (combinacoes[valorMenor] != null)
                    return combinacoes[valorMenor];
            }

            return null;
        }
    }
}

