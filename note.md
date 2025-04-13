1. Các cấu hình bắt buộc trên PostgreSQL
   a. Bật logical replication (WAL level)
   Debezium cần PostgreSQL ghi lại thay đổi dữ liệu dưới dạng logical decoding.
   Thực hiện:

sql
Copy
ALTER SYSTEM SET wal_level = logical; -- Yêu cầu restart PostgreSQL nếu đang chạy
SELECT pg_reload_conf(); -- Áp dụng thay đổi không cần restart (nếu hỗ trợ)
b. Cấp quyền cho user postgres
User kết nối đến Debezium cần quyền REPLICATION và SELECT trên bảng cần theo dõi.
Thực hiện:

sql
Copy
ALTER ROLE postgres WITH REPLICATION; -- Cho phép đọc WAL logs
GRANT SELECT ON TABLE public.Todos TO postgres; -- Cho phép đọc dữ liệu
c. Tạo replication slot (nếu Debezium không tự tạo)
Debezium thường tự tạo slot, nhưng bạn có thể tạo thủ công để kiểm soát:

sql
Copy
SELECT pg_create_logical_replication_slot('debezium_slot_todos', 'pgoutput'); 2. Tại sao phải cấu hình PostgreSQL?
Yêu cầu Mục đích Hậu quả nếu thiếu
wal_level = logical Cho phép đọc thay đổi dữ liệu dạng logic (thay vì chỉ vật lý) Debezium không thể bắt sự kiện
Quyền REPLICATION Để đọc WAL logs Lỗi permission denied khi kết nối
Quyền SELECT Để đọc schema và dữ liệu ban đầu Không đồng bộ được snapshot đầu tiên 3. Kiểm tra cấu hình PostgreSQL
Sau khi áp dụng, kiểm tra bằng các lệnh sau:

sql
Copy
-- Kiểm tra WAL level
SHOW wal_level; -- Phải hiển thị 'logical'

-- Kiểm tra replication slot
SELECT \* FROM pg_replication_slots WHERE slot_name = 'debezium_slot_todos';

-- Kiểm tra quyền user
\du postgres -- Phải có attribute "Replication" 4. Lưu ý khi dùng Docker
Nếu PostgreSQL của bạn chạy trong Docker (như file docker-compose.yml), có 2 cách áp dụng cấu hình:

Cách 1: Thêm trực tiếp vào environment
yaml
Copy
environment:

- POSTGRES_USER=postgres
- POSTGRES_PASSWORD=postgres
- POSTGRES_DB=tododb
- POSTGRES_INITDB_ARGS=--wal-level=logical # Áp dụng ngay khi khởi tạo
  Cách 2: Chạy lệnh SQL sau khi container khởi động
  bash
  Copy
  docker exec -it todoapp-postgres psql -U postgres -c "ALTER SYSTEM SET wal_level = logical; SELECT pg_reload_conf();"

5. Tự động hóa với Docker (khuyến nghị)
   Thêm script khởi tạo vào volume PostgreSQL để tự động chạy khi container khởi động:

Tạo file init.sql:

sql
Copy
ALTER SYSTEM SET wal_level = logical;
ALTER ROLE postgres WITH REPLICATION;
Mount vào container:

yaml
Copy
volumes:

- ./init.sql:/docker-entrypoint-initdb.d/init.sql

6. Xử lý lỗi thường gặp
   Lỗi "WAL level không đủ":

log
Copy
ERROR: logical decoding requires wal_level >= logical
→ Khắc phục: Đảm bảo đã chạy ALTER SYSTEM SET wal_level = logical và restart PostgreSQL.

Lỗi quyền:

log
Copy
ERROR: permission denied to create replication slot
→ Khắc phục: Cấp quyền REPLICATION cho user.

Kết luận
✅ Bạn phải cấu hình PostgreSQL (WAL level + quyền user) trước khi chạy Debezium Connector.
✅ Chỉ cần làm 1 lần duy nhất (trừ khi xóa database/container).
✅ Nếu dùng Docker, có thể tự động hóa qua POSTGRES_INITDB_ARGS hoặc volume init script.

Sau khi hoàn thành các bước này, Debezium sẽ hoạt động ngay lập tức với bảng Todos! 🎉

ERROR: logical decoding requires wal_level >= logical
Lỗi này xảy ra khi bạn chưa cấu hình PostgreSQL đúng cách để hỗ trợ logical decoding - yêu cầu bắt buộc để Debezium hoạt động. Đây là cách khắc phục chi tiết:

1. Nguyên nhân lỗi
   PostgreSQL mặc định sử dụng wal_level = replica, trong khi Debezium cần wal_level = logical để đọc thay đổi dữ liệu.

2. Cách khắc phục
   Bước 1: Kiểm tra WAL level hiện tại
   Kết nối đến PostgreSQL (qua psql hoặc Docker exec):

