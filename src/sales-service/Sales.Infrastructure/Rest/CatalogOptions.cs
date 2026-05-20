namespace Sales.Infrastructure.Rest;

/// <summary>
/// Settings for the REST client to <c>catalog-service</c>, bound from the host
/// configuration section <c>CatalogService</c>.
/// </summary>
public sealed class CatalogOptions
{
    public const string SectionName = "CatalogService";

    /// <summary>Base address of <c>catalog-service</c>'s REST API.</summary>
    public string BaseUrl { get; set; } = "http://localhost:5002";
}
