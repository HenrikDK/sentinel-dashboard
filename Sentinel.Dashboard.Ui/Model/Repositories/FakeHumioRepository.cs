using System.Text.Json;

namespace Sentinel.Dashboard.Ui.Model.Repositories;

public class FakeHumioRepository : IHumioRepository
{
    public IList<Issue> GetOverview(string environment)
    {
        return new List<Issue>
        {
            new Issue
            {
                Events24Hours = 2,
                Events30Days = 232,
                Id = "111111",
                ContainerImage = "https://hub.docker.com/henrikdk/misc-ui:20221103-88-sdf2as",
                Service = "misc-ui",
                ExceptionType = "Sql.Data.SomeException",
                FirstSeen = DateTime.Now.AddDays(-4),
                LastSeen = DateTime.Now.AddHours(3),
                MessageTemplate = "Something went wrong",
                SourceContext = "Misc.Ui.SomeClass.Get",
                RenderedMessage = "Something went wrong",
            },

            new Issue
            {
                Events24Hours = 12,
                Events30Days = 4425,
                Id = "222222",
                ContainerImage = "https://hub.docker.com/henrikdk/message-consumer:20221103-88-ks421s",
                Service = "message-consumer",
                ExceptionType = "Microsoft.HTTP.OtherException",
                FirstSeen = DateTime.Now.AddDays(-12),
                LastSeen = DateTime.Now.AddHours(6),
                MessageTemplate = "Explosion in machine room",
                SourceContext = "Message.Consumer.SomeClass.Get",
                RenderedMessage = "Explosion",
            },
            new Issue
            {
                Events24Hours = 32,
                Events30Days = 233,
                Id = "333333",
                ContainerImage = "https://hub.docker.com/henrikdk/pets-api:20221103-88-xsf2ds",
                Service = "pets-api",
                ExceptionType = "Sql.Data.SomeException",
                FirstSeen = DateTime.Now.AddDays(-14),
                LastSeen = DateTime.Now.AddMinutes(3),
                MessageTemplate = "Something else went wrong",
                SourceContext = "Pets.Api.SomeClass.Get",
                RenderedMessage = "Something else went wrong",
            },
            new Issue
            {
                Events24Hours = 1,
                Events30Days = 3,
                Id = "444444",
                ContainerImage = "https://hub.docker.com/henrikdk/lobster-worker:20221103-2-sfd123f",
                Service = "lobster-worker",
                ExceptionType = "Lobster.Worker.CustomException",
                FirstSeen = DateTime.Now.AddDays(-23),
                LastSeen = DateTime.Now.AddMinutes(3),
                MessageTemplate = "Something completely different went wrong",
                SourceContext = "Lobster.Worker.SomeClass.Get",
                RenderedMessage = "Something completely different went wrong",
            }
        };
    }
    
    public IList<TimeSeriesElement> GetActivity(string environment, string timeSpan)
    {
        var result = new List<TimeSeriesElement>();
        var tmp = GetActivityFromServer("misc-ui", new [] {0,0,42,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,44,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,23,21,0,0,0,0,0,0,0,0,0,0,0,0,0,0,48,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,39,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,40,0,0,0,0,0,0,0,0,0,0,0,0,0,0});
        result.AddRange(tmp);
        tmp = GetActivityFromServer("message-consumer",new [] {0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,1,2,2,0,2,1,0,0,0,0,0});
        result.AddRange(tmp);
        tmp = GetActivityFromServer("pets-api",new [] {0,0,0,0,0,0,0,0,61,0,0,0,0,0,1,1,0,0,1,1,1,0,0,0,0,1,0,0,0,0,1,0,103,156,0,0,0,0,296,2,0,0,0,0,0,0,0,0,0,51,60,59,60,59,60,59,39,1,0,0,0,0,1,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,91,7,0,122,237,123,344,7,346,443,0,0,0,0,0});
        result.AddRange(tmp);
        tmp = GetActivityFromServer("lobster-worker",new []{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0});
        result.AddRange(tmp);
        return result;
    }

    private IList<TimeSeriesElement> GetActivityFromServer(string service, int[] array)
    {
        var time = DateTime.Now.Date.AddDays(-1).AddHours(9);
        return array.Select((x, i) => new TimeSeriesElement
        {
            Bucket = time.AddMinutes(i * 15),
            Count = x,
            Name = service
        }).ToList();
    }
    
