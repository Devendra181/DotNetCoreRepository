using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Person domain model
/// </summary>
public class Person
{
    [Key]
    public Guid PersonID { get; set; }

    //nvarchar(max) i.e it is string type with unicode support and max 2 billion characters per value 2^31-1 bytes i.e 2GB
    [StringLength(40)] //nvarchar(40) i.e it is string type with unicode support and max 40 characters per value 80 bytes i.e 40*2
    //[Required]
    public string? PersonName { get; set; }

    [StringLength(40)]
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    //unniqueidentifier i.e GUID type
    public Guid? CountryID { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    //bit i.e boolean type
    public bool? ReceiveNewsLetters { get; set; }

    public string? TIN { get; set; }

    [ForeignKey(nameof(CountryID))]
    public virtual Country? Country { get; set; }
}
