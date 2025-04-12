# Tài liệu dự án Todo API (.NET 8.0)

## 1. Giới thiệu (Introduction)

![alt text](<Untitled diagram-2025-04-11-175743.png>)

### 1.1. Mục tiêu dự án (Project Goals)

Todo API là một ứng dụng API hiện đại được xây dựng trên nền tảng .NET Core 8.0, sử dụng kiến trúc CQRS (Command Query Responsibility Segregation) để tách biệt hoạt động đọc và ghi dữ liệu. Mục tiêu của dự án là:

- Xây dựng hệ thống API quản lý công việc (Todo) với đầy đủ các chức năng CRUD (Create, Read, Update, Delete)
- Áp dụng kiến trúc CQRS để tối ưu hiệu suất và khả năng mở rộng
- Sử dụng PostgreSQL làm cơ sở dữ liệu chính
- Sử dụng Kafka để xử lý sự kiện và đồng bộ dữ liệu giữa các service
- Sử dụng Elasticsearch để tìm kiếm và phân tích dữ liệu
- Tuân thủ nguyên tắc Clean Architecture để dễ bảo trì và mở rộng

### 1.2. Phạm vi dự án (Scope)

- **Quản lý Todo**: Thêm, sửa, xóa, xem danh sách, tìm kiếm công việc
- **CQRS Pattern**: Tách biệt xử lý đọc và ghi với hai API riêng biệt
- **Event Sourcing**: Sử dụng Kafka để xử lý sự kiện và đồng bộ dữ liệu
- **Tìm kiếm**: Sử dụng Elasticsearch để tìm kiếm nhanh và chính xác

## 2. Kiến trúc hệ thống (System Architecture)

### 2.1. Sơ đồ tổng quan

Hệ thống được tổ chức theo mô hình CQRS với các thành phần chính:

1. **Write API**: Xử lý các thao tác thay đổi dữ liệu (Create, Update, Delete)
2. **Read API**: Xử lý các thao tác đọc dữ liệu (Query, Search)
3. **PostgreSQL**: Lưu trữ dữ liệu chính
4. **Kafka**: Message broker để xử lý sự kiện và đồng bộ dữ liệu
5. **Elasticsearch**: Cơ sở dữ liệu tìm kiếm

Luồng dữ liệu:

- Khi có thao tác thay đổi dữ liệu, Write API cập nhật vào PostgreSQL và gửi sự kiện vào Kafka
- Kafka consumer nhận sự kiện và cập nhật dữ liệu vào Elasticsearch
- Read API đọc dữ liệu từ Elasticsearch để phục vụ truy vấn và tìm kiếm

### 2.2. Công nghệ sử dụng (Tech Stack)

- **.NET Core 8.0**: Framework phát triển API
- **Entity Framework Core**: ORM để tương tác với database
- **MediatR**: Thực hiện CQRS pattern và Mediator pattern
- **PostgreSQL**: Cơ sở dữ liệu chính
- **Kafka**: Message broker để xử lý sự kiện
- **Elasticsearch**: Cơ sở dữ liệu tìm kiếm
- **Serilog**: Logging framework
- **Swagger/OpenAPI**: Tài liệu API
- **Docker & Docker Compose**: Containerization và orchestration

### 2.3. Các dịch vụ bên thứ 3 (Third-party Services)

- **PostgreSQL**: Cơ sở dữ liệu quan hệ
- **Elasticsearch**: Cơ sở dữ liệu tìm kiếm
- **Kafka**: Message broker
- **Kibana**: Giao diện trực quan hóa dữ liệu Elasticsearch
- **Kafka UI**: Giao diện quản lý Kafka

## 3. Hướng dẫn cài đặt & chạy dự án (Setup Guide)

### 3.1. Yêu cầu môi trường

#### Yêu cầu cơ bản:

- .NET SDK 8.0 hoặc cao hơn
- Docker và Docker Compose
- Git

#### Yêu cầu tùy chọn (cho các tính năng đầy đủ):

- Visual Studio 2022 hoặc JetBrains Rider
- Entity Framework Core Tools (dotnet-ef)

### 3.2. Các bước cài đặt

#### Phương pháp 1: Sử dụng .NET CLI

1. Clone repository:

   ```bash
   git clone https://github.com/your-repo/todoapp.git
   cd todoapp
   ```

