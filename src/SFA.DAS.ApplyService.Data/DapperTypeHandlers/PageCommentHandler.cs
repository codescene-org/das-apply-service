using System.Collections.Generic;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Data.DapperTypeHandlers
{
    public class PageCommentHandler : SqlMapper.TypeHandler<PageComments>
    {
        public override PageComments Parse(object value)
        {
            return JsonConvert.DeserializeObject<PageComments>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, PageComments value)
        {
            parameter.Value = JsonConvert.SerializeObject(value);
        }
    }
}
