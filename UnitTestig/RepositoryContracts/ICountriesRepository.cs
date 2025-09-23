using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents data access logic for managing country-related data.
    /// </summary>
    public interface ICountriesRepository
    {
        Task<Country> AddCountry(Country country);
        Task<List<Country>> GetAllCountries();
        Task<Country?> GetCountryByCountryID(Guid countryID);
        Task<Country?> GetCountryByCountryName(String countryName);
    }
}
