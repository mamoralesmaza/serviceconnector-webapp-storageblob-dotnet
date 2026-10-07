using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace WebStorageSample
{
    public class StorageHelper
    {
        // Método auxiliar para construir las credenciales de forma segura
        private static DefaultAzureCredential GetCredential()
        {
            return new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                // Evita que busque la IP de Managed Identity (169.254.169.254) cuando estás corriendo la app localmente
                ExcludeManagedIdentityCredential = true 
            });
        }

        // Método auxiliar para validar el Endpoint
        private static string EnsureEndpoint(string containerEndpoint)
        {
            if (string.IsNullOrWhiteSpace(containerEndpoint))
            {
                // Valor de respaldo para tu entorno local en caso de que launchSettings.json no lea la variable
                return "https://martinurl.blob.core.windows.net";
            }
            return containerEndpoint;
        }

        static public async Task UploadBlob(string containerEndpoint, string containerName, string blobName, string blobContents)
        {
            containerEndpoint = EnsureEndpoint(containerEndpoint);
            var blobContainerUri = new Uri(new Uri(containerEndpoint), containerName);
            
            BlobContainerClient containerClient = new BlobContainerClient(blobContainerUri, GetCredential());

            try
            {
                // Create the container if it does not exist.
                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload text to a new block blob.
                byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);

                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }
            catch (Exception)
            {
                throw; // Conserva el Stack Trace original (corrige advertencia CA2200)
            }
        }

        static public async Task<string> GetBlob(string containerEndpoint, string containerName, string blobName)
        {
            containerEndpoint = EnsureEndpoint(containerEndpoint);
            var blobContainerUri = new Uri(new Uri(containerEndpoint), containerName);
            
            BlobContainerClient containerClient = new BlobContainerClient(blobContainerUri, GetCredential());

            try
            {
                // Create the container if it does not exist.
                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                if (await blobClient.ExistsAsync())
                {
                    var response = await blobClient.DownloadAsync();
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var line = await streamReader.ReadLineAsync();
                            Console.WriteLine(line);
                            return line;
                        }
                    }
                }
                return "";
            }
            catch (Exception)
            {
                throw; // Conserva el Stack Trace original (corrige advertencia CA2200)
            }
        }
    }
}