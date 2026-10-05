# Sistema de caixa eletrônico

## Requisitos e execução

Projeto do teste técnico Escalasoft, desenvolvido em C# com interface de console e apenas bibliotecas nativas do .NET, conforme o enunciado.

Para executar, é necessário:

- SDK do .NET 10.
- Terminal para informar os dados e selecionar as operações.
- Permissão de escrita no diretório de trabalho para registrar operações em texto.

Na pasta do projeto, execute:

```sh
dotnet restore
dotnet build Teste-tecnico-Sistema_de_caixa_eletronico.csproj
dotnet run --project Teste-tecnico-Sistema_de_caixa_eletronico.csproj
```

Também é possível abrir a solução `Teste-tecnico-Sistema_de_caixa_eletronico.slnx` em um ambiente compatível com o .NET 10.

Ao iniciar, cadastre a primeira conta com identificador único e saldo inicial. Antes de cada operação, informe o ID de uma conta cadastrada. Como o estoque começa vazio, carregue o caixa antes de realizar saques.

O sistema disponibiliza carga, descarga, consulta de estoque, saque, cadastro de contas, consulta de saldo e troca da estratégia de saque. Digite `0` no pedido de identificação ou no menu principal para sair.

Os registros são gravados em `operacoes.txt`, no diretório de trabalho, com data, hora e mensagem da operação. Contas e estoque são mantidos em memória durante a execução.

## Organização do projeto

A organização corresponde às cinco questões do PDF: inventário, estratégias de saque, notificações, sugestões de valores e contas de usuário.

```text
Models/
  CedulaMoeda.cs              Denominação, quantidade e resumo do estoque.
  Conta.cs                   Identificador único, saldo e débito da conta.

Services/
  Inventario.cs              Estoque por denominação usando Dictionary;
                             carga, descarga, consulta, saque e sugestões.
  CadastroContas.cs          Cadastro e identificação de contas por ID.

Estrategias/
  IEstrategiaSaque.cs        Contrato para composição das cédulas/moedas.
  EstrategiaComposicao.cs    Algoritmo compartilhado, com cálculo em centavos.
  MenorQuantidade.cs        Estratégia que busca a menor quantidade de unidades.
  PreservarMaioresValores.cs Estratégia que prioriza preservar notas maiores.

Notificacoes/
  INotificacao.cs            Contrato para registro de eventos.
  Notificacao.cs            Registro em arquivo de texto com data e hora.

Program.cs                  Interface de console, identificação da conta
                            e seleção das operações e da estratégia.

Teste-tecnico-Sistema_de_caixa_eletronico.csproj
                            Configuração do projeto .NET 10.

Teste-tecnico-Sistema_de_caixa_eletronico.slnx
                            Arquivo da solução.

Teste Escalasoft - programador.pdf
                            Enunciado do teste técnico.
```

O inventário executa a estratégia pelo contrato `IEstrategiaSaque`, sem decidir qual algoritmo será usado. As duas estratégias compartilham a busca de combinações e definem seus critérios de escolha separadamente.

As notificações são registradas por meio de `INotificacao`. O inventário permite adicionar várias implementações e envia os eventos a todas elas. Por padrão, utiliza a gravação em arquivo de texto.

O saque valida o saldo da conta, o saldo do caixa e a composição exata antes de debitar e retirar as unidades do estoque. Quando há saldo suficiente, mas a composição é impossível, o sistema informa o problema e sugere um valor menor disponível, sem alterar os saldos.
