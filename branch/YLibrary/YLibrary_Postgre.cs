using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Reflection;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using Npgsql;
using Classes;
using System.Configuration;

namespace Facade
{
    class YLibrary_Postgre
    {
        private NpgsqlConnection connect = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["YLibrary_Postgre"] == null ? ConfigurationSettings.AppSettings["YLibrary_Postgre"] : ConfigurationManager.ConnectionStrings["YLibrary_Postgre"].ConnectionString);
        private NpgsqlCommand command;
        private YConfiguracao configuracao;
        private List<Persist> ListaPersistEntidade;
        private List<String> Pilha = new List<String>();
        private String tabela;

        public YLibrary_Postgre(YConfiguracao configuracao = null)
        {
            this.configuracao = CriarConfiguração(configuracao);
        }

        private YConfiguracao CriarConfiguração(YConfiguracao config)
        {
            configuracao = new YConfiguracao();
            config = config == null ? new YConfiguracao() : config;

            configuracao.SEMPRE_VERIFICAR_BANCO = config.SEMPRE_VERIFICAR_BANCO == null ? ConfigurationManager.AppSettings["SEMPRE_VERIFICAR_BANCO"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["SEMPRE_VERIFICAR_BANCO"].ToUpper()) : false : config.SEMPRE_VERIFICAR_BANCO;
            configuracao.CLASS_LIBRARY_ENTIDADE = config.CLASS_LIBRARY_ENTIDADE == null ? ConfigurationManager.AppSettings["CLASS_LIBRARY_ENTIDADE"] != null ? Convert.ToString(ConfigurationManager.AppSettings["CLASS_LIBRARY_ENTIDADE"].ToUpper()) : "ENTIDADE" : config.CLASS_LIBRARY_ENTIDADE;
            configuracao.ALIAS_TABELA = config.ALIAS_TABELA == null ? ConfigurationManager.AppSettings["ALIAS_TABELA"] != null ? Convert.ToString(ConfigurationManager.AppSettings["ALIAS_TABELA"].ToUpper()) : "TB_" : config.ALIAS_TABELA;
            configuracao.ALTERA_TABELA = config.ALTERA_TABELA == null ? ConfigurationManager.AppSettings["ALTERA_TABELA"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["ALTERA_TABELA"].ToUpper()) : true : config.ALTERA_TABELA;
            configuracao.TAMANHO_PADRAO_STRING = config.TAMANHO_PADRAO_STRING == null ? ConfigurationManager.AppSettings["TAMANHO_PADRAO_STRING"] != null ? Convert.ToString(ConfigurationManager.AppSettings["TAMANHO_PADRAO_STRING"].ToUpper()) : "255" : config.TAMANHO_PADRAO_STRING;
            configuracao.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA = config.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA == null ? ConfigurationManager.AppSettings["ALIAS_CAMPO_TABELA_AUTO_RELACIONADA"] != null ? Convert.ToString(ConfigurationManager.AppSettings["ALIAS_CAMPO_TABELA_AUTO_RELACIONADA"].ToUpper()) : "ISN_" : config.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA;
            configuracao.ALIAS_CLASSE_FACADE = config.ALIAS_CLASSE_FACADE == null ? ConfigurationManager.AppSettings["ALIAS_CLASSE_FACADE"] != null ? Convert.ToString(ConfigurationManager.AppSettings["ALIAS_CLASSE_FACADE"].ToUpper()) : "F" : config.ALIAS_CLASSE_FACADE;
            configuracao.ENTIDADE_COMPLETA = config.ENTIDADE_COMPLETA == null ? ConfigurationManager.AppSettings["ENTIDADE_COMPLETA"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["ENTIDADE_COMPLETA"].ToUpper()) : false : config.ENTIDADE_COMPLETA;

            return configuracao;
        }

        #region Nova Estrutura

        private void ExecuteNonQuery(String sql)
        {
            try
            {
                connect.Open();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                connect.Close();
            }
        }

        private DataTable ExecuteReader(String sql)
        {
            try
            {
                connect.Open();
                command = new NpgsqlCommand(sql, connect);
                command.CommandText = sql;
                DataTable dt = new DataTable();
                dt.Load(command.ExecuteReader());
                return dt;
            }
            catch
            {
                throw;
            }
            finally
            {
                connect.Close();
            }
        }

