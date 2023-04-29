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
    UserId VARCHAR(15) NOT NULL UNIQUE COMMENT '유저 아이디',
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
    AccountId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정 번호',
    UserId VARCHAR(15) NOT NULL UNIQUE COMMENT '아이디',
    Level INT NOT NULL DEFAULT(1) COMMENT '레벨',
    Exp INT NOT NULL DEFAULT(0) COMMENT '경험치',
    Hp INT NOT NULL DEFAULT(20) COMMENT '체력',
    Ap INT NOT NULL DEFAULT(10) COMMENT '공격력',
    Mp INT NOT NULL DEFAULT(10) COMMENT '마력',
    Stage INT NOT NULL DEFAULT(0) COMMENT '최종 클리어 스테이지 번호'
) COMMENT '플레이어 게임 데이터';
```

### ItemData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. ItemData
(
    ItemId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    ItemCode INT NOT NULL COMMENT '아이템 코드',
    Amount INT NOT NULL DEFAULT(1) COMMENT '수량',
    EnchanceCount INT NOT NULL DEFAULT(0) COMMENT '강화 횟수',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력',
) COMMENT '플레이어 아이템 데이터';
```

### Mailbox Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. Mailbox
(
    MailId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '메일 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    Title VARCHAR(20) NOT NULL COMMENT '메일 제목',
    ItemCode INT NOT NULL COMMENT '아이템 코드',
    Amount INT NOT NULL COMMENT '아이템 수량',
    ExpiryDate NOT NULL DATETIME COMMENT '만료 날짜',
    IsRead BOOL NOT NULL COMMENT '확인 여부',
) COMMENT '우편함 데이터';
```

### Attendance Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. Attendance
(
    AccountId BIGINT NOT NULL PRIMARY KEY COMMENT '계정 번호',
    LastCheckDate DATE NOT NULL COMMENT '마지막 출석 날짜',
    IsChecked BOOL NOT NULL COMMENT '출석 여부'
) COMMENT '출석부 데이터';
```

### PayInfo Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. PayInfo
(
    PaymentId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '결제 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    PaymentDate DATETIME NOT NULL COMMENT '결제 날짜',
    SerialNum VARCHAR(50) NOT NULL COMMENT '일련 번호'
) COMMENT '결제 정보';
```