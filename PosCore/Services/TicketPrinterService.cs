using System;
using System.Collections.Generic;
using System.IO.Ports;
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
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) return;

                using (var serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One))
                {
                    serialPort.Open();
                    serialPort.Write(ESC_INIT, 0, ESC_INIT.Length);
                    serialPort.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    serialPort.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(serialPort, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    serialPort.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(serialPort, "Ticket de Venta\n");
                    WriteString(serialPort, $"Fecha: {order.OrderDate:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(serialPort, $"Ticket ID: {order.Id}\n");
                    WriteString(serialPort, "--------------------------------\n");
                    
                    serialPort.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    foreach (var item in order.Items)
                    {
                        string productName = item.Product?.Name ?? "Producto Indefinido";
                        if (productName.Length > 20) productName = productName.Substring(0, 20);
                        
                        string line = $"{item.Quantity}x {productName.PadRight(20)} {item.SubTotal.ToString("C").PadLeft(8)}\n";
                        WriteString(serialPort, line);
                    }
                    WriteString(serialPort, "--------------------------------\n");
                    
                    serialPort.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    serialPort.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(serialPort, $"TOTAL: {order.TotalAmount.ToString("C")}\n");
                    serialPort.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    
                    WriteString(serialPort, "\n¡Gracias por su compra!\n\n\n\n");
                    serialPort.Write(ESC_CUT, 0, ESC_CUT.Length);
                    serialPort.Close();
                    
                    Log.Information($"Ticket impreso exitosamente para la orden {order.Id} en {portName}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al intentar imprimir el ticket en el puerto {portName}");
            }
        }

        public void PrintCreditNote(Order order, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) return;

                using (var serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One))
                {
                    serialPort.Open();
                    serialPort.Write(ESC_INIT, 0, ESC_INIT.Length);
                    serialPort.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    serialPort.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(serialPort, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    WriteString(serialPort, "*** NOTA DE CREDITO ***\n");
                    serialPort.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(serialPort, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(serialPort, $"Ref Ticket ID: {order.Id}\n");
                    WriteString(serialPort, "--------------------------------\n");
                    
                    serialPort.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    foreach (var item in order.Items)
                    {
                        string productName = item.Product?.Name ?? "Producto";
                        if (productName.Length > 20) productName = productName.Substring(0, 20);
                        
                        string line = $"{item.Quantity}x {productName.PadRight(20)} {item.SubTotal.ToString("C").PadLeft(8)}\n";
                        WriteString(serialPort, line);
                    }

                    WriteString(serialPort, "--------------------------------\n");
                    serialPort.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    serialPort.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(serialPort, $"TOTAL DEVUELTO: {order.TotalAmount.ToString("C")}\n");
                    serialPort.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(serialPort, "\nComprobante de devolucion\n\n\n\n");
                    serialPort.Write(ESC_CUT, 0, ESC_CUT.Length);
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al intentar imprimir la nota de credito en {portName}");
            }
        }

        private void WriteString(SerialPort port, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            port.Write(bytes, 0, bytes.Length);
        }
    }
}
