using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YLibrary
{
    public class Entidade
    {
        private Facade.SqlServer conn = new Facade.SqlServer();

        public virtual List<Object> Buscar()
        {
            return conn.Select(this, FlagPredicado.And);
        }
        
        public virtual List<Object> Buscar(FlagPredicado condicao)
        {
            return conn.Select(this, condicao);
        }

        public virtual List<Object> Buscar(params SelectCondition[] condicoes)
        {
            return conn.Select(this, condicoes);
        }

        public virtual Int32 Inserir()
        {
            return conn.Insert(this);
        }

        public virtual void Atualizar()
        {
            conn.Update(this);
        }

        public virtual void Excluir()
        {
            conn.Delete(this);
        }
    }
}
