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
    Money INT NOT NULL DEFAULT(0) COMMENT '돈',
    Stage INT NOT NULL DEFAULT(0) COMMENT '최종 클리어 스테이지 번호'
) COMMENT '플레이어 게임 데이터';
```

### ItemData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. ItemData
(
    UserItemId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    ItemCode INT NOT NULL COMMENT '아이템 코드',
    Amount INT NOT NULL DEFAULT(1) COMMENT '수량',
    EnchanceCount INT NOT NULL DEFAULT(0) COMMENT '강화 횟수',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력'
) COMMENT '플레이어 아이템 데이터';
```

### Mailbox Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. Mailbox
(
    MailId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '메일 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    Title VARCHAR(40) NOT NULL COMMENT '메일 제목',
    ExpiryDate DATETIME NOT NULL COMMENT '만료 날짜',
    IsRead BOOL NOT NULL COMMENT '확인 여부',
    Comment VARCHAR(100) NOT NULL COMMENT '메일 내용'
) COMMENT '우편함 데이터';
```

### MailboxItem Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. MailboxItem
(
    MailId BIGINT NOT NULL COMMENT '메일 번호',
    ItemCode INT NOT NULL COMMENT '아이템 코드',
    Amount INT NOT NULL COMMENT '아이템 수량',
    EnchanceCount INT NOT NULL DEFAULT(0) COMMENT '강화 횟수',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력',
    FOREIGN KEY(MailId) REFERENCES Mailbox(MailId) ON DELETE Cascade
) COMMENT '우편함 아이템 데이터';
```

### Attendance Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. Attendance
(
    AccountId BIGINT NOT NULL PRIMARY KEY COMMENT '계정 번호',
    LastCheckDate DATE NOT NULL COMMENT '마지막 출석 날짜',
    ContinuousPeriod INT NOT NULL DEFAULT(0) COMMENT '연속 출석 기간',
    FOREIGN KEY(AccountId) REFERENCES GameData(AccountId) ON DELETE Cascade
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
## Masterdata
### ItemMasterData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. ItemMasterData
(
    ItemCode INT NOT NULL PRIMARY KEY COMMENT '아이템 코드',
    Name VARCHAR(20) NOT NULL COMMENT '이름',
    Attribute INT NOT NULL COMMENT '특성',
    Sell INT NOT NULL COMMENT '판매 금액',
    Buy INT NOT NULL COMMENT '구입 금액',
    UseLv INT NOT NULL COMMENT '사용가능 레벨',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력',
    EnchanceCount INT NOT NULL COMMENT '최대 강화 횟수',
    IsOverlapped BOOL NOT NULL COMMENT '겹침 가능 여부'
) COMMENT '아이템 마스터 데이터';
```

### ItemAttributeMasterData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. ItemAttributeMasterData
(
    ItemCode INT NOT NULL PRIMARY KEY COMMENT '아이템 코드',
    Name VARCHAR(20) NOT NULL COMMENT '특성 이름'
) COMMENT '아이템 속성 마스터 데이터';
```

### AttendanceMasterData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. AttendanceMasterData
(
    Code INT NOT NULL PRIMARY KEY COMMENT '날짜',
    ItemCode INT NOT NULL COMMENT '아이템 코드',
    Count INT NOT NULL COMMENT '개수'
) COMMENT '출석부보상 마스터 데이터';
```

### ProductMasterData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. ProductMasterData
(
    Code INT NOT NULL COMMENT '상품번호',
    ItemCode INT NOT NULL COMMENT '아이템 코드',
    ItemName VARCHAR(20) NOT NULL COMMENT '아이템 이름',
    ItemCount INT NOT NULL COMMENT '개수'
) COMMENT '인앱상품 마스터 데이터';
```

### StageItemMasterData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. StageItemMasterData
(
    Code INT NOT NULL COMMENT '스테이지 단계',
    ItemCode INT NOT NULL COMMENT '파밍가능 아이템'
) COMMENT '스테이지 아이템 마스터 데이터';
```

### StageNpcMasterData Table
```sql
CREATE TABLE IF NOT EXISTS GameDB. StageNpcMasterData
(
    Code INT NOT NULL COMMENT '스테이지 단계',
    NPCCode INT NOT NULL COMMENT '공격 NPC',
    Count INT NOT NULL COMMENT '개수',
    Exp INT NOT NULL COMMENT '1개당 보상경험치'
) COMMENT '스테이지 NPC 마스터 데이터';
```