sql
Copy
SHOW wal_level;
Nếu kết quả là replica hoặc minimal, bạn cần thay đổi.

Bước 2: Thay đổi WAL level
Cách 1: Dùng SQL (yêu cầu restart PostgreSQL)

sql
Copy
ALTER SYSTEM SET wal_level = logical;
→ Sau đó restart container PostgreSQL:

bash
Copy
docker restart todoapp-postgres
Cách 2: Thêm trực tiếp trong Docker Compose
Sửa file docker-compose.yml của service postgres:

yaml
Copy
environment:

- POSTGRES_USER=postgres
- POSTGRES_PASSWORD=postgres
- POSTGRES_DB=tododb
- POSTGRES_INITDB_ARGS=--wal-level=logical # Quan trọng!
  Bước 3: Kiểm tra lại
  Sau khi restart, kiểm tra lại:

sql
Copy
SHOW wal_level; -- Phải hiển thị "logical" 3. Nếu PostgreSQL không restart được
Nếu không thể restart PostgreSQL (do dữ liệu quan trọng), dùng lệnh sau để áp dụng không cần restart (PostgreSQL 9.6+):

sql
Copy
ALTER SYSTEM SET wal_level = logical;
SELECT pg_reload_conf(); -- Áp dụng thay đổi động 4. Tạo lại Replication Slot (nếu cần)
Nếu slot cũ được tạo khi wal_level chưa đúng, xóa và tạo lại:

sql
Copy
SELECT pg_drop_replication_slot('debezium_slot_todos');
SELECT pg_create_logical_replication_slot('debezium_slot_todos', 'pgoutput'); 5. Kiểm tra quyền User
Đảm bảo user postgres có quyền:

sql
Copy
ALTER ROLE postgres WITH REPLICATION;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO postgres; 6. Khởi động lại Debezium Connector
Sau khi sửa PostgreSQL:

bash
Copy
curl -X DELETE http://localhost:8083/connectors/todoapp-todos-connector
curl -X POST http://localhost:8083/connectors -H "Content-Type: application/json" -d '{
"name": "todoapp-todos-connector",
"config": {
... // Giữ nguyên cấu hình trước đó
}
}' 7. Lưu ý quan trọng
Production: Nếu PostgreSQL đang chạy production data, hãy thực hiện vào thời điểm ít traffic.

Backup: Sao lưu dữ liệu trước khi thay đổi WAL level.

Docker: Nếu dùng volume, cấu hình sẽ tồn tại cả khi container bị xóa.

Kiểm tra tổng thể
bash
Copy

# Kiểm tra PostgreSQL

docker exec -it todoapp-postgres psql -U postgres -c "SHOW wal_level;"

# Kiểm tra Debezium logs

docker logs todoapp-kafka-connect
Sau các bước này, Debezium sẽ hoạt động bình thường! 🎉 Nếu còn lỗi, hãy chia sẻ log từ kafka-connect để mình phân tích sâu hơn.

{
"name": "todoapp-todos-connector",
"config": {
// Loại connector (bắt buộc)
"connector.class": "io.debezium.connector.postgresql.PostgresConnector",

    // Thông tin kết nối PostgreSQL (bắt buộc)
    "database.hostname": "postgres",       // Tên service trong Docker network
    "database.port": "5432",               // Port mặc định của PostgreSQL
    "database.user": "postgres",           // User có quyền replication
    "database.password": "postgres",       // Password
    "database.dbname": "tododb",           // Tên database cần theo dõi

    // Định danh logical (bắt buộc)
    "database.server.name": "todoapp-pg",  // Tiền tố cho topic Kafka

    // Theo dõi bảng cụ thể (bắt buộc)
    "table.include.list": "public.Todos",  // Format: schema.table

    // Cấu hình Kafka (bắt buộc)
    "topic.prefix": "todoapp-cdc",         // Tiền tố topic (sẽ thành: todoapp-cdc.public.Todos)

    // Replication slot (khuyến nghị đặt tên tường minh)
    "slot.name": "debezium_slot_todos",    // Tạo slot nếu chưa tồn tại
    "publication.name": "debezium_pub_todos", // Publication name cho PostgreSQL 10+

    // Plugin decoding (bắt buộc)
    "plugin.name": "pgoutput",             // Sử dụng cho PostgreSQL 10+

    // Transform để đơn giản hóa message (tùy chọn nhưng khuyến nghị)
    "transforms": "unwrap",                // Bật tính năng transform
    "transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState", // Chỉ lấy trạng thái sau thay đổi
    "transforms.unwrap.drop.tombstones": "false", // Giữ lại tombstone records
    "transforms.unwrap.delete.handling.mode": "rewrite" // Chuẩn hóa sự kiện DELETE

}
}
