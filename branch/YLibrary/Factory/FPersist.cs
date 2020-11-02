using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classes;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Factory;

namespace Factory
{
    public class FPersist
    {
        YConfiguracao configuracao;

        public FPersist(YConfiguracao config) 
        {
            this.configuracao = config;
            //fabrica = new FPersist(config);
        }

        /// <summary>
        /// Criar Persist para o Select de parametro tipo string
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public List<Persist> CriarPersist(String o)
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

        public List<Persist> CriarPersist(Object o)
        {
            List<Persist> lst = new List<Persist>();
            Persist obj = new Persist();
            Object pai = null;
            bool tempai = false;
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
                                obj.FlgFlag = VerificarAnnotation(obj, property, ObjectType);
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
                            //if (((System.Reflection.MemberInfo)(property)).DeclaringType == ObjectType)
                            //{
                                obj.DscNome = property.Name.ToString();
                                obj.ObjValor = property.GetValue(o, null);
                                obj.FlgFlag = VerificarAnnotation(obj, property, ObjectType);
                                obj.Type = property.PropertyType.Name.ToUpper().Equals("STRING") ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
                                lst.Add(obj);
                            //}
                            //else 
                            //{
                            //    if (pai == null)
                            //    {
                            //        pai = Activator.CreateInstance(((System.Reflection.MemberInfo)(property)).DeclaringType);
                            //        tempai = true;
                            //    }
                            //    FieldInfo fi = pai.GetType().GetField(property.Name.ToString(), BindingFlags.NonPublic | BindingFlags.Instance);
                            //    fi.SetValue(pai, property.GetValue(o, null));
                            //}
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

        public List<Persist> CriarPersistPuro(Object o)
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
                            obj.FlgFlag = VerificarAnnotation(obj, property, ObjectType);
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
                        obj.FlgFlag = VerificarAnnotation(obj, property, ObjectType);
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
        public List<Persist> CriarPersistOriginal(Object o)
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
                    obj.FlgFlag = VerificarAnnotation(obj, property, ObjectType);
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

        /// <summary>
        /// Verifica a Annotation de um campo da Entidade
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public String VerificarAnnotation(Persist obj, PropertyInfo property, Type ObjectType)
        {
            try
            {
                if (((System.Reflection.MemberInfo)(property)).DeclaringType.Name.ToUpper().Equals("ENTIDADE"))
                {
                    return ((KeyAttribute)property.GetCustomAttributes(typeof(KeyAttribute), false).First()).ToString().ToUpper();
                }
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
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public System.Reflection.PropertyInfo[] Ordenar(System.Reflection.PropertyInfo[] properties)
        {
            System.Reflection.PropertyInfo[] vet = new System.Reflection.PropertyInfo[properties.Length];
            int pos = -1;

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].GetCustomAttributes(false).Length > 0)
                {
                    if (properties[i].GetCustomAttributes(false).First().ToString().Contains("Key"))
                    {
                        pos = i;
                        break;
                    }
                }
            }

            System.Reflection.PropertyInfo temp = properties[0];
            try
            {
                properties[0] = properties[pos];
                properties[pos] = temp;
            }
            catch
            {
                throw new System.ArgumentException("Não foi indentificado uma chave primária na classe " + properties.ToString(), "YLibrary");
            }
            return properties;
        }

        public String VerificaPK(Object o)
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
                        if (VerificarAnnotation(obj, property, ObjectType).ToUpper().Contains("KEY"))
                            return property.Name.ToString();
                    }
                }
            }
            throw new System.ArgumentException("Não foi indentificado uma chave primária na classe " + o.GetType().Name, "YLibrary");
        }

        public Object[] CriarObjeto(List<Persist> lst)
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
        public String[] CriarColunas(List<Persist> lst)
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

        public String GerarScriptInsert(String[] colunas, String tabela)
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

        public String GerarScriptUpdate(String[] colunas, String tabela)
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

        public String GerarScriptDelete(String[] colunas, String tabela)
        {
            String script = string.Empty;
            String script2 = string.Empty;
            script = String.Format("delete from {0} where ", tabela);
            script += colunas[0] + "=@0";
            return script;
        }

        public String GerarScriptSelect(String[] colunas, String tabela, List<Persist> obj, String where, String ordenacao)
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

    }
}
