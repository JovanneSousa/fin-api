using Microsoft.AspNetCore.Mvc;

namespace fin_api.Controllers
{
    [ApiController]
    [Route("api/transacoes")]
    public class TransacoesController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            return "Teste";
        }
    }
}
