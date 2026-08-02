                string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}";
                
                var modal = new SuccessModal(outputDir, creds);
                modal.Owner = this;
                modal.ShowDialog();
                
                Close();
            }
            else
            {
                NotificationService.Instance.ShowError("Error de integridad al generar los archivos.");
            }
        }
