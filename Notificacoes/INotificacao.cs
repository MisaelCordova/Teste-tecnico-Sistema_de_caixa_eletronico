using System;
using System.Collections.Generic;
using System.Text;

namespace Teste_tecnico_Sistema_de_caixa_eletronico.Notificacoes
{
    public interface INotificacao
    {
        void Registrar(string mensagem);
    }
}
