using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonsService _personsService;
    private readonly ICountryService _countryService;

    private readonly Mock<IPersonsRepository> _personsRepositoryMock;
    private readonly IPersonsRepository _personsRepository;

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFixture _fixture;


    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();
        _testOutputHelper = testOutputHelper;

        _personsRepositoryMock = new Mock<IPersonsRepository>();
        _personsRepository = _personsRepositoryMock.Object; //Mock Object(Repository)
        //_personsRepository = new PersonsRepository; //Actual Repository


        var countriesInitialData = new List<Country>() { };
        var peronsInitialData = new List<Person>() { };

        DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        var dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
        dbContextMock.CreateDbSetMock(temp => temp.Persons, peronsInitialData);

        //_countryService = new CountriesService(dbContext);
        _countryService = new CountriesService(null);

        //_personsService = new PersonsService(dbContext, _countryService);
        _personsService = new PersonsService(_personsRepository);

        //_countryService = new CountriesService(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().Options));

        //_personsService = new PersonsService(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().Options), _countryService);
    }

    #region AddPerson tests

    //When we supply null PersonAddRequest to AddPerson method, it should throw ArgumentNullException
    [Fact]
    public async Task AddPerson_NullPerson_ToBeArgumentNullException()
    {
        // Arrange
        PersonAddRequest? personAddRequest = null;

        // Act & Assert
        //await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //{
        //    await _personsService.AddPerson(personAddRequest);
        //});

        Func<Task> action = (async () =>
        {
            await _personsService.AddPerson(personAddRequest);
        });

        //action?.Invoke();

        await action.Should().ThrowAsync<ArgumentNullException>();

    }


    //When we supply null PersonName to AddPerson method, it should throw ArgumentException
    [Fact]
    public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
    {
        // Arrange
        //PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = null };
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.PersonName, null as string).Create();

        // Act & Assert
        //await Assert.ThrowsAsync<ArgumentException>(async () =>
        //{
        //    await _personsService.AddPerson(personAddRequest);
        //});

        Person person = personAddRequest.ToPerson();
        //When PersonsRepository.AddPerson method is called with any Person object, it should return the same "person" object
        _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);

        Func<Task> action = (async () =>
        {
            await _personsService.AddPerson(personAddRequest);
        });

        await action.Should().ThrowAsync<ArgumentException>();
    }

    //When we supply proper PersonAddRequest to AddPerson method, it should return PersonResponse object
    [Fact]
    public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
    {
        // Arrange
        //PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = "Dev", Email = "dev12@gmail", Address = "test Add", CountryID = Guid.NewGuid(), Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"), ReceiveNewsLetters = true };

        // Arrange Use AutoFixture
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone@example.com")
            .Create();

        Person person = personAddRequest.ToPerson();
        PersonResponse person_response_expted = person.ToPersonResponse();
        //If we supply any argument value to the AddPerson method, it should return the same return value
        _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);

        // Act
        PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);
        person_response_expted.PersonID = person_response_from_add.PersonID;

        //List<PersonResponse> person_list = await _personsService.GetAllPersons();

        // Assert
        //Assert.True(person_response_from_add.PersonID != Guid.Empty);
        person_response_from_add.Should().NotBe(Guid.Empty);

        //Assert.Contains(person_response_from_add, person_list);
        //person_list.Should().Contain(person_response_from_add);

        person_response_from_add.Should().Be(person_response_expted);
    }

    #endregion

    #region GetPersonByPersonID tests

    //If we supply empty Guid to GetPersonByPersonID method, it should return null as PersonResponse

    [Fact]
    public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
    {
        // Arrange
        Guid? personID = null;
        // Act
        PersonResponse? person_response = await _personsService.GetPersonsByPersonID(personID);

        // Assert
        //Assert.Null(person_response);
        person_response.Should().BeNull();
    }


    //If we supply vallid PersonID, id should return PersonResponse object

    [Fact]
    public async Task GetPersonByPersonID_ValidPersonID_ToBeSuccessful()
    {
        // Arrange
        //CountryAddRequest? country_request = new CountryAddRequest() { CountryName = "India" };

        //CountryAddRequest? country_request = _fixture.Create<CountryAddRequest>();
        //CountryResponse country_response = await _countryService.AddCountry(country_request);


        //PersonAddRequest? person_request = new PersonAddRequest() { PersonName = "Dev", Email = "dev12@gmail", Address = "test Add", CountryID = Guid.NewGuid(), Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"), ReceiveNewsLetters = true };

        //PersonAddRequest? person_request = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone@example.com").Create();

        Person? person = _fixture.Build<Person>()
           .With(temp => temp.Email, "someone@example.com")
           .With(temp => temp.Country, null as Country)
           .Create();
           
        PersonResponse person_response_expted = person.ToPersonResponse();


        _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
            .ReturnsAsync(person);

        //Act

        //PersonResponse person_response_from_AddPerson = await _personsService.AddPerson(person_request);

        //PersonResponse? person_response_from_Get =  await _personsService.GetPersonsByPersonID(person_response_from_AddPerson.PersonID);

        PersonResponse? person_response_from_Get = await _personsService.GetPersonsByPersonID(person.PersonID);

        //Assert

        //Assert.Equal(person_response_from_AddPerson, person_response_from_Get);
        //person_response_from_Get.Should().Be(person_response_from_AddPerson);
        person_response_from_Get.Should().Be(person_response_expted);
    }
    #endregion

    #region GetAllPersons
    //The GetAllPersons() should return an empty list by default
    [Fact]
    public async Task GetAllPersons_EmptyList_ToBeSuccess()
    {
        //Arrange
        _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(new List<Person>());

        //Act
        List<PersonResponse> person_rsponses = await _personsService.GetAllPersons();

        //Assert
        //Assert.Empty(person_rsponses);
        person_rsponses.Should().BeEmpty();
    }

    //First , we add multiple persons using AddPerson() method; and then when we call GetAllPersons() method, it should return all the persons that were added

    [Fact]
    public async Task GetAllPersons_AddFewPersons()
    {
        //Arrange
        //CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "India" };
        //CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "USA" };

        //CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
        //CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();

        //CountryResponse countryResponse1 = await _countryService.AddCountry(countryAddRequest1);
        //CountryResponse countryResponse2 = await _countryService.AddCountry(countryAddRequest2);

        //PersonAddRequest? personAddRequest1 = new PersonAddRequest() { PersonName = "Dev1", Email = "dev12@gmail", Address = "test Add", CountryID = countryResponse1.CountryID, Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"), ReceiveNewsLetters = true };

        //PersonAddRequest? personAddRequest1 = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone1@example.com")
        //    .Create();
        //PersonAddRequest? personAddRequest2 = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone2@example.com")
        //    .Create();

        //PersonAddRequest? personAddRequest3 = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone3@example.com")
        //    .Create();

        List<Person> person = new List<Person>(){

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .Create()
         };

        //List<PersonAddRequest> personAddRequests = new List<PersonAddRequest>() { personAddRequest1, personAddRequest2, personAddRequest3 };

        //List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

        //foreach (PersonAddRequest personAddRequest in personAddRequests)
        //{
        //    PersonResponse person_response = await _personsService.AddPerson(personAddRequest);
        //    person_response_list_from_add.Add(person_response);
        //}

        List<PersonResponse> person_response_list_from_expected = person.Select(temp => temp.ToPersonResponse()).ToList();

        //print person_list_from_add
        _testOutputHelper.WriteLine("Exptexted:");
        //foreach (var person_response_from_add in person_response_list_from_add)
        //{
        //    _testOutputHelper.WriteLine(person_response_from_add.ToString());
        //}

        foreach (var person_response_from_add in person_response_list_from_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }


        _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(person);

        //Act
        List<PersonResponse> persons_list_from_get = await _personsService.GetAllPersons();
        _testOutputHelper.WriteLine("Actual:");
        foreach (var persons_from_get in persons_list_from_get)
        {
            _testOutputHelper.WriteLine(persons_from_get.ToString());
        }

        //Assert
        //foreach (PersonResponse person_response_from_add in person_response_list_from_add)
        //{
        //    Assert.Contains(person_response_from_add, persons_list_from_get);
        //}

        //persons_list_from_get.Should().BeEquivalentTo(person_response_list_from_add);

        persons_list_from_get.Should().BeEquivalentTo(person_response_list_from_expected);
    }
    #endregion

    #region GetFilteredPersons
    //If the search text is empty and search by is "PersonName", then GetFilteredPersons() should return all persons
    [Fact]
    public async Task GetAllPersons_EmptySearchText_ToBeSuccess()
    {
        //Arrange
        //CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
        //CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();

        //CountryResponse countryResponse1 = await _countryService.AddCountry(countryAddRequest1);
        //CountryResponse countryResponse2 = await _countryService.AddCountry(countryAddRequest2);

        //PersonAddRequest? personAddRequest1 = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone1@example.com")
        //    .Create();
        //PersonAddRequest? personAddRequest2 = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone2@example.com")
        //    .Create();

        //PersonAddRequest? personAddRequest3 = _fixture.Build<PersonAddRequest>()
        //    .With(temp => temp.Email, "someone3@example.com")
        //    .Create();

        //List<PersonAddRequest> personAddRequests = new List<PersonAddRequest>() { personAddRequest1, personAddRequest2, personAddRequest3 };
        //List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

        List<Person> person = new List<Person>(){

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .Create()
         };

        //foreach (PersonAddRequest personAddRequest in personAddRequests)
        //{
        //    PersonResponse person_response = await _personsService.AddPerson(personAddRequest);
        //    person_response_list_from_add.Add(person_response);
        //}

        List<PersonResponse> person_response_list_from_expected = person.Select(temp => temp.ToPersonResponse()).ToList();

        _testOutputHelper.WriteLine("Exptexted:");
    
        foreach (var person_response_from_add in person_response_list_from_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(person);

        //print person_list_from_add
        //_testOutputHelper.WriteLine("Exptexted:");
        //foreach (var person_response_from_add in person_response_list_from_add)
        //{
        //    _testOutputHelper.WriteLine(person_response_from_add.ToString());
        //}

        //Act
        List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");
        _testOutputHelper.WriteLine("Actual:");
        foreach (var persons_from_search in persons_list_from_search)
        {
            _testOutputHelper.WriteLine(persons_from_search.ToString());
        }

        //Assert
        //foreach (PersonResponse person_response_from_add in person_response_list_from_add)
        //{
        //    Assert.Contains(person_response_from_add, persons_list_from_search);
        //}
        //persons_list_from_search.Should().BeEquivalentTo(person_response_list_from_add);
        persons_list_from_search.Should().BeEquivalentTo(person_response_list_from_expected);

    }

    //First we will add few persons; and then we will search based on PersonName using GetFilteredPersons() method; it should return the list of persons whose names contain the search text
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName_ToBeSccessful()
    {
        List<Person> person = new List<Person>(){

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .Create()
         };

        List<PersonResponse> person_response_list_from_expected = person.Select(temp => temp.ToPersonResponse()).ToList();

        _testOutputHelper.WriteLine("Exptexted:");

        foreach (var person_response_from_add in person_response_list_from_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(person);

        //Act
        List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");
        _testOutputHelper.WriteLine("Actual:");
        foreach (var persons_from_search in persons_list_from_search)
        {
            _testOutputHelper.WriteLine(persons_from_search.ToString());
        }

        persons_list_from_search.Should().BeEquivalentTo(person_response_list_from_expected);
    }
    #endregion

    #region GetSortedPersons
    //

    [Fact]
    public async Task GetSortedPersons()
    {
        List<Person> person = new List<Person>(){

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .Create()
         };

        List<PersonResponse> person_response_list_from_expected = person.Select(temp => temp.ToPersonResponse()).ToList();

        _personsRepositoryMock
                   .Setup(temp => temp.GetAllPersons())
                   .ReturnsAsync(person);


        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response_from_add in person_response_list_from_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        List<PersonResponse> allPersons = await _personsService.GetAllPersons();

        //Act
        List<PersonResponse> persons_list_from_sort = await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

        //print persons_list_from_get
        _testOutputHelper.WriteLine("Actual:");
        foreach (PersonResponse person_response_from_get in persons_list_from_sort)
        {
            _testOutputHelper.WriteLine(person_response_from_get.ToString());
        }

        //Assert
        persons_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
    }
    #endregion


    #region UpdatePerson

    //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
    [Fact]
    public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
    {
        //Arrange
        PersonUpdateRequest? person_update_request = null;

        //Act
        Func<Task> action = async () =>
        {
            await _personsService.UpdatePerson(person_update_request);
        };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }


    //When we supply invalid person id, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
    {
        //Arrange
        PersonUpdateRequest? person_update_request = _fixture.Build<PersonUpdateRequest>()
         .Create();

        //Act
        Func<Task> action = async () =>
        {
            await _personsService.UpdatePerson(person_update_request);
        };

        //Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }


    //When PersonName is null, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
    {
        //Arrange
        Person person = _fixture.Build<Person>()
         .With(temp => temp.PersonName, null as string)
         .With(temp => temp.Email, "someone@example.com")
         .With(temp => temp.Country, null as Country)
         .With(temp => temp.Gender, "Male")
         .Create();

        PersonResponse person_response_from_add = person.ToPersonResponse();

        PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();


        //Act
        var action = async () =>
        {
            await _personsService.UpdatePerson(person_update_request);
        };

        //Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }


    //First, add a new person and try to update the person name and email
    [Fact]
    public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
    {
        //Arrange
        Person person = _fixture.Build<Person>()
         .With(temp => temp.Email, "someone@example.com")
         .With(temp => temp.Country, null as Country)
         .With(temp => temp.Gender, "Male")
         .Create();

        PersonResponse person_response_expected = person.ToPersonResponse();

        PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

        _personsRepositoryMock
         .Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
         .ReturnsAsync(person);

        _personsRepositoryMock
         .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
         .ReturnsAsync(person);

        //Act
        PersonResponse person_response_from_update = await _personsService.UpdatePerson(person_update_request);

        //Assert
        person_response_from_update.Should().Be(person_response_expected);
    }

    #endregion


    #region DeletePerson

    //If you supply an valid PersonID, it should return true
    [Fact]
    public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
    {
        //Arrange
        Person person = _fixture.Build<Person>()
         .With(temp => temp.Email, "someone@example.com")
         .With(temp => temp.Country, null as Country)
         .With(temp => temp.Gender, "Female")
         .Create();


        _personsRepositoryMock
         .Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
         .ReturnsAsync(true);

        _personsRepositoryMock
         .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
         .ReturnsAsync(person);

        //Act
        bool isDeleted = await _personsService.DeletePerson(person.PersonID);

        //Assert
        isDeleted.Should().BeTrue();
    }


    //If you supply an invalid PersonID, it should return false
    [Fact]
    public async Task DeletePerson_InvalidPersonID()
    {
        //Act
        bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

        //Assert
        isDeleted.Should().BeFalse();
    }

    #endregion
}