2. Khởi động các dịch vụ phụ thuộc bằng Docker Compose:

   ```bash
   docker-compose up -d postgres kafka elasticsearch
   ```

3. Chạy migrations để tạo cơ sở dữ liệu:

   ```sql
   -- Tạo bảng Todos
   CREATE TABLE "Todos" (
    "Id" UUID NOT NULL PRIMARY KEY,
    "Title" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "IsCompleted" BOOLEAN NOT NULL,
    "Priority" INTEGER NOT NULL,
    "DueDate" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    "UpdatedAt" TIMESTAMPTZ
   );

   -- Tạo index trên cột IsCompleted
   CREATE INDEX "IX_Todos_IsCompleted" ON "Todos" ("IsCompleted");

   -- Tạo index trên cột Priority
   CREATE INDEX "IX_Todos_Priority" ON "Todos" ("Priority");

   -- Tạo index trên cột DueDate
   CREATE INDEX "IX_Todos_DueDate" ON "Todos" ("DueDate");

   ```

4. Chạy Write API:

   ```bash
   dotnet run
   ```

5. Mở terminal khác và chạy Read API:
   ```bash
   cd src/TodoApp.ReadApi
   dotnet run
   ```
6. Mở terminal khác và chạy ApiGateway: (cái này không cần mở cũng được cho đỡ tốn time || chạy chay 2 service api trên là được)
   ```bash
   cd src/TodoApp.ApiGateway
   dotnet run
   ```

#### Phương pháp 2: Sử dụng Docker

1. Clone repository:

   ```bash
   git clone https://github.com/thuanlevan72/Redis.Elasticsearch.Kafka.Net.Advanced
   cd src
   ```

2. Khởi động tất cả các dịch vụ bằng Docker Compose:
   ```bash
   docker-compose up -d
   ```

### 3.3. Cấu hình biến môi trường (Environment Variables)

Ứng dụng sử dụng các biến môi trường sau:

