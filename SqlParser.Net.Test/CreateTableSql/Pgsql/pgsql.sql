CREATE TABLE source_table (
  id INT PRIMARY KEY,
  name VARCHAR(255)
);


CREATE TABLE target_table (
  id INT PRIMARY KEY,
  name VARCHAR(255)
);

DROP TABLE IF EXISTS articles;

CREATE TABLE articles (
                          id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                          stance VARCHAR(50) NOT NULL,
                          created_at TIMESTAMP NOT NULL
);

INSERT INTO articles (stance, created_at)
VALUES
    ('A', '2024-01-01 10:00:00'),
    ('A', '2024-02-01 10:00:00'),
    ('A', '2024-03-01 10:00:00'),
    ('B', '2024-01-05 10:00:00'),
    ('B', '2024-02-05 10:00:00'),
    ('C', '2024-01-10 10:00:00');

SELECT DISTINCT ON (stance)
    id,
    stance,
    created_at
FROM articles
ORDER BY stance, created_at DESC;



CREATE TABLE users (
                       id   bigint,
                       name text
);

CREATE TABLE orders (
                        id         bigint,
                        user_id    bigint,
                        amount     numeric,
                        created_at timestamp
);


CREATE TYPE "order status" AS ENUM (
    'pending',
    'completed'
);

SELECT "order status" 'pending';


select date_trunc('minute',order_date at time zone 'Asia/ShangHai') at time zone 'Asia/ShangHai' as b from orders2 where (date_trunc('minute',order_date at time zone 'Asia/ShangHai') = '2023-04-19 03:11'::timestamp)


CREATE TABLE orders2 (
                         order_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                         order_date TIMESTAMPTZ NOT NULL
);