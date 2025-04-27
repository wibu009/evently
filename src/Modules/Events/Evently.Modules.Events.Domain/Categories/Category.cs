using Evently.Modules.Events.Domain.Abstractions;

namespace Evently.Modules.Events.Domain.Categories;

public sealed class Category : Entity
{
    private Category() { }
    
    public Guid Id { get; private init; }
    public string Name { get; private set; }
    public bool IsArchived { get; private set; }

    public static Category Create(string name)
    {
        var category = new Category
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            IsArchived = false
        };
        
        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id));
        
        return category; 
    }

    public void Archive()
    {
        IsArchived = true;
        
        RaiseDomainEvent(new CategoryArchivedDomainEvent(Id));
    }

    public void ChangeName(string name)
    {
        if (name == Name)
        {
            return;
        }
        
        Name = name;
        
        RaiseDomainEvent(new CategoryNameChangedDomainEvent(Id, name));
    }
}
