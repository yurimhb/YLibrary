using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Data;
using System.Reflection;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using Npgsql;
using Classes;
using Util;
using Factory;

namespace Conexao
{
    public class Postgresql : Facade.YLibrary
    {

        #region Nova Estrutura

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
                            if (property.Name.ToString().Equals(lst[count].DscNome))
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

        public Int32 Insert(Object o)
        {
            try
            {
                List<Persist> lst = fabrica.CriarPersist(o);
                String tabela = auxiliar.CriarNomeTabela(o);
                auxiliar.VerificaTabelaBanco(lst, tabela, o);
                lst.RemoveAt(0);
                String[] colunas = fabrica.CriarColunas(lst);
                String script = fabrica.GerarScriptInsert(colunas, tabela);
                return dao.ExecuteNonQuery(script, lst);
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
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                auxiliar.VerificaTabelaBanco(lst, tabela, o);
                String script = fabrica.GerarScriptUpdate(colunas, tabela);
                dao.ExecuteNonQuery(script, lst);
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
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                auxiliar.VerificaTabelaBanco(lst, tabela, o);
                String script = fabrica.GerarScriptDelete(colunas, tabela);
                lst.RemoveRange(1, lst.Count - 1);
                dao.ExecuteNonQuery(script, lst);
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
                List<Persist> lst = fabrica.CriarPersist(o);
                String[] colunas = fabrica.CriarColunas(lst);
                String tabela = auxiliar.CriarNomeTabela(o);
                auxiliar.VerificaTabelaBanco(lst, tabela, o);
                String script = fabrica.GerarScriptSelect(colunas, tabela, lst, condicao, ordenacao);
                return ConvertList(dao.ExecuteReader(script, lst), fabrica.CriarPersistPuro(o), o);
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
                List<Persist> lst = fabrica.CriarPersist(sql);
                String[] colunas = fabrica.CriarColunas(lst);
                return ConvertList(dao.ExecuteReader(sql), lst);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}