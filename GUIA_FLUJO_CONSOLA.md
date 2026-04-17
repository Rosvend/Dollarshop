# Guía del Flujo de la Aplicación - Tienda Online

## 📋 Descripción General

Esta es una aplicación de tienda en línea con interfaz de consola. El sistema permite a los usuarios autenticarse, navegar productos, agregar artículos a un carrito de compras, aplicar descuentos y realizar pagos utilizando diferentes métodos.

**Usuarios predeterminados para pruebas:**
- **Usuario:** samuel
- **Contraseña:** 123

---

## 🛍️ Productos Disponibles

La tienda cuenta con los siguientes productos:

| ID | Nombre | Precio | Stock |
|---|---|---|---|
| 1 | Portatil | $1.000,00 | 1 |
| 2 | Celular | $550,00 | 1 |
| 3 | Airpods | $80,00 | 1 |
| 4 | Tarjeta Grafica | $800,00 | 1 |
| 5 | Mouse | $22,90 | 1 |
| 6 | Parlantes | $300,00 | 1 |
| 7 | Bose | $400,00 | 1 |

---

## 🔐 Pantalla de Bienvenida e Inicio de Sesión

Cuando se ejecuta la aplicación, aparece la pantalla de bienvenida:

```
-----BIENVENIDO-----

1.Iniciar Sesion
2.Registrarse

Seleccione una opcion:
```

### Opción 1: Iniciar Sesión

**Pasos:**
1. Seleccione la opción `1` (Iniciar Sesion)
2. Ingrese el **Usuario** (en este caso: `samuel`)
3. Ingrese la **Contraseña** (en este caso: `123`)

**Resultado:**
- Si los datos son correctos, verá el mensaje: `INICIO EXITOSO` y accederá al menú principal
- Si los datos son incorrectos, verá: `Usuario o contrasena incorrectas. Por favor, intentelo de nuevo.`

### Opción 2: Registrarse

**Pasos:**
1. Seleccione la opción `2` (Registrarse)
2. Complete los siguientes campos:
   - **Nombre:** Nombre del usuario (solo letras)
   - **Apellido:** Apellido del usuario (solo letras)
   - **Email:** Correo electrónico válido (máximo 5 caracteres, solo letras y números)
   - **Teléfono:** Número de teléfono (exactamente 10 dígitos)
   - **Contraseña:** Contraseña de acceso

3. Se mostrará un resumen de los datos registrados
4. Verá el mensaje: `REGISTRO EXITOSO`
5. Se iniciará sesión automáticamente con la cuenta nueva

---

## 📱 Menú Principal

Una vez autenticado, accede al **Menú Principal**:

```
-----MENU PRINCIPAL-----

1.CLIENTE
2.PRODUCTOS
3.CARRITO
0.SALIR

Ingrese una opcion:
```

### Opción 1: CLIENTE

Muestra un submenú con opciones relacionadas al usuario:

```
CLIENTE
1.Ver Productos
2.Ver Carrito
0.Volver

Elija una opcion:
```

**Opción 1.1 - Ver Productos:**
- Muestra la lista completa de productos disponibles con sus IDs, nombres, precios y cantidad en stock
- Regresa al submenú de CLIENTE

**Opción 1.2 - Ver Carrito:**
- Muestra los productos actualmente en el carrito de compras
- Si el carrito está vacío, muestra: `(vacio)`
- Regresa al submenú de CLIENTE

---

### Opción 2: PRODUCTOS

Muestra el menú para gestionar productos:

```
PRODUCTO
1.Agregar al carrito
0.Volver

Elija una opcion:
```

**Opción 2.1 - Agregar al Carrito:**

Este es el proceso principal para comprar productos. Los pasos son:

1. Se muestra la **lista completa de productos** con sus detalles (ID, nombre, precio, stock)
2. Ingrese el **ID del producto** que desea agregar al carrito (o `0` para salir)
3. El sistema verifica:
   - Si el producto existe
   - Si hay stock disponible
4. Si es válido:
   - El producto se agrega al carrito (cantidad: 1)
   - Se reduce el stock del producto en 1
   - Se muestra el mensaje: `Producto 'NombreProducto' agregado al carrito.`
5. Puede continuar agregando más productos o presionar `0` para salir

**Ejemplo de compra de múltiples productos:**
```
LISTA DE PRODUCTOS:
  ID: 1, Nombre: Portatil, Precio: $1.000,00, Stock: 1
  ID: 2, Nombre: Celular, Precio: $550,00, Stock: 1
  ID: 3, Nombre: Airpods, Precio: $80,00, Stock: 1
  ...

Ingrese el ID del producto que desea agregar al carrito (0 para salir):
> 1
Producto 'Portatil' agregado al carrito.

> 3
Producto 'Airpods' agregado al carrito.

> 0
(vuelve al menú anterior)
```

