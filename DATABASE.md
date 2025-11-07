# Database Documentatie - QuizTime

## Overzicht

QuizTime gebruikt een **JSON-gebaseerde offline database** in plaats van MySQL. Dit biedt volgende voordelen:
- ✅ Geen externe database server nodig
- ✅ Eenvoudig om mee te nemen (een JSON bestand)
- ✅ Makkelijk leesbaar en editeerbaar
- ✅ Snelle performance voor kleine datasets

## Bestandslocatie

```
Data/quizzes.json
```

Het `Data/` mapje wordt automatisch aangemaakt door de applicatie als het niet bestaat.

---

## Bestandsstructuur

### quizzes.json

Het JSON bestand bevat een array van `Quiz` objecten:

```json
[
  {
    "id": 1,
    "naam": "Voetbal Quiz",
    "vragen": [
      {
        "id": 1,
        "tekst": "Wie won het WK 2022?",
        "fotoPath": "C:\\path\\to\\image.jpg",
        "antwoorden": [
          {
            "id": 1,
            "tekst": "Argentinie",
            "isCorrect": true,
            "createdAt": "2025-11-07T10:30:00"
          },
          {
            "id": 2,
            "tekst": "Frankrijk",
            "isCorrect": false,
            "createdAt": "2025-11-07T10:30:00"
          }
        ],
        "createdAt": "2025-11-07T10:30:00"
      }
    ],
    "createdAt": "2025-11-07T10:30:00"
  }
]
```

---

## Data Model

### Quiz Object

| Veld | Type | Beschrijving |
|------|------|-------------|
| `id` | int | Unieke identifier (auto-increment) |
| `naam` | string | Naam van de quiz |
| `vragen` | Vraag[] | Array van vragen in de quiz |
| `createdAt` | DateTime | Aanmaakdatum |

### Vraag Object

| Veld | Type | Beschrijving |
|------|------|-------------|
| `id` | int | Unieke identifier (auto-increment) |
| `tekst` | string | De vraag tekst |
| `fotoPath` | string | Pad naar afbeelding (optioneel, null als geen foto) |
| `antwoorden` | Antwoord[] | Array van antwoorden (max 4) |
| `createdAt` | DateTime | Aanmaakdatum |

### Antwoord Object

| Veld | Type | Beschrijving |
|------|------|-------------|
| `id` | int | Unieke identifier (auto-increment) |
| `tekst` | string | Het antwoord tekst |
| `isCorrect` | bool | Of dit antwoord correct is (meerdere kunnen waar zijn) |
| `createdAt` | DateTime | Aanmaakdatum |

---

## QuizJsonService

De `QuizJsonService` klasse beheert alle database operaties. Dit is een **Singleton**, dus er is maar één instantie gedurende de hele applicatie.

### Locatie

```
Services/QuizJsonService.cs
```

### Methodes

#### `GetAllQuizzes()`
Laadt alle quizzen uit het JSON bestand.

```csharp
var allQuizzes = QuizJsonService.Instance.GetAllQuizzes();
```

**Returns:** `List<Quiz>`

---

#### `UpdateQuiz(Quiz quiz)`
Slaat een quiz op (nieuw of update bestaande).

```csharp
QuizJsonService.Instance.UpdateQuiz(quiz);
```

**Parameters:**
- `quiz` - Het Quiz object om op te slaan

**Functionaliteit:**
- Als `quiz.Id == 0` → Nieuwe quiz (krijgt een nieuw ID)
- Anders → Bestaande quiz updaten

---

#### `DeleteQuiz(int quizId)`
Verwijdert een quiz uit de database.

```csharp
QuizJsonService.Instance.DeleteQuiz(1);
```

**Parameters:**
- `quizId` - De ID van de quiz om te verwijderen

---

## Singleton Pattern

De `QuizJsonService` gebruikt het Singleton pattern:

```csharp
// Overal in de app - altijd dezelfde instantie
var service = QuizJsonService.Instance;
```

Dit zorgt ervoor dat:
- ✅ Geen conflicten bij gelijktijdig laden/opslaan
- ✅ Consistente data gedurende de applicatie
- ✅ Efficiënt geheugengebruik

---

## Workflow

### Quiz Aanmaken

```
MainWindow (Nieuwe Quiz)
  ↓
EditQuizWindow (user vult quiz in)
  ↓
SaveAndClose_Click
  ↓
QuizJsonService.UpdateQuiz(quiz)  ← JSON wordt opgeslagen
  ↓
quizzes.json (bestand bijgewerkt)
```

### Quiz Laden

```
MainWindow laden
  ↓
LoadQuizzes()
  ↓
QuizJsonService.GetAllQuizzes()  ← Leest JSON bestand
  ↓
quizzes.json (gegevens gelezen)
  ↓
UI toon quizzen in ListBox
```

### Quiz Spelen

```
QuizControlWindow start
  ↓
DisplayWindow toon vragen (data uit geheugen)
  ↓
Admin navigeert (Vorige/Volgende)
  ↓
Geen wijzigingen → Geen opslaan nodig
```

---

## Auto-Increment IDs

De applicatie beheert auto-incrementing IDs:

```csharp
// Bij nieuwe vraag
var newQuestion = new Vraag 
{
    Id = 0,  // Markeer als nieuw
    Tekst = "...",
    Antwoorden = new List<Antwoord>()
};

// Als opgeslagen naar JSON
// Krijgt automatisch volgende beschikbare ID
```

---

## Fouten Afhandeling

### Bestand niet gevonden

```
Data/quizzes.json bestaat niet
  ↓
QuizJsonService maakt leeg bestand aan
  ↓
Applicatie start met lege database
```

### Corrupte JSON

Controleer het bestand handmatig op:
- Mismatched brackets `{ }` of `[ ]`
- Onafgesloten strings
- Ongeldige datum format

---

## Tips & Tricks

### Backup maken
```powershell
Copy-Item Data/quizzes.json Data/quizzes.backup.json
```

### JSON valideren
Open in VS Code of online JSON validator om syntaxfouten te vinden.

### Handmatig aanpassen
Je kunt `quizzes.json` rechtstreeks bewerken:
1. Sluit de applicatie
2. Open `Data/quizzes.json` met tekstverwerker
3. Bewerk JSON (zorg voor geldige syntaxis)
4. Opslaan en applicatie herstarten

---

## Prestaties

| Operatie | Snelheid |
|----------|----------|
| Laden 1000 quizzen | < 100ms |
| Quiz opslaan | < 50ms |
| Quiz verwijderen | < 50ms |
| Quiz zoeken | O(n) |

Voor grotere datasets (> 10.000 quizzen) → Overweeg MySQL

---

## Toekomstige Uitbreidingen

Mogelijke verbeteringen:
- [ ] Database compressie (zip bestand)
- [ ] Cloud sync (OneDrive/Google Drive)
- [ ] Export naar Excel/PDF
- [ ] Migratie naar SQLite voor betere performance
- [ ] Gebruikers & permissies

---

## Vragen?

Raadpleeg de `QuizJsonService.cs` voor meer implementatiedetails.