- **PostgreSQL\_\_Host**: Host của PostgreSQL (mặc định: localhost)
- **PostgreSQL\_\_Port**: Port của PostgreSQL (mặc định: 5432)
- **PostgreSQL\_\_Database**: Tên database (mặc định: tododb)
- **PostgreSQL\_\_Username**: Tên đăng nhập PostgreSQL (mặc định: postgres)
- **PostgreSQL\_\_Password**: Mật khẩu PostgreSQL (mặc định: postgres)
- **KafkaSettings\_\_BootstrapServers**: Danh sách máy chủ Kafka (mặc định: localhost:9092)
- **KafkaSettings\_\_TodoEventsTopic**: Tên topic cho sự kiện Todo (mặc định: todo-events)
- **ElasticsearchSettings\_\_Url**: URL của Elasticsearch (mặc định: http://localhost:9200)

### 3.4. Troubleshooting

#### Vấn đề kết nối cơ sở dữ liệu

- Đảm bảo PostgreSQL đang chạy và có thể kết nối được
- Kiểm tra thông tin kết nối (host, port, username, password) trong appsettings.json

#### Lỗi phân tích cú pháp chuỗi kết nối

- Đảm bảo chuỗi kết nối có định dạng đúng
- Kiểm tra các ký tự đặc biệt trong mật khẩu

#### Lỗi khi chạy migration

- Đảm bảo đã cài đặt Entity Framework Core Tools
- Kiểm tra database có tồn tại và có quyền truy cập

#### Docker phổ biến

- Kiểm tra logs: `docker-compose logs -f [service-name]`
- Khởi động lại service: `docker-compose restart [service-name]`
- Xóa và tạo lại containers: `docker-compose down && docker-compose up -d`

## 4. Đặc tả chi tiết tính năng (Feature Specifications)

### 4.1. User Stories/Use Cases

#### Quản lý Todo

- Là người dùng, tôi muốn tạo mới một công việc với tiêu đề, mô tả, mức độ ưu tiên và ngày hạn
- Là người dùng, tôi muốn xem danh sách các công việc
- Là người dùng, tôi muốn cập nhật thông tin của một công việc
- Là người dùng, tôi muốn đánh dấu một công việc là đã hoàn thành hoặc chưa hoàn thành
- Là người dùng, tôi muốn xóa một công việc
- Là người dùng, tôi muốn tìm kiếm công việc theo từ khóa
- Là người dùng, tôi muốn lọc danh sách công việc theo trạng thái hoàn thành hoặc mức độ ưu tiên

### 4.2. API Endpoints

#### Write API (Port 8000)

- **POST /api/todos**

  - Tạo mới một Todo
  - Request Body: `{ "title": "string", "description": "string", "priority": int, "dueDate": "datetime" }`
  - Response: 201 Created với ID của Todo

- **PUT /api/todos/{id}**

  - Cập nhật một Todo
  - Request Body: `{ "id": "guid", "title": "string", "description": "string", "isCompleted": boolean, "priority": int, "dueDate": "datetime" }`
  - Response: 204 No Content

- **DELETE /api/todos/{id}**

  - Xóa một Todo
  - Response: 204 No Content

- **PATCH /api/todos/{id}/complete**

  - Đánh dấu Todo là đã hoàn thành
  - Response: 204 No Content

- **PATCH /api/todos/{id}/incomplete**
  - Đánh dấu Todo là chưa hoàn thành
  - Response: 204 No Content

#### Read API (Port 5000)

- **GET /api/todos/{id}**

  - Lấy thông tin chi tiết của một Todo
  - Response: 200 OK với dữ liệu Todo

- **GET /api/todos**

  - Lấy danh sách Todo với phân trang
  - Query Parameters: `pageNumber`, `pageSize`, `isCompleted`, `priority`
  - Response: 200 OK với danh sách Todo và thông tin phân trang

- **GET /api/todos/search**
  - Tìm kiếm Todo theo từ khóa
  - Query Parameters: `term`, `pageNumber`, `pageSize`
  - Response: 200 OK với danh sách Todo phù hợp và thông tin phân trang

## 5. Phụ lục (Appendix)

### 5.1. Cấu trúc dự án

- **TodoApp.Domain**: Chứa các entity, interfaces, exceptions và logic nghiệp vụ cốt lõi
- **TodoApp.Application**: Chứa các use cases, commands, queries và validators
- **TodoApp.Infrastructure**: Chứa các triển khai cụ thể của interfaces, repositories, services
- **TodoApp.WriteApi**: API xử lý các thao tác ghi
- **TodoApp.ReadApi**: API xử lý các thao tác đọc

### 5.2. Mô hình CQRS

Command Query Responsibility Segregation (CQRS) là một pattern tách biệt các thao tác đọc (queries) và ghi (commands) thành các mô hình riêng biệt. Trong dự án này:

- **Commands**: Xử lý các thao tác thay đổi dữ liệu (Create, Update, Delete)
- **Queries**: Xử lý các thao tác truy vấn dữ liệu (Read, Search)

### 5.3. Event Sourcing với Kafka

Khi có thay đổi dữ liệu:

1. Write API cập nhật PostgreSQL
2. Write API gửi sự kiện vào Kafka (TodoCreatedEvent, TodoUpdatedEvent, TodoDeletedEvent)
3. Kafka Consumer nhận sự kiện và cập nhật dữ liệu vào Elasticsearch
4. Read API đọc dữ liệu từ Elasticsearch để phục vụ các query

Lợi ích:

- Tách biệt nguồn dữ liệu cho đọc và ghi
- Khả năng mở rộng tốt hơn
- Tối ưu hiệu suất cho từng loại thao tác
## 6. Mô tả về Truy vấn LINQ và Elasticsearch với NEST (.NET) (để có cái nhìn trực quan về truy vấn trong cả 2)

Tài liệu mô tả cú pháp truy vấn từ **cơ bản đến nâng cao** trong:

- LINQ (áp dụng cho danh sách hoặc EF Core)
- Elasticsearch với thư viện **NEST**

---

## ✅ Truy vấn Cơ bản

| Mục tiêu | LINQ | NEST |
|----------|------|------|
| **Lọc theo điều kiện (Where)** | `list.Where(x => x.Price > 100)` | ```Query(q => q.Range(r => r.Field(f => f.Price).GreaterThan(100)))``` |
| **Lọc nhiều điều kiện** | `list.Where(x => x.Price > 100 && x.Status == "Active")` | ```Query(q => q.Bool(b => b.Must( q.Range(r => r.Field(f => f.Price).GreaterThan(100)), q.Term(t => t.Field(f => f.Status).Value("Active")))))``` |
| **Sắp xếp tăng dần** | `list.OrderBy(x => x.Name)` | ```Sort(s => s.Ascending(f => f.Name))``` |
| **Sắp xếp giảm dần** | `list.OrderByDescending(x => x.CreatedDate)` | ```Sort(s => s.Descending(f => f.CreatedDate))``` |
| **Lấy 1 phần tử đầu tiên** | `list.FirstOrDefault(x => x.Id == id)` | ```Query(q => q.Term(t => t.Field(f => f.Id).Value(id)))``` + `.Size(1)` |
| **Phân trang (skip/take)** | `.Skip(10).Take(20)` | `.From(10).Size(20)` |

---

## 🔍 Truy vấn Trung bình

| Mục tiêu | LINQ | NEST |
|----------|------|------|
| **Chứa chuỗi (contains)** | `list.Where(x => x.Name.Contains("abc"))` | ```Query(q => q.Match(m => m.Field(f => f.Name).Query("abc")))``` |
| **Tìm trong danh sách (Any)** | `list.Where(x => x.Tags.Any(tag => tag == "Hot"))` | ```Query(q => q.Terms(t => t.Field(f => f.Tags).Terms("Hot")))``` |
| **Tìm nhiều giá trị (IN)** | `list.Where(x => new[] { "a", "b" }.Contains(x.Category))` | ```Query(q => q.Terms(t => t.Field(f => f.Category).Terms("a", "b")))``` |
| **Không chứa (NOT)** | `list.Where(x => !x.Tags.Contains("abc"))` | ```Query(q => q.Bool(b => b.MustNot(q.Term(t => t.Field(f => f.Tags).Value("abc")))))``` |
| **Tìm theo khoảng ngày** | `list.Where(x => x.Date >= from && x.Date <= to)` | ```Query(q => q.DateRange(r => r.Field(f => f.Date).GreaterThanOrEquals(from).LessThanOrEquals(to)))``` |

---

## 🚀 Truy vấn Nâng cao

| Mục tiêu nâng cao | LINQ | NEST |
|------------------|------|------|
| **Full-text search** | `list.Where(x => x.Description.Contains("keyword"))` | ```Query(q => q.Match(m => m.Field(f => f.Description).Query("keyword")))``` |
| **Wildcard / fuzzy** | Không có trực tiếp | ```Query(q => q.Wildcard(w => w.Field(f => f.Name).Value("s*ring")))``` |
| **Bool query (nhiều điều kiện phức tạp)** | `.Where(x => x.A > 10 && (x.B == "abc" || x.C < 5))` | ```Query(q => q.Bool(b => b.Must( q.Range(r => r.Field(f => f.A).GreaterThan(10)), b.Should( q.Term(t => t.Field(f => f.B).Value("abc")), q.Range(r => r.Field(f => f.C).LessThan(5))) )))``` |
| **Truy vấn lồng nhau (nested)** | `.Where(x => x.Manufacturer.Country == "VN")` | ```Query(q => q.Nested(n => n.Path(p => p.Manufacturer).Query(nq => nq.Term(t => t.Field("manufacturer.country").Value("VN")))))``` |
| **Truy vấn theo object con** | `.Where(x => x.Dimensions.Height > 100)` | ```Query(q => q.Range(r => r.Field("dimensions.height").GreaterThan(100)))``` |
| **Aggregation (GroupBy)** | `list.GroupBy(x => x.Category)` | ```Aggregations(a => a.Terms("by_category", t => t.Field(f => f.Category)))``` |
| **Đếm số bản ghi** | `list.Count()` | `.Size(0) + Aggregation.Count("total")` |

---


##7. Tài liệu So sánh Truy vấn LINQ và NEST (Elasticsearch)

## Giới thiệu
Tài liệu này cung cấp các ví dụ minh họa cho việc chuyển đổi truy vấn từ LINQ sang NEST (Elasticsearch). Dưới đây là bảng so sánh các toán tử cơ bản, ví dụ và cách thực hiện tương đương trong NEST.

### Các truy vấn cơ bản
1. **Select / SelectMany**
2. **Where** (`==`, `Contains`, `Any`, `All`, `||`, `&&`)
3. **First / Last**
4. **OrderBy (ASC, DESC)**
5. **Paging (Skip, Take)**

---

## 1. `Select` và `SelectMany`

- **LINQ (Select)**: Dùng để ánh xạ dữ liệu từ một tập hợp.
- **NEST (Elasticsearch)**: Dùng `SourceIncludes` hoặc `Fields`.

### LINQ:
```csharp
var result = products.Select(p => new { p.Name, p.Price });
```

### NEST (Elasticsearch):
```csharp
var result = client.Search<ProductDocument>(s => s
    .SourceIncludes(f => f.Name, f => f.Price)
);
```

---

## 2. `Where` với `==`, `Contains`, `Any`, `All`, `||`, `&&`

- **LINQ Where**: Dùng để lọc các đối tượng theo các điều kiện.
- **NEST (Elasticsearch)**: Dùng `Query` trong NEST để lọc các tài liệu theo các điều kiện khác nhau.

### LINQ `==`:
```csharp
var result = products.Where(p => p.Category == "Electronics");
```

### NEST `==`:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Term(t => t.Category, "Electronics")
    )
);
```

### LINQ `Contains`:
```csharp
var result = products.Where(p => p.Name.Contains("Hat"));
```

### NEST `Contains`:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Match(m => m
            .Field(f => f.Name)
            .Query("Hat")
        )
    )
);
```

