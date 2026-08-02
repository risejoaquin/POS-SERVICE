using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PosBuilder
{
    public partial class MainWindow : Window
    {
        private const string DPAPI_PREFIX = "DPAPI:";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                TxtLogoPath.Text = dialog.FileName;
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ImgPreview.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la imagen: " + ex.Message);
                }
            }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtStoreName.Text))
            {
                MessageBox.Show("El nombre del comercio es requerido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(TxtApiBaseUrl.Text) || !Uri.TryCreate(TxtApiBaseUrl.Text, UriKind.Absolute, out Uri apiUri))
            {
                MessageBox.Show("La API Base URL es inválida. Debe ser una URL completa (ej. https://api.midominio.com/).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (apiUri != null && apiUri.Scheme != Uri.UriSchemeHttps && !apiUri.IsLoopback)
            {
                MessageBox.Show("La API Base URL debe usar HTTPS por seguridad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync(TxtApiBaseUrl.Text);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch
            {
                var result = MessageBox.Show("No se pudo conectar a la API. ¿Desea continuar de todos modos?", "Advertencia de Conexión", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            if (string.IsNullOrWhiteSpace(TxtEmployeeUsername.Text) || TxtEmployeeUsername.Text.Length < 3 || !Regex.IsMatch(TxtEmployeeUsername.Text, "^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show("El username del empleado es inválido. Debe tener al menos 3 caracteres alfanuméricos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtEmployeePin.Text) || TxtEmployeePin.Text.Length < 4)
            {
                MessageBox.Show("El PIN del empleado debe tener al menos 4 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(TxtPort.Text, out int parsedPort) || parsedPort < 1 || parsedPort > 65535)
            {
                MessageBox.Show("El puerto debe ser un número válido entre 1 y 65535.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = TxtDatabaseUrl.Text };
                if (!builder.ContainsKey("Host") || !builder.ContainsKey("Database"))
                {
                    MessageBox.Show("La cadena de conexión debe contener al menos Host y Database.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("La cadena de conexión (dbUrl) es inválida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(CmbPrimaryColor.Text) || !Regex.IsMatch(CmbPrimaryColor.Text, "^#[0-9A-Fa-f]{6}"))
            {
                MessageBox.Show("El color primario debe ser un código HEX válido (ej. #1976D2).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtAdminUsername.Text) || TxtAdminUsername.Text.Length < 3 || !Regex.IsMatch(TxtAdminUsername.Text, "^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show("El username del admin es inválido. Debe tener al menos 3 caracteres alfanuméricos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtAdminPin.Text) || TxtAdminPin.Text.Length < 4)
            {
                MessageBox.Show("El PIN del admin debe tener al menos 4 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtJwtIssuer.Text) || string.IsNullOrWhiteSpace(TxtJwtAudience.Text))
            {
                MessageBox.Show("Issuer y Audience de JWT son requeridos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(PwdSecretKey.Password) || PwdSecretKey.Password.Length < 16)
            {
                MessageBox.Show("La clave secreta JWT debe tener al menos 16 caracteres para ser segura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }



            BtnGenerate.IsEnabled = false;
            TxtLog.Text = "Iniciando proceso de empaquetado del POS Cliente...\n";

            try
            {
                await Task.Run(() => ProcessGeneration());
                MessageBox.Show("¡Generación del instalador completada con éxito!\nRevisa la consola para ver la ruta.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR: " + ex.Message);
                MessageBox.Show("Ocurrió un error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerate.IsEnabled = true;
            }
        }

        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                TxtLog.Text += message + "\n";
                SvLog.ScrollToEnd();
            });
        }

        private string ExtractHexColor(string input)
        {
            var match = Regex.Match(input, @"#[0-9A-Fa-f]{6}");
            if (match.Success) return match.Value;
            return "#1976D2"; // default
        }

        private void ProcessGeneration()
        {
            string rootDir = AppDomain.CurrentDomain.BaseDirectory;
            while (rootDir != null && !Directory.Exists(Path.Combine(rootDir, "PosCore")))
            {
                rootDir = Directory.GetParent(rootDir)?.FullName;
            }
            if (rootDir == null) throw new Exception("No se encontró el directorio del proyecto.");
            string posCoreDir = Path.Combine(rootDir, "PosCore");
            if (!File.Exists(Path.Combine(posCoreDir, "build_and_package.ps1"))) throw new Exception("build_and_package.ps1 no encontrado.");

            AppendLog("Directorio del POS Cliente (PosCore): " + posCoreDir);

            // 1. Copy Logo
            string logoSource = string.Empty;
            Dispatcher.Invoke(() => logoSource = TxtLogoPath.Text);
            
            string logoDestPath = "";
            if (!string.IsNullOrEmpty(logoSource) && File.Exists(logoSource))
            {
                string assetsDir = Path.GetFullPath(Path.Combine(posCoreDir, "Assets"));
                if (!Directory.Exists(assetsDir))
                    Directory.CreateDirectory(assetsDir);
                
                logoDestPath = Path.GetFullPath(Path.Combine(assetsDir, "logo.png"));
                if (!logoDestPath.StartsWith(assetsDir))
                {
                    throw new Exception("Ruta de logo inválida (Path Traversal detectado).");
                }
                
                // Magic bytes validation
                byte[] buffer = new byte[4];
                using (var fs = new FileStream(logoSource, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }
                string hex = BitConverter.ToString(buffer).Replace("-", "");
                if (!hex.StartsWith("89504E47") && !hex.StartsWith("FFD8FF"))
                {
                    throw new Exception("El archivo seleccionado no es un PNG o JPG válido.");
                }

                File.Copy(logoSource, logoDestPath, true);
                AppendLog("Logo personalizado copiado exitosamente.");
            }

            // 2. Generate appsettings.json
            AppendLog("Generando configuración de appsettings.json...");
            string tenantId = "";
            string storeName = "";
            string primaryColor = "";
            string apiBaseUrl = "";
            string secretKey = "";
            
            string port = "";
            string dbUrl = "";
            string jwtIssuer = "";
            string jwtAudience = "";
            string adminUser = "";
            string adminPin = "";
            string empUser = "";
            string empPin = "";
            
            bool modCoupons = false, modLoyalty = false, modInventory = false;
            bool payCash = false, payCard = false, payTransfer = false;

            Dispatcher.Invoke(() =>
            {
                if (CmbTenants.SelectedItem is ComboBoxItem tenantItem)
                {
                    tenantId = tenantItem.Tag?.ToString() ?? "TENANT_001";
                }

                storeName = TxtStoreName.Text;
                
                // Color extraction
                string colorRaw = CmbPrimaryColor.Text;
                primaryColor = ExtractHexColor(colorRaw);

                apiBaseUrl = TxtApiBaseUrl.Text;
                if (!apiBaseUrl.EndsWith("/")) apiBaseUrl += "/";
                
                var secureSecretKey = PwdSecretKey.SecurePassword;
                var ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(secureSecretKey);
                try {
                    secretKey = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
                } finally {
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
                }
                PwdSecretKey.Clear();
                port = TxtPort.Text;
                dbUrl = TxtDatabaseUrl.Text;
                jwtIssuer = TxtJwtIssuer.Text;
                jwtAudience = TxtJwtAudience.Text;
                adminUser = TxtAdminUsername.Text;
                adminPin = TxtAdminPin.Text;
                empUser = TxtEmployeeUsername.Text;
                empPin = TxtEmployeePin.Text;

                modCoupons = ChkCoupons.IsChecked == true;
                modLoyalty = ChkLoyalty.IsChecked == true;
                modInventory = ChkInventory.IsChecked == true;

                payCash = ChkCash.IsChecked == true;
                payCard = ChkCard.IsChecked == true;
                payTransfer = ChkTransfer.IsChecked == true;
            });

            string finalSecretKey = string.IsNullOrEmpty(secretKey) ? "" : DPAPI_PREFIX + EncryptString(secretKey);

            var appSettings = new
            {
                ApiSettings = new
                {
                    BaseUrl = apiBaseUrl,
                    SecretKey = finalSecretKey
                },
                DatabaseSettings = new
                {
                    ConnectionString = "Data Source=pos_local.db"
                },
                WhiteLabel = new
                {
                    CompanyName = storeName,
                    PrimaryColor = primaryColor,
                    LogoPath = string.IsNullOrEmpty(logoDestPath) ? "" : "Assets/logo.png"
                },
                Modules = new
                {
                    EnableTableManagement = false,
                    EnableInventoryControl = modInventory,
                    EnableCoupons = modCoupons,
                    EnableLoyalty = modLoyalty
                },
                PaymentMethods = new
                {
                    EnableCash = payCash,
                    EnableCard = payCard,
                    EnableTransfer = payTransfer
                },
                Tenant = new
                {
                    CurrentTenantId = tenantId
                }
            };

            string settingsJson = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
            string appSettingsPath = Path.Combine(posCoreDir, "appsettings.json");
            File.WriteAllText(appSettingsPath, settingsJson);
            AppendLog("Archivo appsettings.json configurado correctamente para: " + storeName);

            // Generate backend .env file for Railway
            string envContent = $"PORT={port}\n" +
                                $"ConnectionStrings__DefaultConnection={dbUrl}\n" +
                                $"DATABASE_URL={dbUrl}\n" +
                                $"Jwt__Key={secretKey}\n" +
                                $"Jwt__Issuer={jwtIssuer}\n" +
                                $"Jwt__Audience={jwtAudience}\n";
            string envFilePath = Path.Combine(rootDir, "railway.env.example");
            File.WriteAllText(envFilePath, envContent.Replace(secretKey, "YOUR_SECRET_KEY").Replace(dbUrl, "YOUR_DB_URL"));
            AppendLog($"Plantilla de entorno para Railway generada en: {envFilePath} (Rellene los secretos manualmente)");

            // Generate tenant SQL seed
            string safeStoreName = storeName.Replace("'", "''");
            string safeTenantId = tenantId.Replace("'", "''");
            string safeAdminUser = adminUser.Replace("'", "''");
            string safeAdminPin = adminPin.Replace("'", "''");
            string safeEmpUser = empUser.Replace("'", "''");
            string safeEmpPin = empPin.Replace("'", "''");
            
            string tenantSql = $@"-- Initial users for {safeStoreName} ({safeTenantId})
INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Role"", ""TenantId"") VALUES 
('{safeAdminUser}', crypt('{safeAdminPin}', gen_salt('bf')), 'Admin', '{safeTenantId}'),
('{safeEmpUser}', crypt('{safeEmpPin}', gen_salt('bf')), 'Cajero', '{safeTenantId}')
ON CONFLICT DO NOTHING;
";
            string sqlFilePath = Path.Combine(rootDir, $"{tenantId}_seed.sql");
            File.WriteAllText(sqlFilePath, tenantSql);
            AppendLog($"Archivo de inicialización SQL generado en: {sqlFilePath}");


            // 3. Execute build_and_package.ps1
            string scriptPath = Path.Combine(posCoreDir, "build_and_package.ps1");
            if (!File.Exists(scriptPath))
            {
                AppendLog("ADVERTENCIA: No se encontró build_and_package.ps1. Verifica los archivos del proyecto.");
                return;
            }

            AppendLog("Ejecutando script de empaquetado (Squirrel)...");
            
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy RemoteSigned -File \"{scriptPath}\"",
                WorkingDirectory = Path.GetDirectoryName(scriptPath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                if (process == null) throw new Exception("No se pudo iniciar PowerShell.");

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) AppendLog(e.Data);
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) AppendLog("ERROR: " + e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"El script de PowerShell falló con código {process.ExitCode}");
                }
            }

            AppendLog("==========================================");
            AppendLog("¡Éxito! El instalador (Setup.exe) se encuentra en la carpeta Releases del proyecto.");
        }

        private static string EncryptString(string plainText)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                throw new PlatformNotSupportedException("Encryption is only supported on Windows.");

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
