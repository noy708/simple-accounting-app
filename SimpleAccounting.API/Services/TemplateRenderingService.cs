using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace SimpleAccounting.API.Services;

public class TemplateRenderingService : ITemplateRenderingService
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public TemplateRenderingService(
        IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        try
        {
            Console.WriteLine($"テンプレート検索開始: {templateName}");
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using var sw = new StringWriter();
            var viewResult = _razorViewEngine.FindView(actionContext, templateName, false);

            if (viewResult.View == null)
            {
                Console.WriteLine($"テンプレートが見つかりません: {templateName}");
                Console.WriteLine($"検索されたロケーション: {string.Join(", ", viewResult.SearchedLocations ?? new string[0])}");
                throw new ArgumentNullException($"Template '{templateName}' not found. Searched locations: {string.Join(", ", viewResult.SearchedLocations ?? new string[0])}");
            }

            Console.WriteLine($"テンプレートが見つかりました: {templateName}");

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            Console.WriteLine("テンプレートレンダリング実行開始");
            await viewResult.View.RenderAsync(viewContext);
            Console.WriteLine("テンプレートレンダリング実行完了");
            
            var result = sw.ToString();
            Console.WriteLine($"レンダリング結果の長さ: {result.Length}文字");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TemplateRenderingServiceでエラー: {ex.Message}");
            Console.WriteLine($"TemplateRenderingServiceスタックトレース: {ex.StackTrace}");
            throw;
        }
    }
}