### LINQ `Any`:
```csharp
var result = products.Where(p => p.Tags.Any(tag => tag == "New"));
```

### NEST `Any`:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Terms(t => t.Tags, new[] { "New" })
    )
);
```

### LINQ `All`:
```csharp
var result = products.Where(p => p.Tags.All(tag => tag.StartsWith("A")));
```

### NEST `All`:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Bool(b => b
            .Must(m => m
                .Terms(t => t.Tags, new[] { "A" })
            )
        )
    )
);
```

### LINQ với `||` (OR):
```csharp
var result = products.Where(p => p.Category == "Electronics" || p.Category == "Clothing");
```

### NEST với `||` (OR):
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Bool(b => b
            .Should(
                sh => sh.Term(t => t.Category, "Electronics"),
                sh => sh.Term(t => t.Category, "Clothing")
            )
        )
    )
);
```

### LINQ với `&&` (AND):
```csharp
var result = products.Where(p => p.Category == "Electronics" && p.Price > 100);
```

### NEST với `&&` (AND):
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Bool(b => b
            .Must(m => m
                .Term(t => t.Category, "Electronics"),
                m => m
                .Range(r => r.Field(f => f.Price).GreaterThan(100))
            )
        )
    )
);
```

---

## 3. `First` và `Last`

- **LINQ First**: Lấy phần tử đầu tiên thỏa mãn điều kiện.
- **NEST First**: Lấy phần tử đầu tiên trong kết quả.

