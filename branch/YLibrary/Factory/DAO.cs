using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;
using System.Data.SqlClient;
using System.Data.OracleClient;
using Classes;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Factory
{
    public class DAO
    {
        public static String Tabela { get; set; }
        private IDbConnection connect;
        private IDbCommand command;
        private IDbDataParameter parameters;
        System.Data.IDbTransaction transacao;

        public DAO(String banco)
        {
            connect = Conexao(banco);
            command = Command(banco);

            if (connect == null)
                throw new Exception("Configuração ausente: YLibrary_{BANCO}.");
        }

        public void ExecuteNonQuery(String sql)
        {
            try
            {
                connect.Open();
                transacao = connect.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Connection = connect;
                command.Transaction = transacao;
                command.CommandText = sql;
                try
                {
                    command.ExecuteNonQuery();
                    transacao.Commit();
                }
                catch (Exception q)
                {
                    transacao.Rollback();
                    throw new ApplicationException(q.ToString());
                }
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

        public DataTable ExecuteReader(String sql)
        {
            try
            {
                connect.Open();
                transacao = connect.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Connection = connect;
                command.Transaction = transacao;
                command.CommandText = sql;
                try
                {
                    DataTable dt = new DataTable();
                    dt.Load(command.ExecuteReader());
                    transacao.Commit();
                    return dt;
                }
                catch (Exception q)
                {
                    transacao.Rollback();
                    throw new ApplicationException(q.ToString());
                }
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

        public Int32 ExecuteNonQuery(String sql, List<Persist> param)
        {
            try
            {
                connect.Open();
                transacao = connect.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Connection = connect;
                command.Transaction = transacao;
                command.CommandText = sql;
                int retorno;
                command.Parameters.Clear();

                try
                {
                    for (int i = 0; i < param.Count; i++)
                    {
                        DbType tipo = RetornaTipo(param[i].Type.Name.ToUpper());

                        parameters = command.CreateParameter();
                        parameters.ParameterName = "" + i;
                        parameters.DbType = tipo;
                        parameters.Value = param[i].ObjValor == null ? DBNull.Value : param[i].ObjValor;
                        command.Parameters.Add(parameters);

                    }
                    retorno = Convert.ToInt32(command.ExecuteScalar());
                    transacao.Commit();
                    return retorno;
                }
                catch (Exception q)
                {
                    transacao.Rollback();
                    throw new ApplicationException(q.ToString());
                }
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

        public DataTable ExecuteReader(String sql, List<Persist> param)
        {
            try
            {
                connect.Open();
                transacao = connect.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Connection = connect;
                command.Transaction = transacao;
                command.CommandText = sql;
                command.Parameters.Clear();

                try
                {

                    for (int i = 0; i < param.Count; i++)
                    {
                        if (param[i].ObjValor != null)
                        {
                            DbType tipo = RetornaTipo(param[i].Type.Name.ToUpper());
                            parameters = command.CreateParameter();
                            parameters.ParameterName = "@" + i;
                            parameters.DbType = tipo;
                            parameters.Value = param[i].ObjValor == null ? DBNull.Value : param[i].ObjValor;
                            command.Parameters.Add(parameters);
                        }
                    }
                    DataTable dt = new DataTable();
                    dt.Load(command.ExecuteReader());
                    transacao.Commit();
                    return dt;
                }
                catch (Exception q)
                {
                    transacao.Rollback();
                    throw new ApplicationException(q.ToString());
                }
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

        private IDbConnection Conexao(String banco)
        {
            switch (banco.ToUpper())
            {
                case "POSTGRESQL":
                    return new NpgsqlConnection(ConfigurationManager.ConnectionStrings["YLibrary_Postgresql"] == null ? ConfigurationSettings.AppSettings["YLibrary_Postgresql"] : ConfigurationManager.ConnectionStrings["YLibrary_Postgresql"].ConnectionString);
                case "SQLSERVER":
                    return new SqlConnection(ConfigurationManager.ConnectionStrings["YLibrary_Sqlserver"] == null ? ConfigurationSettings.AppSettings["YLibrary_Sqlserver"] : ConfigurationManager.ConnectionStrings["YLibrary_Sqlserver"].ConnectionString);
                case "ORACLE":
                    return new OracleConnection(ConfigurationManager.ConnectionStrings["YLibrary_Oracle"] == null ? ConfigurationManager.AppSettings["YLibrary_Oracle"] : ConfigurationManager.ConnectionStrings["YLibrary_Oracle"].ConnectionString);
                default:
                    return null;
            }
        }

        private IDbCommand Command(String banco)
        {
            switch (banco.ToUpper())
            {
                case "POSTGRESQL":
                    return new NpgsqlCommand();
                case "SQLSERVER":
                    return new SqlCommand();
                case "ORACLE":
                    return new OracleCommand();
                default:
                    return null;
            }
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
        /// Verifica o tipo de campo para retornar sua criação no banco de dados.
        /// </summary>
        /// <returns></returns>
        public String DbTipo(String campo, YConfiguracao configuracao)
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
    }
}
