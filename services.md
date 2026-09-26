# NetNotepad — API

Документ содержит только сервисы, HTTP-ручки, методы, статус-коды и назначение ответов.

## Общий принцип

Большинство внешних операций выполняются асинхронно.

Для асинхронных ручек можно разделить два этапа:

1. `HttpHandler` отправляет запрос в RabbitMQ и получает через Direct Reply-to только результат проверки/принятия запроса.
2. После успешной проверки сама операция продолжает выполняться в фоне.
3. Для обычных async-ручек клиент получает `202 Accepted` и `requestId`.
4. `requestId` используется для просмотра статуса и получения готового результата.

---

# Статус-коды

| Код | Значение |
| --- | --- |
| 200 | Успешный ответ |
| 201 | Ресурс создан |
| 202 | Запрос принят и выполняется в фоне |
| 204 | Успех без body |
| 400 | Некорректный запрос |
| 401 | Отсутствует или недействительна авторизация |
| 403 | Операция запрещена |
| 404 | Объект или запрос не найден |
| 409 | Конфликт состояния / бизнес-конфликт |
| 429 | Превышен rate limit |
| 500 | Внутренняя ошибка |
| 503 | Сервис временно не может принимать работу |

Бизнес-причина ошибки передаётся в `code`.

Пример:

```json
{
  "code": "FRIEND_REQUEST_FORBIDDEN",
  "message": "User does not accept friend requests"
}
```

---

# Auth Service

## POST /auth/register

Регистрация пользователя.

**Успех:**

- `201 Created` — пользователь создан.

**Ошибки:**

- `400 Bad Request`
- `409 Conflict`
- `500 Internal Server Error`

---

## POST /auth/login

Аутентификация пользователя.

**Успех:**

- `200 OK` — возвращает результат аутентификации.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `500 Internal Server Error`

---

## POST /auth/refresh

Обновление JWT и Refresh Token.

**Успех:**

- `200 OK` — возвращает новую пару токенов.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `409 Conflict`
- `500 Internal Server Error`

---

## PATCH /auth/update

Изменение данных пользователя в Auth Service.

**Успех:**

- `202 Accepted` — запрос принят в фоновую обработку.
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `409 Conflict`
- `500 Internal Server Error`

---

## POST /auth/logout

Выход пользователя.

Ручка только подтверждает, что запрос на logout принят.

**Успех:**

- `200 OK` — запрос принят и отправлен на выполнение.

**Ошибки:**

- `500 Internal Server Error` — запрос не удалось принять/отправить.

---

## POST /auth/logout/all

Выход пользователя со всех устройств.

**Успех:**

- `200 OK` — запрос принят и отправлен на выполнение.

**Ошибки:**

- `500 Internal Server Error`

---

## DELETE /auth/remove

Удаление пользователя.

**Успех:**

- `202 Accepted` — запрос принят в фоновую обработку.
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `409 Conflict`
- `500 Internal Server Error`

---

# User Service

## GET /user

Получение данных текущего пользователя.

**Успех:**

- `200 OK` — данные пользователя.

**Ошибки:**

- `401 Unauthorized`
- `404 Not Found`
- `500 Internal Server Error`

---

## PATCH /user

Изменение данных текущего пользователя.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `409 Conflict`
- `500 Internal Server Error`

---

## GET /user/search

Поиск пользователей.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

Результат поиска получается через `/request/result`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `429 Too Many Requests`
- `500 Internal Server Error`

---

## GET /friends

Получение списка друзей.

**Успех:**

- `200 OK` — список друзей.

**Ошибки:**

- `401 Unauthorized`
- `500 Internal Server Error`

---

## POST /friends

Отправка запроса в друзья.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `409 Conflict`
- `500 Internal Server Error`

Пример бизнес-ошибки:
`FRIEND_REQUEST_FORBIDDEN` — пользователь запрещает добавлять его в друзья.

---

