# DB Schema

## Account Database
계정 정보 데이터베이스
```sql
CREATE DATABASE IF NOT EXISTS AccountDB;
USE AccountDB;
```

### account table
```sql
CREATE TABLE IF NOT EXISTS AccountDB. Account
(
    AccountId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정 번호',
    UserId VARCHAR(15) NOT NULL UNIQUE COMMENT '아이디',
    SaltValue VARCHAR(100) NOT NULL COMMENT '암호화 값',
    HashedPassword VARCHAR(100) NOT NULL COMMENT '해싱된 비밀번호'
) COMMENT '계정 정보 테이블';
```


## Game Database
게임 데이터 데이터베이스
```sql
CREATE DATABASE IF NOT EXISTS GameDB;
USE GameDB;
```
### GameData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. GameData
(
    AccountId BIGINT NOT NULL PRIMARY KEY COMMENT '계정 번호',
    Level INT DEFAULT(1) COMMENT '레벨',
    Exp INT DEFAULT(0) COMMENT '경험치',
    Hp INT DEFAULT(0) COMMENT '체력',
    Ap INT DEFAULT(0) COMMENT '공격력',
    Mp INT DEFAULT(0) COMMENT '마력',
    LastLoginDate DATE COMMENT '마지막 로그인 날짜'
) COMMENT '플레이어 게임 데이터';
```

### ItemData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. ItemData
(
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    ItemCode INT COMMENT '아이템 코드',
    Amount INT DEFAULT(1) COMMENT '수량',
    EnchanceCount INT COMMENT '강화 횟수'
) COMMENT '플레이어 아이템 데이터';
```

### Mailbox Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. Mailbox
(
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    Title VARCHAR(20) COMMENT '메일 제목',
    ItemCode INT COMMENT '아이템 코드',
    Amount INT COMMENT '아이템 수량',
    ExpiryData DATETIME COMMENT '만료 날짜',
    IsRead BOOL COMMENT '확인 여부'
) COMMENT '우편함 데이터';
```