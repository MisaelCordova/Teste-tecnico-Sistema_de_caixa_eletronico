using System.Globalization;
using System.Text;
using Teste_tecnico_Sistema_de_caixa_eletronico.Models;
using Teste_tecnico_Sistema_de_caixa_eletronico.Services;

Console.OutputEncoding = Encoding.UTF8;
var cultura = CultureInfo.GetCultureInfo("pt-BR");
var inventario = new Inventario();
var cadastroContas = new CadastroContas();
Console.WriteLine("Caixa eletrônico — gestão de inventário");
Console.WriteLine("Cadastre a primeira conta para começar.");
if (!CadastrarConta()) return;

while (true)
{
    try
    {
        Console.Write("\nID da conta (0 para sair): ");
        string? idConta = Console.ReadLine()?.Trim();
        if (idConta is null or "0") break;

        var conta = cadastroContas.Autenticar(idConta);
        Console.WriteLine($"Conta: {conta.Id} | Saldo: {conta.Saldo.ToString("C2", cultura)}");
        Console.WriteLine($"Estratégia atual: {NomeEstrategia(inventario.Estrategia)}");

        Console.WriteLine("\n1 - Carregar" +
                          "\n2 - Descarregar" +
                          "\n3 - Consultar estoque" +
                          "\n4 - Saque" +
                          "\n5 - Cadastrar conta" +
                          "\n6 - Consultar saldo da conta" +
                          "\n7 - Trocar estratégia de saque" +
                          "\n0 - Sair");

        Console.Write("Opção: ");
        string? opcao = Console.ReadLine();

        if (opcao is null or "0")
        {
            Console.WriteLine("Até mais! Obrigado por utilizar o caixa.");
            break;
        }

        if (opcao == "3")
        {
            Console.WriteLine("\nDINHEIRO DISPONÍVEL NO CAIXA");
            var resumo = inventario.Consultar();
            Console.WriteLine("Estoque de cedulas/moedas:");
            foreach (var item in resumo)
            {
                Console.WriteLine($"Cedula de R${item.Cedula}| {item.Quantidade} unidade(s) | Total {item.ValorTotal.ToString("c2", cultura)}");
            }
            Console.WriteLine($"Valor Total disponivel:{inventario.ValorTotal.ToString("c2", cultura)}");
            continue;
        }

        if (opcao == "5")
        {
            if (!CadastrarConta()) break;
            continue;
        }

        if (opcao == "6")
        {
            Console.WriteLine($"Saldo da conta {conta.Id}: {conta.Saldo.ToString("C2", cultura)}");
            continue;
        }

        if (opcao == "7")
        {
            if (!TrocarEstrategia()) break;
            continue;
        }

        if (opcao is not ("1" or "2" or "4" ))
        {
            Console.WriteLine("Opção inválida. Digite uma opção de 0 a 7.");
            continue;
        }

        if(opcao == "4")
        {
            Console.WriteLine("Digite o valor que deseja sacar");
            string? valorSaque = Console.ReadLine()?.Trim();
            if (!decimal.TryParse(valorSaque?.Replace('.', ','), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, cultura, out var valorSolicitado))
                throw new FormatException("Valor de saque inválido.");
            inventario.Saque(conta, valorSolicitado);
            continue;
        }

        bool carregar = opcao == "1";
        Console.WriteLine(carregar ? "\nCARREGAR DINHEIRO" : "\nDESCARREGAR DINHEIRO");
        Console.WriteLine("Valores aceitos (R$): 1;");
        Console.WriteLine("1,2; 5; 10; 20; 50; 100; 200.");
        Console.WriteLine("Digite 0 para cancelar e voltar ao menu.");

        decimal valor;
        while (true)
        {
            Console.Write("Valor da cédula ou moeda (ex.: 50 ou 1): ");
            string? entrada = Console.ReadLine();
            if (entrada is null) return;

            if (!decimal.TryParse(entrada.Trim().Replace('.', ','),
                NumberStyles.AllowDecimalPoint, cultura, out valor))
            {
                Console.WriteLine("Valor inválido. Digite apenas o número, sem R$.");
                continue;
            }

            if (valor == 0) break;

            try
            {
                _ = new CedulaMoeda { Valor = valor };
                break;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Escolha um dos valores aceitos acima.");
            }
        }

        if (valor == 0) continue;

        long quantidade;
        while (true)
        {
            Console.Write("Quantidade de unidades (0 para cancelar): ");
            string? entrada = Console.ReadLine();
            if (entrada is null) return;

            if (long.TryParse(entrada, out quantidade) && quantidade >= 0) break;
            Console.WriteLine("Digite uma quantidade inteira e positiva.");
        }

        if (quantidade == 0) continue;

        try
        {
            if (carregar)
                inventario.Carregar(valor, quantidade);
            else
                inventario.Descarregar(valor, quantidade);

            Console.WriteLine($"\nOperação concluída: {quantidade} unidade(s) de {valor.ToString("c2", cultura)}.");
            Console.WriteLine($"Total {(carregar ? "adicionado" : "retirado")}: {(valor * quantidade).ToString("c2", cultura)}");
        }
        catch (InvalidOperationException erro)
        {
            Console.WriteLine($"\nNão foi possível concluir: {erro.Message}");
        }
        catch (OverflowException)
        {
            Console.WriteLine("\nQuantidade muito alta. Tente uma quantidade menor.");
        }
    }
    catch (EndOfStreamException) { break; }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FormatException or OverflowException)
    {
        Console.WriteLine($"Operação rejeitada: {ex.Message}");

    }
}