---

### Opción 3: CARRITO

Accede a la gestión completa del carrito de compras:

```
-----CARRITO-----

PRODUCTOS EN EL CARRITO:
  ID: 1, Nombre: Portatil, Precio: $1.000,00, Cantidad: 1
  ID: 3, Nombre: Airpods, Precio: $80,00, Cantidad: 1

1.Sacar producto
2.Calcular total
3.Hacer pago
0.Volver

Elija una opcion:
```

**Opción 3.1 - Sacar producto del carrito:**

1. Se solicita el **ID del producto** que desea remover (o `0` para cancelar)
2. El sistema verifica que el producto esté en el carrito
3. Si es válido:
   - El producto se elimina del carrito
   - Se devuelve el stock al inventario
   - Se muestra: `Producto 'NombreProducto' sacado del carrito.`

**Opción 3.2 - Calcular total:**

1. Se muestra el total del carrito
2. El total incluye **descuentos automáticos**:
   - **Descuento por porcentaje:** 10%
   - **Descuento adicional fijo:** $5
3. Ejemplo:
   ```
   Total del Carrito: $1.505,00
   ```

**Opción 3.3 - Hacer pago (Proceso de Checkout):**

Este es el proceso de finalización de la compra. Siga estos pasos:

---

## 💳 Proceso de Pago

### Paso 1: Verificación del Carrito

Si el carrito está vacío, verá:
```
El carrito esta vacio. Agregue productos antes de pagar.
```

Debe agregar productos antes de continuar.

### Paso 2: Seleccionar Método de Pago

Se muestra el mensaje:
```
-----PAGAR-----

Ingrese el metodo de pago (Tarjeta, Efectivo, Cuenta):
```

Los métodos disponibles son:
1. **Tarjeta** - Pago con tarjeta de crédito/débito
2. **Efectivo** - Pago en efectivo
3. **Cuenta** - Pago desde cuenta bancaria

**Ingrese el nombre del método** (por ejemplo: `Tarjeta`, `Efectivo` o `Cuenta`)

### Paso 3: Número de Cuenta (Según método de pago)

- **Si selecciona "Tarjeta":** Se solicita el número de cuenta (solo dígitos)
- **Si selecciona "Cuenta":** Se solicita el número de cuenta (solo dígitos)
- **Si selecciona "Efectivo":** NO se solicita número de cuenta

**Ejemplo:**
```
Ingrese numero de cuenta: 1234567890
```

### Paso 4: Confirmación del Monto y Pago

Se muestra:
```
Total a pagar: $1.505,00

1.Confirmar pago
2.Cancelar

Elija una opcion:
```

**Opción 1 - Confirmar pago:**
- Se solicita su **número de identificación** (solo dígitos)
- El pago se procesa

**Opción 2 - Cancelar:**
- Se cancela el pago: `Pago cancelado.`
- Regresa al menú del carrito

### Paso 5: Procesamiento del Pago

El sistema realiza el procesamiento:

**Si el pago es ACEPTADO:**
```
Pago ACEPTADO.
```

**Si el pago es RECHAZADO:**
```
Pago rechazado. Verifique sus datos.
```
Se regresa al menú del carrito.

---

## 📄 Generación de Factura

Cuando el pago es aceptado, se genera automáticamente una factura:

```
-----FACTURA-----

Identificacion: 1234567890
Total a pagar: $1.505,00
Fecha: 18-03-26
Metodo de pago: Tarjeta

-----SU RECIBO HA SIDO GENERADO-----
```

La factura muestra:
- **Identificación:** Número de identificación del cliente
- **Total a pagar:** Monto final (con descuentos aplicados)
- **Fecha:** De la transacción (DD-MM-AA)
- **Método de pago:** El método seleccionado en el checkout

Después de la factura, regresa automáticamente al **Menú Principal**.

---

### Opción 0: Volver

Regresa al menú anterior (Menú Principal en caso del carrito).

---

## 🏁 Salir de la Aplicación (Opción 0 del Menú Principal)

Seleccione la opción `0` en el Menú Principal para salir:

```
Gracias por su visita.
```

La aplicación se cierra.

---

## 📝 Ejemplo de Flujo Completo: Compra de 2 Productos

### 1. Iniciar sesión
```
-----BIENVENIDO-----

1.Iniciar Sesion
2.Registrarse

Seleccione una opcion: 1

Ingrese su Usuario: samuel
Ingrese su Contrasena: 123

Usuario: samuel
INICIO EXITOSO
```

