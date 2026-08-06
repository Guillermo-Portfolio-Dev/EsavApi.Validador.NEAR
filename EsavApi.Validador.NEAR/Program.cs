using System;

namespace EsavApi.Validador.NEAR
{
    class Program
    {
        static void Main(string[] args)
        {
            Validartor2 validador = new Validartor2();
            Console.Title = $"API - VALIDADOR - NEAR";
            validador.Inicio();
            Console.Read();
        }
    }
}
