using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Factory;
using Util;
using Classes;
using System.Data;
using System.Reflection;

namespace Util
{
    public class Auxiliar
    {
        private FPersist fabrica;
        private YConfiguracao configuracao;
        private DAO dao;
        private Xml xml;

        public Auxiliar(FPersist fabrica, YConfiguracao configuracao, DAO dao) 
        {
            this.fabrica = fabrica;
            this.configuracao = configuracao;
            this.dao = dao;
            this.xml = new Xml();
        }

        /// <summary>
        /// Verifica os tipos dos campos no Banco.
        /// </summary>
        /// <returns></returns>
        public bool VerificaTipo(String tipo)
        {
            switch (tipo)
            {
                case "STRING":
                case "DATETIME":
                case "INT32":
                case "INT64":
                case "INT16":
                case "FLOAT":
                case "DOUBLE":
                case "DECIMAL":
                case "CHAR":
                case "DATE":
                case "TIMESPAN":
                case "BOOL":
                case "BOOLEAN":
                case "SINGLE":
                case "BYTE":
                case "NULL":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Verifica o tipo de campo para retornar sua criação no banco de dados.
        /// </summary>
        /// <returns></returns>
        public String DbTipo(String campo)
        {
            switch (campo)
            {
                case "STRING":
                    return "varchar(255) NULL, ";
                case "DATETIME":
                case "TIMESPAN":
                    return "datetime NULL, ";
                case "INT32":
                    return "int NULL, ";
                case "INT64":
                    return "bigint NULL, ";
                case "INT16":
                    return "smallint NULL, ";
                case "FLOAT":
                case "DOUBLE":
                    return "float NULL, ";
                case "DECIMAL":
                    return "money NULL, ";
                case "CHAR":
                    return "varchar(1) NULL, ";
                case "DATE":
                    return "date NULL, ";
                case "BOOL":
                case "BOOLEAN":
                    return "bit NULL, ";
                case "SINGLE":
                    return "real NULL, ";
                case "BYTE":
                    return "tinyint NULL, ";
            }

            return "int NULL, ";
        }

        private DbType RetornaTipo(String campo)
        {
            switch (campo)
            {
                case "STRING":
                    return System.Data.DbType.String;
                case "DATETIME":
                case "TIMESPAN":
                    return System.Data.DbType.DateTime;
                case "INT32":
                    return System.Data.DbType.Int32;
                case "INT64":
                    return System.Data.DbType.Int64;
                case "INT16":
                    return System.Data.DbType.Int16;
                case "FLOAT":
                case "DOUBLE":
                    return System.Data.DbType.Double;
                case "DECIMAL":
                    return System.Data.DbType.Decimal;
                case "CHAR":
                    return System.Data.DbType.String;
                case "DATE":
                    return System.Data.DbType.Date;
                case "BOOL":
                case "BOOLEAN":
                    return System.Data.DbType.Boolean;
                case "SINGLE":
                    return System.Data.DbType.Single;
                case "BYTE":
                    return System.Data.DbType.Byte;
            }
            return System.Data.DbType.Guid;
        }

        /// <summary>
        /// Criado devido a necessidade de criar um nullable de um determinado valor
        /// </summary>
        /// <returns>object</returns>
        public object CriarNullable(String nome, object valor)
        {
            try
            {
                switch (nome)
                {
                    case "STRING":
                        return valor.ToString();
                    case "DATETIME":
                    case "DATE":
                    case "TIMESPAN":
                        return new Nullable<DateTime>(Convert.ToDateTime(valor));
                    case "INT32":
                        return new Nullable<Int32>(Convert.ToInt32(valor));
                    case "INT64":
                        return new Nullable<Int64>(Convert.ToInt64(valor));
                    case "INT16":
                        return new Nullable<Int16>(Convert.ToInt16(valor));
                    case "FLOAT":
                    case "DOUBLE":
                        return new Nullable<Double>(Convert.ToDouble(valor));
                    case "DECIMAL":
                        return new Nullable<Decimal>(Convert.ToDecimal(valor));
                    case "CHAR":
                        return new Nullable<Char>(Convert.ToChar(valor));
                    case "BOOL":
                    case "BOOLEAN":
                        return new Nullable<Boolean>(Convert.ToBoolean(valor));
                    case "SINGLE":
                        return new Nullable<Single>(Convert.ToSingle(valor));
                    case "BYTE":
                        return new Nullable<Byte>(Convert.ToByte(valor));
                }
            }
            catch { }
            return null;
        }

        public String CriarTabelaCondicoes(String o)
        {
            String tabela = o.ToUpper().Substring(o.ToUpper().IndexOf("FROM") + 4, o.Length - (o.ToUpper().IndexOf("FROM") + 4));
            return tabela;
        }

        /// <summary>
        /// Verifica se a tabela do banco está faltando algum campo
        /// da entidade.
        /// </summary>
        public void CheckTable(String tabela, List<Persist> param, Object o, String metodos)
        {
            String sql = @"select column_name from information_schema.columns where 0=0 and table_name='{0}' or ";
            sql = String.Format(sql, tabela);

            foreach (Persist pst in param)
            {
                sql += "column_name = '{0}' or ";
                sql = string.Format(sql, pst.DscNome);
            }
            sql = sql.Substring(0, sql.Length - 3);

            DataTable dt = dao.ExecuteReader(sql);

            String[] lstm = metodos.ToLower().Split(',');
            metodos = "";
            foreach (DataRow dr in dt.Rows)
                metodos += dr[0].ToString() + ",";
            metodos = metodos.Substring(0, metodos.Length - 1);
            String[] lstr = metodos.ToLower().Split(',');

            IEnumerable<String> diferenca = lstm.Except(lstr);

            List<Persist> lsts = new List<Persist>();
            sql = "";

            foreach (string s in diferenca)
                lsts.Add(param.Find(x => x.DscNome.ToLower() == s.ToLower()));
            if (lsts.Count > 0)
                AlterTable(tabela, lsts, o);
        }

        /// <summary>
        /// Insere uma coluna no banco referente ao metodo da entidade.
        /// </summary>
        public void AlterTable(String tabela, List<Persist> param, Object o)
        {
            if (!(bool)configuracao.ALTERA_TABELA)
                throw new System.ArgumentException("A tabela " + tabela + " referente a entidade " + o.GetType().ToString() + " possui colunas diferentes", "YLibrary");

            string alter = @"ALTER TABLE {0} ADD ";
            alter = String.Format(alter, tabela);
            foreach (Persist s in param)
            {
                if (VerificaTipo(s.Type.Name.ToUpper()))
                {
                    alter += " {0} " + dao.DbTipo(s.Type.Name.ToString().ToUpper(), configuracao);
                    alter = String.Format(alter, s.DscNome);
                }
                else
                {
                    if (s.ObjValor == null)
                        o = Activator.CreateInstance(s.Type);
                    else
                        o = s.ObjValor;
                    List<Persist> obj = fabrica.CriarPersist(o);
                    tabela = CriarNomeTabela(o);
                    alter += s.DscNome.ToLower() + " int FOREIGN KEY REFERENCES " + tabela + "(" + s.DscNome.ToLower() + "), ";
                    VerificaTabelaBanco(obj, tabela, o);
                }
            }
            alter = alter.Substring(0, alter.Length - 2);
            dao.ExecuteNonQuery(alter);
        }

        /// <summary>
        /// Cria a tabela do banco referente a Entidade.
        /// </summary>
        public void CreateTable(String tabela, List<Persist> param, Object o)
        {
            if (!(bool)configuracao.ALTERA_TABELA)
                throw new System.ArgumentException("A tabela " + tabela + " referente a entidade " + o.GetType().ToString() + " não existe no Banco", "YLibrary");

            string sequence = "create sequence {0}";
            string insert = @"CREATE TABLE {0} ( ";
            insert = String.Format(insert, tabela);
            List<Persist> lst = fabrica.CriarPersistOriginal(o);
            for (int i = 0; i < param.Count; i++)
            {
                if (VerificaTipo(lst[i].Type.Name.ToUpper()))
                {
                    if (param[i].FlgFlag != null)
                    {
                        if (param[i].FlgFlag.Contains("KEY"))
                        {
                            sequence = String.Format(sequence, tabela + "_seq");
                            insert += param[0].DscNome.ToLower() + " integer PRIMARY KEY not null unique default nextval('" + tabela + "_seq" + "'), ";
                        }
                        else
                        {
                            insert += " {0} " + dao.DbTipo(param[i].Type.Name.ToString().ToUpper(), configuracao).Replace(configuracao.TAMANHO_PADRAO_STRING, param[i].FlgFlag);
                            insert = String.Format(insert, param[i].DscNome.ToLower());
                        }
                    }
                    else
                    {
                        insert += " {0} " + dao.DbTipo(param[i].Type.Name.ToString().ToUpper(), configuracao);
                        insert = String.Format(insert, param[i].DscNome.ToLower());
                    }
                }
                else
                {
                    if (lst[i].ObjValor == null)
                        o = Activator.CreateInstance(lst[i].Type);
                    else
                        o = lst[i].ObjValor;
                    List<Persist> lstInterno = fabrica.CriarPersist(o);
                    tabela = CriarNomeTabela(o);
                    insert += lstInterno[0].DscNome.ToLower() + " int references " + tabela + "(" + lstInterno[0].DscNome.ToLower() + "), ";
                    VerificaTabelaBanco(lstInterno, tabela, o);
                }
            }
            insert = insert.Substring(0, insert.Length - 2);
            insert += ")";
            dao.ExecuteNonQuery(sequence);
            dao.ExecuteNonQuery(insert);
        }

        /// <summary>
        /// Verifica se os campos da Entidade estão diferentes do xml de configuração.
        /// Caso esteja ele verifica se a tabela referente a entidade exista.
        /// </summary>
        public void VerificaTabelaBanco(List<Persist> lst, String tabela, Object o)
        {
            DAO.Tabela = tabela;
            String ConfigOld = String.Empty;
            String Config = MetodosDll(tabela, o);

            if (!(bool)configuracao.SEMPRE_VERIFICAR_BANCO)
                ConfigOld = xml.LerXml(tabela);

            if (Config.Equals(ConfigOld))
                return;
            else
            {
                String sql = @"SELECT tablename FROM pg_catalog.pg_tables
                                WHERE schemaname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
                                and tablename = '" + tabela.Trim() + "'";
                DataTable dt = dao.ExecuteReader(sql);
                if (dt.Rows.Count <= 0)
                    CreateTable(tabela, lst, o);
                else
                    CheckTable(tabela, lst, o, Config);
                xml.InserirXml(tabela, Config);
            }
        }

        /// <summary>
        /// Verifica os metodos da classe referente a tabela passada por parametro
        /// dentro do assembly Entidade.dll.
        /// </summary>
        /// <returns>String com os metodos separados por virgulas</returns>
        public String MetodosDll(String tabela, Object o)
        {
            String nome = "";

            List<Assembly> lstentidade = AppDomain.CurrentDomain.GetAssemblies().ToList<Assembly>();
            Assembly a = lstentidade.Find(item => item.GetName().Name.ToUpper().Contains(configuracao.CLASS_LIBRARY_ENTIDADE));

            if (a == null)
                throw new System.ArgumentException("Não foi indentificado uma Class Library com o nome de " + configuracao.CLASS_LIBRARY_ENTIDADE, "YLibrary");

            List<Type> lstttype = a.GetExportedTypes().ToList<Type>();
            Type facade = (lstttype.Where<Type>(item => item.Name.Equals(o.GetType().Name))).First<Type>();

            List<MethodInfo> infom = facade.GetMethods().ToList<MethodInfo>();
            infom = infom.FindAll(car => car.Name.Substring(0, 4).Equals("get_"));

            foreach (MethodInfo name in infom)
            {
                if (name.ReturnType.Assembly.Equals(a))
                    nome += VerificaPK(Activator.CreateInstance(name.ReturnType)) + ",";
                else
                    nome += name.Name.Remove(0, 4) + ",";
            }
            return nome.Substring(0, nome.Length - 1);
        }


        private String VerificaPK(Object o)
        {
            List<Persist> lst = new List<Persist>();
            Persist obj = new Persist();
            if (o != null)
            {
                Dictionary<string, object> propertyValues = new Dictionary<string, object>();
                Type ObjectType = o.GetType();
                System.Reflection.PropertyInfo[] properties = fabrica.Ordenar(ObjectType.GetProperties());

                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    if (!(property.PropertyType.IsClass && !property.PropertyType.Name.ToUpper().Equals("STRING")))
                    {
                        if (fabrica.VerificarAnnotation(obj, property, ObjectType).ToUpper().Contains("KEY"))
                            return property.Name.ToString();
                    }
                }
            }
            throw new System.ArgumentException("Não foi indentificado uma chave primária na classe " + o.GetType().Name, "YLibrary");
        }

        public String CriarNomeTabela(Object o)
        {
            Type ObjectType = o.GetType();
            String[] vet = ObjectType.ToString().Split('.');
            return configuracao.ALIAS_TABELA.ToLower() + vet[vet.Length - 1].ToLower();
        }
    }
}
