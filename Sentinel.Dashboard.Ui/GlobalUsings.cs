global using System;
global using System.Linq;
global using System.Numerics;
global using System.Collections.Generic;
global using System.Security.Cryptography;
global using System.Text;

global using Azure.Extensions.AspNetCore.Configuration.Secrets;
global using Azure.Identity;
global using Azure.Security.KeyVault.Secrets;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using Flurl;
global using Flurl.Http;
global using Lamar.Microsoft.DependencyInjection;
global using MoreLinq.Extensions;
global using Prometheus;
global using Serilog;
global using Serilog.Exceptions;
global using Serilog.Formatting.Json;