### LINQ `First`:
```csharp
var product = products.First(p => p.Price > 100);
```

### NEST `First`:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Range(r => r.Field(f => f.Price).GreaterThan(100))
    )
    .Size(1)
);
```

### LINQ `Last`:
```csharp
var product = products.Last(p => p.Price > 100);
```

### NEST `Last`:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Query(q => q
        .Range(r => r.Field(f => f.Price).GreaterThan(100))
    )
    .Sort(s => s.Descending(p => p.ManufacturingDate))
    .Size(1)
);
```

---

## 4. `OrderBy` (ASC, DESC)

- **LINQ OrderBy**: Dùng để sắp xếp dữ liệu theo thứ tự tăng dần.
- **NEST OrderBy**: Dùng `Sort` trong Elasticsearch để sắp xếp.

### LINQ `OrderBy` ASC:
```csharp
var result = products.OrderBy(p => p.Price);
```

### NEST `OrderBy` ASC:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Sort(st => st
        .Ascending(p => p.Price)
    )
);
```

### LINQ `OrderBy` DESC:
```csharp
var result = products.OrderByDescending(p => p.Price);
```

### NEST `OrderBy` DESC:
```csharp
var result = client.Search<ProductDocument>(s => s
    .Sort(st => st
        .Descending(p => p.Price)
    )
);
```

---

## 5. Paging (Skip, Take)

- **LINQ Paging**: Dùng `Skip` và `Take` để phân trang.
- **NEST Paging**: Dùng `From` và `Size` trong Elasticsearch.

### LINQ Paging:
```csharp
var result = products.Skip(10).Take(20);
```

### NEST Paging:
```csharp
var result = client.Search<ProductDocument>(s => s
    .From(10)
    .Size(20)
);
```

---
