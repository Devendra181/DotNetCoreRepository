using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System.Threading.Tasks;


namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountryService _countriesService;

    public CountriesServiceTest()
    {
        var countriesInitialData = new List<Country>();

        DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        var dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

        //_countriesService = new CountriesService(dbContext);
        _countriesService = new CountriesService(null);
    }

    #region AddCountry
    //When ContryAddRequest is null, ArgumentNullException should be thrown
    [Fact]
    public async Task AddCountry_NullCountry()
    {
        // Arrange
        CountryAddRequest? countryAddRequest = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _countriesService.AddCountry(countryAddRequest));
    }


    //When the CountryName is null or empty, ArgumentException should be thrown
    [Fact]
    public async Task AddCountry_CountryNameIsNull()
    {
        // Arrange
        CountryAddRequest? countryAddRequest = new CountryAddRequest() { CountryName = null };

        // Assert
       await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Act
            await _countriesService.AddCountry(countryAddRequest);
        });
    }

    //When the CountryName is duplicate, ArgumentException should be thrown
    [Fact]
    public async Task AddCountry_CountryNameIsDuplicate()
    {
        // Arrange
        CountryAddRequest? countryAddRequest1 = new CountryAddRequest() { CountryName = "USA" };
        CountryAddRequest? countryAddRequest2 = new CountryAddRequest() { CountryName = "USA" };

        // Assert
       await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Act
           await _countriesService.AddCountry(countryAddRequest1);
           await _countriesService.AddCountry(countryAddRequest2);
        });
    }

    //When the CountryName is valid, it should be added successfully

    [Fact]
    public async Task AddCountry_CountryNameIsValid()
    {
        // Arrange
        CountryAddRequest? countryAddRequest = new CountryAddRequest() { CountryName = "Japan" };

        // Act
        CountryResponse response = await _countriesService.AddCountry(countryAddRequest);

        // Assert
        Assert.True(response.CountryID != Guid.Empty);

    }

    [Fact]
    public async Task AddCountry_ProperCountryDetails()
    {
        // Arrange
        CountryAddRequest? countryAddRequest = new CountryAddRequest() { CountryName = "Japan" };

        // Act
        CountryResponse response = await _countriesService.AddCountry(countryAddRequest);
        List<CountryResponse> countries_from_GetAllCountry = await _countriesService.GetAllCountry();

        // Assert
        Assert.True(response.CountryID != Guid.Empty);
        Assert.Contains(response, countries_from_GetAllCountry);

    }
    #endregion

    #region GetAllCountries
    [Fact]
    //When there are no countries, the returned list should be empty
    public async Task GetAllCountries_EmptyList()
    {
        // Act
        List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountry();

        // Assert
        Assert.Empty(actual_country_response_list);
    }


    [Fact]
    public async Task GetAllCountries_AddFewCountries() 
    {

        //Arrange
        List<CountryAddRequest> country_requests_list = new List<CountryAddRequest>()
        {
            new CountryAddRequest() { CountryName="India"},
            new CountryAddRequest() { CountryName="USA"},
            new CountryAddRequest() { CountryName="UK"},
            new CountryAddRequest() { CountryName="Australia"},
            new CountryAddRequest() { CountryName="Canada"}
        };

        //Act
        List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();

        foreach (CountryAddRequest country_request in country_requests_list)
        {
            countries_list_from_add_country.Add(await _countriesService.AddCountry(country_request));
        }

        List<CountryResponse> actualContryResponseList = await _countriesService.GetAllCountry();

        //read all countries
        foreach (CountryResponse expected_country in countries_list_from_add_country)
        {

            Assert.Contains(expected_country, actualContryResponseList);
        }
    }
    #endregion

    #region GetCountryByCountryID

    [Fact]
    // If we supply null countryID, then the result should be null
    public async Task GetCountryByCountryID_NullCountryID()
    {
        // Arrange
        Guid? countryID = null;

        // Act
        CountryResponse? actual_country_response = await _countriesService.GetContryByCountryID(countryID);

        // Assert
        Assert.Null(actual_country_response);
    }

    [Fact]
    // If we supply valid countryID, then the result should be the corresponding country object

    public async Task GetCountryByCountryID_ValidCountryID()
    {
        // Arrange
        CountryAddRequest? country_add_request = new CountryAddRequest() { CountryName = "Japan" };
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

        // Act
        CountryResponse? country_response_from_get = await _countriesService.GetContryByCountryID(country_response_from_add.CountryID);

        // Assert
        Assert.Equal(country_response_from_add, country_response_from_get);
    }

    #endregion
}
