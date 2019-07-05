# Query initialization

A new query instance for a specific model class may be created like so.

```csharp
dao.Query<ClassName>() 
```



Generelles Beispiel

var dao = new GenericDao();
// Customer muss als Klasse im Ordner „Contacts“ definiert sein
// Der QueryBuilder legt dann per Konvention den Tabellennamen 
// "Contacts.Customers" zugrunde
var result = dao.Query<Customer>()
 	.From("Contacts.Customers") // kann bei Verwendung der o.G. Konvention entfallen
 	.Where(x => x.Field("LastName").IsEqualTo("Wolff"))  // Operatorübersicht s.u.
 	.And(x => x.Field("City").IsEqualTo("Hamm")
 	.OrderBy("LastName")
 	.ToList();

Abfragebedingungen
Die Definition für Abfragebedingungen wird mit der Methode Where(condition) eingeleitet und kann anschließend mit den logischen Operator-Methoden And(condition) und Or(condition) erweitert werden:
query = query.Where(condition1).And(condition2);
query = query.Where(condition1).Or(condition2);
And hat dabei Vorrang vor Or (analog zu C# und SQL).
Eine Klammerung kann durch interne Verschachtelung der Bedingungen erreicht werden:
query = query.Where(condition1.Or(condition2)).And(condition3);
Bedingungssyntax
Bedingungen warden mit der Field-Methode eingeleitet, die das abzufragende Feld in der Datenbank definiert:
var query = query.Where(x => x.Field("Fieldname").ConditionMethod()); 
var query = query.Where(x => x.Field("Fieldname").ConditionMethod())
 	.And(x => x.Field("Fieldname2").ConditionMethod());

!!! Bei verschachtelten Bedingungen muss die Variable im Lambda-Ausdruck geändert werden.
query = query.Where(x => x.Field("Fieldname").ConditionMethod()
 	.Or(y => y.Field("Fieldname2").ConditionOperator()));
Vergleichsoperatoren
Für die oben genannte "ConditionMethod" können die folgenden Operator-Methoden eingefügt werden.
QsMapper-Operator	SQL-Operator
IsEqualTo(object value)	= value
IsGreaterThan(object value)	> value
IsGreaterOrEqual(object value)	>= value
IsLessThan(object value)	< value
IsLessOrEqual(object value)	<= value
IsLike(string value)	like 'value'
Contains(string value)	like '%value%'
IsNull()	is null
IsTrue()	isnull(field, 0) = 1
IsFalse()	isnull(field, 0) = 0
IsBetween(object value1).And(object value2)	between value1 and value2
IsIn(params object[] values)	in (values[0], values[1], …)

Alle Vergleichsoperatoren können mit Not invertiert werden.
var result = dao.Query<Customer>()
 	.Where(x => x.Field("LastName").Not.IsEqualTo("Wolff"))  // Operatorübersicht s.u.
 	.ToList();
Sortierung
An die Abfrage können eine oder mehrere Sortierungen angefügt werden:
query = query.OrderBy("Fieldname")[.Ascending];
query = query.OrderBy("Fieldname").Descending;
query = query.OrderBy("Fieldname").Ascending
 	.ThenBy("Fieldname2").Descending
 	.ThenBy(…);

Rückgabemethoden
Die Abfrage kann mit folgenden Rückgabemethoden abgeschlossen werden:
Rückgabemethode	Beschreibung
IList<T> ToList()	Gibt eine IList<T> mit dem in der Query-Methode angegebenen Objekttyp zurück.
IList<T> ToExtendedList()	Fragt zusätzlich die im Catalog definierten 1:n und n:m-Beziehungen ab.
IList<T1> ToListOf<T1>()	Gibt das Ergebnis als Liste vom Objekttyp T1 zurück, wobei T1 von T ableiten muss.
T FirstOrDefault()	Gibt das erste Objekt des Ergebnisses zurück. Wenn keine Ergebnisse gefunden wurden wird null zurückgegeben.
int Count()	Zählt die gefundenen Ergebnisse (verwendent count(*) im SQL-Ausdruck)





