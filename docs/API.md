# Property Management API Documentation

## Base URL
```
https://localhost:7001/api/v1
```

## Authentication
Currently, the API does not require authentication. This will be added in future versions.

## Endpoints

### Properties

#### Get All Properties
```http
GET /properties
```

**Query Parameters:**
- `type` (optional): Property type (Apartment, House, Villa, etc.)
- `status` (optional): Property status (Available, Rented, Sold, etc.)
- `city` (optional): City name
- `minPrice` (optional): Minimum price
- `maxPrice` (optional): Maximum price
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)

**Response:**
```json
[
  {
    "id": 1,
    "title": "شقة فاخرة في وسط القاهرة",
    "description": "شقة مفروشة بالكامل في موقع متميز",
    "address": "شارع التحرير، وسط البلد، القاهرة",
    "city": "القاهرة",
    "type": "Apartment",
    "status": "Available",
    "price": 15000,
    "currency": "EGP",
    "bedrooms": 3,
    "bathrooms": 2,
    "area": 120,
    "owner": {
      "id": 1,
      "fullName": "أحمد محمد"
    }
  }
]
```

#### Get Property by ID
```http
GET /properties/{id}
```

#### Create Property
```http
POST /properties
```

**Request Body:**
```json
{
  "title": "شقة جديدة",
  "description": "وصف العقار",
  "address": "العنوان",
  "city": "المدينة",
  "type": "Apartment",
  "status": "Available",
  "price": 10000,
  "currency": "EGP",
  "bedrooms": 2,
  "bathrooms": 1,
  "area": 100,
  "ownerId": 1
}
```

#### Update Property
```http
PUT /properties/{id}
```

#### Delete Property
```http
DELETE /properties/{id}
```

### Contracts

#### Get All Contracts
```http
GET /contracts
```

#### Get Contract by ID
```http
GET /contracts/{id}
```

#### Create Contract
```http
POST /contracts
```

#### Terminate Contract
```http
PUT /contracts/{id}/terminate
```

### Payments

#### Get All Payments
```http
GET /payments
```

#### Create Payment
```http
POST /payments
```

#### Mark Payment as Paid
```http
PUT /payments/{id}/mark-paid
```

### Maintenance Requests

#### Get All Maintenance Requests
```http
GET /maintenance
```

#### Create Maintenance Request
```http
POST /maintenance
```

#### Update Maintenance Request Status
```http
PUT /maintenance/{id}/status
```

## Error Responses

All endpoints return standard HTTP status codes:

- `200 OK`: Success
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

Error response format:
```json
{
  "error": "Error message",
  "details": "Detailed error information"
}
```