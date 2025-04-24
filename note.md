1. Các cấu hình bắt buộc trên PostgreSQL
   a. Bật logical replication (WAL level)
   Debezium cần PostgreSQL ghi lại thay đổi dữ liệu dưới dạng logical decoding.
   Thực hiện: Đối với postgres
   + ALTER SYSTEM SET wal_level = logical; -- Yêu cầu restart PostgreSQL nếu đang chạy
   SELECT pg_reload_conf(); -- Áp dụng thay đổi không cần restart (nếu hỗ trợ)
   ALTER ROLE postgres WITH REPLICATION; -- Cho phép đọc WAL logs
   SHOW wal_level; -- Phải hiển thị 'logical'

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

{
"name": "todoapp-connector-all-tables",
"config": {
"connector.class": "io.debezium.connector.postgresql.PostgresConnector",
"database.hostname": "postgres",
"database.port": "5432",
"database.user": "postgres",
"database.password": "postgres",
"database.dbname": "tododb",

    "database.server.name": "todoapp-db",       // Tên logic → tạo prefix topic
    "plugin.name": "pgoutput",

    "slot.name": "debezium_slot_all",
    "publication.name": "debezium_pub_all",

    "topic.prefix": "todoapp-cdc",              // Debezium sẽ tạo topic dạng: todoapp-cdc.public.<table>

    "transforms": "unwrap",
    "transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState",
    "transforms.unwrap.drop.tombstones": "false",
    "transforms.unwrap.delete.handling.mode": "rewrite"

/// 🔥 Chỉ nhận thay đổi mới
"snapshot.mode": "never",
"message.key.columns": "public.products:id",
}
}
///
{
"connector.class": "io.debezium.connector.postgresql.PostgresConnector",
"message.key.columns": "public.products:id",
"database.user": "postgres",
"database.password": "postgres",
"database.hostname": "postgres",
"database.port": "5432",
"database.dbname": "tododb",
"database.server.name": "todoapp-pg2",
"table.include.list": "public.products",
"snapshot.mode": "never",
"slot.name": "debezium_slot_product_v2",
"publication.name": "debezium_pub_todos_v2",
"plugin.name": "pgoutput",
"topic.prefix": "topic-products",
"transforms.unwrap.delete.handling.mode": "rewrite",
"transforms.unwrap.drop.tombstones": "false",
"transforms": "unwrap,tsCreated,tsUpdated",
"transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState",

    // Định nghĩa các transformation để chuyển đổi timestamp sang ISO 8601
    "transforms.tsCreated.type": "org.apache.kafka.connect.transforms.TimestampConverter$Value",
    "transforms.tsCreated.field": "created_at",
    "transforms.tsCreated.target.type": "string",
    "transforms.tsCreated.format": "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'",

    "transforms.tsUpdated.type": "org.apache.kafka.connect.transforms.TimestampConverter$Value",
    "transforms.tsUpdated.field": "updated_at",
    "transforms.tsUpdated.target.type": "string",
    "transforms.tsUpdated.format": "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'",

    "time.precision.mode": "adaptive_time_microseconds",
    "name": "todoapp-products-connector"

}

SELECT pg_reload_conf();

/// xem tất cả 
SELECT \* FROM pg_replication_slots;

SELECT pg_drop_replication_slot('debezium_slot_log');
