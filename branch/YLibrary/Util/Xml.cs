using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;

namespace Util
{
    public class Xml
    {
        public void InserirXml(string nome, string key)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string caminho = assm.CodeBase.Replace("file:///", "").Replace("YLibrary", "YLibrary_config");

            if (File.Exists(caminho))
                AlterarXml(nome, key, caminho);
            else
                CriarXml(nome, key, caminho);
        }

        /// <summary>
        /// Cria o xml de configuração com o No raiz Entidade.
        /// </summary>
        public void CriarXml(string nome, string key, string caminho)
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
        public void EscreverXml(string nome, string key, string caminho)
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
        public void AlterarXml(string nome, string key, string caminho)
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
        public String LerXml(string nome)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string caminho = assm.CodeBase.Replace("file:///", "").Replace("YLibrary", "YLibrary_config");
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
    }
}
