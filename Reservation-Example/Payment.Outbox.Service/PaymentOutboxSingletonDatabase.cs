using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Outbox.Service
{
    public static class PaymentOutboxSingletonDatabase
    {
        static IDbConnection _connection;
        static bool _dataReaderState = true;

        static PaymentOutboxSingletonDatabase() =>
            _connection = new NpgsqlConnection("User ID=postgres;Password=123456;Host=localhost;Port=5432;Database=ReservationExamplePaymentDB;");

        public static IDbConnection Connection
        {
            get
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null) =>
            await Connection.QueryAsync<T>(sql, param);

        public static async Task<int> ExecuteAsync(string sql, object? param = null) =>
            await Connection.ExecuteAsync(sql, param);

        public static void DataReaderReady() =>
            _dataReaderState = true;

        public static void DataReaderBusy() =>
            _dataReaderState = false;

        public static bool DataReaderState => _dataReaderState;
    }
}