### 2. Ir a la sección de Productos
```
-----MENU PRINCIPAL-----

1.CLIENTE
2.PRODUCTOS
3.CARRITO
0.SALIR

Ingrese una opcion: 2

PRODUCTO
1.Agregar al carrito
0.Volver

Elija una opcion: 1
```

### 3. Agregar productos al carrito
```
LISTA DE PRODUCTOS:
  ID: 1, Nombre: Portatil, Precio: $1.000,00, Stock: 1
  ID: 2, Nombre: Celular, Precio: $550,00, Stock: 1
  ID: 3, Nombre: Airpods, Precio: $80,00, Stock: 1
  ID: 4, Nombre: Tarjeta Grafica, Precio: $800,00, Stock: 1
  ID: 5, Nombre: Mouse, Precio: $22,90, Stock: 1
  ID: 6, Nombre: Parlantes, Precio: $300,00, Stock: 1
  ID: 7, Nombre: Bose, Precio: $400,00, Stock: 1

Ingrese el ID del producto que desea agregar al carrito (0 para salir):
> 2
Producto 'Celular' agregado al carrito.

> 5
Producto 'Mouse' agregado al carrito.

> 0
```

### 4. Verificar carrito
```
-----MENU PRINCIPAL-----

1.CLIENTE
2.PRODUCTOS
3.CARRITO
0.SALIR

Ingrese una opcion: 3

-----CARRITO-----

PRODUCTOS EN EL CARRITO:
  ID: 2, Nombre: Celular, Precio: $550,00, Cantidad: 1
  ID: 5, Nombre: Mouse, Precio: $22,90, Cantidad: 1

1.Sacar producto
2.Calcular total
3.Hacer pago
0.Volver

Elija una opcion: 2

Total del Carrito: $570,71
```

(Descuentos aplicados: 10% sobre total + $5 fijos)

### 5. Realizar el pago
```
Elija una opcion: 3

-----PAGAR-----

Ingrese el metodo de pago (Tarjeta, Efectivo, Cuenta): Tarjeta

Ingrese numero de cuenta: 1234567890

Total a pagar: $570,71

1.Confirmar pago
2.Cancelar

Elija una opcion: 1

Ingrese su identificacion: 1001

Pago ACEPTADO.

-----FACTURA-----

Identificacion: 1001
Total a pagar: $570,71
Fecha: 18-03-26
Metodo de pago: Tarjeta

-----SU RECIBO HA SIDO GENERADO-----
```

---

## ⚠️ Validaciones Importantes

La aplicación incluye las siguientes validaciones:

### Para el Registro:
- **Nombre y Apellido:** Solo letras (sin números ni caracteres especiales)
- **Email:** Máximo 5 caracteres, solo letras y números
- **Teléfono:** Exactamente 10 dígitos
- **Contraseña:** Texto libre

### Para Agregar Productos:
- El producto debe existir
- Debe haber stock disponible
- Se debe ingresar un ID válido

### Para Remover Productos:
- El producto debe estar en el carrito
- La cantidad debe ser válida

### Para el Pago:
- El carrito no debe estar vacío
- El método de pago debe ser válido (Tarjeta, Efectivo o Cuenta)
- Si no es Efectivo, se debe ingresar un número de cuenta válido
- La identificación debe ser un número válido

---

## 🎯 Sistema de Descuentos

El carrito aplica descuentos automáticos:
- **Descuento por porcentaje:** 10%
- **Descuento adicional fijo:** $5

Estos descuentos se aplican automáticamente al calcular el total del carrito.

---

## 💾 Datos Persistentes

**Nota:** Los datos se almacenan en memoria durante la sesión. Cuando se cierra la aplicación, toda la información se pierde. La próxima vez que se ejecute, se reiniciará con los productos y usuario predeterminados.

---

## 🔄 Flujo de Menús

```
┌──────────────────────────┐
│  PANTALLA DE BIENVENIDA  │
│  (Iniciar/Registrarse)   │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│   MENÚ PRINCIPAL         │
│ 1. Cliente               │
│ 2. Productos             │
│ 3. Carrito               │
│ 0. Salir                 │
└────┬─────────┬───────────┘
     │         │
     ▼         ▼
┌─────────┐ ┌──────────────┐
│ CLIENTE │ │  PRODUCTOS   │
│ Menú    │ │ Agregar      │
│         │ │ al Carrito   │
└─────────┘ └──────────────┘
     │              │
     └──────┬───────┘
            ▼
    ┌────────────────┐
    │ CARRITO        │
    │ • Sacar        │
    │ • Calcular     │
    │ • Pagar        │
    └────────┬───────┘
             │
        ┌────▼─────┐
        │   PAGO    │
        │ • Método  │
        │ • Confirmar
        │ • Factura │
        └───────────┘
```

---

**Última actualización:** 18 de Marzo de 2026

