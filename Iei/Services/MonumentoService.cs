using Iei.Modelos_Fuentes;
using Iei.Models;
using Microsoft.EntityFrameworkCore;

namespace Iei.Services
{
    public class MonumentoService
    {
        private readonly IeiContext _context;

        public MonumentoService(IeiContext context)
        {
            _context = context;
        }

        public async Task<int> InsertarMonumento(List<Monumento> monumentos)
        {
            int monumentosInsertados = 0;  // Variable para contar los monumentos insertados

            try
            {
                foreach (var monumento in monumentos)
                {
                    // Verificar si ya existe un monumento con el mismo nombre
                    var monumentoExistente = await _context.Monumento
                        .FirstOrDefaultAsync(m => m.Nombre == monumento.Nombre);

                    if (monumentoExistente != null)
                    {
                        // Si el monumento ya existe, se imprime un mensaje y se omite la inserción
                        Console.WriteLine($"El monumento '{monumento.Nombre}' ya existe en la base de datos. No se insertará.");
                        continue;  // Omite la inserción de este monumento
                    }

                    // Verificar si la Localidad ya existe en la base de datos
                    var localidadExistente = await _context.Localidad
                        .FirstOrDefaultAsync(l => l.Nombre == monumento.Localidad.Nombre && l.Provincia.Nombre == monumento.Localidad.Provincia.Nombre);

                    if (localidadExistente != null)
                    {
                        // Si existe, asociar la localidad encontrada
                        monumento.LocalidadId = localidadExistente.Id;
                        monumento.Localidad = localidadExistente; // Asociamos la entidad Localidad
                    }
                    else
                    {
                        // Si no existe, crear una nueva localidad y asociarla
                        var provinciaExistente = await _context.Provincia
                            .FirstOrDefaultAsync(p => p.Nombre == monumento.Localidad.Provincia.Nombre);

                        if (provinciaExistente == null)
                        {
                            // Si la provincia no existe, crearla
                            provinciaExistente = new Provincia
                            {
                                Nombre = monumento.Localidad.Provincia.Nombre
                            };
                            _context.Provincia.Add(provinciaExistente);
                            await _context.SaveChangesAsync(); // Guardar la nueva provincia
                        }

                        // Crear una nueva localidad y asociar la provincia existente
                        var nuevaLocalidad = new Localidad
                        {
                            Nombre = monumento.Localidad.Nombre,
                            ProvinciaId = provinciaExistente.Id,
                            Provincia = provinciaExistente // Asociamos la provincia con la localidad
                        };

                        _context.Localidad.Add(nuevaLocalidad);
                        await _context.SaveChangesAsync(); // Guardar la nueva localidad

                        // Asignar la localidad recién creada al monumento
                        monumento.LocalidadId = nuevaLocalidad.Id;
                        monumento.Localidad = nuevaLocalidad;
                    }

                    // Añadir el monumento a la base de datos
                    _context.Monumento.Add(monumento);
                    monumentosInsertados++;  // Aumentar el contador solo si se insertó un monumento
                }

                // Guardar todos los monumentos de una sola vez
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar monumentos: {ex.Message}");
                // Manejo de errores adecuado (puedes lanzar una excepción o hacer un log)
            }

            return monumentosInsertados;  // Devolver cuántos monumentos se insertaron realmente
        }

    
    }
}
