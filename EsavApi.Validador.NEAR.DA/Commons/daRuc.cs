using EsavApi.Validador.NEAR.BE.Commons;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EsavApi.Validador.NEAR.DA.Commons
{
    public class daRuc
    {
        public beRUC fObtener(SqlConnection oConnection, String RUC, String IdEmisor)
        {

            beRUC _beRUC = new beRUC();
            beDireccion _beDireccion;
            List<beDireccion> lbeDireccion = new List<beDireccion>();


            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_Ruc_Obtener", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@ruc", SqlDbType.VarChar).Value = RUC;
                oCommand.Parameters.Add("@IdEmisor", SqlDbType.VarChar).Value = IdEmisor;
                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            _beRUC = new beRUC();
                            _beRUC.RUC = odr.IsDBNull(odr.GetOrdinal("RUC")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("RUC")));
                            _beRUC.RazonSocial = odr.IsDBNull(odr.GetOrdinal("RazonSocial")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("RazonSocial")));
                            _beRUC.Correo = odr.IsDBNull(odr.GetOrdinal("Correo")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Correo")));
                            _beRUC.Estado = odr.IsDBNull(odr.GetOrdinal("Estado")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Estado")));
                            _beRUC.CondicionDomicilio = odr.IsDBNull(odr.GetOrdinal("CondicionDomicilio")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("CondicionDomicilio")));
                            _beRUC.AgenteRetencion = odr.IsDBNull(odr.GetOrdinal("RETENCION")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("RETENCION")));
                            _beRUC.AgentePercepcion = odr.IsDBNull(odr.GetOrdinal("PERCEPCION")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("PERCEPCION")));
                            _beRUC.AgentePercepcionVI = odr.IsDBNull(odr.GetOrdinal("PERCEPCIONVI")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("PERCEPCIONVI")));
                            _beRUC.BuenContribuyente = odr.IsDBNull(odr.GetOrdinal("BUENCONTRIBUYENTE")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("BUENCONTRIBUYENTE")));
                        }
                        odr.NextResult();

                        while (odr.Read())
                        {
                            _beDireccion = new beDireccion();
                            _beDireccion.Ubigeo = odr.IsDBNull(odr.GetOrdinal("Ubigeo")) ? "" : odr.GetString(odr.GetOrdinal("Ubigeo"));
                            _beDireccion.Distrito = odr.IsDBNull(odr.GetOrdinal("Distrito")) ? "" : odr.GetString(odr.GetOrdinal("Distrito"));
                            _beDireccion.Provincia = odr.IsDBNull(odr.GetOrdinal("Provincia")) ? "" : odr.GetString(odr.GetOrdinal("Provincia"));
                            _beDireccion.Departamento = odr.IsDBNull(odr.GetOrdinal("Departamento")) ? "" : odr.GetString(odr.GetOrdinal("Departamento"));
                            _beDireccion.Direccion = odr.IsDBNull(odr.GetOrdinal("Direccion")) ? "" : odr.GetString(odr.GetOrdinal("Direccion"));
                            _beDireccion.TipoDireccion = odr.IsDBNull(odr.GetOrdinal("TipoDireccion")) ? "" : odr.GetString(odr.GetOrdinal("TipoDireccion"));
                            lbeDireccion.Add(_beDireccion);
                        }
                        _beRUC.Direcciones = lbeDireccion;
                    }
                }
            }
            return _beRUC;
        }
    }
}
