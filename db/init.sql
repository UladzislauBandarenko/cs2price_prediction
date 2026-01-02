-- 1. Высокоуровневые роли
CREATE ROLE app_read  NOLOGIN;
CREATE ROLE app_write NOLOGIN;
CREATE ROLE app_admin NOLOGIN;

-- 2. Наследование ролей
GRANT app_read TO app_write;
GRANT app_write TO app_admin;

-- 3. Пользователь только для чтения / обычной работы приложения
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'cs2_user') THEN
    CREATE USER cs2_user WITH PASSWORD 'cs2_password';
  END IF;
END$$;
GRANT app_read TO cs2_user;

-- 4. Администратор БД для миграций и сидера
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'cs2_admin') THEN
    CREATE USER cs2_admin WITH PASSWORD 'cs2_admin_password';
  END IF;
END$$;
GRANT app_admin TO cs2_admin;

-- 5. Схема cs2 для всех таблиц приложения
CREATE SCHEMA IF NOT EXISTS cs2 AUTHORIZATION cs2_admin;

-- 6. Права на схему cs2
GRANT USAGE ON SCHEMA cs2 TO app_read;
GRANT USAGE ON SCHEMA cs2 TO app_write;
GRANT USAGE, CREATE ON SCHEMA cs2 TO app_admin;

GRANT USAGE ON SCHEMA cs2 TO cs2_user;
GRANT USAGE, CREATE ON SCHEMA cs2 TO cs2_admin;

-- 7. DEFAULT PRIVILEGES в схеме cs2 для объектов, создаваемых cs2_admin
ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
GRANT SELECT ON TABLES TO app_read;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER
ON TABLES TO app_admin;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
GRANT USAGE, SELECT ON SEQUENCES TO app_admin;

-- 8. Права на схему public (служебные объекты EF)
GRANT USAGE ON SCHEMA public TO app_read;
GRANT USAGE ON SCHEMA public TO app_write;
GRANT USAGE, CREATE ON SCHEMA public TO app_admin;
GRANT USAGE, CREATE ON SCHEMA public TO cs2_admin;

-- 9. DEFAULT PRIVILEGES в схеме public для объектов, создаваемых cs2_admin
ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA public
GRANT SELECT ON TABLES TO app_read;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA public
GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER
ON TABLES TO app_admin;

-- 10. Немного безопасности: запретить CREATE в public для всех
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
