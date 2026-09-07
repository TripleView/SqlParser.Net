CREATE TABLE source_table (
	id integer NULL,
	"name" varchar NULL
);

CREATE TABLE target_table (
	id integer NULL,
	"name" varchar NULL
);

CREATE OR REPLACE TYPE splitstr_table AS TABLE OF VARCHAR2(4000);
CREATE OR REPLACE FUNCTION splitstr (
    p_str   IN VARCHAR2,
    p_delim IN VARCHAR2
)
RETURN splitstr_table
PIPELINED
IS
    l_start  PLS_INTEGER := 1;
    l_pos    PLS_INTEGER;
    l_token  VARCHAR2(4000);
BEGIN
    -- 输入字符串为空时，不返回数据
    IF p_str IS NULL THEN
        RETURN;
END IF;

    -- 分隔符不能为空
    IF p_delim IS NULL THEN
        RAISE_APPLICATION_ERROR(
            -20001,
            '分隔符不能为空'
        );
END IF;

    LOOP
l_pos := INSTR(p_str, p_delim, l_start);

        IF l_pos = 0 THEN
            -- 最后一段字符串
            l_token := SUBSTR(p_str, l_start);
PIPE ROW (l_token);
EXIT;
ELSE
            -- 截取当前分隔符之前的字符串
            l_token := SUBSTR(
                p_str,
                l_start,
                l_pos - l_start
            );

PIPE ROW (l_token);

-- 移动到下一个字符串片段
l_start := l_pos + LENGTH(p_delim);
END IF;
END LOOP;

    RETURN;
END;