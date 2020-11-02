using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Reflection;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using Classes;
using YLibrary;

namespace YLibrary
{
    public class SqlServer : Facade.YLibrary
    {
        public SqlServer()
        {
        }

        #region Metodos Internos

        private String GerarScriptInsert(String[] colunas, String tabela, String pk)
        {
            String script = string.Empty;
            String script2 = string.Empty;
            String script3 = string.Empty;
            script = String.Format("Insert into {0} (", tabela);
            script2 = "values(";
            script3 = String.Format(" Output Inserted.{0} ", pk);
            for (int i = 0; i < colunas.Length; i++)
            {
                script += colunas[i] + ",";
                script2 += "@" + i + ",";
            }
            script = script.Remove(script.Length - 1, 1) + ") ";
            script2 = script2.Remove(script2.Length - 1, 1) + ") ";
            script += script3 + script2;
            return script;
        }

        private String GerarScriptUpdate(String[] colunas, String tabela)
        {
            String script = string.Empty;
            script = String.Format("update {0} set ", tabela);
            int count = 0;
            for (int i = 1; i < colunas.Length; i++)
            {
                script += colunas[i] + "= @" + i + ",";
                count = i;
            }
            script = script.Remove(script.Length - 1, 1) + " ";
            script += "where " + colunas[0] + "= @0";
            return script;
        }

        private String GerarScriptDelete(String[] colunas, String tabela)
        {
            String script = string.Empty;
            String script2 = string.Empty;
            script = String.Format("delete from {0} where ", tabela);
            script += colunas[0] + "=@0";
            return script;
        }

        private String GerarScriptSelect(String[] colunas, String tabela, List<Persist> obj, FlagPredicado where)
        {
            Boolean ini = true;
            String script = string.Empty;
            script = "Select ";
            for (int i = 0; i < colunas.Length; i++)
                script += colunas[i] + ",";
            script = script.Remove(script.Length - 1, 1) + " ";
            script += String.Format(" from {0} ", tabela);
            script += "where 0=0";
            //if (where != "")
            //{
            for (int i = 0; i < obj.Count; i++)
            {
                if (obj[i].ObjValor != null)
                {
                    if (where == FlagPredicado.And)
                        script += " " + (ini ? "and" : "and") + " " + colunas[i] + " = @" + i;

                    if (where == FlagPredicado.Or)
                        script += " " + (ini ? "and" : "or") + " " + colunas[i] + " = @" + i;
                    ini = false;
                }
            }
            //}
            return script;
        }

