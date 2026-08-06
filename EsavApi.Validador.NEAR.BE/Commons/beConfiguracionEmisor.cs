namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beConfiguracionEmisor
    {
        public string Emis_IdEmisor { get; set; }
        public string Emis_RazonSocial { get; set; }
        public string Emis_Direccion { get; set; }
        public string Emis_Correo { get; set; }
        public string Emis_Telefono { get; set; }
        public int Rubr_IdRubro { get; set; }
        public string Emis_RutaPFX { get; set; }
        public string Emis_ClavePFX { get; set; }
        public string Emis_UsuarioSunat { get; set; }
        public string Emis_ClaveSunat { get; set; }
        public string Emis_OSEBalanceado { get; set; }
        public string Sucu_IdSucursal { get; set; }
        public string Sucu_CodigoLocalSunat { get; set; }
        public string Sucu_Nombre { get; set; }
        public string Sucu_Direccion { get; set; }
        public string Sucu_Ubigeo { get; set; }
        public string Sucu_Telefono { get; set; }
        public string Sucu_Correo { get; set; }
        public string Sucu_Web { get; set; }
        public string Anotacion { get; set; }
        public int CSuc_CantidadDecimal { get; set; }
        public int CEmi_CantidadDecimalDetalle { get; set; }
        public string CSuc_ColorCss { get; set; }
        public string CSuc_EstiloCss { get; set; }
        public string CSuc_FormatoNumericoDetalle { get; set; }
        public string CSuc_FormatoNumerico { get; set; }
        public string CSuc_NombreFuncionDll { get; set; }
        public string CSuc_NroResolucion { get; set; }
        public string CSuc_VistaPdf { get; set; }
        public string CEmi_PagoDetraccion { get; set; }
        public string CSuc_ComentarioLegal { get; set; }
        public string CSuc_CuentaCorriente { get; set; }
        public string CSuc_ComentarioLegalExportacion { get; set; }
        public decimal CSuc_PorcentajeIGV { get; set; }
        public string Form_Icono { get; set; }
        public string Dist_Descripcion { get; set; }
        public string Prov_Descripcion { get; set; }
        public string Depa_Descripcion { get; set; }
        public string CSuc_CuentaDetraccion { get; set; }
        public bool CSuc_Detra027OrigDestPdf { get; set; }
    }
}
