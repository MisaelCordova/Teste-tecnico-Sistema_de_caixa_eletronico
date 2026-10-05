namespace Teste_tecnico_Sistema_de_caixa_eletronico.Estrategias
{
    public class PreservarMaioresValores : EstrategiaComposicao
    {
        public override string Nome => "Preservar notas de maior valor";

        protected override bool DeveSubstituir(long novaQuantidade, long quantidadeAnterior)
        {
            return false;
        }
    }
}
