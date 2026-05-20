# Rúbrica de Evaluación 3: Diseño e Implementación de Arquitecturas

## I. Aspectos de Diseño

### 1. Documento Diseño Servicios SOA (10 puntos)

| Criterio de Evaluación |
| :--- |
| Parten el diseño de la identificación de las capacidades organizacionales – mapa de capacidades |
| Para cada capacidad organizacional identificaron los servicios candidatos |
| Diseñaron una arquitectura jerárquica para los servicios donde se nota desde los servicios altamente granulados hasta servicios agrupados por subdominios, dominios y/o servicios para funciones end to end |
| Diseñaron un catálogo de servicios donde se especifica el contrato: nombre, entrada, salida |

### 2. Documento Diseño DDD (15 puntos)

*A partir de acá pintar el dominio seleccionado de otro color en los gráficos.*

| Criterio de Evaluación |
| :--- |
| Gráfica con flujo de la estructura organizacional agrupados por afinidad, que permita ver los posibles dominios del problema |
| Gráficos con Dominios y dentro de ellos entidades y agregados |
| Gráfico Bounded Context donde se vea el flujo de entidades, agregados y los posibles servicios que se ofrecen y las APIs que consume |
| Lenguaje Ubicuo presentado en un glosario de términos de negocio |
| Objetos de Valor del dominio seleccionado |
| Triggers y Eventos del Dominio seleccionado |
| Definir Servicios del domino seleccionado |

---

## II. Aspectos de Implementación (25 puntos)
*De acuerdo con la arquitectura(s) elegida(s). **Nota:** Si existen dominios corruptos, la nota de la implementación es 0 puntos.*

### 1. Capa de Dominio (10 puntos)
*Tiene sus preocupaciones y lógica de negocio claramente implementadas.*

| Elemento | Característica / Comportamiento | Detalles de Implementación |
| :--- | :--- | :--- |
| **Entidades** | Enriquecidas, ricas implementan comportamientos | |
| **Value Objects inmutables** | Solo tienen setters y getters | Reciben los datos en constructor con lanzamiento de excepciones. |
| **Agregados con consistencia** | Se identifica claramente la raíz y su contenido | La raíz es el único punto de acceso |
| **Domain Events** | Se invocan desde agregados (subscribers) | Representan algo que sucedió en el dominio. Se nombran en pasado. |
| **Interfaces de Dominio** | Definidas en el Dominio e implementadas en otras capas | |

### 2. Capa de Aplicación u Orquestadora (5 puntos)

| Criterio | Descripción |
| :--- | :--- |
| **Use cases específicos** | Hay use case y cada uno realiza una única acción de negocio |
| **Orquestación** | Se implementa coordinación entre servicios de dominio, no contiene lógica de negocio |
| **Transacciones bien manejadas** | Garantiza consistencia en la operación completa |

### 3. Capa de Infraestructura (5 puntos)
*Capa dedicada a manejar temas de Infraestructura.*

| Elemento | Descripción / Regla | Detalles de Implementación |
| :--- | :--- | :--- |
| **Repositorios** | Implementan interfaces de dominio | No hacen referencias a tecnologías específicas: SQL, APIs, NoSQL. Retornan entidades y agregados del dominio. |
| **ORM** | Se tiene implementado ORM para la conversión y transformación de tablas a objetos del dominio | Se encarga de las consultas y la persistencia. Solo se usa en la capa de infraestructura, el dominio no depende de los ORM. |
| **Implementación de Caché** | Se tiene implementados mecanismos de caché | |

### 4. Capa Externa (5 puntos)
*Capa más externa que maneja las interacciones con el resto del ecosistema digital.*

| Elemento | Descripción | Detalles de Implementación |
| :--- | :--- | :--- |
| **Controllers delgados** | Implementan los casos de uso | Manejan consistencia (commits, rollbacks) |
| **DTOs para entrada/salida** | Los tiene implementados para transportar datos entre capas | Mapeo explícito hacia/desde el modelo de dominio. Validación de datos de entrada en el punto de recepción (Boundaries). |