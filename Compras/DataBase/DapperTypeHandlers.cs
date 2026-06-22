using Dapper;
using System;
using System.Data;

namespace Compras
{
    public static class DapperTypeHandlers
    {
        private static bool configured;

        public static void Configure()
        {
            if (configured)
                return;

            SqlMapper.AddTypeHandler(new DateTimeHandler());
            SqlMapper.AddTypeHandler(new NullableDateTimeHandler());
            configured = true;
        }

        private sealed class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
        {
            public override DateTime Parse(object value) => value switch
            {
                DateOnly date => date.ToDateTime(TimeOnly.MinValue),
                DateTime dateTime => dateTime,
                _ => Convert.ToDateTime(value)
            };

            public override void SetValue(IDbDataParameter parameter, DateTime value)
            {
                parameter.Value = value;
            }
        }

        private sealed class NullableDateTimeHandler : SqlMapper.TypeHandler<DateTime?>
        {
            public override DateTime? Parse(object value)
            {
                if (value is null or DBNull)
                    return null;

                return value switch
                {
                    DateOnly date => date.ToDateTime(TimeOnly.MinValue),
                    DateTime dateTime => dateTime,
                    _ => Convert.ToDateTime(value)
                };
            }

            public override void SetValue(IDbDataParameter parameter, DateTime? value)
            {
                parameter.Value = value.HasValue ? value.Value : DBNull.Value;
            }
        }
    }
}
