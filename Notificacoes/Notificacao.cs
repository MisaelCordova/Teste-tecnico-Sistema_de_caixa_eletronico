using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Notificacoes
{
    public class Notificacao : INotificacao
    {
        private readonly string _caminhoArquivo;

        public Notificacao(string caminhoArquivo)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(caminhoArquivo);
            _caminhoArquivo = Path.GetFullPath(caminhoArquivo);
        }

        public void Registrar(string mensagem)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(mensagem);
            Directory.CreateDirectory(Path.GetDirectoryName(_caminhoArquivo)!);

            string dataHora  = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss ", 
                CultureInfo.InvariantCulture);
            string linha = $"[{dataHora}] {mensagem.ReplaceLineEndings(" ")}";

            File.AppendAllText(_caminhoArquivo, linha + Environment.NewLine, Encoding.UTF8);

        }
    }
}
