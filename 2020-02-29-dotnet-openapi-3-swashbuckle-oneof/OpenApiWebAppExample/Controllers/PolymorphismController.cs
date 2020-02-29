using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenApiWebAppExample.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class PolymorphismController : ControllerBase
    {    
        private readonly ILogger<PolymorphismController> _logger;

        public PolymorphismController(ILogger<PolymorphismController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DynamicType>), StatusCodes.Status200OK)]
        public IEnumerable<DynamicType> Get() 
        {
          
            return Enumerable.Range(1, 5).Select(index => {
                if(index % 2 == 0)
                {
                    return new DynamicType<string>() 
                    { 
                        DataType="string", 
                        Value = "This is my string value" 
                    } as DynamicType;
                }
                 return new DynamicType<decimal?>() 
                 { 
                     DataType="number", 
                     Value = new decimal?(99.0m) 
                 } as DynamicType;
            })
            .ToArray();
        }
    }
}
