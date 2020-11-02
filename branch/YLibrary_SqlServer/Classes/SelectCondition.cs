using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YLibrary
{
    public class SelectCondition
    {
        public String atributo { get; set; }
        public FlagPredicado predicado { get; set; }
        public Object valor { get; set; }

        public SelectCondition(String atributo, FlagPredicado flag, Object valor=null) 
        {
            this.atributo = atributo;
            this.predicado = flag;
            this.valor = valor;
        }
        public SelectCondition() { }
    }
}