        private Int32 ExecuteNonQuery(String sql, List<Persist> param)
        {
            try
            {
                sql += ";select currval('"+tabela+"_seq"+"');";
                connect.Open();
                command = new NpgsqlCommand(sql, connect);
                int count;
                for (int i = 0; i < param.Count; i++)
                {
                    DbType tipo = RetornaTipo(param[i].Type.Name.ToUpper());
                    count = command.Parameters.Count;
                    command.Parameters.Add(new NpgsqlParameter("" + i, tipo));
                    command.Parameters[count].Value = param[i].ObjValor == null ? DBNull.Value : param[i].ObjValor;
                }
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception q)
            {
                throw new ApplicationException(q.ToString());
            }
            finally
            {
                connect.Close();
            }
        }

        private DataTable ExecuteReader(String sql, List<Persist> param)
        {
            try
            {

                connect.Open();
                command = new NpgsqlCommand(sql, connect);
                int count;
                for (int i = 0; i < param.Count; i++)
                {
                    if (param[i].ObjValor != null)
                    {
                        DbType tipo = RetornaTipo(param[i].Type.Name.ToUpper());
                        count = command.Parameters.Count;
                        command.Parameters.Add(new NpgsqlParameter("" + i, tipo));
                        command.Parameters[count].Value = param[i].ObjValor;
                    }
                }
                DataTable dt = new DataTable();
                dt.Load(command.ExecuteReader());
                return dt;
            }
            catch (Exception q)
            {
                throw new ApplicationException(q.ToString());
            }
            finally
            {
                connect.Close();
            }
        }

