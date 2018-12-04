using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace WebApiAuthSample
{
    public class AssignJwtSecurityRequirements : IOperationFilter
    {
        /// <summary>
        /// Swagger UI用のフィルタ。
        /// Swagger上でAPIを実行する際のJWTトークン認証対応を実現する。
        /// </summary>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Security == null)
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

            //AllowAnonymousが付いている場合は、アクセスコードを要求しない
            var allowAnonymousAccess = context.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(AllowAnonymousAttribute));
            
            if (allowAnonymousAccess == false)
            {
                var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
            {
                { "api_key", new List<string>() }
            };

                operation.Security.Add(oAuthRequirements);
            }
        }
    }
}
