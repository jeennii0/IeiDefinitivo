using Iei.Modelos_Fuentes;
using Iei.Models;
using Microsoft.EntityFrameworkCore;

namespace Iei.Repository
{
    public class MonumentoService
    {
        private readonly IeiContext _context;

        public MonumentoService(IeiContext context)
        {
            _context = context;
        }

        public async Task InsertarMonumento(List<Monumento> monumentos)
        {
            try
            {
                foreach (var monumento in monumentos)
                {
                    // Verificar si ya existe un monumento con el mismo nombre
                    var monumentoExistente = await _context.Monumento
                        .FirstOrDefaultAsync(m => m.Nombre == monumento.Nombre);

                    if (monumentoExistente != null)
                    {
                        // Si existe, continuar con el siguiente monumento
                        continue;
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
                }

                // Guardar todos los monumentos de una sola vez
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar monumentos: {ex.Message}");
                // Manejo de errores adecuado (puedes lanzar una excepción o hacer un log)
            }
        }
    }
}
