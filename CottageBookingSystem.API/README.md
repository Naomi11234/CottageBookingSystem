# CottageBookingSystem API

מערכת Web API לניהול צימרים והזמנתם. ה-API מאפשר לצפות בצימרים ובמתקנים שלהם, ליצור הזמנות, לרשום משתמשים ולהתחבר באמצעות JWT. פעולות ניהול צימרים מוגבלות למשתמשים בתפקיד Admin.

## טכנולוגיות

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8 עם SQL Server
- JWT Bearer authentication
- Swagger / OpenAPI
- xUnit ו-Moq לבדיקות יחידה

## המשאב המוגבל ו-Optimistic Concurrency

המשאב המוגבל הוא צימר בטווח תאריכים מסוים: אין לאפשר שתי הזמנות חופפות לאותו צימר. המאגר בודק אם קיימת הזמנה חופפת, וטוען את רשומת הצימר לפני שמירת ההזמנה.

לישות `Cottage` יש `RowVersion` המוגדר ב-EF Core באמצעות `IsRowVersion()` וממופה לעמודת SQL Server מסוג `rowversion`. כחלק מיצירת כל הזמנה, `BookingVersion` של הצימר משתנה ונשמר יחד עם שורת ההזמנה באותה פעולת `SaveChangesAsync`. כאשר שתי בקשות קוראות את אותה גרסת צימר, העדכון המאוחר עם גרסת `RowVersion` הישנה אינו מעדכן שורה ו-EF Core זורק `DbUpdateConcurrencyException`. השירות רושם Warning ומתרגם את החריגה ל-`BookingConflictException`; ה-API מחזיר `409 Conflict` עם הודעה שמבקשת לבחור תאריכים אחרים.

הגנה זו נשענת על SQL Server ועל שמירת שינוי הצימר וההזמנה יחד. יש להחיל את מיגרציות EF Core לפני הרצת המערכת מול מסד נתונים חדש או קיים.

## משתמשים ופרטי דמו

לא מוגדרים בפרויקט משתמשי דמו או סיסמאות מוכנות, ולא נוצר אוטומטית משתמש Admin. בהרשמה דרך `POST /api/auth/register` נוצר משתמש בתפקיד `Customer` בלבד; ה-API מחזיר token, ולאחר מכן ניתן להיכנס דרך `POST /api/auth/login`.

לצורך בדיקת פעולות Admin בסביבת פיתוח בלבד:

1. רשמו משתמש Customer עם סיסמה חזקה שבחרתם.
2. במסד הנתונים המקומי, שנו את תפקיד המשתמש ל-`Admin` לפי כתובת הדוא"ל שנשמרה. השירות מנרמל כתובות דוא"ל לאותיות גדולות.
3. התחברו מחדש כדי לקבל JWT חדש המכיל את התפקיד המעודכן.

