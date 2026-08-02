using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;
using Npgsql;

namespace PosBuilder
{
    public static class ValidationRules
    {
        public static (bool isValid, string errorMessage) ValidateUsername(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "El nombre de usuario es requerido.");
            if (input.Length < 3 || input.Length > 50) return (false, "El usuario debe tener entre 3 y 50 caracteres.");
            if (!Regex.IsMatch(input, "^[a-zA-Z0-9_]+$")) return (false, "Solo se permiten caracteres alfanuméricos y guión bajo.");
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidatePin(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "El PIN es requerido.");
            if (input.Length < 4 || input.Length > 8) return (false, "El PIN debe tener entre 4 y 8 dígitos.");
            if (!Regex.IsMatch(input, "^[0-9]+$")) return (false, "El PIN debe contener solo números.");
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidatePort(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "El puerto es requerido.");
            if (!int.TryParse(input, out int port)) return (false, "El puerto debe ser un número válido.");
            if (port < 1024 || port > 65535) return (false, "El puerto debe estar entre 1024 y 65535 (puertos < 1024 están reservados).");
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateConnectionString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "La cadena de conexión es requerida.");
            try
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder { ConnectionString = input };
                if (string.IsNullOrEmpty(builder.Host) || string.IsNullOrEmpty(builder.Database))
                {
                    return (false, "La cadena debe contener al menos 'Host' y 'Database'.");
                }
                return (true, string.Empty);
            }
            catch
            {
                return (false, "La cadena de conexión tiene un formato inválido.");
            }
        }

        public static (bool isValid, string errorMessage) ValidateJwtKey(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "La clave JWT es requerida.");
            if (input.Length < 32) return (false, "La clave JWT debe tener al menos 32 caracteres.");
            if (Regex.IsMatch(input, @"[\x00-\x1F]")) return (false, "La clave JWT no debe contener caracteres de control.");
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateStoreName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "El nombre del comercio es requerido.");
            if (input.Length < 2 || input.Length > 100) return (false, "El nombre debe tener entre 2 y 100 caracteres.");
            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9\s\.\&\-]+$")) return (false, "El nombre contiene caracteres no permitidos (solo alfanuméricos, espacios, guiones, puntos y &).");
            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateLogoFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return (true, string.Empty); // Optional if empty initially, checked separately if required
            if (!File.Exists(path)) return (false, "El archivo de logo no existe.");
            
            try
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Length > 2 * 1024 * 1024) return (false, "El tamaño del archivo no puede exceder los 2MB.");

                byte[] buffer = new byte[4];
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }
                string hex = BitConverter.ToString(buffer).Replace("-", "");
                if (!hex.StartsWith("89504E47") && !hex.StartsWith("FFD8FF"))
                {
                    return (false, "El archivo seleccionado no es un PNG o JPG válido.");
                }

                // Resolucion mínima (opcional leer bytes exactos, pero la regla dice verificar. Usaremos Bitmap Decoder o solo magia bytes)
                // Para no depender de System.Drawing en .NET Core / WPF de forma compleja, confiaremos en los magic bytes y validaremos 200x200 si es posible.
                // En WPF, podemos intentar cargarlo. Aquí omitiremos resolución estricta si requiere dependencias adicionales o la haremos basica.
                // Resolucion minima 200x200
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(stream, 
                        System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation, 
                        System.Windows.Media.Imaging.BitmapCacheOption.None);
                    var frame = decoder.Frames[0];
                    if (frame.PixelWidth < 200 || frame.PixelHeight < 200)
                    {
                        return (false, "La resolución de la imagen debe ser al menos 200x200.");
                    }
                }
                
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Error al verificar el archivo: {ex.Message}");
            }
        }

        public static async Task<(bool isValid, string errorMessage)> ValidateApiUrlAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "La API Base URL es requerida.");
            if (!Uri.TryCreate(input, UriKind.Absolute, out Uri apiUri))
                return (false, "Debe ser una URL completa (ej. https://api.midominio.com/).");
            
            if (apiUri.Scheme != Uri.UriSchemeHttps && !apiUri.IsLoopback)
                return (false, "La API Base URL debe usar HTTPS por seguridad.");

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var healthUri = new Uri(apiUri, "/health");
                    var response = await client.GetAsync(healthUri);
                    // Just check if it responds (doesn't need to be 200 OK for /health if we just do a root ping, but usually /health is better)
                    // We'll just assume success if no exception is thrown
                }
            }
            catch
            {
                // Warn but maybe allow? The prompt says "intentar GET /health", we'll just fail validation if it fails?
                // For now, let's return true with a warning if needed, but since it's a validation error, we return false.
                // Let's return false for the sake of completeness. 
                return (false, "No se pudo conectar a la API (Timeout o error). Asegúrese de que esté en línea.");
            }

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateColor(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (false, "El color primario es requerido.");
            if (!Regex.IsMatch(input, "^#[0-9A-Fa-f]{6}$")) return (false, "El color debe ser un código HEX válido (ej. #1976D2).");
            return (true, string.Empty);
        }
    }
}
