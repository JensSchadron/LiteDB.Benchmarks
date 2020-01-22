using BenchmarkDotNet.Attributes;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace LiteDB.Benchmarks.Benchmarks.Base
{
    public abstract class DatabaseBenchmarkBase : BenchmarkBase
    {
        protected abstract string DatabasePath { get; }

        [Params(ConnectionType.Direct)]
        public ConnectionType ConnectionType;

        [Params(null, "SecurePassword")]
        public string Password;

        private ConnectionString _connectionString;
        protected ConnectionString ConnectionString => _connectionString ??= new ConnectionString(DatabasePath)
        {
            Connection = ConnectionType,
            Password = Password
        };
    }
}