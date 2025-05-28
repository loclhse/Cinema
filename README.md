











Các bước:

- Tạo EntityVM và EntityAddVM

- Tạo Interface I<Entity>Service với các thao tác crud cơ bản

- Tạo Class <Entity>Service, sử dụng IUnitOfWork để thực hiện CRUD

- Vào Dependency Injection add scoped cho Interface tới Repo và Service tương ứng

- Tại thư viện Infra, cấu hình MapperConfig

- Viết controller ứng với phương thức của interface IService

