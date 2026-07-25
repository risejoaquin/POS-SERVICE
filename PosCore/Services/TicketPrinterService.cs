using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PosCore.Models;
using Serilog;
using Microsoft.Extensions.Options;

namespace PosCore.Services
{
    public class TicketPrinterService
    {
        private readonly AppSettings _settings;

        public TicketPrinterService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        // ESC/POS Commands
        private static readonly byte[] ESC_INIT = new byte[] { 27, 64 };
        private static readonly byte[] ESC_ALIGN_CENTER = new byte[] { 27, 97, 1 };
        private static readonly byte[] ESC_ALIGN_LEFT = new byte[] { 27, 97, 0 };
        private static readonly byte[] ESC_BOLD_ON = new byte[] { 27, 69, 1 };
        private static readonly byte[] ESC_BOLD_OFF = new byte[] { 27, 69, 0 };
        private static readonly byte[] ESC_CUT = new byte[] { 29, 86, 66, 0 };

        public void PrintTicket(Order order, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) 
                {
                    Log.Warning("La impresión directa solo es compatible en Windows.");
                    return;
                }

                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, "Ticket de Venta\n");
                    WriteString(ms, $"Fecha: {order.OrderDate:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(ms, $"Ticket ID: {order.Id}\n");
                    WriteString(ms, "--------------------------------\n");
                    
                    ms.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    foreach (var item in order.Items)
                    {
                        string productName = item.Product?.Name ?? "Producto Indefinido";
                        if (productName.Length > 20) productName = productName.Substring(0, 20);
                        
                        string line = $"{item.Quantity}x {productName.PadRight(20)} {item.SubTotal.ToString("C").PadLeft(8)}\n";
                        WriteString(ms, line);
                    }
                    WriteString(ms, "--------------------------------\n");
                    
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"TOTAL: {order.TotalAmount.ToString("C")}\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    
                    WriteString(ms, "\n¡Gracias por su compra!\n\n\n\n\n\n");
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                    
                    byte[] dataToPrint = ms.ToArray();
                    bool success = RawPrinterHelper.SendBytesToPrinter(portName, dataToPrint);
                    
                    if (success)
                        Log.Information($"Ticket impreso exitosamente para la orden {order.Id} en la impresora {portName}");
                    else
                        Log.Error($"Error de WinSpool al enviar ticket de la orden {order.Id} a la impresora {portName}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al intentar imprimir el ticket en la impresora {portName}");
            }
        }

        public void PrintCreditNote(Order order, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    Log.Warning("La impresión directa solo es compatible en Windows.");
                    return;
                }

                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    WriteString(ms, "*** NOTA DE CREDITO ***\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(ms, $"Ref Ticket ID: {order.Id}\n");
                    WriteString(ms, "--------------------------------\n");
                    
                    ms.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    foreach (var item in order.Items)
                    {
                        string productName = item.Product?.Name ?? "Producto";
                        if (productName.Length > 20) productName = productName.Substring(0, 20);
                        
                        string line = $"{item.Quantity}x {productName.PadRight(20)} {item.SubTotal.ToString("C").PadLeft(8)}\n";
                        WriteString(ms, line);
                    }
                    WriteString(ms, "--------------------------------\n");
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"TOTAL DEVUELTO: {order.TotalAmount.ToString("C")}\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, "\nComprobante de devolucion\n\n\n\n\n\n");
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                    
                    byte[] dataToPrint = ms.ToArray();
                    bool success = RawPrinterHelper.SendBytesToPrinter(portName, dataToPrint);
                    
                    if (success)
                        Log.Information($"Nota de credito impresa exitosamente para la orden {order.Id} en la impresora {portName}");
                    else
                        Log.Error($"Error de WinSpool al enviar nota de credito de la orden {order.Id} a la impresora {portName}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al intentar imprimir la nota de credito en {portName}");
            }
        }

        
        public void TestPrinter(string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    Log.Warning("La impresión directa solo es compatible en Windows.");
                    return;
                }

                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, "\n*** PRUEBA DE IMPRESION ***\n\n");
                    WriteString(ms, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(ms, $"Impresora configurada: {portName}\n");
                    WriteString(ms, "--------------------------------\n");
                    
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    WriteString(ms, "Si puedes leer esto, la impresora\n");
                    WriteString(ms, "esta configurada correctamente.\n\n\n\n\n");
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                    
                    byte[] dataToPrint = ms.ToArray();
                    bool success = RawPrinterHelper.SendBytesToPrinter(portName, dataToPrint);
                    
                    if (success)
                        Log.Information($"Prueba de impresión exitosa en la impresora {portName}");
                    else
                        Log.Error($"Error de WinSpool al enviar prueba a la impresora {portName}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al intentar imprimir la prueba en {portName}");
            }
        }

        private void WriteString(MemoryStream ms, string text)
        {
            // Encoding 850 / UTF8 can be adjusted here if special characters appear wrong, 
            // but ASCII is safest for standard ESC/POS
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
        }
    }
}
