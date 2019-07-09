using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Models.MongoDB
{
    public class ARCEmpleado
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int Empleado { get; set; }
        public string Nombre { get; set; }
        public int Ano { get; set; }

        public List<ARCEmpleado_MontosMes> MontosMensuales { get; set; }

        public DateTime Ingreso { get; set; }
        public DateTime UltAct { get; set; }
        public string Usuario { get; set; }

        public int Cia { get; set; }


        public ARCEmpleado()
        {
            this.MontosMensuales = new List<ARCEmpleado_MontosMes>(); 
        }
    }

    public class ARCEmpleado_MontosMes
    {
        public int Mes { get; set; }
        public decimal Remuneracion { get; set; }
        public decimal Sso { get; set; }
        public decimal Islr { get; set; }
    }
}