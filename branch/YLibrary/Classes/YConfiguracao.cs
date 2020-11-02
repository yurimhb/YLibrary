using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Classes
{
    public class YConfiguracao
    {
        public YConfiguracao() { }

        /// <summary>
        /// TRUE (Menor Desempenho): 
        /// Para todo acesso no banco, será verificado se a tabela possui
        /// os mesmos campos da entidade, caso não possua será feito um
        /// alter table adicionando a tabela, coluna.
        /// 
        /// FALSE (Maior Desempenho):
        /// É feito um espelhamento do banco, evitando da YLibrary consultar
        /// a estrutura da tabela para qualquer acesso ao banco.
        /// 
        /// </summary>
        public Boolean? SEMPRE_VERIFICAR_BANCO;

        /// <summary>
        /// Define o nome do projeto que possui as entidades do banco.
        /// 
        /// </summary>
        public String CLASS_LIBRARY_ENTIDADE;

        /// <summary>
        /// Define o alias inicial das tabelas do banco.
        /// 
        /// </summary>
        public String ALIAS_TABELA;

        /// <summary>
        /// Define se a YLibrary irá alterar as tabelas do banco
        /// no caso de encontrar diferenças entre a entidade e tabela.
        /// 
        /// </summary>
        public Boolean? ALTERA_TABELA;


        /// <summary>
        /// Define tamanho padrão da String no banco
        /// 
        /// </summary>
        public String TAMANHO_PADRAO_STRING;

        /// <summary>
        /// Define o alias inicial para colunas de tabelas com
        /// auto-relacionamento
        /// 
        /// </summary>
        public String ALIAS_CAMPO_TABELA_AUTO_RELACIONADA;

        /// <summary>
        /// Define o alias inicial para as classes facades das entidades
        /// 
        /// </summary>
        public String ALIAS_CLASSE_FACADE;

        /// <summary>
        /// Define se a YLibrary irá trazer a informação completa para todas
        /// as entidades vinculadas a Entidade principal
        /// 
        /// </summary>
        public Boolean? ENTIDADE_COMPLETA;

        /// <summary>
        /// Define o nome do banco
        /// 
        /// </summary>
        public String NOME_BANCO;
        
    }
}
