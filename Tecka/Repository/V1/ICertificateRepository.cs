using System.Threading.Tasks;

namespace Tecka.Repository.V1
{
    public interface ICertificateRepository
    {
        Task AddCertificate(string certificate);

        Task AddCertificates(string[] certificates);

        Task DeleteCertificate(string certificate);

        Task<string[]> GetCertificates();
    }
}
