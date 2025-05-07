using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.Customers;

public sealed class Customer : Entity
{
    private Customer()
    {
    }
    
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    
    public static Customer Create(Guid id, string email, string firstName, string lastName) => 
        new()
        {
            Id = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
    
    public void Update(string firstName, string lastName) => 
        (FirstName, LastName) = (firstName, lastName);
}
