using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebStorageSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public string DisplayWords { get; private set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        // Se cambia 'public void OnGet()' a 'public async Task OnGetAsync()'
        public async Task OnGetAsync()
        {
            string endpoint = Environment.GetEnvironmentVariable(Const.ENDPOINT_ENV_KEY);
            
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception($"La variable de entorno '{Const.ENDPOINT_ENV_KEY}' no está configurada en Azure.");
            }

            string content = string.Format("Hello Service Connector! UTC Now: {0}.", DateTimeOffset.UtcNow.ToString());
            await StorageHelper.UploadBlob(endpoint, Const.CONTAINER_NAME, Const.BLOB_NAME, content);
            DisplayWords = await StorageHelper.GetBlob(endpoint, Const.CONTAINER_NAME, Const.BLOB_NAME);
        }
    }
}