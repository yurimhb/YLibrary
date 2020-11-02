using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classes;
using System.Configuration;
using Factory;
using Util;
using System.Reflection;

namespace Facade
{
    public class YLibrary
    {
        protected YConfiguracao configuracao;
        protected List<Persist> ListaPersistEntidade;
        protected List<String> Pilha = new List<String>();
        protected FPersist fabrica;
        protected Xml xml;
        protected DAO dao;
        protected Auxiliar auxiliar;

        public YLibrary()
        {
            this.configuracao = CriarConfiguração(configuracao);
            fabrica = new FPersist(this.configuracao);
            xml = new Xml();
            dao = new DAO(configuracao.NOME_BANCO);
            auxiliar = new Auxiliar(fabrica, configuracao, dao);
        }

        protected YConfiguracao CriarConfiguração(YConfiguracao config)
        {
            configuracao = new YConfiguracao();
            config = config == null ? new YConfiguracao() : config;

            configuracao.SEMPRE_VERIFICAR_BANCO = config.SEMPRE_VERIFICAR_BANCO == null ? ConfigurationManager.AppSettings["SEMPRE_VERIFICAR_BANCO"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["SEMPRE_VERIFICAR_BANCO"].ToUpper()) : false : config.SEMPRE_VERIFICAR_BANCO;
            configuracao.CLASS_LIBRARY_ENTIDADE = config.CLASS_LIBRARY_ENTIDADE == null ? ConfigurationManager.AppSettings["CLASS_LIBRARY_ENTIDADE"] != null ? Convert.ToString(ConfigurationManager.AppSettings["CLASS_LIBRARY_ENTIDADE"].ToUpper()) : "ENTIDADE" : config.CLASS_LIBRARY_ENTIDADE;
            configuracao.ALIAS_TABELA = config.ALIAS_TABELA == null ? ConfigurationManager.AppSettings["ALIAS_TABELA"] != null ? Convert.ToString(ConfigurationManager.AppSettings["ALIAS_TABELA"].ToUpper()) : "tb_" : config.ALIAS_TABELA;
            configuracao.ALTERA_TABELA = config.ALTERA_TABELA == null ? ConfigurationManager.AppSettings["ALTERA_TABELA"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["ALTERA_TABELA"].ToUpper()) : true : config.ALTERA_TABELA;
            configuracao.TAMANHO_PADRAO_STRING = config.TAMANHO_PADRAO_STRING == null ? ConfigurationManager.AppSettings["TAMANHO_PADRAO_STRING"] != null ? Convert.ToString(ConfigurationManager.AppSettings["TAMANHO_PADRAO_STRING"].ToUpper()) : "255" : config.TAMANHO_PADRAO_STRING;
            configuracao.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA = config.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA == null ? ConfigurationManager.AppSettings["ALIAS_CAMPO_TABELA_AUTO_RELACIONADA"] != null ? Convert.ToString(ConfigurationManager.AppSettings["ALIAS_CAMPO_TABELA_AUTO_RELACIONADA"].ToUpper()) : "isn_" : config.ALIAS_CAMPO_TABELA_AUTO_RELACIONADA;
            configuracao.ALIAS_CLASSE_FACADE = config.ALIAS_CLASSE_FACADE == null ? ConfigurationManager.AppSettings["ALIAS_CLASSE_FACADE"] != null ? Convert.ToString(ConfigurationManager.AppSettings["ALIAS_CLASSE_FACADE"].ToUpper()) : "F" : config.ALIAS_CLASSE_FACADE;
            configuracao.ENTIDADE_COMPLETA = config.ENTIDADE_COMPLETA == null ? ConfigurationManager.AppSettings["ENTIDADE_COMPLETA"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["ENTIDADE_COMPLETA"].ToUpper()) : false : config.ENTIDADE_COMPLETA;
            configuracao.NOME_BANCO = config.NOME_BANCO == null ? ConfigurationManager.AppSettings["NOME_BANCO"] != null ? Convert.ToString(ConfigurationManager.AppSettings["NOME_BANCO"].ToUpper()) : "SQLSERVER" : config.NOME_BANCO;

            return configuracao;
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
            List<Persist> lst = ListaPersistEntidade = fabrica.CriarPersist(entidade);
            String[] colunas = fabrica.CriarColunas(lst);
            String tabela = auxiliar.CriarNomeTabela(entidade);
            auxiliar.VerificaTabelaBanco(lst, tabela, entidade);
            configuracao.SEMPRE_VERIFICAR_BANCO = false;
        }

        public virtual void PersistirTodasEntidades(Object[] entidade)
        {
            foreach (Object o in entidade)
                PersistirTodasEntidades(o);
        }
    }
}
