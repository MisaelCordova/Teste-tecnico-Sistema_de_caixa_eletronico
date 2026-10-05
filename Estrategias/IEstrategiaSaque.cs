using Teste_tecnico_Sistema_de_caixa_eletronico.Models;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Estrategias
{
    public interface IEstrategiaSaque
    {
        string Nome { get; }

        Dictionary<long, long>? ComporNotas(
            decimal valor, List<CedulaMoeda> cedulas,
            bool permitirValorMenor = false);
    }
}
