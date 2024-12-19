#PROBLEMAS CON LA ENTREGA 1

## CV [ACABADO]
- No informan --> No hay conteo de registros insertados, reparados o rechazados. ✅
- Aligante --> Alicante ✅

## CLE
- No rechazan erróneos
- No detectan duplicados --> No se debe insertar más de un registro de un monumento dado

## EUS
- informan mal --> Los informes de registros cargados, reparados y rechazados 
no se corresponden con lo que se pidió (ver formulario de carga en 
las diapositivas de la segunda parte). Muy pocos grupos han mostrado el 
informe al final del proceso. Algunos han mezclado mensajes “del sistema” 
con los mensajes de error relacionados con los registros, pero eso es muy difícil 
de trazar.
- No rechazan erróneos

## GENERAL
- No unifican idiomas --> Aseguraos de que los nombres de provincias estén 
todos en un idioma, o en los dos, tanto en CV como en EUS.
- insertan provincias mal --> No se detecta que Castellón y Castellon son lo 
mismo, y se insertan ambas. Lo mismo con Alicante y Alacant, València con Valencia, etc.
- no selenium --> El extractor de la CV no utiliza Selenium para aconvertir las coordenadas
- insertan CP null --> No puede haber Códigos postales a cero en la BD, por tratarse de un 
criterio de búsqueda. Lo mismo con otros campos importantes.

NO RECHAZAN ERRÓNEOS:
No se rechazan registros que deberían ser rechazados:

Código postal fuera de rango
Coordenadas fuera de rango
Nombres de provincia erróneos
campos importantes que no estén, en particular los criterios de búsqueda y las coordenadas
códigos postales con caracteres no numéricos
…