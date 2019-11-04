using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace SFA.DAS.ApplyService.Data.DapperTypeHandlers
{
    public class GenericTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public override T Parse(object value)
        {
            return JsonConvert.DeserializeObject<T>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = JsonConvert.SerializeObject(value);
        }
    }
}
