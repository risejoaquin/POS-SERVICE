using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosCore.Models;
using PosCore.Data;

namespace PosCore
{
    public enum ShortcutAction
    {
        None,
        Mostrador,
        Descuento,
        AdminPanel
    }

    public class BarcodeProcessor
    {
        private readonly PosDbContext _dbContext;
        private static List<DateTime> _keyTimestamps = new List<DateTime>();

        public BarcodeProcessor(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static void RegisterKeystroke()
        {
            _keyTimestamps.Add(DateTime.Now);
        }

        public static void ClearKeystrokes()
        {
            _keyTimestamps.Clear();
        }

        public async Task<bool> DetectBarcodeTiming(string input)
        {
            // mide tiempo entre caracteres, si < 50ms es barcode
            if (_keyTimestamps.Count < 2) return false;
            
            double totalMilliseconds = 0;
            for (int i = 1; i < _keyTimestamps.Count; i++)
            {
                totalMilliseconds += (_keyTimestamps[i] - _keyTimestamps[i - 1]).TotalMilliseconds;
            }
            
            double averageTime = totalMilliseconds / (_keyTimestamps.Count - 1);
            
            return await Task.FromResult(averageTime < 50);
        }

        public bool ValidateChecksum(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || (code.Length != 13 && code.Length != 8))
                return false;

            if (!code.All(char.IsDigit))
                return false;

            int sum = 0;
            int checkDigit = int.Parse(code.Substring(code.Length - 1));

            for (int i = 0; i < code.Length - 1; i++)
            {
                int digit = int.Parse(code.Substring(i, 1));
                if (code.Length == 13)
                {
                    sum += (i % 2 == 0) ? digit : digit * 3;
                }
                else // EAN-8
                {
                    sum += (i % 2 == 0) ? digit * 3 : digit;
                }
            }

            int expectedCheck = (10 - (sum % 10)) % 10;
            return checkDigit == expectedCheck;
        }

        public Product LookupProduct(string code)
        {
            // Búsqueda case-insensitive en caché local (simulado con la BD en este caso)
            if (string.IsNullOrWhiteSpace(code)) return null;
            var lowerCode = code.ToLower();
            return _dbContext.Products.FirstOrDefault(p => p.Barcode.ToLower() == lowerCode || p.Name.ToLower() == lowerCode);
        }

        public ShortcutAction ProcessShortcuts(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return ShortcutAction.None;
            var upperCode = code.ToUpper().Trim();
            
            if (upperCode == "M") return ShortcutAction.Mostrador;
            if (upperCode == "D") return ShortcutAction.Descuento;
            if (upperCode == "P") return ShortcutAction.AdminPanel;
            
            return ShortcutAction.None;
        }
    }
}
