using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;

        public PersonsRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Person> AddPerson(Person person)
        {
            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return person;
        }

        public async Task<bool> DeletePersonByPersonID(Guid personID)
        {
            //_db.Persons.Remove(_db.Persons.First(temp => temp.PersonID == personID));

            _db.Persons.RemoveRange(_db.Persons.Where(temp => temp.PersonID == personID));
            int rowsAffected = await _db.SaveChangesAsync();

            return rowsAffected > 0;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            return await _db.Persons.Include(nameof(Person.Country)).ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            return await _db.Persons.Include(nameof(Person.Country))
                .Where(predicate).ToListAsync();
        }

        public async Task<Person?> GetPersonByPersonID(Guid personID)
        {
            //return await _db.Persons.Include(nameof(Person.Country))
            //    .Where(temp => temp.PersonID == personID).FirstOrDefaultAsync();

            return await _db.Persons.Include(nameof(Person.Country))
                .FirstOrDefaultAsync(temp => temp.PersonID == personID);
        }

        public async Task<Person?> GetPersonByPersonName(string PersonName)
        {
            return await _db.Persons.Include(nameof(Person.Country))
                .FirstOrDefaultAsync(temp => temp.PersonName == PersonName);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            Person? matchingPerosn = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == person.PersonID);

            if (matchingPerosn == null)
                return person;

            matchingPerosn.PersonName = person.PersonName;
            matchingPerosn.Email = person.Email;
            matchingPerosn.Gender = person.Gender;
            matchingPerosn.CountryID = person.CountryID;
            matchingPerosn.Address = person.Address;
            matchingPerosn.ReceiveNewsLetters = person.ReceiveNewsLetters;

            int countOfRowsAffected = await _db.SaveChangesAsync();

            return matchingPerosn;
        }
    }
}