## DELETE /friends

Удаление пользователя из друзей.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `500 Internal Server Error`

---

## GET /friends/requests

Получение запросов в друзья.

**Успех:**

- `200 OK` — список запросов.

**Ошибки:**

- `401 Unauthorized`
- `500 Internal Server Error`

---

## PATCH /friends/requests

Принятие или отклонение запроса в друзья.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

---

## POST /user/friends/icons

Получение иконок друзей по hash.

Клиент передаёт список `hashes`.

Сервис проверяет доступ к hash, удаляет дубли и запускает обработку в фоне.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

Результат получается через `/request/result`, по одному готовому chunk за запрос.

Размер chunk и количество chunk являются внутренними параметрами сервиса.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `429 Too Many Requests`
- `500 Internal Server Error`
- `503 Service Unavailable`

---

# Notepad Service

## GET /notepad

Получение списка блокнотов.

**Успех:**

- `200 OK` — список блокнотов.

**Ошибки:**

- `401 Unauthorized`
- `500 Internal Server Error`

---

## POST /notepad

Создание блокнота.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `409 Conflict`
- `500 Internal Server Error`

---

## PATCH /notepad

Изменение блокнота.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

---

## DELETE /notepad

Удаление блокнота.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `500 Internal Server Error`

---

## POST /notepad/share

Предоставление доступа к блокноту.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

---

## POST /notepad/unshare

Отзыв доступа к блокноту.

**Успех:**

- `202 Accepted`
- Возвращает `requestId`.

**Ошибки:**

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

---

# Request Service

Общий API для просмотра состояния асинхронных операций и получения их результатов.

## GET /request

Получение списка собственных запросов.

Каждый элемент содержит только:

```json
{
  "id": "user:123",
  "endpoint": "/user/update",
  "status": "Processing"
}
```

**Успех:**

- `200 OK` — список запросов.

**Ошибки:**

- `401 Unauthorized`
- `500 Internal Server Error`

Состояния:

```text
Accepted
Processing
Completed
Failed
Expired
```

---

## POST /request/result

Получение результата асинхронного запроса.

Тело:

```json
{
  "requestId": "user:123"
}
```

Проверяется принадлежность запроса текущему пользователю.

Для обычных операций возвращается готовый результат.

Для `/user/friends/icons` возвращается один готовый chunk за запрос.

**Ответы:**

- `200 OK` — результат или один готовый chunk;
- `403 Forbidden` — запрос принадлежит другому пользователю;
- `404 Not Found` — запрос или результат не существует;
- `409 Conflict` — запрос ещё выполняется.

---

# Общий контракт async API

Обычная асинхронная операция:

```http
POST/PATCH/DELETE ...
```

возвращает:

```http
202 Accepted
Content-Type: application/json
```

```json
{
  "requestId": "service:guid"
}
```

После этого клиент может проверить состояние:

```http
GET /request
```

и забрать результат:

```http
POST /request/result
{
  "requestId": "service:guid"
}
```

---

# Direct Reply для проверки async-ручек

Для асинхронных операций `HttpHandler` может использовать RabbitMQ Direct Reply-to только для короткой синхронной проверки:

```text
HTTP
  |
  v
HttpHandler
  |
  +--> RabbitMQ request
  |       |
  |       +--> Direct Reply-to
  |               |
  |               v
  |          ACCEPT / ERROR
  |
  +--> если ACCEPT:
          вернуть 202 + RequestID
          |
          v
       выполнение в фоне
```

То есть HTTP-запрос не ждёт завершения бизнес-операции. Direct Reply нужен только для подтверждения, что запрос принят и может быть запущен.

`/auth/register`, `/auth/login` и `/auth/refresh` остаются полноценными синхронными операциями: HTTP ждёт их результат.

`/auth/logout` и `/auth/logout/all` — особый случай: HTTP ждёт только подтверждение отправки запроса и возвращает `200`, либо `500`.
