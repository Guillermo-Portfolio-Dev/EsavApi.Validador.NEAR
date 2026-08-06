using System.Collections.Generic;
using System.Linq;

namespace EsavApi.Validador.NEAR.UTIL
{
    public static class UTilidades
    {
        public static string LimpiarTexto(string texto)
        {
            return new string(texto
                .Where(c => !char.IsControl(c) && c >= 32 || char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                .ToArray()).Trim();
        }
        public static bool TipoOperacion(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0101", "0112", "0113", "0200", "0201", "0202","0203","0204","0205",
                         "0206", "0207", "0208", "0301", "0302", "0303","0401","1001","1002",
                         "1003", "1004","2106", "2001"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionNotaCredito(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "01","02","03","04","05","06"
                         ,"07","08","09","10","11","12","13"
                    };

            return validCodes.Contains(codigo);
        }

        public static string ObtenerMotivoEmisionND(string tipoNotaCredito)
        {
            switch (tipoNotaCredito)
            {
                case "01": return "Intereses por mora";
                case "02": return "Aumento en el valor";
                case "03": return "Penalidades/ otros conceptos";
                case "11": return "Ajuste de operaciones de exportación";
                case "12": return "Ajustes afectos al IVAP";
                default: return "";
            }
        }
        public static bool TipoOperacionNotaDebito(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "01","02","03","11","12"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionFacturaServicio(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0201", "0202","0205","0208"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionExportacion(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0200", "0201", "0202","0203","0204","0205",
                         "0206", "0207", "0208"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionExportacionFactura(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0200", "0201","0204"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoAfectacion(string codigo)
        {

            var validCodes = new HashSet<string>
                    {
                        "10", "11", "12", "13", "14", "15", "16", "17",
                        "20", "21", "30", "31", "32", "33", "34", "35", "36", "37", "40"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoAfectacionImporte0(string codigo)
        {

            var validCodes = new HashSet<string>
                    {
                        "10", "11", "12", "13", "14", "15", "16", "21", "31", "32", "33", "34", "35", "36", "37", "40"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool EsCodigoGratuito(string codigo)
        {
            var codigosGratuitos = new HashSet<string>
            {
                "11", "12", "13", "14", "15", "16", "17", "21", "31", "32", "33", "34", "35", "36","37"
            };

            return codigosGratuitos.Contains(codigo);
        }
        public static bool EsCodigoGratuitoGravado(string codigo)
        {
            var codigosGratuitos = new HashSet<string>
            {
                "11", "12", "13", "14", "15", "16"
            };

            return codigosGratuitos.Contains(codigo);
        }
        public static bool MotivoOtrosCargos(string codigo)
        {
            var codigosOC = new HashSet<string>
            {
                "45", "46", "49", "50", "51", "52","53"
            };

            return codigosOC.Contains(codigo);
        }
        public static bool EsGravadoGratuito(string codigo)
        {
            var codigosGratuitos = new HashSet<string>
            {
                "11", "12", "13", "14", "15", "16", "17"
            };

            return codigosGratuitos.Contains(codigo);
        }
        public static bool EsCodigoGravado(string codigo)
        {
            var codigosGravados = new HashSet<string>
            {
                "10", "11", "12", "13", "14", "15", "16", "17"

            };

            return codigosGravados.Contains(codigo);
        }
        public static bool EsGR_EXO_EXP_INA(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "10","11", "12", "13", "14", "15","16"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool Documentos5Dias(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "08","07", "03"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool Documento3Dias(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "01","07", "08"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool Exonerado(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "20"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool Inafecto(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "30","32","33","34","35","36","37"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool sinIGV(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "20","21","31","40"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoDocumentos(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "01", "03", "06", "07", "08", "09","20","31","40","41","99"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoDocumentoIdentidad(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0", "1", "4", "6", "7", "A","B","C","D","F"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoDocumentoIdentidadPaqueteTuristico(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0", "4", "6", "7", "A","B","C","D"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoDocumentoIdentidadExtra(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0", "1", "4","6", "7", "A","B","C","D","E","F","G"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionBoletaExportacion(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "0200", "0201", "0202","0203","0204","0205",
                         "0206", "0207","0401"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionDetraccion(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "1001", "1002", "1003","1004"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionDetraccionParaIscBoleta(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "1001", "1002", "1003","0101"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TipoOperacionDetraccionParaIscFactura(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "1001", "1002", "1003","0101"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool MotivoDescuento(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "00", "01", "02","03","04","05","06"
                    };

            return validCodes.Contains(codigo);
        }
        public static bool TasaICBPER(string valor)
        {
            var validCodes = new HashSet<string>
                    {
                         "0.50"
                    };

            return validCodes.Contains(valor);
        }
        public static string MedioPagoDetraccion(string codigo)
        {
            switch (codigo)
            {
                case "001": return "Depósito en cuenta";
                case "002": return "Giro";
                case "003": return "Transferencia de fondos";
                case "004": return "Orden de pago";
                case "005": return "Tarjeta de débito";
                case "006": return "Tarjeta de crédito emitida en el país por una empresa del sistema financiero";
                case "007": return "Cheques con la cláusula de 'no negociable', 'intransferibles', 'no a la orden' u otra equivalente, a que se refiere el inciso g) del artículo 5° de la ley";
                case "008": return "Efectivo, por operaciones en las que no existe obligación de utilizar medio de pago";
                case "009": return "Efectivo, en los demás casos";
                case "010": return "Medios de pago usados en comercio exterior";
                case "011": return "Documentos emitidos por las edpymes y las cooperativas de ahorro y crédito no autorizadas a captar depósitos del público";
                case "012": return "Tarjeta de crédito emitida en el país o en el exterior por una empresa no perteneciente al sistema financiero, cuyo objeto principal sea la emisión y administración de tarjetas de crédito";
                case "013": return "Tarjetas de crédito emitidas en el exterior por empresas bancarias o financieras no domiciliadas";
                case "101": return "Transferencias - comercio exterior";
                case "102": return "Cheques bancarios - comercio exterior";
                case "103": return "Orden de pago simple - comercio exterior";
                case "104": return "Orden de pago documentario - comercio exterior";
                case "105": return "Remesa simple - comercio exterior";
                case "106": return "Remesa documentaria - comercio exterior";
                case "107": return "Carta de crédito simple - comercio exterior";
                case "108": return "Carta de crédito documentario - comercio exterior";
                case "999": return "Otros medios de pago";
                default: return "Código desconocido";
            }
        }
        public static string codBbSsDetraccionText(string codigo)
        {
            switch (codigo)
            {
                case "001": return "Azúcar y melaza de caña";
                case "002": return "Arroz";
                case "003": return "Alcohol etílico";
                case "004": return "Recursos hidrobiológicos";
                case "005": return "Maíz amarillo duro";
                case "007": return "Caña de azúcar";
                case "008": return "Madera";
                case "009": return "Arena y piedra";
                case "010": return "Residuos, subproductos, desechos, recortes y desperdicios";
                case "011": return "Bienes gravados con el IGV, o renuncia a la exoneración";
                case "012": return "Intermediación laboral y tercerización";
                case "013": return "Animales vivos";
                case "014": return "Carnes y despojos comestibles";
                case "015": return "Abonos, cueros y pieles de origen animal";
                case "016": return "Aceite de pescado";
                case "017": return "Harina, polvo y 'pellets' de pescado, crustáceos, moluscos y demás invertebrados acuáticos";
                case "019": return "Arrendamiento de bienes muebles";
                case "020": return "Mantenimiento y reparación de bienes muebles";
                case "021": return "Movimiento de carga";
                case "022": return "Otros servicios empresariales";
                case "024": return "Comisión mercantil";
                case "025": return "Fabricación de bienes por encargo";
                case "026": return "Servicio de transporte de personas";
                case "027": return "Servicio de transporte de carga";
                case "028": return "Transporte de pasajeros";
                case "030": return "Contratos de construcción";
                case "031": return "Oro gravado con el IGV";
                case "034": return "Minerales metálicos no auríferos";
                case "035": return "Bienes exonerados del IGV";
                case "036": return "Oro y demás minerales metálicos exonerados del IGV";
                case "037": return "Demás servicios gravados con el IGV";
                case "039": return "Minerales no metálicos";
                case "040": return "Bien inmueble gravado con IGV";
                default: return "";
            }
        }
        public static string ObtenerDescripcionOperacion(string codigo)
        {
            switch (codigo)
            {
                case "0101": return "Venta interna";
                case "0103": return "Venta interna - Itinerante";
                case "0110": return "Venta Interna - Sustenta Traslado de Mercadería - Remitente";
                case "0111": return "Venta Interna - Sustenta Traslado de Mercadería - Transportista";
                case "0112": return "Venta Interna - Sustenta Gastos Deducibles Persona Natural";
                case "0120": return "Venta Interna - Sujeta al IVAP";
                case "0121": return "Venta Interna - Sujeta al FISE";
                case "0122": return "Venta Interna - Sujeta a otros impuestos";
                case "0130": return "Venta Interna - Realizadas al Estado";
                case "0200": return "Exportación de Bienes";
                case "0201": return "Exportación de Servicios – Prestación servicios realizados íntegramente en el país";
                case "0202": return "Exportación de Servicios – Prestación de servicios de hospedaje No Domiciliado";
                case "0203": return "Exportación de Servicios – Transporte de navieras";
                case "0204": return "Exportación de Servicios – Servicios a naves y aeronaves de bandera extranjera";
                case "0205": return "Exportación de Servicios - Servicios que conformen un Paquete Turístico";
                case "0206": return "Exportación de Servicios – Servicios complementarios al transporte de carga";
                case "0207": return "Exportación de Servicios – Suministro de energía eléctrica a favor de sujetos domiciliados en ZED";
                case "0208": return "Exportación de Servicios – Prestación servicios realizados parcialmente en el extranjero";
                case "0301": return "Operaciones con Carta de porte aéreo (emitidas en el ámbito nacional)";
                case "0302": return "Operaciones de Transporte ferroviario de pasajeros";
                case "0303": return "Operaciones de Pago de regalía petrolera";
                case "1001": return "Operación Sujeta a Detracción";
                case "1002": return "Operación Sujeta a Detracción- Recursos Hidrobiológicos";
                case "1003": return "Operación Sujeta a Detracción- Servicios de Transporte Pasajeros";
                case "1004": return "Operación Sujeta a Detracción- Servicios de Transporte de Carga";
                case "2001": return "Operación Sujeta a Percepción";
                default: return "Código desconocido";
            }
        }
        public static bool CodigoCargos(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "45","46","49","50","51","52","53"
                    };

            return validCodes.Contains(codigo);
        }
        public static string ObtenerTipoDocumentoText(string tipoDocumento)
        {
            switch (tipoDocumento)
            {
                case "0": return "DT. NO DOM. S-RUC";
                case "1": return "DNI";
                case "4": return "C. EXT.";
                case "6": return "RUC";
                case "7": return "PASAPORTE";
                case "A": return "CED. DIPL. ";
                case "B": return "DIPR.  NO DOM.";
                case "C": return "DT. TIN PP.NN.";
                case "D": return "DT. IN PP.JJ.";
                case "E": return "TAM";
                case "F": return "PTP";
                case "G": return "SALVOCONDUTO";
                default: return "";
            }
        }
        public static string ObtenerMotivoEmision(string tipoNotaCredito)
        {
            switch (tipoNotaCredito)
            {
                case "01": return "Anulación de la operación";
                case "02": return "Anulación por error en el RUC";
                case "03": return "Corrección por error en la descripción";
                case "04": return "Descuento global";
                case "05": return "Descuento por ítem";
                case "06": return "Devolución total";
                case "07": return "Devolución por ítem";
                case "08": return "Bonificación";
                case "09": return "Disminución en el valor";
                case "10": return "Otros conceptos";
                case "13": return "Corrección del monto neto pendiente de pago y/o la(s) fechas(s) de vencimiento del pago";
                default: return "";
            }
        }
        public static string MsgServicio(string codigo)
        {
            switch (codigo)
            {
                case "0": return "Baja";
                case "1": return "Activo";
                case "2": return "Bloque por deuda";
                case "3": return "Aviso de bloqueo por deuda";
                case "4": return "Suspension temporal";
                default: return "";
            }
        }
        public static bool CodigoMotivoTraslado(string codigo)
        {
            var validCodes = new HashSet<string>
                    {
                         "01","02","04","06","05","08","09","13","14","18","19"
                    };

            return validCodes.Contains(codigo);
        }
        public static string CodigoMotivoTrasladoText(string codigo)
        {
            switch (codigo)
            {
                case "01":
                    return "Venta";
                case "02":
                    return "Compra";
                case "04":
                    return "Traslado entre establecimientos de la misma empresa";
                case "05":
                    return "Consignación";
                case "06":
                    return "Devolución";
                case "08":
                    return "Importación";
                case "09":
                    return "Exportación";
                case "13":
                    return "Otros";
                case "14":
                    return "Venta sujeta a confirmación del comprador";
                case "18":
                    return "Traslado emisor itinerante CP";
                case "19":
                    return "Traslado a zona primaria";
                default:
                    return "";
            }
        }
        public static bool EsAlfanumerico(string valor, int longitudMaxima)
        {
            return !string.IsNullOrWhiteSpace(valor)
                && valor.Length <= longitudMaxima
                && valor.All(c => char.IsLetterOrDigit(c));
        }
        public static bool EsAlfanumericoConSlashYEspacio(string input, int maxLength)
        {
            if (input.Length > maxLength)
                return false;

            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c) && c != '/' && c != ' ')
                    return false;
            }

            return true;
        }
        #region LISTAREGIONBOLETA 0205 0202
        //listaPropiedad = (eCabecera[4] == "0205" || eCabecera[4] == "0202") && detalleExtraHPT.Length != 6 ? null : new List<beBoletaDeliveryDetalle_>
        //{
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4000",
        //        descripcionPropiedad = "Hospedajes: Código de país de emisión del pasaporte",
        //        valorPropiedad = detalleExtra.Split('|')[6],
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4001",
        //        descripcionPropiedad = "Hospedajes: Código de país de residencia del sujeto no domiciliado",
        //        valorPropiedad = null,
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4002",
        //        descripcionPropiedad = "Hospedajes: Fecha de ingreso al país",
        //        valorPropiedad = null,
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //         IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4003",
        //        descripcionPropiedad = "Hospedajes: Fecha de Ingreso al Establecimiento",
        //        valorPropiedad = null,
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4004",
        //        descripcionPropiedad = "Hospedajes: Fecha de Salida del Establecimiento",
        //        valorPropiedad = null,
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4005",
        //        descripcionPropiedad = "Hospedajes: Número de Días de Permanencia",
        //        valorPropiedad = null,
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4006",
        //        descripcionPropiedad = "Hospedajes: Fecha de Consumo",
        //        valorPropiedad = null,
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4007",
        //        descripcionPropiedad = "Hospedajes: Nombres y apellidos del huesped",
        //        valorPropiedad = detalleExtra.Split('|')[4],
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4008",
        //        descripcionPropiedad = "Hospedajes: Tipo de documento de identidad del huesped",
        //        valorPropiedad = detalleExtra.Split('|')[4],
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //    new beBoletaDeliveryDetalle_
        //    {
        //        Accion = 1,
        //        IdEmisor = oBoleta.eCabecera.rucEmisor,
        //        Serie = oBoleta.eCabecera.serie,
        //        Numero = oBoleta.eCabecera.numero.ToString(),
        //        IdDetalle = dex.ToString(),
        //        idPropiedad = "4009",
        //        descripcionPropiedad = "Hospedajes: Número de documento de identidad del huesped",
        //        valorPropiedad = detalleExtra.Split('|')[5],
        //        enXML = false,
        //        index = 1,
        //        enRepresentacionImpresa = false,
        //        Item = "1"
        //    },
        //},
        #endregion
    }
}
