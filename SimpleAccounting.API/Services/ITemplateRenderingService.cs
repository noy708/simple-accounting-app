namespace SimpleAccounting.API.Services;

public interface ITemplateRenderingService
{
    Task<string> RenderTemplateAsync<T>(string templateName, T model);
}