        private String GerarScriptSelect(String[] colunas, String tabela, List<Persist> obj, SelectCondition[] where)
        {
            String script = string.Empty;
            SelectCondition condicao;
            String colarray = null;
            DateTime date;
            int num;
            double num2;
            colunas = colunas.Select(s => s.ToUpper()).ToArray();

            script = "Select ";
            for (int i = 0; i < colunas.Length; i++)
                script += colunas[i] + ",";
            script = script.Remove(script.Length - 1, 1) + " ";
            script += String.Format(" from {0} ", tabela);
            script += "where 0=0";
            if (where.Length > 0)
            {
                for (int i = 0; i < where.Length; i++)
                {
                    colarray = Array.Find(colunas, item => item.ToUpper().Equals(where[i].atributo.ToUpper()));
                    if (colarray == null)
                        throw new System.ArgumentException("Não foi localizado o atributo " + where[i].atributo + " referente a tabela " + tabela, "YLibrary");

                    condicao = where[i];
                    switch (condicao.predicado)
                    {
                        case FlagPredicado.And:
                            script = script.Replace(("or " + condicao.atributo), "and " + condicao.atributo);
                            break;
                        case FlagPredicado.Or:
                            script = script.Replace(("and " + condicao.atributo), "or " + condicao.atributo);
                            break;
                        case FlagPredicado.Maior:
                            if (int.TryParse(condicao.valor.ToString(), out num) || double.TryParse(condicao.valor.ToString(), out num2))
                                script += " " + "and" + " " + condicao.atributo + " > " + (condicao.valor == null ? "@" + i : condicao.valor);
                            if (DateTime.TryParse(condicao.valor.ToString(), out date))
                                script += " " + "and" + " " + condicao.atributo + " < " + (condicao.valor == null ? "@" + i : "'" + condicao.valor + "'");
                            break;
                        case FlagPredicado.MaiorIgual:
                            if (int.TryParse(condicao.valor.ToString(), out num) || double.TryParse(condicao.valor.ToString(), out num2))
                                script += " " + "and" + " " + condicao.atributo + " >= " + (condicao.valor == null ? "@" + i : condicao.valor);
                            if (DateTime.TryParse(condicao.valor.ToString(), out date))
                                script += " " + "and" + " " + condicao.atributo + " < " + (condicao.valor == null ? "@" + i : "'" + condicao.valor + "'");
                            break;
                        case FlagPredicado.Menor:
                            if (int.TryParse(condicao.valor.ToString(), out num) || double.TryParse(condicao.valor.ToString(), out num2))
                                script += " " + "and" + " " + condicao.atributo + " < " + (condicao.valor == null ? "@" + i : condicao.valor);
                            if (DateTime.TryParse(condicao.valor.ToString(), out date))
                                script += " " + "and" + " " + condicao.atributo + " < " + (condicao.valor == null ? "@" + i : "'" + condicao.valor + "'");
                            break;

                        case FlagPredicado.MenorIgual:
                            if (int.TryParse(condicao.valor.ToString(), out num) || double.TryParse(condicao.valor.ToString(), out num2))
                                script += " " + "and" + " " + condicao.atributo + " <= " + (condicao.valor == null ? "@" + i : condicao.valor);
                            if (DateTime.TryParse(condicao.valor.ToString(), out date))
                                script += " " + "and" + " " + condicao.atributo + " < " + (condicao.valor == null ? "@" + i : "'" + condicao.valor + "'");
                            break;
                        case FlagPredicado.Igual:
                            if (int.TryParse(condicao.valor.ToString(), out num) || double.TryParse(condicao.valor.ToString(), out num2))
                                script += " " + "and" + " " + condicao.atributo + " = " + (condicao.valor == null ? "@" + i : condicao.valor);
                            if (DateTime.TryParse(condicao.valor.ToString(), out date))
                                script += " " + "and" + " " + condicao.atributo + " < " + (condicao.valor == null ? "@" + i : "'" + condicao.valor + "'");
                            break;
                        case FlagPredicado.NotIn:
                            script += " " + "and" + " " + condicao.atributo + " not in ('" + (condicao.valor == null ? "@" + i : condicao.valor) + "')";
                            break;
                        case FlagPredicado.Like:
                            script += " " + "and" + " " + condicao.atributo + " like '%" + (condicao.valor == null ? "@" + i : condicao.valor) + "%'";
                            break;
                    }
                }
            }
            return script;
        }