        /// <summary>
        /// Verifica os tipos dos campos no Banco.
        /// </summary>
        /// <returns></returns>
        private bool VerificaTipo(String tipo)
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
        private String DbTipo(String campo)
        {
            switch (campo)
            {
                case "STRING":
                    return "varchar(" + configuracao.TAMANHO_PADRAO_STRING + ") NULL, ";
                case "DATETIME":
                    return "date NULL, ";
                case "TIMESPAN":
                    return "time NULL, ";
                case "INT32":
                    return "int NULL, ";
                case "INT64":
                    return "bigint NULL, ";
                case "INT16":
                    return "smallint NULL, ";
                case "FLOAT":
                case "DOUBLE":
                    return "float8 NULL, ";
                case "DECIMAL":
                    return "money NULL, ";
                case "CHAR":
                    return "varchar(1) NULL, ";
                case "DATE":
                    return "date NULL, ";
                case "BOOL":
                case "BOOLEAN":
                    return "bool NULL, ";
                case "SINGLE":
                    return "real NULL, ";
                case "BYTE":
                    return "Bytea NULL, ";
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
                case "TIMESPAN":
                    return System.Data.DbType.Time;
            }
            return System.Data.DbType.Guid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        private System.Reflection.PropertyInfo[] Ordenar(System.Reflection.PropertyInfo[] properties)
        {
            System.Reflection.PropertyInfo[] vet = new System.Reflection.PropertyInfo[properties.Length];
            int count = 0;

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].GetCustomAttributes(false).Length > 0)
                {
                    if (properties[i].GetCustomAttributes(false).First().ToString().Contains("Key"))
                    {
                        vet[0] = properties[i];
                        properties[i] = null;
                        count++;
                        break;
                    }
                }
            }
            for (int i = 1; i < properties.Length; i++)
            {
                if (properties[i] != null)
                    vet[i] = properties[i];
            }
            return vet;
        }

        /// <summary>
        /// Verifica a Annotation de um campo da Entidade
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private String VerificarAnnotation(Persist obj, PropertyInfo property)
        {
            try
            {
                return ((KeyAttribute)property.GetCustomAttributes(typeof(KeyAttribute), false).First()).ToString().ToUpper();
            }
            catch
            {
                try
                {
                    return ((StringLengthAttribute)property.GetCustomAttributes(typeof(StringLengthAttribute), false).First()).MaximumLength.ToString().ToUpper();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Criar Persist para o Select de parametro tipo string
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private List<Persist> CriarPersist(String o)
        {
            String[] vet = o.ToUpper().Substring(o.ToUpper().IndexOf("SELECT") + 6, (o.ToUpper().IndexOf("FROM")) - 6).Split(',');
            List<Persist> p = new List<Persist>();
            Persist pst;
            for (int i = 0; i < vet.Length; i++)
            {
                pst = new Persist();
                pst.DscNome = vet[i].Trim();
                pst.ObjValor = null;
                p.Add(pst);
            }
            return p;
        }

        /// <summary>
        /// Criar o persist padrão (Utilizado em Insert,Delete, Update e Select)
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        //private List<Persist> CriarPersist(Object o)
        //{
        //    List<Persist> lst = new List<Persist>();
        //    Persist obj = new Persist();
        //    try
        //    {
        //        if (o != null)
        //        {
        //            Dictionary<string, object> propertyValues = new Dictionary<string, object>();
        //            Type ObjectType = o.GetType();
        //            System.Reflection.PropertyInfo[] properties = Ordenar(ObjectType.GetProperties());

        //            foreach (System.Reflection.PropertyInfo property in properties)
        //            {
        //                obj = new Persist();
        //                if (property.PropertyType.IsClass && !property.PropertyType.Name.ToUpper().Equals("STRING"))
        //                {
        //                    if (property.GetValue(o, null) == null)
        //                    {
        //                        //Aqui - Pegar o nome do chave primaria dentro da Entidade.
        //                        obj.ObjValor = Activator.CreateInstance(property.PropertyType);
        //                        obj.DscNome = CriarPersist(obj.ObjValor)[0].DscNome.ToLower();
        //                        obj.ObjValor = null;
        //                        obj.FlgFlag = VerificarAnnotation(obj, property);
        //                        obj.Type = property.PropertyType;
        //                    }
        //                    else
        //                        obj = CriarPersist(property.GetValue(o, null))[0];
        //                    lst.Add(obj);
        //                }
        //                else
        //                {
        //                    obj.DscNome = property.Name.ToString();
        //                    obj.ObjValor = property.GetValue(o, null);
        //                    obj.FlgFlag = VerificarAnnotation(obj, property);
        //                    obj.Type = property.PropertyType.Name.ToUpper().Equals("STRING") ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
        //                    lst.Add(obj);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            obj = null;
        //            lst.Add(obj);
        //        }
        //    }
        //    catch (System.NullReferenceException ex)
        //    {
        //        throw new System.ArgumentException("Não foi indentificado Annotations na Entidade", "YLibrary");
        //    }
        //    catch (IndexOutOfRangeException ex)
        //    {
        //        throw new System.ArgumentException("A atributo da classe não é nullable", "YLibrary");
        //    }
        //    return lst;
        //}

        private List<Persist> CriarPersist(Object o)
        {
            List<Persist> lst = new List<Persist>();
            Persist obj = new Persist();
            int count;
            try
            {
                if (o != null)
                {
                    Dictionary<string, object> propertyValues = new Dictionary<string, object>();
                    Type ObjectType = o.GetType();
                    System.Reflection.PropertyInfo[] properties = Ordenar(ObjectType.GetProperties());

                    foreach (System.Reflection.PropertyInfo property in properties)
                    {
                        obj = new Persist();
                        if (property.PropertyType.IsClass && !property.PropertyType.Name.ToUpper().Equals("STRING"))
                        {
                            if (property.GetValue(o, null) == null)
                            {
                                //Aqui - Pegar o nome do chave primaria dentro da Entidade.
                                obj.ObjValor = Activator.CreateInstance(property.PropertyType);
                                if (obj.ObjValor.GetType().Equals(ObjectType))
                                    obj.DscNome = (configuracao.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA + property.GetGetMethod().Name.Remove(0, 4)).ToLower();
                                else
                                    obj.DscNome = VerificaPK(obj.ObjValor).ToLower();
                                obj.ObjValor = null;
                                obj.FlgFlag = VerificarAnnotation(obj, property);
                                obj.Type = property.PropertyType;
                            }
                            else
                                obj = CriarPersist(property.GetValue(o, null))[0];
                            count = lst.Count(item => item.DscNome.Equals(obj.DscNome));
                            if (count > 0)
                                obj.DscNome = obj.DscNome + count;
                            lst.Add(obj);
                        }
                        else
                        {
                            obj.DscNome = property.Name.ToString();
                            obj.ObjValor = property.GetValue(o, null);
                            obj.FlgFlag = VerificarAnnotation(obj, property);
                            obj.Type = property.PropertyType.Name.ToUpper().Equals("STRING") ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
                            lst.Add(obj);
                        }
                    }
                }
                else
                {
                    obj = null;
                    lst.Add(obj);
                }
            }
            catch (System.NullReferenceException ex)
            {
                throw new System.ArgumentException("Não foi indentificado Annotations na Entidade " + o.GetType().Name, "YLibrary");
            }
            catch (System.IndexOutOfRangeException ex)
            {
                throw new System.ArgumentException("Existem atributos non-nullable na classe " + o.GetType().ToString(), "YLibrary");
            }
            return lst;
        }

        private List<Persist> CriarPersistPuro(Object o)
        {
            List<Persist> lst = new List<Persist>();
            Persist obj = new Persist();
            if (o != null)
            {
                Dictionary<string, object> propertyValues = new Dictionary<string, object>();
                Type ObjectType = o.GetType();
                System.Reflection.PropertyInfo[] properties = Ordenar(ObjectType.GetProperties());

                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    obj = new Persist();
                    if (property.PropertyType.IsClass && !property.PropertyType.Name.ToUpper().Equals("STRING"))
                    {
                        if (property.GetValue(o, null) == null)
                        {
                            obj.ObjValor = property.PropertyType;
                            obj.DscNome = property.Name;
                            obj.FlgFlag = VerificarAnnotation(obj, property);
                            obj.Type = property.PropertyType;
                        }
                        else
                        {
                            obj = CriarPersistOriginal(property.GetValue(o, null))[0];
                            obj.Type = property.GetValue(o, null).GetType();
                            obj.ObjValor = property.GetValue(o, null);
                        }
                        lst.Add(obj);
                    }
                    else
                    {
                        obj.DscNome = property.Name;
                        obj.ObjValor = property.GetValue(o, null);
                        obj.FlgFlag = VerificarAnnotation(obj, property);
                        obj.Type = property.PropertyType.Name.ToUpper().Equals("STRING") ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
                        lst.Add(obj);
                    }
                }
            }
            else
            {
                obj = null;
                lst.Add(obj);
            }
            return lst;
        }

        /// <summary>
        /// Mesmo que o CriarPersist, com diferença que no caso de um campo ser uma Entidade
        /// ele irá prenceher com a entidade, no caso de prencher somente com a chave primaria
        /// como é no CriarPersist
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private List<Persist> CriarPersistOriginal(Object o)
        {
            List<Persist> lst = new List<Persist>();
            Persist obj = new Persist();
            if (o != null)
            {
                Dictionary<string, object> propertyValues = new Dictionary<string, object>();
                Type ObjectType = o.GetType();
                System.Reflection.PropertyInfo[] properties = Ordenar(ObjectType.GetProperties());

                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    obj = new Persist();
                    obj.DscNome = property.Name;
                    obj.ObjValor = property.GetValue(o, null);
                    obj.FlgFlag = VerificarAnnotation(obj, property);
                    //if (property.PropertyType.IsGenericType)
                    obj.Type = property.PropertyType.IsClass ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
                    //else
                    //obj.Type = property.PropertyType;
                    lst.Add(obj);
                }
            }
            else
            {
                obj = null;
                lst.Add(obj);
            }
            return lst;
        }

        private Object[] CriarObjeto(List<Persist> lst)
        {
            int num = 0;
            Object[] param = new Object[lst.Count];
            foreach (Persist obj in lst)
            {
                param[num] = obj.ObjValor;
                num++;
            }
            return param;
        }

        /// <summary>
        /// Cria um array de String com o nome das colunas.
        /// </summary>
        /// <returns>Array com os nomes das colunas</returns>
        private String[] CriarColunas(List<Persist> lst)
        {
            int num = 0;
            String[] param = new String[lst.Count];
            foreach (Persist obj in lst)
            {
                if (obj.ObjValor != null)
                {
                    if (obj.Type.IsClass && !obj.Type.Name.ToUpper().Equals("STRING"))
                    {
                        if (obj.ObjValor != null)
                            param[num] += CriarPersist(obj.ObjValor)[0].DscNome.ToLower();
                        else
                        {
                            obj.ObjValor = Activator.CreateInstance(obj.Type);
                            param[num] += CriarPersist(obj.ObjValor)[0].DscNome.ToLower();
                        }
                    }
                    else
                    {
                        param[num] = obj.DscNome.ToLower();
                        num++;
                    }
                }
                else
                {
                    param[num] = obj.DscNome.ToLower();
                    num++;
                }
            }
            return param;
        }

        private String GerarScriptInsert(String[] colunas, String tabela)
        {
            String script = string.Empty;
            String script2 = string.Empty;
            script = String.Format("Insert into {0} (", tabela);
            script2 = "values(";
            for (int i = 0; i < colunas.Length; i++)
            {
                script += colunas[i] + ",";
                script2 += "@" + i + ",";
            }
            script = script.Remove(script.Length - 1, 1) + ") ";
            script2 = script2.Remove(script2.Length - 1, 1) + ") ";
            script += script2;
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

        private String GerarScriptSelect(String[] colunas, String tabela, List<Persist> obj, String where, String ordenacao)
        {

            String script = string.Empty;
            script = "Select ";
            for (int i = 0; i < colunas.Length; i++)
                script += colunas[i] + ",";
            script = script.Remove(script.Length - 1, 1) + " ";
            script += String.Format(" from {0} ", tabela);
            script += "where 0=0 ";
            if (where != "")
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    if (obj[i].ObjValor != null)
                        script += where + " " + colunas[i] + " = @" + i + " ";
                }
            }
            if (ordenacao != null)
                script += " order by " + ordenacao.ToLower();
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
                    if (!VerificaTipo(lst[count].Type.Name.ToUpper()))
                    {
                        propertyValues = new Dictionary<string, object>();
                        System.Reflection.PropertyInfo[] properties = Ordenar(ObjectType.GetProperties());
                        foreach (System.Reflection.PropertyInfo property in properties)
                        {
                            if (property.Name.ToString().Equals(lst[count].DscNome))
                            {
                                ObjectType2 = property.PropertyType;
                                System.Reflection.PropertyInfo[] properties2 = Ordenar(ObjectType2.GetProperties());
                                subentidade = Activator.CreateInstance(ObjectType2);
                                numberPropertyInfo = ObjectType2.GetProperty(properties2[0].Name);
                                valor = CriarNullable(numberPropertyInfo.PropertyType.GetGenericArguments()[0].Name.ToUpper(), table.Rows[0][count]);
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
                                        numberPropertyInfo = ObjectType.GetProperty(lst[count].DscNome);
                                        numberPropertyInfo.SetValue(ent, lobj.Count > 0 ? lobj[0] : null, null);
                                    }
                                }
                                else
                                {
                                    numberPropertyInfo = ObjectType.GetProperty(lst[count].DscNome);
                                    numberPropertyInfo.SetValue(ent, subentidade, null);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        numberPropertyInfo = ObjectType.GetProperty(lst[count].DscNome);
                        numberPropertyInfo.SetValue(ent, CriarNullable((lst[count].Type.Name.ToUpper()), row), null);

                    }
                    count++;
                }
                list.Add(ent);
            }
            return list;
        }

        /// <summary>
        /// Criado devido a necessidade de criar um nullable de um determinado valor
        /// </summary>
        /// <returns>object</returns>
        private object CriarNullable(String nome, object valor)
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
                //return null;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Lista de objetos</returns>
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

        /// <summary>
        /// Cria a tabela do banco referente a Entidade.
        /// </summary>
        private void CreateTable(String tabela, List<Persist> param, Object o)
        {
            if (!(bool)configuracao.ALTERA_TABELA)
                throw new System.ArgumentException("A tabela " + tabela + " referente a entidade " + o.GetType().ToString() + " não existe no Banco", "YLibrary");

            string sequence = "create sequence {0}";
            string insert = @"CREATE TABLE {0} ( ";
            insert = String.Format(insert, tabela);
            List<Persist> lst = CriarPersistOriginal(o);
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
                            insert += " {0} " + DbTipo(param[i].Type.Name.ToString().ToUpper()).Replace(configuracao.TAMANHO_PADRAO_STRING, param[i].FlgFlag);
                            insert = String.Format(insert, param[i].DscNome.ToLower());
                        }
                    }
                    else
                    {
                        insert += " {0} " + DbTipo(param[i].Type.Name.ToString().ToUpper());
                        insert = String.Format(insert, param[i].DscNome.ToLower());
                    }
                }
                else
                {
                    if (lst[i].ObjValor == null)
                        o = Activator.CreateInstance(lst[i].Type);
                    else
                        o = lst[i].ObjValor;
                    List<Persist> lstInterno = CriarPersist(o);
                    tabela = CriarNomeTabela(o);
                    insert += lstInterno[0].DscNome.ToLower() + " int references " + tabela + "(" + lstInterno[0].DscNome.ToLower() + "), ";
                    VerificaTabelaBanco(lstInterno, tabela, o);
                }
            }
            insert = insert.Substring(0, insert.Length - 2);
            insert += ")";
            ExecuteNonQuery(sequence);
            ExecuteNonQuery(insert);
        }

        /// <summary>
        /// Verifica se os campos da Entidade estão diferentes do xml de configuração.
        /// Caso esteja ele verifica se a tabela referente a entidade exista.
        /// </summary>
        private void VerificaTabelaBanco(List<Persist> lst, String tabela, Object o)
        {
            this.tabela = tabela;
            String ConfigOld = String.Empty;
            String Config = MetodosDll(tabela, o);

            if (!(bool)configuracao.SEMPRE_VERIFICAR_BANCO)
                ConfigOld = LerXml(tabela);

            if (Config.Equals(ConfigOld))
                return;
            else
            {
                String sql = @"SELECT tablename FROM pg_catalog.pg_tables
                                WHERE schemaname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
                                and tablename = '" + tabela.Trim() + "'";
                DataTable dt = ExecuteReader(sql);
                if (dt.Rows.Count <= 0)
                    CreateTable(tabela, lst, o);
                else
                    CheckTable(tabela, lst, o, Config);
                InserirXml(tabela, Config);
            }
        }

        /// <summary>
        /// Verifica os metodos da classe referente a tabela passada por parametro
        /// dentro do assembly Entidade.dll.
        /// </summary>
        /// <returns>String com os metodos separados por virgulas</returns>
        //private String MetodosDll(String tabela, Object o)
        //{
        //    foreach (Assembly b in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        if (b.ManifestModule.Name.ToUpper().Contains("ENTIDADE"))
        //        {
        //            String nome = "";
        //            foreach (Type t in b.GetExportedTypes())
        //            {
        //                if (t.Name.ToUpper().Equals(tabela.ToUpper().Replace("TB_", "")))
        //                {
        //                    List<Persist> lst = CriarPersistOriginal(o);
        //                    foreach (MemberInfo mi in t.GetMembers())
        //                    {
        //                        if (mi.Name.Contains("get_"))
        //                        {
        //                            Persist obj = lst.Find(x => x.DscNome == mi.Name.Replace("get_", ""));
        //                            if (obj.Type.IsClass && !obj.Type.Name.ToUpper().Equals("STRING"))
        //                            {
        //                                if (obj.ObjValor != null)
        //                                    nome += CriarPersist(obj.ObjValor)[0].DscNome + ",";
        //                                else
        //                                {
        //                                    obj.ObjValor = Activator.CreateInstance(obj.Type);
        //                                    nome += CriarPersist(obj.ObjValor)[0].DscNome + ",";
        //                                }
        //                            }
        //                            else
        //                                nome += mi.Name.Replace("get_", "") + ",";

        //                        }
        //                    }
        //                    nome = nome.Substring(0, nome.Length - 1);
        //                    return nome;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

        private String MetodosDll(String tabela, Object o)
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
                System.Reflection.PropertyInfo[] properties = Ordenar(ObjectType.GetProperties());

                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    if (!(property.PropertyType.IsClass && !property.PropertyType.Name.ToUpper().Equals("STRING")))
                    {
                        if (VerificarAnnotation(obj, property).ToUpper().Contains("KEY"))
                            return property.Name.ToString();
                    }
                }
            }
            throw new System.ArgumentException("Não foi indentificado uma chave primária na classe " + o.GetType().Name, "YLibrary");
        }

        /// <summary>
        /// Verifica se o xml de configuração do YLibrary ja existe
        /// Para editar ou criar.
        /// </summary>
        private void InserirXml(string nome, string key)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string caminho = assm.CodeBase.Replace("file:///", "").Replace("YLibrary.DLL", "YLibrary.config.dll");

            if (File.Exists(caminho))
                AlterarXml(nome, key, caminho);
            else
                CriarXml(nome, key, caminho);
        }

        /// <summary>
        /// Cria o xml de configuração com o No raiz Entidade.
        /// </summary>
        private void CriarXml(string nome, string key, string caminho)
        {
            if (!File.Exists(caminho))
            {
                XmlDocument doc = new XmlDocument();
                XmlNode raiz = doc.CreateElement("Entidade");
                doc.AppendChild(raiz);
                doc.Save(caminho);
            }
            EscreverXml(nome, key, caminho);
        }

        /// <summary>
        /// Escreve no Xml de configuração um novo node.
        /// </summary>
        private void EscreverXml(string nome, string key, string caminho)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(caminho);
            XmlNode linha = doc.CreateElement(nome);
            XmlNode Id = doc.CreateElement("id");
            Id.InnerText = key;
            linha.AppendChild(Id);
            doc.SelectSingleNode("/Entidade").AppendChild(linha);
            doc.Save(caminho);
        }

        /// <summary>
        /// Altera um node existente no xml de configuração
        /// </summary>
        private void AlterarXml(string nome, string key, string caminho)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(caminho);
            XmlNode no;
            no = doc.SelectSingleNode("/Entidade/" + nome);
            if (no == null)
            {
                EscreverXml(nome, key, caminho);
            }
            else
            {
                no.SelectSingleNode("./id").InnerText = key;
                doc.Save(caminho);
            }
        }

        /// <summary>
        /// Ler um node dentro do xml de configuração
        /// </summary>
        /// <returns>Campo id do xml</returns>
        private String LerXml(string nome)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string caminho = assm.CodeBase.Replace("file:///", "").Replace("YLibrary.DLL", "YLibrary.config.dll");
            XmlDocument doc = new XmlDocument();
            if (File.Exists(caminho))
                doc.Load(caminho);
            else
                return "";
            XmlNode no;
            no = doc.SelectSingleNode("/Entidade/" + nome);
            if (no == null)
                return "";
            else
                return no.SelectSingleNode("./id").InnerText;
        }

        /// <summary>
        /// Verifica se a tabela do banco está faltando algum campo
        /// da entidade.
        /// </summary>
        private void CheckTable(String tabela, List<Persist> param, Object o, String metodos)
        {
            String sql = @"select column_name from information_schema.columns where 0=0 and table_name='{0}' or ";
            sql = String.Format(sql, tabela);

            foreach (Persist pst in param)
            {
                sql += "column_name = '{0}' or ";
                sql = string.Format(sql, pst.DscNome);
            }
            sql = sql.Substring(0, sql.Length - 3);

            DataTable dt = ExecuteReader(sql);

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
        private void AlterTable(String tabela, List<Persist> param, Object o)
        {
            if (!(bool)configuracao.ALTERA_TABELA)
                throw new System.ArgumentException("A tabela " + tabela + " referente a entidade " + o.GetType().ToString() + " possui colunas diferentes", "YLibrary");

            string alter = @"ALTER TABLE {0} ADD ";
            alter = String.Format(alter, tabela);
            foreach (Persist s in param)
            {
                if (VerificaTipo(s.Type.Name.ToUpper()))
                {
                    alter += " {0} " + DbTipo(s.Type.Name.ToString().ToUpper());
                    alter = String.Format(alter, s.DscNome);
                }
                else
                {
                    if (s.ObjValor == null)
                        o = Activator.CreateInstance(s.Type);
                    else
                        o = s.ObjValor;
                    List<Persist> obj = CriarPersist(o);
                    tabela = CriarNomeTabela(o);
                    alter += s.DscNome.ToLower() + " int FOREIGN KEY REFERENCES " + tabela + "(" + s.DscNome.ToLower() + "), ";
                    VerificaTabelaBanco(obj, tabela, o);
                }
            }
            alter = alter.Substring(0, alter.Length - 2);
            ExecuteNonQuery(alter);
        }

        private String CriarTabelaCondicoes(String o)
        {
            String tabela = o.ToUpper().Substring(o.ToUpper().IndexOf("FROM") + 4, o.Length - (o.ToUpper().IndexOf("FROM") + 4));
            return tabela;
        }

        private String CriarNomeTabela(Object o)
        {
            Type ObjectType = o.GetType();
            String[] vet = ObjectType.ToString().Split('.');
            return configuracao.ALIAS_TABELA.ToLower() + vet[vet.Length - 1].ToLower();
        }

        public Int32 Insert(Object o)
        {
            try
            {
                List<Persist> lst = CriarPersist(o);
                String tabela = CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                lst.RemoveAt(0);
                String[] colunas = CriarColunas(lst);
                String script = GerarScriptInsert(colunas, tabela);
                return ExecuteNonQuery(script, lst);
            }
            catch
            {
                throw;
            }
        }

        public void Update(Object o)
        {
            try
            {
                List<Persist> lst = CriarPersist(o);
                String[] colunas = CriarColunas(lst);
                String tabela = CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptUpdate(colunas, tabela);
                ExecuteNonQuery(script, lst);
            }
            catch
            {
                throw;
            }
        }

        public void Delete(Object o)
        {
            try
            {
                List<Persist> lst = CriarPersist(o);
                //Object[] obj = lst.ToArray();//CriarObjeto(lst);
                //Object[] obj2 = new Object[1];
                //obj2[0] = obj[0];
                String[] colunas = CriarColunas(lst);
                String tabela = CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptDelete(colunas, tabela);
                lst.RemoveRange(1, lst.Count - 1);
                ExecuteNonQuery(script, lst);
            }
            catch
            {
                throw;
            }
        }

        public List<Object> Select(Object o, String condicao = "and", String ordenacao = null)
        {
            try
            {
                List<Persist> lst = CriarPersist(o);
                String[] colunas = CriarColunas(lst);
                String tabela = CriarNomeTabela(o);
                VerificaTabelaBanco(lst, tabela, o);
                String script = GerarScriptSelect(colunas, tabela, lst, condicao, ordenacao);
                return ConvertList(ExecuteReader(script, lst), CriarPersistPuro(o), o);
            }
            catch
            {
                throw;
            }
        }

        public List<Object[]> Select(String sql)
        {
            try
            {
                List<Persist> lst = CriarPersist(sql);
                String[] colunas = CriarColunas(lst);
                return ConvertList(ExecuteReader(sql), lst);
            }
            catch
            {
                throw;
            }
        }

        public virtual void PersistirTodasEntidades()
        {
            object entidade = null;
            List<Assembly> lstassem = AppDomain.CurrentDomain.GetAssemblies().ToList<Assembly>();
            Assembly a = lstassem.Find(item => item.GetName().Name.ToUpper() == configuracao.CLASS_LIBRARY_ENTIDADE);

            foreach (Type t in a.GetExportedTypes())
            {
                entidade = Activator.CreateInstance(t);
                PersistirTodasEntidades(entidade);
            }
        }

        public virtual void PersistirTodasEntidades(Object entidade)
        {
            configuracao.SEMPRE_VERIFICAR_BANCO = true;
            List<Persist> lst = ListaPersistEntidade = CriarPersist(entidade);
            String[] colunas = CriarColunas(lst);
            String tabela = CriarNomeTabela(entidade);
            VerificaTabelaBanco(lst, tabela, entidade);
            configuracao.SEMPRE_VERIFICAR_BANCO = false;
        }

        public virtual void PersistirTodasEntidades(Object[] entidade)
        {
            foreach (Object o in entidade)
                PersistirTodasEntidades(o);
        }

        #endregion
    }
}