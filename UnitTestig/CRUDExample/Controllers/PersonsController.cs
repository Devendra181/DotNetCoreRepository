using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CRUDExample.Controllers
{
    public class PersonsController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountryService _countryService;
        public PersonsController(IPersonsService personsService, ICountryService countryService)
        {
            _personsService = personsService;
            _countryService = countryService;
        }

        [Route("Persons/Index")]
        [Route("/")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PersonResponse.PersonName), "Person Name"},
                {nameof(PersonResponse.Email), "Email"},
                {nameof(PersonResponse.DateOfBirth), "Date Of Birth"},
                {nameof(PersonResponse.CountryID), "Country ID"},
                {nameof(PersonResponse.Address), "Address"},
                {nameof(PersonResponse.Gender), "Gender"},
            };
            List<PersonResponse> personResponses = await _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            // List<PersonResponse> personResponses = _personsService.GetAllPersons();

            List<PersonResponse> sortedPersonResponses = await _personsService.GetSortedPersons(personResponses, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersonResponses); //Views/Persons/Index.cshtml
        }

        //Executes when the user clicks on "Create Person" link while opening the create view
        [HttpGet]
        [Route("Persons/Create")]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countryService.GetAllCountry();

            ViewBag.Countries = countries.Select(temp => new SelectListItem()
            {
                Text = temp.CountryName,
                Value = temp.CountryID.ToString()
            });
            //new SelectListItem() { Text = "Harsha", Value = "1" };
            //<option value="1">Harsha</option >

            return View(); //Views/Persons/Create.cshtml
        }

        [HttpPost]
        [Route("Persons/Create")]
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countryService.GetAllCountry();
                ViewBag.Countries = countries.Select(temp =>
        new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            //Call the service method to add person
            PersonResponse personResponse1 = await _personsService.AddPerson(personAddRequest);

            //navigate to Index() action method (it makes another get request to "/Persons/Index")
            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")] //Eg: /Persons/Details/1
        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonsByPersonID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            List<CountryResponse> countries = await _countryService.GetAllCountry();
            ViewBag.Countries = countries.Select(temp =>
    new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonsByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                PersonResponse? updatedPersonResponse = await _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
            else 
            {
                List<CountryResponse> countries = await _countryService.GetAllCountry();
                ViewBag.Countries = countries.Select(temp =>
        new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(personResponse.ToPersonUpdateRequest());
            }
                
        }

        [HttpGet]
        [Route("[action]/{personID}")] //Eg: /Persons/Details/1
        public async Task<IActionResult> Delete(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonsByPersonID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            return View(personResponse); //Views/Persons/Details.cshtml
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonsByPersonID(personUpdateRequest.PersonID);
            if (personResponse != null)
            {
                bool isDeleted = await _personsService.DeletePerson(personResponse.PersonID);
            }

            return RedirectToAction("Index");
        }

        [Route("PersonsPDF")]
        public async Task<IActionResult> PersonsPDF()
        {
            //Get list of persons
            List<PersonResponse> persons = await _personsService.GetAllPersons();

            //Return View as pdf
            return new ViewAsPdf("PersonsPDF", persons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins()
                {
                    Top = 20,
                    Right = 20,
                    Bottom = 20,
                    Left = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("PersonsCSV")]
        public async Task<IActionResult> PersonsCSV()
        {
            MemoryStream memoryStream =  await _personsService.GetPersonsCSV();
            return File(memoryStream, "application/octet-stream", "persons.csv");
        }


        [Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }
    }
}
