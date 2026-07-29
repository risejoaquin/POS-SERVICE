const fs = require('fs');
let content = fs.readFileSync('PosCore/Services/TicketPrinterService.cs', 'utf8');

// Change return types to bool
content = content.replace('public void PrintTicket', 'public bool PrintTicket');
content = content.replace('public void PrintCreditNote', 'public bool PrintCreditNote');
content = content.replace('public void TestPrinter', 'public bool TestPrinter');

// Fix return paths in PrintTicket
content = content.replace(
    'Log.Warning("La impresión directa solo es compatible en Windows.");\n                    return;',
    'Log.Warning("La impresión directa solo es compatible en Windows.");\n                    return false;'
);

content = content.replace(
    'if (success)\n                        Log.Information($"Ticket impreso exitosamente para la orden {order.Id} en la impresora {portName}");\n                    else\n                        Log.Error($"Error de WinSpool al enviar ticket de la orden {order.Id} a la impresora {portName}");\n                }',
    'if (success)\n                        Log.Information($"Ticket impreso exitosamente para la orden {order.Id} en la impresora {portName}");\n                    else\n                        Log.Error($"Error de WinSpool al enviar ticket de la orden {order.Id} a la impresora {portName}");\n                    return success;\n                }'
);

content = content.replace(
    'catch (Exception ex)\n            {\n                Log.Error(ex, $"Error al intentar imprimir el ticket en la impresora {portName}");\n            }',
    'catch (Exception ex)\n            {\n                Log.Error(ex, $"Error al intentar imprimir el ticket en la impresora {portName}");\n                return false;\n            }'
);

// Fix return paths in PrintCreditNote
content = content.replace(
    'Log.Warning("La impresión directa solo es compatible en Windows.");\n                    return;',
    'Log.Warning("La impresión directa solo es compatible en Windows.");\n                    return false;'
);

content = content.replace(
    'if (success)\n                        Log.Information($"Nota de credito impresa exitosamente para la orden {order.Id} en la impresora {portName}");\n                    else\n                        Log.Error($"Error de WinSpool al enviar nota de credito de la orden {order.Id} a la impresora {portName}");\n                }',
    'if (success)\n                        Log.Information($"Nota de credito impresa exitosamente para la orden {order.Id} en la impresora {portName}");\n                    else\n                        Log.Error($"Error de WinSpool al enviar nota de credito de la orden {order.Id} a la impresora {portName}");\n                    return success;\n                }'
);

content = content.replace(
    'catch (Exception ex)\n            {\n                Log.Error(ex, $"Error al intentar imprimir la nota de credito en {portName}");\n            }',
    'catch (Exception ex)\n            {\n                Log.Error(ex, $"Error al intentar imprimir la nota de credito en {portName}");\n                return false;\n            }'
);

// Fix return paths in TestPrinter
content = content.replace(
    'Log.Warning("La impresión directa solo es compatible en Windows.");\n                    return;',
    'Log.Warning("La impresión directa solo es compatible en Windows.");\n                    return false;'
);

content = content.replace(
    'if (success)\n                        Log.Information($"Prueba de impresión exitosa en la impresora {portName}");\n                    else\n                        Log.Error($"Error de WinSpool al enviar prueba a la impresora {portName}");\n                }',
    'if (success)\n                        Log.Information($"Prueba de impresión exitosa en la impresora {portName}");\n                    else\n                        Log.Error($"Error de WinSpool al enviar prueba a la impresora {portName}");\n                    return success;\n                }'
);

content = content.replace(
    'catch (Exception ex)\n            {\n                Log.Error(ex, $"Error al intentar imprimir la prueba en {portName}");\n            }',
    'catch (Exception ex)\n            {\n                Log.Error(ex, $"Error al intentar imprimir la prueba en {portName}");\n                return false;\n            }'
);

fs.writeFileSync('PosCore/Services/TicketPrinterService.cs', content);
