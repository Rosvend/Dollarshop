  
Entrega 2 

Arquitecturas de software

Roy Sandoval   
Juan David Londono

Universidad Pontificia Bolivariana

Medellín

2026 

# 

# 

**Tabla de contenidos**

**[1\. Entender las capacidades empresariales	2](#1.-entender-las-capacidades-empresariales)**

[1.0.1 Gestión de Identidad y Clientes	2](#1.0.1-gestión-de-identidad-y-clientes)

[1.0.2 Gestión de Catálogo	2](#1.0.2-gestión-de-catálogo)

[1.0.3 Gestión de Pagos y Precios	2](#1.0.3-gestión-de-pagos-y-precios)

[1.0.4 Procesamiento de Pedidos (Ventas)	3](#1.0.4-procesamiento-de-pedidos-\(ventas\))

[**1.1 Mapa de capacidades	3**](#1.1-mapa-de-capacidades)

[**1.2 Acoplamiento de servicios	5**](#1.2-acoplamiento-de-servicios)

[**1.3 Determinar los servicios que se necesitan y las funciones que deben cumplir	5**](#1.3-determinar-los-servicios-que-se-necesitan-y-las-funciones-que-deben-cumplir)

[**2\. Candidatos a servicios con definición preliminar de sus límites	5**](#2.-candidatos-a-servicios-con-definición-preliminar-de-sus-límites)

[**3\. Contratos de servicio (contract-first)	5**](#3.-contratos-de-servicio-\(contract-first\))

[**4\. Estilos de comunicación	5**](#4.-estilos-de-comunicación)

[**5\. Catálogo de servicios	5**](#5.-catálogo-de-servicios)

# 

# 

# 

# 

# 

# 

# 

# 

# 

# 

# 

# 

# **1\. Entender las capacidades empresariales** {#1.-entender-las-capacidades-empresariales}

Al analizar el dominio de Dollarshop, debemos agrupar la lógica en funciones que sean altamente cohesivas. El objetivo es mantener un equilibrio estricto: si hacemos un servicio por cada clase pequeña o componente, saturamos la red con llamadas; pero si agrupamos demasiadas funciones, seguiremos teniendo un monolito.

Aprovechando la separación de responsabilidades que tenemos estructurada, las capacidades empresariales de alto nivel quedan definidas de la siguiente manera:

## **1.0.1 Gestión de Identidad y Clientes** {#1.0.1-gestión-de-identidad-y-clientes}

* **Propósito:** Administrar el ciclo de vida del usuario en el sistema.  
* **Elementos involucrados:** User, RegistrationData, IUserRepository, los validadores (RegexValidator, EmailValidator, NameValidator) y el AuthenticationService.  
* **Justificación:** Agrupa todo lo relacionado con quién es el usuario y si tiene permiso para operar en la plataforma.

## **1.0.2 Gestión de Catálogo** {#1.0.2-gestión-de-catálogo}

* **Propósito:** Centralizar la información de los artículos que se pueden vender.  
* **Elementos involucrados:** Product y IProductRepository.  
* **Justificación:** Esta es una capacidad altamente autónoma. La información y estructura del producto cambia por motivos completamente distintos a los del proceso de compra o pago.

## **1.0.3 Gestión de Pagos y Precios** {#1.0.3-gestión-de-pagos-y-precios}

* **Propósito:** Calcular el valor real a cobrar y procesar las transacciones financieras.  
* **Elementos involucrados:** PaymentData, métodos de pago (CashPayment, CardPayment, AccountPayment), y las estrategias de descuento (FixedAmountDiscount, PercentageDiscount).  
* **Justificación:** Agrupar aquí las estrategias individuales y combinadas (como las implementadas bajo el patrón *Composite* para acumular descuentos) garantiza que las reglas de negocio financieras estén unificadas en un solo límite claro. Esto nos ayuda específicamente a evitar crear servicios demasiado pequeños dedicados únicamente al cálculo de promociones.

## **1.0.4 Procesamiento de Pedidos (Ventas)** {#1.0.4-procesamiento-de-pedidos-(ventas)}

* **Propósito:** Orquestar la intención de compra del cliente de principio a fin.  
* **Elementos involucrados:** CartItem, ShoppingCart, Invoice, ICheckoutService y el ensamblaje paso a paso de la factura (InvoiceBuilder).  
* **Justificación:** Esta capacidad actúa como el director de la orquesta. Recopila los productos del catálogo, solicita la aplicación de precios y descuentos al área de pagos, y construye el registro final de la venta.

# **1.1 Mapa de capacidades** {#1.1-mapa-de-capacidades}

![][image1]

**1\. Dominio: Usuarios y Clientes** 

Este dominio agrupa todas las capacidades necesarias para administrar el ciclo de vida del usuario dentro del sistema, desde su registro hasta su validación en la plataforma.

* **Capacidad: Gestión de Identidad (L1)**

  Función Interna: Autenticar Credenciales (L2)

  Función Interna: Emitir Token de Acceso (L2)

  Función Interna: Restablecer Contraseña (L2)

* **Capacidad: Registro de Clientes (L1)**

  Función Interna: Crear Nuevo Perfil de Usuario (L2)

  Función Interna: Validar Datos de Registro (Email, Formato, etc.) (L2)


**2\. Dominio: Catálogo e Inventario** 

Centraliza toda la información relacionada con los productos disponibles para la venta. Esta capacidad opera de manera independiente al proceso de compra o pago.

* **Capacidad: Gestión de Catálogo (L1)**

  Función Interna: Consultar Listado de Productos (L2)

  Función Interna: Ver Detalle de Producto (Nombre, Descripción, SKU) (L2)

  Función Interna: Actualizar Precios Base (L2)

**3\. Dominio: Finanzas y Precios** 

Esta capacidad es vital para garantizar la consistencia en el cálculo de los cobros, unificando las reglas de negocio sobre descuentos y precios dinámicos antes de procesar cualquier transacción financiera.

* **Capacidad: Procesamiento de Pagos (L1)**

  Función Interna: Procesar Transacción (Efectivo, Tarjeta, Cuenta) (L2)

  Función Interna: Validar Método de Pago Externo (L2)

* **Capacidad: Reglas de Descuento (L1)**

  Función Interna: Calcular Descuento Simple (Monto Fijo, Porcentaje) (L2)

  Función Interna: Calcular Descuentos Combinados (Composite) (L2)

**4\. Dominio: Operaciones de Venta** 

Este dominio actúa como el orquestador general de la experiencia de compra del cliente, interactuando con los otros dominios para completar el flujo de principio a fin.

* **Capacidad: Procesamiento de Pedidos (L1)**

  Función Interna: Iniciar Flujo de Checkout (L2)

  Función Interna: Orquestar Solicitudes de Pago y Catálogo (L2)

  Función Interna: Registrar Confirmación de Venta (L2)

* **Capacidad: Gestión del Carrito (L1)**

  Función Interna: Añadir/Remover Artículos del Carrito (L2)

  Función Interna: Actualizar Cantidades (L2)

* **Capacidad: Facturación (Invoice) (L1)**

  Función Interna: Construir Factura Paso a Paso (Builder) (L2)

  Función Interna: Generar Registro Final de Factura (L2)

# **1.2 Acoplamiento de servicios** {#1.2-acoplamiento-de-servicios}

Partiendo del mapa de capacidades (§1.1), analizamos cómo se relacionan los bloques candidatos entre sí. El objetivo es detectar dónde hay un vínculo tan fuerte que los componentes deben vivir en el mismo servicio, y dónde el vínculo es suficientemente débil para separarlos con seguridad.

Se evalúan cuatro tipos de acoplamiento:

* **Semántico:** los dos bloques hablan del mismo concepto de negocio.
* **De datos:** comparten entidades o atributos.
* **Temporal:** uno bloquea al otro durante su ejecución (síncrono).
* **De contrato:** uno depende de una firma pública del otro.

## **1.2.1 Matriz de acoplamiento entre capacidades** {#1.2.1-matriz-de-acoplamiento-entre-capacidades}

| Par de capacidades | Tipo principal | Fuerza | Observación |
| :---- | :---- | :---- | :---- |
| Identidad ↔ Registro de Clientes | Datos, semántico | Alta | Ambos manipulan la entidad `User`; conviene un único dueño del dato. |
| Pedidos ↔ Pagos | Temporal, contrato | Alta | Hoy el `CheckoutService` bloquea esperando la respuesta de `PaymentService`; la caída del segundo rompe la venta. |
| Pedidos ↔ Facturación | Semántico | Alta | Una venta confirmada siempre produce una factura; no tiene sentido separarlas. |
| Pedidos ↔ Catálogo | Datos | Media | Se consulta precio y stock en el momento del checkout, pero solo como lectura. |
| Pedidos ↔ Pricing y Descuentos | Contrato | Media | Se delega el cálculo de descuentos combinados (`CompositeDiscount`) sin compartir estado. |
| Carrito ↔ Catálogo | Datos | Media | El carrito necesita leer `Product` al agregar un ítem. |
| Carrito ↔ Pricing y Descuentos | Contrato | Baja | Ya está resuelto con el patrón *Strategy* (`IDiscountStrategy`); el carrito no conoce la implementación. |
| Identidad ↔ resto de capacidades | Contrato | Baja | Solo se consulta para validar el token o autorizar la operación. |

## **1.2.2 Interpretación para el diseño SOA** {#1.2.2-interpretación-para-el-diseño-soa}

Los acoplamientos altos indican agrupación forzosa; los bajos habilitan separación limpia:

1. **Identidad + Registro de Clientes** deben permanecer juntos en un único servicio de Identidad, porque comparten la entidad `User`.
2. **Pedidos + Facturación** deben permanecer juntos en un servicio de Pedidos, porque una factura es el resultado natural de un pedido confirmado.
3. **Pedidos ↔ Pagos** queda como frontera entre dos servicios distintos, pero mediada por comunicación asíncrona a través del ESB; así el checkout no se bloquea si el procesador de pagos está lento.
4. **Pricing y Descuentos** se extrae como servicio autónomo. Dado que ya está resuelto con *Strategy*, la separación no requiere refactor de dominio, solo cambio de ubicación.
5. **Catálogo** es el candidato más independiente: cambia por razones distintas al resto del negocio (promociones de producto, reposición de stock) y puede evolucionar sin tocar Ventas.

Este análisis justifica directamente la partición propuesta en §1.3 y las decisiones de estilo de comunicación descritas en §4.

# **1.3 Determinar los servicios que se necesitan y las funciones que deben cumplir** {#1.3-determinar-los-servicios-que-se-necesitan-y-las-funciones-que-deben-cumplir}

A partir de las capacidades empresariales (§1.1) y la matriz de acoplamiento (§1.2), proponemos **seis servicios de negocio** más una pieza de infraestructura (ESB / API Gateway) que no constituye un servicio de dominio pero que es necesaria para operar el conjunto.

## **1.3.1 Servicios de negocio** {#1.3.1-servicios-de-negocio}

| \# | Servicio | Capacidad de origen | Funciones L2 que absorbe |
| :---- | :---- | :---- | :---- |
| S1 | Identidad | 1.0.1 | Autenticar credenciales, emitir token de acceso, restablecer contraseña, crear nuevo perfil, validar datos de registro (email, formato, teléfono). |
| S2 | Catálogo | 1.0.2 | Consultar listado de productos, ver detalle, actualizar precios base, actualizar stock. |
| S3 | Pricing y Descuentos | 1.0.3 (parcial) | Calcular descuento simple (monto fijo, porcentaje), calcular descuentos combinados (Composite), aplicar reglas de precio dinámico. |
| S4 | Pagos | 1.0.3 (parcial) | Procesar transacción (efectivo, tarjeta, cuenta), validar método de pago externo, emitir confirmación o rechazo. |
| S5 | Carrito | 1.0.4 (parcial) | Añadir/remover artículos, actualizar cantidades, calcular subtotal del carrito. |
| S6 | Pedidos y Facturación | 1.0.4 (parcial) | Iniciar flujo de checkout, orquestar solicitudes a Catálogo/Pricing/Pagos, construir factura paso a paso, registrar confirmación de venta. |

## **1.3.2 Infraestructura de soporte** {#1.3.2-infraestructura-de-soporte}

* **ESB (Enterprise Service Bus):** media la comunicación entre servicios, enruta mensajes, transforma formatos y publica eventos de dominio (por ejemplo, `PaymentConfirmed`, `OrderPlaced`).
* **API Gateway:** fachada única para los consumidores externos (web, móvil, consola). Evita que cada cliente conozca la ubicación interna de los seis servicios.

## **1.3.3 Justificación de la granularidad elegida** {#1.3.3-justificación-de-la-granularidad-elegida}

Se descartaron dos extremos:

* **Cuatro servicios (uno por capacidad empresarial):** obliga a mezclar Pagos y Pricing dentro de un mismo servicio, pese a que cambian por razones distintas (Pricing responde al equipo de marketing, Pagos al equipo financiero).
* **Ocho o más servicios finos:** fragmentaría Pedidos y Facturación, generando llamadas de red innecesarias para una operación que semánticamente es atómica (una venta produce una factura).

La elección de seis servicios preserva la cohesión de negocio y mantiene las llamadas entre servicios en un número manejable.

# **2\. Candidatos a servicios con definición preliminar de sus límites** {#2.-candidatos-a-servicios-con-definición-preliminar-de-sus-límites}

Cada uno de los seis servicios identificados en §1.3 se describe con el mismo esqueleto: propósito de negocio, entidades que posee, operaciones expuestas, responsabilidades que explícitamente no asume y dependencias con otros servicios. Este formato uniforme permite detectar fugas de responsabilidad y zonas grises de propiedad del dato.

![][image2]

*Figura 2\. Vista de alto nivel de la arquitectura SOA propuesta. Los seis servicios de negocio se comunican a través del ESB, mientras que el API Gateway expone una fachada única a los clientes externos (consola, web, móvil).*

## **2.1 Servicio de Identidad (S1)** {#2.1-servicio-de-identidad-s1}

* **Propósito:** administrar el ciclo de vida del usuario y su autorización para operar en la plataforma.
* **Entidades que posee:** `User`, `RegistrationData`, credenciales, tokens de sesión.
* **Operaciones expuestas:** registrar usuario, autenticar credenciales, emitir token, validar token, obtener perfil, restablecer contraseña.
* **No responsabilidades:** no conoce productos, pedidos ni facturas; no registra eventos comerciales.
* **Dependencias:** ninguna. Es un servicio hoja consultado por los demás.

## **2.2 Servicio de Catálogo (S2)** {#2.2-servicio-de-catálogo-s2}

* **Propósito:** ser la fuente única de verdad sobre qué productos existen, su precio base y su disponibilidad.
* **Entidades que posee:** `Product`, inventario (stock por SKU).
* **Operaciones expuestas:** listar productos, consultar detalle por `ProductId`, actualizar precio base, descontar stock, reponer stock.
* **No responsabilidades:** no aplica descuentos (eso es de Pricing); no conoce al cliente que compra.
* **Dependencias:** ninguna. Puede ser consumido por Carrito y Pedidos.

## **2.3 Servicio de Pricing y Descuentos (S3)** {#2.3-servicio-de-pricing-y-descuentos-s3}

* **Propósito:** centralizar las reglas de negocio que determinan el precio final a cobrar, incluyendo promociones simples y combinadas.
* **Entidades que posee:** reglas de descuento (porcentaje, monto fijo, combinadas), políticas de precios dinámicos.
* **Operaciones expuestas:** calcular precio final dado un precio base y un contexto (cliente, fecha, lista de reglas aplicables), listar reglas activas.
* **No responsabilidades:** no cobra dinero (eso es de Pagos); no persiste la transacción comercial.
* **Dependencias:** puede consultar al Catálogo si necesita atributos del producto para decidir una regla; en el flujo base recibe los datos como entrada.

## **2.4 Servicio de Pagos (S4)** {#2.4-servicio-de-pagos-s4}

* **Propósito:** ejecutar y confirmar la transacción financiera por cualquiera de los métodos soportados.
* **Entidades que posee:** `PaymentInfo`, registro de transacciones, confirmación del procesador externo.
* **Operaciones expuestas:** listar métodos disponibles, procesar pago (efectivo, tarjeta, cuenta), consultar estado de una transacción.
* **No responsabilidades:** no construye la factura; no conoce el carrito; no decide el precio a cobrar (lo recibe como entrada).
* **Dependencias:** ninguna interna; depende de pasarelas externas (no modeladas en este documento).

## **2.5 Servicio de Carrito (S5)** {#2.5-servicio-de-carrito-s5}

* **Propósito:** mantener la intención de compra del cliente mientras navega la tienda.
* **Entidades que posee:** `Cart`, `CartItem`, asociación `Cart → User`.
* **Operaciones expuestas:** crear carrito para un usuario, añadir ítem, remover ítem, actualizar cantidad, obtener subtotal, vaciar carrito.
* **No responsabilidades:** no cobra; no confirma la venta; no emite factura.
* **Dependencias:** Catálogo (lectura de `Product` al agregar ítem), Pricing (cálculo opcional de subtotal con descuento).

## **2.6 Servicio de Pedidos y Facturación (S6)** {#2.6-servicio-de-pedidos-y-facturación-s6}

* **Propósito:** orquestar el checkout de principio a fin y producir el registro formal de la venta.
* **Entidades que posee:** `Order`, `Invoice`, líneas de factura, estado del pedido.
* **Operaciones expuestas:** iniciar checkout desde un carrito, confirmar pedido tras pago exitoso, generar factura, consultar histórico de pedidos por cliente.
* **No responsabilidades:** no administra productos, ni usuarios, ni métodos de pago; coordina, no implementa.
* **Dependencias:** Identidad (validar token), Carrito (leer ítems), Catálogo (leer precios y descontar stock), Pricing (calcular total con descuentos), Pagos (ejecutar cobro, vía ESB asíncrono).

# **3\. Contratos de servicio (contract-first)** {#3.-contratos-de-servicio-(contract-first)}

La estrategia *contract-first* consiste en definir primero la firma pública de cada servicio (operaciones, parámetros, respuestas, errores) y solo después avanzar con su implementación interna. Esto permite que equipos distintos trabajen en paralelo y que los consumidores validen sus expectativas sin depender del calendario del proveedor.

Los contratos se expresan aquí en formato neutral (pseudo-REST), independiente de la tecnología final que se elija (REST/HTTP, gRPC o mensajería). Todos los contratos arrancan en versión `v1` y siguen una política de compatibilidad hacia atrás: cambios que rompan el contrato requieren una nueva versión (`v2`) que conviva con la anterior durante un período de transición.

## **3.1 Contrato del Servicio de Identidad (S1)** {#3.1-contrato-del-servicio-de-identidad-s1}

| Operación | Entrada | Salida | Errores | Idempotencia |
| :---- | :---- | :---- | :---- | :---- |
| `POST /v1/identity/users` | `RegistrationData` | `UserId`, estado 201 | 400 datos inválidos, 409 usuario existente | No |
| `POST /v1/identity/sessions` | `username`, `password` | `AuthToken`, expiración | 401 credenciales inválidas | No |
| `GET  /v1/identity/sessions/{token}` | `token` | `User` resumido, `valid: bool` | 401 token inválido o expirado | Sí |
| `GET  /v1/identity/users/{id}` | `userId` | `User` completo | 404 no encontrado | Sí |
| `POST /v1/identity/password-resets` | `email` | confirmación 202 | 404 email desconocido | Sí |

## **3.2 Contrato del Servicio de Catálogo (S2)** {#3.2-contrato-del-servicio-de-catálogo-s2}

| Operación | Entrada | Salida | Errores | Idempotencia |
| :---- | :---- | :---- | :---- | :---- |
| `GET  /v1/catalog/products` | filtros opcionales | lista de `Product` | — | Sí |
| `GET  /v1/catalog/products/{id}` | `productId` | `Product` | 404 no encontrado | Sí |
| `PATCH /v1/catalog/products/{id}/price` | `productId`, `newPrice` | `Product` actualizado | 404, 400 precio inválido | Sí |
| `POST /v1/catalog/products/{id}/reservations` | `productId`, `quantity` | `reservationId`, estado 201 | 409 stock insuficiente | No |
| `POST /v1/catalog/products/{id}/stock-adjustments` | `productId`, `delta` | `Product` actualizado | 400 delta inválido | No |

## **3.3 Contrato del Servicio de Pricing y Descuentos (S3)** {#3.3-contrato-del-servicio-de-pricing-y-descuentos-s3}

| Operación | Entrada | Salida | Errores | Idempotencia |
| :---- | :---- | :---- | :---- | :---- |
| `POST /v1/pricing/quotes` | lista de ítems, `customerId`, fecha | precio final por ítem y total | 400 entrada inválida | Sí |
| `GET  /v1/pricing/rules` | — | reglas activas | — | Sí |
| `POST /v1/pricing/rules` | definición de regla | `ruleId` | 400, 409 conflicto | No |

## **3.4 Contrato del Servicio de Pagos (S4)** {#3.4-contrato-del-servicio-de-pagos-s4}

| Operación | Entrada | Salida | Errores | Idempotencia |
| :---- | :---- | :---- | :---- | :---- |
| `GET  /v1/payments/methods` | — | lista de métodos habilitados | — | Sí |
| `POST /v1/payments/transactions` | `amount`, `method`, `paymentInfo`, `orderId`, `idempotencyKey` | `transactionId`, estado | 402 pago rechazado, 400 datos inválidos | Sí (vía `idempotencyKey`) |
| `GET  /v1/payments/transactions/{id}` | `transactionId` | estado actual | 404 | Sí |
| Evento publicado: `PaymentConfirmed` | `transactionId`, `orderId`, `amount` | — | — | — |
| Evento publicado: `PaymentRejected` | `transactionId`, `orderId`, `reason` | — | — | — |

## **3.5 Contrato del Servicio de Carrito (S5)** {#3.5-contrato-del-servicio-de-carrito-s5}

| Operación | Entrada | Salida | Errores | Idempotencia |
| :---- | :---- | :---- | :---- | :---- |
| `POST /v1/carts` | `userId` | `cartId` | 401 no autenticado | No |
| `POST /v1/carts/{id}/items` | `productId`, `quantity` | carrito actualizado | 404 carrito, 409 stock | No |
| `DELETE /v1/carts/{id}/items/{productId}` | — | carrito actualizado | 404 | Sí |
| `PATCH /v1/carts/{id}/items/{productId}` | `quantity` | carrito actualizado | 404, 409 | Sí |
| `GET  /v1/carts/{id}` | `cartId` | contenido y subtotal | 404 | Sí |
| `DELETE /v1/carts/{id}` | `cartId` | estado 204 | 404 | Sí |

## **3.6 Contrato del Servicio de Pedidos y Facturación (S6)** {#3.6-contrato-del-servicio-de-pedidos-y-facturación-s6}

| Operación | Entrada | Salida | Errores | Idempotencia |
| :---- | :---- | :---- | :---- | :---- |
| `POST /v1/orders/checkouts` | `cartId`, `paymentInfo`, `customerNumber` | `orderId`, estado `Pending` | 400, 401, 409 stock | No |
| `GET  /v1/orders/{id}` | `orderId` | `Order` con estado | 404 | Sí |
| `GET  /v1/orders/{id}/invoice` | `orderId` | `Invoice` si existe | 404, 409 aún sin pago | Sí |
| `GET  /v1/orders?customerId=` | `customerId` | lista de `Order` | — | Sí |
| Evento consumido: `PaymentConfirmed` | — | transición de `Pending` a `Confirmed` y emisión de `Invoice` | — | — |
| Evento consumido: `PaymentRejected` | — | transición de `Pending` a `Cancelled` | — | — |
| Evento publicado: `OrderPlaced` | `orderId`, `customerId`, `total` | — | — | — |

## **3.7 Política transversal de contratos** {#3.7-política-transversal-de-contratos}

* **Versionado:** toda ruta incluye `/v{n}` al inicio. Una nueva versión se publica sin remover la anterior.
* **Formato:** JSON con identificadores explícitos (`orderId`, `productId`), nunca arrays crudos.
* **Errores:** estructura uniforme `{ code, message, details }`, mapeada a códigos HTTP estándar.
* **Autorización:** todas las operaciones excepto las de Identidad (`sessions`, `password-resets`) requieren un `AuthToken` emitido por S1 y validado vía ESB.
* **Idempotencia:** las operaciones de escritura sensibles (pagos, creación de pedidos) aceptan un `idempotencyKey` para tolerar reintentos.

# **4\. Estilos de comunicación** {#4.-estilos-de-comunicación}

No todas las interacciones entre servicios tienen los mismos requisitos. Algunas necesitan respuesta inmediata (por ejemplo, validar un token antes de permitir una operación), otras admiten demora y se benefician de procesamiento asíncrono (por ejemplo, un pago con tarjeta que puede tardar varios segundos). Esta sección decide, por cada par de servicios, el estilo de comunicación apropiado y justifica la elección.

## **4.1 Matriz de estilos por interacción** {#4.1-matriz-de-estilos-por-interacción}

| Origen → Destino | Estilo | Protocolo conceptual | Justificación |
| :---- | :---- | :---- | :---- |
| Pedidos → Identidad | Síncrono request/response | REST/HTTP mediado por ESB | Validar token antes de avanzar; sin respuesta no se puede autorizar. |
| Pedidos → Catálogo | Síncrono request/response | REST/HTTP | Se requiere precio y stock actualizados en el instante del checkout. |
| Pedidos → Pricing | Síncrono request/response | REST/HTTP | Cálculo determinístico y breve; el resultado influye en la siguiente operación. |
| Pedidos → Pagos | Asíncrono publish-subscribe | Cola de mensajes vía ESB | El cobro puede tardar o requerir confirmación externa; no debe bloquear el checkout. |
| Pagos → Pedidos | Asíncrono vía evento | `PaymentConfirmed` / `PaymentRejected` | Pedidos reacciona al resultado sin acoplarse temporalmente al procesador de pagos. |
| Carrito → Catálogo | Síncrono request/response | REST/HTTP | Lectura puntual de `Product` al agregar ítem. |
| Carrito → Pricing | Síncrono request/response | REST/HTTP | Cálculo opcional de subtotal con descuento; respuesta inmediata. |
| Cualquier servicio → Identidad | Síncrono request/response | REST/HTTP | Validación de token es un *gate* de autorización. |
| Pedidos → resto de suscriptores | Asíncrono broadcast | Evento `OrderPlaced` | Otros servicios (analítica, notificaciones) pueden suscribirse sin que Pedidos los conozca. |
| API Gateway → cualquier servicio | Síncrono request/response | REST/HTTP | Fachada para clientes externos; traduce una petición del usuario a una o varias llamadas internas. |

## **4.2 Rol del ESB** {#4.2-rol-del-esb}

El ESB (Enterprise Service Bus) cumple cuatro funciones:

1. **Enrutamiento:** resuelve la dirección física del servicio destino a partir del nombre lógico (por ejemplo, `pricing.v1`).
2. **Transformación:** adapta formatos entre servicios si algún consumidor usa una versión distinta del contrato.
3. **Mediación de eventos:** implementa el patrón publish-subscribe para los eventos de dominio (`PaymentConfirmed`, `OrderPlaced`, `PaymentRejected`).
4. **Observabilidad:** centraliza la trazabilidad de las llamadas y permite auditar el flujo de una venta de principio a fin.

## **4.3 Eventos de dominio publicados** {#4.3-eventos-de-dominio-publicados}

| Evento | Publicador | Suscriptores típicos | Carga útil |
| :---- | :---- | :---- | :---- |
| `PaymentConfirmed` | Pagos (S4) | Pedidos (S6) | `transactionId`, `orderId`, `amount`, `method` |
| `PaymentRejected` | Pagos (S4) | Pedidos (S6) | `transactionId`, `orderId`, `reason` |
| `OrderPlaced` | Pedidos (S6) | analítica, notificaciones, fidelización (futuros) | `orderId`, `customerId`, `total`, `items` |
| `StockDepleted` | Catálogo (S2) | Pricing, notificaciones (futuros) | `productId`, `currentStock` |

## **4.4 Decisiones transversales** {#4.4-decisiones-transversales}

* **Sincrónico por defecto** para operaciones de lectura y validación: simplifica el razonamiento y evita introducir latencia innecesaria.
* **Asíncrono para transacciones externas** (pagos, integraciones con terceros): protege la disponibilidad del flujo de ventas.
* **Eventos de dominio en lugar de llamadas directas** cuando varios servicios pueden interesarse en un mismo hecho de negocio: evita que el publicador deba conocer a todos sus consumidores.
* **Sin comunicación servicio-a-servicio por fuera del ESB:** toda interacción pasa por el bus, para garantizar observabilidad y control de versiones.

# **5\. Catálogo de servicios** {#5.-catálogo-de-servicios}

El catálogo de servicios es el registro operativo del ecosistema SOA. Funciona como fuente única de verdad sobre qué servicios existen, en qué versión están, quién los mantiene y cómo se consumen. En producción, esta tabla se convierte en un artefacto vivo (por ejemplo, un *service registry* tipo Consul o una base de datos interna), no en un documento estático.

## **5.1 Registro de servicios de negocio** {#5.1-registro-de-servicios-de-negocio}

| ID | Nombre | Versión | Dueño funcional | Endpoint base | Estilo dominante | SLA objetivo | Estado |
| :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- |
| S1 | Identidad | v1 | Equipo Cuentas | `/v1/identity` | Síncrono | 99.95% — p95 \< 150 ms | Propuesto |
| S2 | Catálogo | v1 | Equipo Producto | `/v1/catalog` | Síncrono | 99.9% — p95 \< 200 ms | Propuesto |
| S3 | Pricing y Descuentos | v1 | Equipo Comercial | `/v1/pricing` | Síncrono | 99.9% — p95 \< 100 ms | Propuesto |
| S4 | Pagos | v1 | Equipo Finanzas | `/v1/payments` | Asíncrono (eventos) | 99.5% — p95 \< 3 s | Propuesto |
| S5 | Carrito | v1 | Equipo Ventas | `/v1/carts` | Síncrono | 99.9% — p95 \< 150 ms | Propuesto |
| S6 | Pedidos y Facturación | v1 | Equipo Ventas | `/v1/orders` | Mixto (orquesta) | 99.9% — p95 \< 500 ms (cierre \< 10 s incluyendo pago) | Propuesto |

## **5.2 Registro de infraestructura de soporte** {#5.2-registro-de-infraestructura-de-soporte}

| Componente | Rol | Dueño | Observación |
| :---- | :---- | :---- | :---- |
| ESB | Enrutamiento, mediación, publish-subscribe | Plataforma | Toda comunicación inter-servicio pasa por aquí |
| API Gateway | Fachada externa | Plataforma | Autentica clientes, agrega llamadas, limita tasa |
| Registro de servicios | Descubrimiento dinámico | Plataforma | Los servicios se registran al iniciar y se dan de baja al apagarse |

## **5.3 Política de ciclo de vida** {#5.3-política-de-ciclo-de-vida}

* Un servicio nuevo entra al catálogo en estado *Propuesto* con su contrato en `v1` y la ficha de la §2.
* Pasa a *Activo* cuando tiene una implementación verificada y un dueño que responde por él.
* Pasa a *Obsoleto* cuando se publica una versión mayor y se anuncia una fecha de retiro; durante ese período conviven ambas versiones.
* Pasa a *Retirado* cuando ya ningún consumidor lo invoca. Los consumidores se verifican vía trazabilidad del ESB antes de retirar.

Este ciclo de vida garantiza que la evolución del ecosistema sea incremental y que ningún consumidor quede huérfano por un cambio no anunciado.