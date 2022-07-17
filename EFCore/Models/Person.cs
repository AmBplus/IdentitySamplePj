using System.ComponentModel.DataAnnotations;

namespace EFCore.Models;

public class Person
{
    [Key]
    public int id { get; set; }
    [Required]
    public string Name { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
}