    public IList<TimeSeriesElement> GetIssueActivity(string environment, string timeSpan, Issue issue)
    {   
        var rnd = new Random();
        var slots = timeSpan.EndsWith("hours") ? 24 : 30;
        var tmp = Enumerable.Range(0, slots).ToArray();
        var time = slots == 24 ? DateTime.Now.Date.AddDays(-1).AddHours(9) : DateTime.Now.Date.AddDays(-30);

        return tmp.Select((x, i) => new TimeSeriesElement
        {
            Bucket = slots == 24 ? time.AddHours(1 * i) : time.AddDays(1 * i),
            Count = rnd.Next(100),
            Name = issue.Service
        }).ToList();
    }
    
    public IList<string> GetEnvironments()
    {
        return new List<string> {"app", "app-dev", "app-tst", "app-uat"};
    }
    
    public IList<Error> GetErrorIndex(string environment, Issue issue)
    {
        var tmp = """
{"Timestamp":"2022-11-26T16:46:38.8513590+01:00",
"Level":"Error",
"MessageTemplate":"Connection id \"{ConnectionId}\", Request id \"{TraceIdentifier}\": An unhandled exception was thrown by the application.",
"RenderedMessage":"Connection id \"\"0HMMFOLBU9KEP\"\", Request id \"\"0HMMFOLBU9KEP:00000005\"\": An unhandled exception was thrown by the application.",
"Exception":"System.InvalidOperationException: The exception handler configured on ExceptionHandlerOptions produced a 404 status response. This InvalidOperationException containing the original exception was thrown since this is often due to a misconfigured ExceptionHandlingPath. If the exception handler is expected to return 404 status responses then set AllowStatusCode404Response to true.\n ---> System.AggregateException: Wak Wak Wak\n   at Sentinel.Dashboard.Ui.Model.Repositories.FakioRepository.GetActivity(String environment, String timeSpan) in /Users/henrik/Documents/git/sentinel-dashboard/src/Sentinel.Dashboard.Ui/Model/Repositories/FakioRepository.cs:line 67\n   at Sentinel.Dashboard.Ui.Pages.OverviewModel.OnGetActivity(String environment, String timeSpan) in /Users/henrik/Documents/git/work/sentinel-dashboard/src/Sentinel.Dashboard.Ui/Pages/Overview.cshtml.cs:line 16\n   at lambda_method7(Closure, Object, Object[])\n   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.ExecutorFactory.ActionResultHandlerMethod.Execute(Object receiver, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageActionInvoker.InvokeHandlerMethodAsync()\n   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageActionInvoker.InvokeNextPageFilterAsync()\n   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageActionInvoker.Rethrow(PageHandlerExecutedContext context)\n   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageActionInvoker.InvokeInnerFilterAsync()\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()\n--- End of stack trace from previous location ---\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\n   at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.<Invoke>g__Awaited|8_0(ExceptionHandlerMiddlewareImpl middleware, HttpContext context, Task task)\n   --- End of inner exception stack trace ---\n   at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.HandleException(HttpContext context, ExceptionDispatchInfo edi)\n   at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.<Invoke>g__Awaited|8_0(ExceptionHandlerMiddlewareImpl middleware, HttpContext context, Task task)\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\n   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpProtocol.ProcessRequests[TContext](IHttpApplication`1 application)",
"Properties":{"ConnectionId":"0HMMFOLBU9KEP",
"TraceIdentifier":"0HMMFOLBU9KEP:00000005",
"EventId":{"Id":13,"Name":"ApplicationError"},
"SourceContext":"Microsoft.AspNetCore.Server.Kestrel","RequestId":"0HMMFOLBU9KEP:00000005","RequestPath":"/environments/app-tst/issues","ExceptionDetail":{"Type":"System.InvalidOperationException","HResult":-2146233079,"Message":"The exception handler configured on ExceptionHandlerOptions produced a 404 status response. This InvalidOperationException containing the original exception was thrown since this is often due to a misconfigured ExceptionHandlingPath. If the exception handler is expected to return 404 status responses then set AllowStatusCode404Response to true.","Source":"System.Private.CoreLib","TargetSite":"Void Throw()","InnerException":{"Type":"System.AggregateException","HResult":-2146233088,"Message":"Wak Wak Wak","Source":"Sentinel.Dashboard.Ui","TargetSite":"System.Collections.Generic.IList`1[Sentinel.Dashboard.Ui.Model.TimeSeriesElement] GetActivity(System.String, System.String)","InnerExceptions":[]}}}}
""";
       return Enumerable.Range(0, 20).Select(x =>
            new Error
            {
                Id = "dfxx21qe1sfsj3vk43jkdnjsdaf321",
                Created = DateTime.Now.ToString("O"),
                Size = "1238713",
                // error.data.message.Exception <-- stack trace
                Data = JsonSerializer.Serialize(new {message = tmp})
            }).ToList();
    }
}