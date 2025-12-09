-- 1. Роли высокого уровня (как в требованиях: read / write / admin)
CREATE ROLE app_read NOLOGIN;
CREATE ROLE app_write NOLOGIN;
CREATE ROLE app_admin NOLOGIN;

-- 2. Права на БД cs2db (подключение + временные таблицы)
GRANT CONNECT, TEMP ON DATABASE cs2db TO app_read, app_write, app_admin;

-- 3. Наследование ролей
-- app_write умеет всё, что app_read
GRANT app_read TO app_write;
-- app_admin умеет всё, что app_write
GRANT app_write TO app_admin;

-- 4. Пользователи приложения

-- read-only пользователь для API / ML
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'cs2_user') THEN
    CREATE USER cs2_user WITH PASSWORD 'cs2_password';
  END IF;
END$$;
GRANT app_read TO cs2_user;

-- администратор БД для миграций и сидера
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'cs2_admin') THEN
    CREATE USER cs2_admin WITH PASSWORD 'cs2_admin_password';
  END IF;
END$$;
GRANT app_admin TO cs2_admin;

-- 5. Схема cs2, где живут все таблицы твоего приложения
CREATE SCHEMA IF NOT EXISTS cs2 AUTHORIZATION cs2_admin;

-- 6. Права на схему cs2

-- читать из схемы cs2 (USAGE = можно обращаться к объектам)
GRANT USAGE ON SCHEMA cs2 TO app_read;
GRANT USAGE ON SCHEMA cs2 TO app_write;
GRANT USAGE, CREATE ON SCHEMA cs2 TO app_admin;

-- на всякий случай прямые гранты пользователям
GRANT USAGE ON SCHEMA cs2 TO cs2_user;
GRANT USAGE, CREATE ON SCHEMA cs2 TO cs2_admin;

-- 7. DEFAULT PRIVILEGES для таблиц, которые создаёт cs2_admin в cs2
ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
GRANT SELECT ON TABLES TO app_read;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER
ON TABLES TO app_admin;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
GRANT USAGE, SELECT ON SEQUENCES TO app_admin;

-- 8. Права на схему public (для служебных объектов EF, типа __EFMigrationsHistory)
GRANT USAGE ON SCHEMA public TO app_read;
GRANT USAGE ON SCHEMA public TO app_write;
GRANT USAGE, CREATE ON SCHEMA public TO app_admin;
GRANT USAGE, CREATE ON SCHEMA public TO cs2_admin;

-- 9. Немного безопасности для схемы public (чтобы не каждый мог создавать объекты)
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
