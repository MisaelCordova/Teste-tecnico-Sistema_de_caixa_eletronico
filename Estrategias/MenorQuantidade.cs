namespace Teste_tecnico_Sistema_de_caixa_eletronico.Estrategias
{
    public class MenorQuantidade : EstrategiaComposicao
    {
        public override string Nome => "Menor quantidade de notas";

        protected override bool DeveSubstituir(long novaQuantidade, long quantidadeAnterior)
        {
            return novaQuantidade < quantidadeAnterior;
        }
    }
}
