with open("PosBuilder/MainWindow.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

old_code = """                var modal = new SuccessModal(outputDir, creds);
                modal.Owner = this;
                modal.ShowDialog();
                
                Close();"""

new_code = """                var modal = new SuccessModal(outputDir, creds);
                modal.Owner = this;
                modal.ShowDialog();
                
                try
                {
                    string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); }
                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); }

                    string serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosServer"));
                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosServer")); }
                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosServer")); }

                    if (System.IO.Directory.Exists(serverPath))
                    {
                        var serverProcess = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"run --project \"{serverPath}\"",
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(serverProcess);
                    }
                    
                    string clientExe = System.IO.Path.Combine(outputDir, "PosClient", "PosCore.exe");
                    if (System.IO.File.Exists(clientExe))
                    {
                        var clientProcess = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = clientExe,
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Path.Combine(outputDir, "PosClient")
                        };
                        System.Diagnostics.Process.Start(clientProcess);
                    }
                    else if (System.IO.Directory.Exists(corePath))
                    {
                         var clientFallbackProcess = new System.Diagnostics.ProcessStartInfo
                         {
                             FileName = "dotnet",
                             Arguments = $"run --project \"{corePath}\"",
                             UseShellExecute = true
                         };
                         System.Diagnostics.Process.Start(clientFallbackProcess);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error al iniciar las aplicaciones: " + ex.Message, "Ejecución Automática", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                
                Close();"""

if old_code in content:
    content = content.replace(old_code, new_code)
    with open("PosBuilder/MainWindow.xaml.cs", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced successfully!")
else:
    print("Could not find old code.")
