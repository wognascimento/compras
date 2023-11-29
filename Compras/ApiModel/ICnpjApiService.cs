using Refit;
using System.Threading.Tasks;

namespace Compras.ApiModel
{
    public interface ICnpjApiService
    {
        [Get("")]
        Task<CnpjResponse> GetCnpjAsync(string cnpj);
    }
}
