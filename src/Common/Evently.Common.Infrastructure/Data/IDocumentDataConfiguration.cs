namespace Evently.Common.Infrastructure.Data;

public interface IDocumentConfiguration<T>
{
    /// <summary>
    /// Configures the mapping of the document type <typeparamref name="T"/> to MongoDB fields.
    /// This method is called when the document store is being initialized and allows you to configure the mapping 
    /// of the document type to MongoDB fields. You can configure the mapping manually or use the <see cref="DocumentDataBuilder{T}"/> 
    /// class to automatically configure the mapping based on the document type properties.
    /// </summary>
    /// <param name="dataBuilder">The builder used to configure the mapping.</param>
    void Configure(DocumentDataBuilder<T> dataBuilder);
}
