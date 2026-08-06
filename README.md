# EsavApi.Validador.NEAR

Servicio API desarrollado en **.NET Framework 4.7.2** diseñado para la validación asíncrona de **comprobantes de baja / anulaciones**, implementando un flujo de **procesamiento diferido (Delayed Processing)** de 1 hora mediante servicios de **Azure Storage**.

---

## ⚡ Diferencial Técnico: Procesamiento Diferido vs. Tiempo Real

A diferencia de los validadores inmediatos, este servicio gestiona el ciclo de vida de documentos de baja que requieren una ventana de espera reglamentaria o de negocio antes de su procesamiento definitivo:

* **Procesamiento Programado / Scheduled:** Encola y programa las solicitudes para ejecutarse tras **1 hora de diferimiento** (`Initial Visibility Delay` en Azure Storage Queue), asegurando la ventana de tiempo requerida.
* **Almacenamiento y Trazabilidad:** Almacena los archivos binarios en **Azure Blob Storage** y registra el histórico, estados y metadatos en **Azure Table Storage**.
* **Ejecución Asíncrona:** Desacopla la recepción de la solicitud de su validación final para optimizar recursos.

---

## 🏗️ Arquitectura de la Solución

El proyecto aplica una separación clara de responsabilidades estructurada en capas:

* `EsavApi.Validador.NEAR.BE`: **Business Entities** (Modelos de datos, DTOs y contratos).
* `EsavApi.Validador.NEAR.BR`: **Business Rules** (Lógica de validación de bajas y temporizadores de negocio).
* `EsavApi.Validador.NEAR.DA`: **Data Access** (Clientes de Azure Queues, Blob Storage y Table Storage).
* `EsavApi.Validador.NEAR.UTIL`: **Utilities** (Helpers de encriptación, serialización y logging).
* `EsavApi.Validador.NEAR`: **API Service** (Controladores REST y configuración del servicio).

---

## 🛠️ Tecnologías Utilizadas

* **Framework:** .NET Framework 4.7.2 (C#)
* **Servicios de Azure:**
  * **Azure Storage Queues:** Manejo de colas con temporización y retardo de visibilidad para tareas diferidas.
  * **Azure Blob Storage:** Resguardo de documentos e insumos de baja.
  * **Azure Table Storage:** Almacenamiento NoSQL rápido para logs y control de estados.
* **Patrones:** Layered Architecture, Deferred Message Pattern, Async/Await, Centralized Logging.

---

## 🚀 Configuración Local

1. Clonar el repositorio:
   ```bash
   git clone [https://github.com/Guillermo24-10/EsavApi.Validador.NEAR.git](https://github.com/Guillermo24-10/EsavApi.Validador.NEAR.git)