דוגמה לשאילתת פיתוח מקומית בלבד (החליפו את הערך בכתובת הדוא"ל שנשמרה):

```sql
UPDATE dbo.Users
SET Role = 'Admin'
WHERE Email = 'ADMIN@EXAMPLE.COM';
```

אין לבצע שינוי תפקיד כזה בסביבת Production ללא מנגנון ניהול והרשאות מאובטח. אין לשמור סיסמאות או מפתחות JWT בקובצי המקור.

## דרישות מקדימות

- .NET SDK 8.0
- SQL Server או SQL Server LocalDB
- כלי `dotnet-ef` בגרסה תואמת ל-EF Core 8 לצורך יצירה ויישום מיגרציות

אם `dotnet ef` אינו מותקן, התקינו אותו:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.22
```

## הגדרה והרצה מקומית (PowerShell)

הריצו את הפקודות מתיקיית השורש של ה-repository. מחרוזת החיבור וערכי JWT נשמרים ב-User Secrets של פרויקט ה-API ולא בקובץ הגדרות שנכנס ל-Git:

```powershell
dotnet user-secrets --project .\CottageBookingSystem.API\CottageBookingSystem.API.csproj set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=CottageBookingSystem;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet user-secrets --project .\CottageBookingSystem.API\CottageBookingSystem.API.csproj set "Jwt:Issuer" "CottageBookingSystem"
dotnet user-secrets --project .\CottageBookingSystem.API\CottageBookingSystem.API.csproj set "Jwt:Audience" "CottageBookingSystem.Client"
```

צרו מפתח חתימה מקומי אקראי באורך 32 בתים לפחות והגדירו אותו ב-User Secrets:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets --project .\CottageBookingSystem.API\CottageBookingSystem.API.csproj set "Jwt:SigningKey" $key
```

### יצירה/עדכון של סכמת מסד הנתונים

מיגרציות EF Core נמצאות בפרויקט Data. עבור מסד נתונים חדש או לאחר הוספת מיגרציה, החילו אותן באמצעות:

```powershell
dotnet ef database update --project .\CottageBookingSystem.Data\CottageBookingSystem.Data.csproj --startup-project .\CottageBookingSystem.API\CottageBookingSystem.API.csproj
```

ה-DbContext כולל נתוני בסיס לצימרים ולמתקנים. הוא אינו מאתחל חשבונות משתמשים או סיסמאות; משתמשים נוצרים בהרשמה דרך ה-API.

### יצירת הזמנה

`POST /api/bookings` דורש JWT של משתמש Customer או Admin. גוף הבקשה הוא JSON, ושמות המאפיינים נשלחים ב-camelCase:

```json
{
  "cottageId": 1,
  "startDate": "2030-07-10T00:00:00Z",
  "endDate": "2030-07-12T00:00:00Z",
  "guestsCount": 2,
  "notes": "Late arrival"
}
```

`guestsCount` חייב להיות חיובי ולא לחרוג מקיבולת הצימר. `notes` אופציונלי, עד 1000 תווים. למשתמשים קיימים או לקוחות ישנים שנשלחים בלי `guestsCount`, ברירת המחדל היא אורח אחד.

### הרצת ה-API והבדיקות

```powershell
dotnet run --project .\CottageBookingSystem.API\CottageBookingSystem.API.csproj
```

בפרופיל HTTP המקומי כתובת ה-Swagger היא `http://localhost:5109/swagger`. פרופיל HTTPS מוגדר גם הוא בקובץ `Properties/launchSettings.json`; אם בחרתם בו, פתחו את כתובת ה-Swagger בכתובת ה-HTTPS של הפרופיל.

לבדיקות יחידה:

```powershell
dotnet test .\CottageBookingSystem.Tests\CottageBookingSystem.Tests.csproj
```

## Endpoints עיקריים

| Method | Route | תיאור |
|---|---|---|
| `GET` | `/api/cottages` | רשימת צימרים מדורגת |
| `GET` | `/api/cottages/{id}` | פרטי צימר |
| `POST` | `/api/cottages` | יצירת צימר (Admin) |
| `PUT` | `/api/cottages/{id}` | עדכון צימר (Admin) |
| `DELETE` | `/api/cottages/{id}` | מחיקת צימר (Admin; הזמנות קיימות עשויות למנוע מחיקה) |
| `POST` | `/api/bookings` | יצירת הזמנה (Customer או Admin) |
| `GET` | `/api/bookings/{id}` | הצגת הזמנה לבעליה או ל-Admin |
| `POST` | `/api/auth/register` | הרשמת Customer |
| `POST` | `/api/auth/login` | התחברות וקבלת JWT |
| `GET` | `/api/auth/me` | פרטי המשתמש מה-token |

ב-Swagger לחצו על **Authorize** והזינו את ה-JWT שהתקבל מההרשמה או מההתחברות כדי לבדוק endpoints מוגנים. תוקף והרשאות ה-token נבדקים בכל בקשה.
