using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace EsavApi.Validador.NEAR.BR.Commons
{
    public class brArchivo
    {
        public bool ObtenerEmpresa(string ruc, DateTime FEmision, string tipoDoc)
        {
            bool status = false;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFileNear", "empresas.json");
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "DataFileNear", "empresas.json"); filePath = Path.GetFullPath(filePath).Replace("\\bin", "");


            if (!File.Exists(filePath))
            {
                return status;
            }

            string jsonContent = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);

            if (jsonData != null && jsonData.ContainsKey("empresas"))
            {
                foreach (var empresa in jsonData["empresas"])
                {
                    var partes = empresa.Split('|');
                    if (partes.Length >= 5 && partes[0] == ruc && partes[2] == tipoDoc)
                    {
                        if (DateTime.TryParseExact(partes[4], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaLimite))
                        {
                            status = FEmision.Date >= fechaLimite && ruc == partes[0].Trim() ? true : false;
                            return status;
                        }
                    }
                }
            }

            return status;
        }
        public bool ObtenerEmpresaSinIndicador(string ruc)
        {
            bool status = false;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFileNear", "sin_indicador.json");
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "DataFileNear", "sin_indicador.json");
            //filePath = Path.GetFullPath(filePath).Replace("\\bin", "");


            if (!File.Exists(filePath))
            {
                return status;
            }

            string jsonContent = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);

            if (jsonData != null && jsonData.ContainsKey("empresas"))
            {
                foreach (var empresa in jsonData["empresas"])
                {
                    var partes = empresa.Split('|');
                    if (partes[0] == ruc)
                    {
                        status = true;
                    }
                }
            }

            return status;
        }
    }
}