string NomeEstrategia(EstrategiaSaque estrategia)
{
    return estrategia == EstrategiaSaque.MenorQuantidade
        ? "Menor quantidade de notas"
        : "Preservar notas de maior valor";
}

bool TrocarEstrategia()
{
    Console.WriteLine($"Estratégia atual: {NomeEstrategia(inventario.Estrategia)}");
    Console.WriteLine("1 - Menor quantidade de notas");
    Console.WriteLine("2 - Preservar notas de maior valor");
    Console.WriteLine("0 - Cancelar");

    while (true)
    {
        Console.Write("Escolha a estratégia: ");
        string? escolha = Console.ReadLine()?.Trim();
        if (escolha is null) return false;
        if (escolha == "0") return true;

        if (escolha is not ("1" or "2"))
        {
            Console.WriteLine("Opção inválida. Digite 1, 2 ou 0.");
            continue;
        }

        var estrategia = escolha == "1"
            ? EstrategiaSaque.MenorQuantidade
            : EstrategiaSaque.PreservarMaioresValores;

        inventario.TrocarEstrategia(estrategia);
        Console.WriteLine($"Estratégia definida: {NomeEstrategia(inventario.Estrategia)}");
        return true;
    }
}

bool CadastrarConta()
{
    while (true)
    {
        Console.Write("ID da nova conta: ");
        string? id = Console.ReadLine()?.Trim();
        if (id is null) return false;
        if (string.IsNullOrWhiteSpace(id) || id == "0")
        {
            Console.WriteLine("Informe um ID não vazio e diferente de 0.");
            continue;
        }

        Console.Write("Saldo inicial (ex.: 100,50): ");
        string? entrada = Console.ReadLine()?.Trim();
        if (entrada is null) return false;
        if (!decimal.TryParse(entrada.Replace('.', ','), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, cultura, out var saldo))
        {
            Console.WriteLine("Saldo inicial inválido.");
            continue;
        }

        try
        {
            cadastroContas.Registrar(new Conta(id, saldo));
            Console.WriteLine($"Conta {id} cadastrada com saldo de {saldo.ToString("C2", cultura)}.");
            return true;
        }
        catch (Exception erro) when (erro is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine(erro.Message);
        }
    }
}