        private List<Object> ConvertList(DataTable table, List<Persist> lst, Object entidade)
        {
            Type ObjectType = entidade.GetType();
            int count;
            Object ent;
            object subentidade;
            Type ObjectType2;
            PropertyInfo numberPropertyInfo;
            List<Object> list = new List<Object>();
            Dictionary<string, object> propertyValues;
            List<Object> lobj;
            Object valor;
            foreach (DataRow col in table.Rows)
            {
                ent = new Object();
                ent = Activator.CreateInstance(entidade.GetType());
                count = 0;
                foreach (Object row in col.ItemArray)
                {
                    if (!auxiliar.VerificaTipo(lst[count].Type.Name.ToUpper()))
                    {
                        propertyValues = new Dictionary<string, object>();
                        System.Reflection.PropertyInfo[] properties = fabrica.Ordenar(ObjectType.GetProperties());
                        foreach (System.Reflection.PropertyInfo property in properties)
                        {
                            if (property.PropertyType == lst[count].Type)
                            {
                                ObjectType2 = property.PropertyType;
                                System.Reflection.PropertyInfo[] properties2 = fabrica.Ordenar(ObjectType2.GetProperties());
                                subentidade = Activator.CreateInstance(ObjectType2);
                                numberPropertyInfo = ObjectType2.GetProperty(properties2[0].Name);
                                valor = auxiliar.CriarNullable(numberPropertyInfo.PropertyType.GetGenericArguments()[0].Name.ToUpper(), table.Rows[0][count]);
                                numberPropertyInfo.SetValue(subentidade, valor, null);
                                if ((bool)configuracao.ENTIDADE_COMPLETA)
                                {
                                    if (entidade.GetType().Equals(property.PropertyType) || valor == null)
                                    {
                                        numberPropertyInfo = ObjectType.GetProperty(lst[count].DscNome);
                                        numberPropertyInfo.SetValue(ent, subentidade, null);
                                    }
                                    else
                                    {
                                        lobj = this.Select(subentidade);
                                        numberPropertyInfo = ObjectType.GetProperty(property.Name.ToString());
                                        numberPropertyInfo.SetValue(ent, lobj.Count > 0 ? lobj[0] : null, null);
                                    }
                                }
                                else
                                {
                                    numberPropertyInfo = ObjectType.GetProperty(property.Name.ToString());
                                    numberPropertyInfo.SetValue(ent, subentidade, null);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        numberPropertyInfo = ObjectType.GetProperty(lst[count].DscNome);
                        numberPropertyInfo.SetValue(ent, auxiliar.CriarNullable((lst[count].Type.Name.ToUpper()), row), null);

                    }
                    count++;
                }
                list.Add(ent);
            }
            return list;
        }

        private List<Object[]> ConvertList(DataTable table, List<Persist> lst)
        {
            List<Object[]> lista = new List<Object[]>();
            Object[] obj;
            foreach (DataRow row in table.Rows)
            {
                obj = new Object[lst.Count];
                for (int i = 0; i < lst.Count; i++)
                {
                    obj[i] = row[i];
                }
                lista.Add(obj);
            }
            return lista;
        }

        private void CreateTable(String tabela, List<Persist> param, Object o)
        {
            if (!(bool)configuracao.ALTERA_TABELA)
                throw new System.ArgumentException("A tabela " + tabela + " referente a entidade " + o.GetType().ToString() + " não existe no Banco", "YLibrary");

            string insert = @"CREATE TABLE {0} ( ";
            insert = String.Format(insert, tabela);
            List<Persist> lst = fabrica.CriarPersistOriginal(o);
            for (int i = 0; i < param.Count; i++)
            {
                if (auxiliar.VerificaTipo(lst[i].Type.Name.ToUpper()))
                {
                    if (param[i].FlgFlag != null)
                    {
                        if (param[i].FlgFlag.Contains("KEY"))
                        {
                            insert += param[0].DscNome.ToLower() + " int IDENTITY(1,1) NOT NULL PRIMARY KEY, ";
                        }
                        else
                        {
                            insert += " {0} " + auxiliar.DbTipo(param[i].Type.Name.ToString().ToUpper()).Replace("255", param[i].FlgFlag);
                            insert = String.Format(insert, param[i].DscNome.ToLower());
                        }
                    }
                    else
                    {
                        insert += " {0} " + auxiliar.DbTipo(param[i].Type.Name.ToString().ToUpper());
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
                    tabela = auxiliar.CriarNomeTabela(o);
                    insert += lstInterno[0].DscNome.ToLower() + " int FOREIGN KEY REFERENCES " + tabela + "(" + lstInterno[0].DscNome.ToLower() + "), ";
                    VerificaTabelaBanco(lstInterno, tabela, o);
                }
            }
            insert = insert.Substring(0, insert.Length - 2);
            insert += ")";
            dao.ExecuteNonQuery(insert);
        }

        private void VerificaTabelaBanco(List<Persist> lst, String tabela, Object o)
        {
            String ConfigOld = String.Empty;
            String Config = auxiliar.MetodosDll(tabela, o);

            if (!(bool)configuracao.SEMPRE_VERIFICAR_BANCO)
                ConfigOld = xml.LerXml(tabela);

            if (Config.Equals(ConfigOld))
                return;
            else
            {
                String sql = "SELECT NAME FROM SYSOBJECTS WHERE TYPE = 'U' AND NAME = '" + tabela.Trim() + "'";
                DataTable dt = dao.ExecuteReader(sql);
                if (dt.Rows.Count <= 0)
                    CreateTable(tabela, lst, o);
                else
                    CheckTable(tabela, lst, o, Config);
                xml.InserirXml(tabela, Config);
            }
        }

        private void CheckTable(String tabela, List<Persist> param, Object o, String metodos)
        {
            String sql = @"Select colunas.name as Coluna_BD
                            from sys.tables tabela inner join sys.columns colunas
                            on tabela.object_id = colunas.object_id
                            where 0=0 and ";
            foreach (Persist pst in param)
            {
                sql += "tabela.name = '{0}' and colunas.name = '{1}' or ";
                sql = string.Format(sql, tabela, pst.DscNome);
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

        private void AlterTable(String tabela, List<Persist> param, Object o)
        {
            string alter = @"ALTER TABLE {0} ADD ";
            alter = String.Format(alter, tabela);
            foreach (Persist s in param)
            {
                if (auxiliar.VerificaTipo(s.Type.Name.ToUpper()))
                {
                    alter += " {0} " + auxiliar.DbTipo(s.Type.Name.ToString().ToUpper());
                    alter = String.Format(alter, s.DscNome);
                }
                else
                {
                    if (s.ObjValor == null)
                        o = Activator.CreateInstance(s.Type);
                    else
                        o = s.ObjValor;
                    List<Persist> obj = fabrica.CriarPersist(o);
                    tabela = auxiliar.CriarNomeTabela(o);
                    alter += s.DscNome.ToLower() + " int FOREIGN KEY REFERENCES " + tabela + "(" + s.DscNome.ToLower() + "), ";
                    VerificaTabelaBanco(obj, tabela, o);
                }
            }
            alter = alter.Substring(0, alter.Length - 2);
            dao.ExecuteNonQuery(alter);
        }

        private String CriarTabelaCondicoes(String o)
        {
            String tabela = o.ToUpper().Substring(o.ToUpper().IndexOf("FROM") + 4, o.Length - (o.ToUpper().IndexOf("FROM") + 4));
            return tabela;
        }

        #endregion

        internal int Insert(Object o)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(o);
                String tabela = auxiliar.CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String pk = lst[0].DscNome;
                lst.RemoveAt(0);
                String[] colunas = fabrica.CriarColunas(lst);
                String script = GerarScriptInsert(colunas, tabela, pk);
                return dao.ExecuteNonQuery(script, lst);
            }
            catch
            {
                throw;
            }
        }

        internal void Update(Object o)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptUpdate(colunas, tabela);
                dao.ExecuteNonQuery(script, lst);
            }
            catch
            {
                throw;
            }
        }

        internal void Delete(Object o)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptDelete(colunas, tabela);
                lst.RemoveRange(1, lst.Count - 1);
                dao.ExecuteNonQuery(script, lst);
            }
            catch
            {
                throw;
            }
        }

        internal List<Object> Select(Object o, FlagPredicado condicao)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptSelect(colunas, tabela, lst, condicao);
                return ConvertList(dao.ExecuteReader(script, lst), fabrica.CriarPersistPuro(o), o);
            }
            catch
            {
                throw;
            }
        }

        internal List<Object> Select(Object o, params SelectCondition[] condicoes)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptSelect(colunas, tabela, lst, condicoes);
                return ConvertList(dao.ExecuteReader(script, lst), fabrica.CriarPersistPuro(o), o);
            }
            catch
            {
                throw;
            }
        }

        public List<Object[]> Execute(String sql)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(sql);
                String[] colunas = fabrica.CriarColunas(lst);
                return ConvertList(dao.ExecuteReader(sql), lst);
            }
            catch
            {
                throw;
            }
        }
    }
}
