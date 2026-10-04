using System.Globalization;
using System.Text;
using Teste_tecnico_Sistema_de_caixa_eletronico.Models;
using Teste_tecnico_Sistema_de_caixa_eletronico.Services;

Console.OutputEncoding = Encoding.UTF8;
var cultura = CultureInfo.GetCultureInfo("pt-BR");
var inventario = new Inventario();
Console.WriteLine("Caixa eletrônico — gestão de inventário");

while (true)
{
    try
    {
        Console.WriteLine("\n1 - Carregar" +
                          "\n2 - Descarregar" +
                          "\n3 - Consultar estoque" +
                          "\n4 - Saque" +
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

        if (opcao is not ("1" or "2" or "4" ))
        {
            Console.WriteLine("Opção inválida. Digite 1, 2, 3 ou 0.");
            continue;
        }

        if(opcao == "4")
        {
            Console.WriteLine("Digite o valor que deseja sacar");
            string? valorSaque = Console.ReadLine()?.Trim();
            inventario.Saque(Convert.ToDecimal(valorSaque));
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
                NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out valor))
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
